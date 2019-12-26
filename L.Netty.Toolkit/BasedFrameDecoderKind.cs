using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public enum BasedFrameDecoderKind
    {
        None,
        LengthFieldBasedFrame,
        LineBasedFrame,//socket通讯 类似串口片段
    }
}
