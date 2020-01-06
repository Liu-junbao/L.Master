using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public abstract class ChannelManager : IChannelManager, INotifyPropertyChanged
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
        protected virtual void OnInActive(ISession session) { }
        protected virtual void OnMessage(ISession session, object message) { }
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
            Channel ch = null;

            lock (_channelLocker)
            {
                if (_channels.ContainsKey(sessionId))
                {
                    ch = _channels[sessionId];
                    _channels.Remove(sessionId);
                }
            }

            if (ch != null)
            {
                OnInActive(ch);
                ch.Inactive();
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

            this.OnMessage(ch, message);
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

        #region INotifyPropertyChanged
        protected void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames != null)
            {
                foreach (var item in propertyNames)
                {
                    RaisePropertyChanged(item);
                }
            }
        }
        protected virtual bool SetProperty<TValue>(ref TValue storage, TValue newValue, [CallerMemberName] string propertyName = null, params string[] propertyNameArgs)
        {
            if (EqualityComparer<TValue>.Default.Equals(storage, newValue) == false)
            {
                TValue oldValue = storage;
                storage = newValue;
                OnPropertyChanged(propertyName, oldValue, newValue);
                this.RaisePropertyChanged(propertyName);
                this.RaisePropertyChanged(propertyNameArgs);
                return true;
            }
            return false;
        }
        protected virtual bool SetProperty<TValue>(ref TValue storage, TValue newValue, Action<TValue, TValue> onChanged, [CallerMemberName] string propertyName = null, params string[] propertyNameArgs)
        {
            if (EqualityComparer<TValue>.Default.Equals(storage, newValue) == false)
            {
                TValue oldValue = storage;
                storage = newValue;
                onChanged?.Invoke(oldValue, newValue);
                OnPropertyChanged(propertyName, oldValue, newValue);
                this.RaisePropertyChanged(propertyName);
                this.RaisePropertyChanged(propertyNameArgs);
                return true;
            }
            return false;
        }
        protected virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue) { }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
