using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class Channel : IChannelMatcher
    {
        private IChannel _channel;
        internal Channel(IChannel channel)
        {
            _channel = channel;
            ChannelId = channel.Id.AsLongText();
            EndPoint = (channel.RemoteAddress as IPEndPoint);
            LocalEndPoint = (channel.LocalAddress as IPEndPoint);
        }
        public string ChannelId { get; }
        public IPEndPoint EndPoint { get; }
        public IPEndPoint LocalEndPoint { get; }
        public override string ToString() => $"[{LocalEndPoint}-->{EndPoint}]{ChannelId}";
        public Task Send(object message)
        {
            return _channel?.WriteAndFlushAsync(message);
        }
        bool IChannelMatcher.Matches(IChannel channel)
        {
            return channel.Id == _channel.Id;
        }
    }
}
