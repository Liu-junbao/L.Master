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
    public class NettyClient : ChannelManager, IDisposable
    {
        private readonly MultithreadEventLoopGroup _group;
        private Bootstrap _bootstrap;
        private bool _isDisposed;
        private bool _isConnected;
        public NettyClient(string ip, int port, BasedFrameDecoderKind kind = BasedFrameDecoderKind.None):base(false)
        {
            _group = new MultithreadEventLoopGroup();
            var endPoint = GetEndPoint(ip, port);
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
            DoConnect(_bootstrap, endPoint);
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
            try
            {
                var channel = await bootstrap.ConnectAsync(hostPoint);
                if (channel != null)
                {
                    var remotePoint = channel.LocalAddress as IPEndPoint;
                    do
                    {
                        await Task.Delay(2000);

                    } while (channel.Active && _isConnected && _isDisposed == false);

                    await channel.CloseCompletion;//断线时或者释放时 释放旧的连接组件
                }
            }
            catch (Exception e)
            {

            }
            await Task.Delay(3000);
            if (_isDisposed) return;
            DoConnect(bootstrap, hostPoint);
        }
        public void Dispose()
        {
            _isDisposed = true;
        }
    }
    
}
