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
        const char SPACE = ':';
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
        public void OnMessage(object message)
        {
            var question = this.FindQuestionAndAnswer(message?.ToString());
            if (question != null && question.IsAnswered)
                this.Answer(question);
            else
                ReceivedMessage?.Invoke(this, message, question);
        }
        private ChannelQuestionAndAnswer FindQuestionAndAnswer(string message)
        {
            if (string.IsNullOrEmpty(message)) return null;
            if (_qas.ContainsKey(message)) return _qas[message];
            var timeLength = TimeFormat.Length;
            var headerLength = QuestionHeader.Length;
            if (message.Length > (timeLength + headerLength))
            {
                DateTime time;
                if (DateTime.TryParse(message.Substring(0, timeLength), out time))
                {
                    var header = message.Substring(timeLength, headerLength);
                    if (header == QuestionHeader)
                    {
                        lock (_qaLocker)
                        {
                            if (_qas.ContainsKey(message)) return _qas[message];
                            var qa = new ChannelQuestionAndAnswer(this, message.Substring(timeLength, headerLength), time, false);
                            _qas.Add(message, qa);
                            return qa;
                        }
                    }
                    else if (header == AnswerHeader)
                    {
                        var questionMessage = $"{message.Substring(0, timeLength)}{AnswerHeader}";
                        lock (_qaLocker)
                        {
                            if (_qas.ContainsKey(message)) return _qas[message];
                            var qa = new ChannelQuestionAndAnswer(this, message.Substring(timeLength, headerLength), time, false);
                            _qas.Add(message, qa);
                            return qa;
                        }
                    }
                }
            }
            return null;
        }
       
        public Task Send(object message)
        {
            return _channel?.WriteAndFlushAsync(message);
        }
        bool IChannelMatcher.Matches(IChannel channel)
        {
            return channel.Id == _channel.Id;
        }
        public async Task<string> Request(string question, int timeoutMilliseconds = 3000)
        {
            //var index = _counter;
            //index++;
            //if (index >= 1000) index = 1;
            //_counter = index;
            //var message = $"Q{_counter:000}{SPACE}{question}";
            //var timeout = DateTime.Now + TimeSpan.FromMilliseconds(3000);
            //do
            //{
            //    await this.Send(message);
            //    await Task.Delay(1000);
            //    lock (_qaLocker)
            //    {
            //        if (_answers.ContainsKey(index))
            //        {
            //            var answer = _answers[index];
            //            return answer;
            //        }
            //    }

            //} while (DateTime.Now < timeout);
            //return null;
            return null;
        }
        public void Answer(ChannelQuestionAndAnswer answer)
        {
            if (answer != null && answer.IsAnswered)
            {
                var message = GetAnswerMessage(answer);
                this.Send(message);
            }
        }
        public string GetQuestionMessage(ChannelQuestionAndAnswer answer) => $"{answer.QuestionTime.ToString(TimeFormat)}{QuestionHeader}{answer.Question}";
        public string GetAnswerMessage(ChannelQuestionAndAnswer answer) => answer.IsAnswered ? null : $"{answer.AnswerTime.Value.ToString(TimeFormat)}{AnswerHeader}{answer.Answer}";
      
        public DateTime CorrectTime(DateTime time) => DateTime.Parse(DateTime.Now.ToString(TimeFormat));
        public override string ToString() => $"[{LocalEndPoint}-->{EndPoint}]{Id}";
        public event EventHandler Inactived;
        public event MessageHandler ReceivedMessage;
    }

    class ChannelQuestionAndAnswer : IQuestion
    {
        private Channel _channel;
        public ChannelQuestionAndAnswer(Channel channel, string question, DateTime questionTime,bool isQuestioner)
        {
            IsQuestioner = isQuestioner;
            _channel = channel;
            Question = question;
            QuestionTime = questionTime;
        }
        public bool IsQuestioner { get; }
        public string Question { get; }
        public DateTime QuestionTime { get; }
        public string Answer { get; set; }
        public DateTime? AnswerTime { get; set; }
        public bool IsAnswered => AnswerTime != null;

        #region question
        DateTime IQuestion.Time => QuestionTime;
        string IQuestion.Content => Question;
        void IQuestion.Answer(string content)
        {
            Answer = content;
            AnswerTime = DateTime.Now;
            _channel.Answer(this);
        }
        #endregion

        public override string ToString() => $"{(IsQuestioner?"提问":"<->")} {_channel.GetQuestionMessage(this)}\r\n{(IsQuestioner ? "<->" : "回答")} {_channel.GetAnswerMessage(this)}";
    }

}
