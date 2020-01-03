using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    interface IChannelManager
    {
        void ChannleActive(IChannelHandlerContext context);
        void ChannelInactive(IChannelHandlerContext context);
        void ChannelRead(IChannelHandlerContext context, object message);
        void ExceptionCaught(IChannelHandlerContext context, Exception e);
    }

    interface IQuestioner
    {
        void Answer();
    }
}
