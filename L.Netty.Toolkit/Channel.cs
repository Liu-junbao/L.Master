using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    class Channel : IChannelMatcher, ISession
    {
        const string TimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        const string QuestionHeader = " Q:";
        const string AnswerHeader = " A:";

        private IChannel _channel;
        private object _qaLocker;
        private Dictionary<string, ChannelQuestionAndAnswer> _qas;
        public Channel(IChannel channel)
        {
            _channel = channel;
            _qaLocker = new object();
            _qas = new Dictionary<string, ChannelQuestionAndAnswer>();
            Id = channel.Id.AsLongText();
            EndPoint = (channel.RemoteAddress as IPEndPoint);
            LocalEndPoint = (channel.LocalAddress as IPEndPoint);
            IsActive = true;
        }
        public Channel(IChannelHandlerContext context) : this(context.Channel) { }
        public string Id { get; }
        public IPEndPoint EndPoint { get; }
        public IPEndPoint LocalEndPoint { get; }
        public bool IsActive { get; private set; }
        public void Inactive()
        {
            IsActive = false;
            Inactived?.Invoke(this, null);
        }
        public async void OnMessage(object message)
        {
            var qa = this.FindQuestionAndAnswer(message?.ToString());
            if (qa != null)
            {
                if (qa.IsQuestioner == false)
                {
                    if (qa.IsAnswered)
                    {
                        await this.Answer(qa);//已答复的消息，自动响应
                    }
                    else if (qa.IsAnswering == false)
                    {
                        Requested?.Invoke(this, qa);
                    }
                }
            }

            RefreshQA();
        }
        private ChannelQuestionAndAnswer FindQuestionAndAnswer(string message)
        {
            if (string.IsNullOrEmpty(message)) return null;
            if (_qas.ContainsKey(message)) return _qas[message];
            var timeLength = TimeFormat.Length;
            var qHeaderLength = QuestionHeader.Length;
            var aHeaderLenght = AnswerHeader.Length;
            if (message.Length >= (timeLength + qHeaderLength))
            {
                DateTime time;
                if (DateTime.TryParse(message.Substring(0, timeLength), out time))
                {
                    var header = message.Substring(timeLength, qHeaderLength);
                    if (header == QuestionHeader)
                    {
                        var content = message.Substring(timeLength + qHeaderLength);
                        var isQuestioner = false;
                        var key = GetQuestionAndAnswerKey(time, isQuestioner);
                        lock (_qaLocker)
                        {
                            if (_qas.ContainsKey(key)) return _qas[key];
                            var qa = new ChannelQuestionAndAnswer(this, content, time, isQuestioner);
                            _qas.Add(key, qa);
                            return qa;
                        }
                    }
                    else
                    {
                        header = message.Substring(timeLength, aHeaderLenght);
                        if (header == AnswerHeader)
                        {
                            var content = message.Substring(timeLength + aHeaderLenght);
                            var isQuestioner = true;
                            var key = GetQuestionAndAnswerKey(time, isQuestioner);
                            lock (_qaLocker)
                            {
                                if (_qas.ContainsKey(key))
                                {
                                    var qa = _qas[key];
                                    qa.Answer = content;
                                    qa.AnswerTime = DateTime.Now;
                                    return qa;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }      
        private void RefreshQA()
        {
            //移除超过一分钟未答复的消息
            var time = DateTime.Now;
            lock (_qaLocker)
            {
                var timeOutQAs = new List<string>();
                foreach (var item in _qas)
                {
                    if (item.Value.QuestionTime < time - TimeSpan.FromMinutes(1))
                    {
                        timeOutQAs.Add(item.Key);
                    }
                }
                foreach (var item in timeOutQAs)
                {
                    _qas.Remove(item);
                }
            }
        }
        public Task Send(object message)
        {
            return _channel?.WriteAndFlushAsync(message);
        }
        bool IChannelMatcher.Matches(IChannel channel)
        {
            return channel.Id == _channel.Id;
        }
        public async Task<IAnswerResult> Request(string question, int timeoutMilliseconds)
        {
            RefreshQA();

            var qa = new ChannelQuestionAndAnswer(this, question, DateTime.Now, true);
            var key = this.GetQuestionAndAnswerKey(qa);
            var message = GetQuestionMessage(qa);
            lock (_qaLocker)
            {
                if (_qas.ContainsKey(key) == false)
                {
                    _qas.Add(key, qa);
                }
                else return new AnswerResult(false, "请求重复!");
            }
            var time = 0;
            do
            {
                await this.Send(message);
                await Task.Delay(500);
                time += 500;
            } while (qa.IsAnswered == false && time < timeoutMilliseconds);
            var isAnswered = qa.IsAnswered;
            var response = isAnswered ? qa.Answer : "超时未响应";
            return new AnswerResult(isAnswered, response);
        }
        public async Task Answer(ChannelQuestionAndAnswer answer)
        {
            if (answer != null && answer.IsAnswered)
            {
                var message = GetAnswerMessage(answer);
                await this.Send(message);
            }
        }
        private string GetQuestionAndAnswerKey(ChannelQuestionAndAnswer answer) => $"{answer.QuestionTime.ToString(TimeFormat)}{(answer.IsQuestioner ? "Q" : "A")}";
        private string GetQuestionAndAnswerKey(DateTime time, bool isQuestioner) => $"{time.ToString(TimeFormat)}{(isQuestioner ? "Q" : "A")}";
        public string GetQuestionMessage(ChannelQuestionAndAnswer answer) => $"{answer.QuestionTime.ToString(TimeFormat)}{QuestionHeader}{answer.Question}";
        public string GetAnswerMessage(ChannelQuestionAndAnswer answer) => answer.IsAnswered ? $"{answer.QuestionTime.ToString(TimeFormat)}{AnswerHeader}{answer.Answer}" : null;
        public override string ToString() => $"[{LocalEndPoint}-->{EndPoint}]{Id}";
        public event EventHandler Inactived;
        public event QuestionHandler Requested;
    }

    class ChannelQuestionAndAnswer : IQuestion
    {
        private Channel _channel;
        public ChannelQuestionAndAnswer(Channel channel, string question, DateTime questionTime, bool isQuestioner)
        {
            IsQuestioner = isQuestioner;
            _channel = channel;
            Question = question;
            QuestionTime = questionTime;
        }
        /// <summary>
        /// 当前为是否为提问者
        /// </summary>
        public bool IsQuestioner { get; }
        public string Question { get; }
        public DateTime QuestionTime { get; }
        public string Answer { get; set; }
        public DateTime? AnswerTime { get; set; }
        public bool IsAnswering { get; private set; }
        public bool IsAnswered => AnswerTime != null;

        #region question
        DateTime IQuestion.Time => QuestionTime;
        string IQuestion.Content => Question;
        async void IQuestion.RaiseAnswer(OnAnswerHandler answer)
        {
            IsAnswering = true;
            try
            {
                Answer = answer?.Invoke(Question);
                AnswerTime = DateTime.Now;
                await _channel.Answer(this);
            }
            catch { }
            IsAnswering = false;
        }
        async void IQuestion.RaiseAnswerAsync(OnAnswerAsyncHandler answerAsync)
        {
            IsAnswering = true;
            try
            {
                if (answerAsync != null)
                    Answer = await answerAsync.Invoke(Question);
                AnswerTime = DateTime.Now;
                await _channel.Answer(this);
            }
            catch { }
            IsAnswering = false;
        }
        #endregion

        public override string ToString() => $"{(IsQuestioner ? "提问 " : "<-> ")} {_channel.GetQuestionMessage(this)}\r\n{(IsQuestioner ? "<-> " : "回答 ")} {_channel.GetAnswerMessage(this)}";
    }
    class AnswerResult : IAnswerResult
    {
        public AnswerResult(bool isAnswered,string message)
        {
            IsAnswered = isAnswered;
            Message = message;
        }
        public AnswerResult(string message) : this(true, message) { }
        public bool IsAnswered { get; }
        public string Message { get; }
    }
}
