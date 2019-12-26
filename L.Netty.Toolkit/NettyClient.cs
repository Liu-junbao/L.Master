using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public abstract class NettyClient:IChannelManager,IDisposable
    {
        private readonly MultithreadEventLoopGroup _group;
        private Bootstrap _bootstrap;
        private bool _isDisposed;
        public NettyClient(string ip, int port, BasedFrameDecoderKind kind = BasedFrameDecoderKind.None)
        {
            _group = new MultithreadEventLoopGroup();
            var endPoint = GetEndPoint(ip,port);
            InitializeAsync(endPoint, kind);
        }
        private async void InitializeAsync(EndPoint endPoint, BasedFrameDecoderKind kind)
        {
            _bootstrap = await Task.Run(() =>
            {
                try
                {
                    var bootstrap = new Bootstrap()
                        .Group(_group)
                        .Channel<TcpSocketChannel>()
                        .Option(ChannelOption.TcpNodelay, true)
                        .Handler(new ActionChannelInitializer<ISocketChannel>(ch =>
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
                            pipeline.AddLast(encoder, decoder, new ChannelHandler(this, false));
                        }));
                    return bootstrap;
                }
                catch (Exception e)
                {

                }
                return null;
            });
            if (_bootstrap == null) throw new Exception();
            DoConnect();
        }
        private IPEndPoint GetEndPoint(string address, int port)
        {
            IPAddress iPAddress;
            if (IPAddress.TryParse(address, out iPAddress) == false)
            {
                var ips = Dns.GetHostAddresses(address);
                if (ips != null && ips.Length > 0)
                {
                    iPAddress = ips[0];
                }
                else
                    throw new Exception(string.Format("地址：{0} 无效！", address));
            }
            return new IPEndPoint(iPAddress, port);
        }
        private async void DoConnect(Bootstrap bootstrap, EndPoint hostPoint)
        {
            //try
            //{
            //    var channel = await bootstrap.ConnectAsync(hostPoint);
            //    if (channel != null)
            //    {
            //        var remotePoint = channel.LocalAddress as IPEndPoint;
                    
            //        await Task.Delay(2000);
            //        while (channel.Active && IsConnected && _isDisposed == false)//检查连接状态
            //        {
            //            await Task.Delay(2000);
            //        }
            //        await channel.CloseCompletion;//断线时或者释放时 释放旧的连接组件
            //    }
            //}
            //catch (Exception e)
            //{

            //}
            await Task.Delay(3000);
            if (_isDisposed) return;
            DoConnect(bootstrap, hostPoint);
        }
        /// <summary>
        /// 断线重连
        /// </summary>
        private void DoConnect()
        {

        }

        #region 
        protected virtual void OnActiveChanged(Channel channel,bool isActive)
        {

        }
        #endregion

        #region channel manager
        void IChannelManager.ChannleActive(IChannelHandlerContext context)
        {

        }
        void IChannelManager.ChannelInactive(IChannelHandlerContext context)
        {
           
        }
        void IChannelManager.ChannelRead(IChannelHandlerContext context, object message)
        {
           
        }
        void IChannelManager.ExceptionCaught(IChannelHandlerContext context, Exception e)
        {
          
        }
        #endregion

        public void Dispose()
        {
            _isDisposed = true;
        }
    }
    
}
