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
        Task<string> Request(string question, int timeoutMilliseconds);
        event EventHandler Inactived;
        event MessageHandler ReceivedMessage;
    }
    public interface IQuestion
    {
        DateTime Time { get; }
        string Content { get; }
        void Answer(string content);
    }
    public delegate void MessageHandler(ISession session, object message, IQuestion question);
}
