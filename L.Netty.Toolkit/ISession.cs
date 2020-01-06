using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public interface ISession
    {
        string Id { get; }
        bool IsActive { get; }
        IPEndPoint EndPoint { get; }
        IPEndPoint LocalEndPoint { get; }
        Task Send(object message);
        Task<IAnswerResult> Request(string question, int timeoutMilliseconds = 5000);
        event EventHandler Inactived;
        event QuestionHandler Requested;
    }
    public interface IQuestion
    {
        DateTime Time { get; }
        string Content { get; }
        void RaiseAnswer(OnAnswerHandler answer = null);
        void RaiseAnswerAsync(OnAnswerAsyncHandler answerAsync = null);
    }
    public interface IAnswerResult
    {
        bool IsAnswered { get; }
        string Message { get; }
    }

    public delegate void QuestionHandler(ISession session,IQuestion question);
    public delegate string OnAnswerHandler(string question);
    public delegate Task<string> OnAnswerAsyncHandler(string question);
}
