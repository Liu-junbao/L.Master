using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public abstract class ChannelManager: IChannelManager
    {
        protected static volatile IChannelGroup _group;
        private static readonly object _groupLocker = new object();
        private bool _isServer;
        private object _channelLocker;
        private Dictionary<string, Channel> _channels;
        public ChannelManager(bool isServer)
        {
            _isServer = isServer;
            _channelLocker = new object();
            _channels = new Dictionary<string, Channel>();
        }
        protected virtual void OnActive(ISession session) { }
        protected virtual void OnExceptionCaught(ISession session, Exception e) { }
        void IChannelManager.ChannleActive(IChannelHandlerContext context)
        {
            var channel = context.Channel;
            var sessionId = channel.Id.AsLongText();
            Channel ch;

            lock (_channelLocker)
            {
                if (_channels.ContainsKey(sessionId) == false)
                {
                    ch = new Channel(channel);
                    _channels.Add(sessionId, ch);
                }
                else ch = _channels[sessionId];
            }
           
            if (_isServer)
            {
                IChannelGroup g = _group;
                if (g == null)
                {
                    lock (_groupLocker)
                    {
                        if (_group == null)
                        {
                            _group = new DefaultChannelGroup(context.Executor);
                        }
                        g = _group;
                    }
                }
                g.Add(channel);
            }
          
            this.OnActive(ch);
        }
        void IChannelManager.ChannelInactive(IChannelHandlerContext context)
        {
            var channel = context.Channel;
            var sessionId = channel.Id.AsLongText();
            Channel ch;

            lock (_channelLocker)
            {
                if (_channels.ContainsKey(sessionId))
                {
                    ch = _channels[sessionId];
                    _channels.Remove(sessionId);
                    ch.Inactive();
                }
            }
        }
        void IChannelManager.ChannelRead(IChannelHandlerContext context, object message)
        {
            var channel = context.Channel;
            var sessionId = channel.Id.AsLongText();

            Channel ch;
            lock (_channelLocker)
            {
                if (_channels.ContainsKey(sessionId) == false)
                {
                    ch = new Channel(channel);
                    _channels.Add(sessionId, ch);
                }
                else ch = _channels[sessionId];
            }
            ch.OnMessage(message);
        }
        void IChannelManager.ExceptionCaught(IChannelHandlerContext context, Exception e)
        {
            var channel = context.Channel;
            var sessionId = channel.Id.AsLongText();
            Channel ch;
            lock (_channelLocker)
            {
                if (_channels.ContainsKey(sessionId) == false)
                {
                    ch = new Channel(channel);
                    _channels.Add(sessionId, ch);
                }
                else ch = _channels[sessionId];
            }
            OnExceptionCaught(ch, e);
        }
    }
}
