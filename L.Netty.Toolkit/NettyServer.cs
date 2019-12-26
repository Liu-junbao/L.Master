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
    public abstract class NettyServer:IChannelManager
    {      
        private readonly MultithreadEventLoopGroup _bossGroup;
        private readonly MultithreadEventLoopGroup _workGroup;
        private ServerBootstrap _bootstrap;

        public NettyServer(int port, BasedFrameDecoderKind kind = BasedFrameDecoderKind.None)
        {
            _bootstrap = new ServerBootstrap();
            _workGroup = new MultithreadEventLoopGroup();
            InitializeAsync(port,kind);
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
        public async Task BroadCastAsync(object message, params Channel[] users)
        {
            if (_group == null) return;
            await _group.WriteAndFlushAsync(message, new UserChannelMatcherCollection(users));
        }

        #region user
        protected virtual void OnLogIn(Channel user) { }
        protected virtual void OnLogOut(Channel user) { }
        protected virtual void OnMessage(Channel user, object message) { }
        protected virtual void OnExceptionCaught(Channel user,Exception e) { }
        #endregion

        #region channel mananger
        private static volatile IChannelGroup _group;
        void IChannelManager.ChannleActive(IChannelHandlerContext context)
        {
            IChannelGroup g = _group;
            if (g == null)
            {
                lock (this)
                {
                    if (_group == null)
                    {
                        _group = new DefaultChannelGroup(context.Executor);
                    }
                    g = _group;
                }
            }
            var channel = context.Channel;
            g.Add(channel);
            OnLogIn(new Channel(channel));
        }
        void IChannelManager.ChannelInactive(IChannelHandlerContext context)
        {
            var channel = context.Channel;
            OnLogOut(new Channel(channel));
        }
        void IChannelManager.ChannelRead(IChannelHandlerContext context, object message)
        {
            var channel = context.Channel;
            OnMessage(new Channel(channel),message);
        }
        void IChannelManager.ExceptionCaught(IChannelHandlerContext context, Exception e)
        {
            var channel = context.Channel;
            OnExceptionCaught(new Channel(channel), e);
        }
        #endregion
    }
  
    class UserChannelMatcherCollection :IChannelMatcher
    {
        private List<Channel> _channels;
        public UserChannelMatcherCollection (params Channel[] users)
        {
            _channels = new List<Channel>();
            if (users != null)
            {
                _channels.AddRange(users);
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
