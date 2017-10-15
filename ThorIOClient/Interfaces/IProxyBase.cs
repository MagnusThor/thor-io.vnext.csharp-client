using System;
using System.Collections.Generic;
using System.Text;
using ThorIOClient.Interfaces;

namespace thorio.csharp.ThorIOClient.Interfaces
{
    public interface IProxyBase
    {
        ISocket Ws
        {
            get;
            set;
        }
    }
}
