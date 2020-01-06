using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModuleC.ViewModels
{
    public class ViewAViewModel : NettyServer
    {
        private string _message;
        private ISession _session;
        public ViewAViewModel() : base(8730, BasedFrameDecoderKind.LengthFieldBasedFrame)
        {
            SendCommand = new Command(OnSend);
            ClientCommand = new Command(OnClient, () => _session == null);
        }
        public Command SendCommand { get; }
        public Command ClientCommand { get; }
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        protected override void OnActive(ISession session)
        {
            AppendLine($"{session} 用户上线");
            _session = session;
            ClientCommand.RaiseCanExecuteChanged();
            base.OnActive(session);
        }

        protected override void OnInActive(ISession session)
        {
            AppendLine($"{session} 用户下线");
            if (_session == session)
            {
                _session = null;
                ClientCommand.RaiseCanExecuteChanged();
            }
            base.OnInActive(session);
        }
        private async void OnSend()
        {
            if (_session == null)
            {
                AppendLine($"无用户上线");
                return;
            }

            AppendLine($"发送:你好");
            var result = await _session.Request("你好");
            AppendLine($"结果:{result.Message}");
            await Task.Delay(1000);
            AppendLine($"发送:吃了吗?");
            result = await _session.Request("吃了吗?");
            AppendLine($"结果:{result.Message}");
            await Task.Delay(1000);
            AppendLine($"发送:你不用回消息?");
            result = await _session.Request("你不用回消息?");
            AppendLine($"结果:{result.Message}");
        }
        private void OnClient()
        {
            new Client();
        }
        private void AppendLine(string message)
        {
            Message = $"{Message}\r\n{message}";
        }
    }

    class Client : NettyClient
    {
        public Client() : base("127.0.0.1", 8730, BasedFrameDecoderKind.LengthFieldBasedFrame) { }
        protected override void OnActive(ISession session)
        {
            session.Requested += Session_Requested;
            base.OnActive(session);
        }
        private void Session_Requested(ISession session, IQuestion question)
        {
            question.RaiseAnswer(OnAnswer);
        }
        private string OnAnswer(string question)
        {
            Thread.Sleep(2000);
            if (question?.Contains("你好") == true)
            {
                return "你好";
            }
            else if (question?.Contains("吃了吗") == true)
            {
                return "吃了";
            }
            return null;
        }
    }

}
