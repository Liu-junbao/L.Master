using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public abstract class NettyServer : ChannelManager
    {
        private readonly MultithreadEventLoopGroup _bossGroup;
        private readonly MultithreadEventLoopGroup _workGroup;
        private ServerBootstrap _bootstrap;
        public NettyServer(int port, BasedFrameDecoderKind kind = BasedFrameDecoderKind.None):base(true)
        {
            _bootstrap = new ServerBootstrap();
            _bossGroup = new MultithreadEventLoopGroup();
            _workGroup = new MultithreadEventLoopGroup();
            InitializeAsync(port, kind);
        }
        private async void InitializeAsync(int port, BasedFrameDecoderKind kind)
        {
            _bootstrap = await Task.Run(() =>
            {
                try
                {
                    var bootstrap = new ServerBootstrap()
                     .Group(_bossGroup, _workGroup)
                     .Channel<TcpServerSocketChannel>()
                     .Option(ChannelOption.SoBacklog, 1024)
                     .Handler(new LoggingHandler(LogLevel.INFO))
                     .ChildHandler(new ActionChannelInitializer<ISocketChannel>(ch =>
                     {

                         var encoder = new StringEncoder();
                         var decoder = new StringDecoder();

                         var pipeline = ch.Pipeline;
                         switch (kind)
                         {
                             case BasedFrameDecoderKind.None:
                                 break;
                             case BasedFrameDecoderKind.LengthFieldBasedFrame:
                                 pipeline.AddLast(new LengthFieldPrepender(2), new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                                 break;
                             case BasedFrameDecoderKind.LineBasedFrame:
                                 pipeline.AddLast(new LineBasedFrameDecoder(int.MaxValue));
                                 break;
                             default:
                                 break;
                         }
                         pipeline.AddLast(encoder, decoder, new ChannelHandler(this));
                     }));
                    bootstrap.BindAsync(port);
                    return bootstrap;
                }
                catch (Exception e)
                {
                    return null;
                }
            });

            if (_bootstrap == null)
            {
                throw new Exception("初始化Netty服务失败!");
            }
        }
        public async Task BroadCastAsync(object message)
        {
            if (_group == null) return;
            await _group.WriteAndFlushAsync(message);
        }
        public async Task BroadCastAsync(object message, params ISession[] users)
        {
            if (_group == null) return;
            await _group.WriteAndFlushAsync(message, new ChannelMatcherCollection(users));
        }
    }
  
    class ChannelMatcherCollection :IChannelMatcher
    {
        private List<Channel> _channels;
        public ChannelMatcherCollection (params ISession[] users)
        {
            _channels = new List<Channel>();
            if (users != null)
            {
                foreach (var item in users)
                {
                    _channels.Add((Channel)item);
                }
            }
        }
        public bool Matches(IChannel channel)
        {
            foreach (IChannelMatcher item in _channels)
            {
                if (item.Matches(channel))
                    return true;
            }
            return false;
        }
    }
}
