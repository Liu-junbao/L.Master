using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    class ChannelHandler : ChannelHandlerAdapter
    {
        private IChannelManager _channelManager;
        private bool _isSharable;
        public ChannelHandler(IChannelManager channelManager, bool isSharable = true)
        {
            _channelManager = channelManager;
            _isSharable = isSharable;
        }
        public override void ChannelActive(IChannelHandlerContext context)
        {
            _channelManager.ChannleActive(context);
        }
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _channelManager.ChannelInactive(context);
        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            _channelManager.ChannelRead(context, message);
        }
        //public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();
        public override void ExceptionCaught(IChannelHandlerContext context, Exception e)
        {
            _channelManager.ExceptionCaught(context, e);
            context.CloseAsync();
        }
        public override bool IsSharable => _isSharable;

    }
}
