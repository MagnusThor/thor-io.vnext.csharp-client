using System;
using System.Collections.Generic;
using System.Text;


namespace ThorIOClient.Interfaces
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
