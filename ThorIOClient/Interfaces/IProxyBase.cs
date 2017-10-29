using System;
using System.Collections.Generic;
using System.Text;


namespace ThorIOClient.Interfaces
{
    public interface IProxyBase
    {
        ISocket Socket
        {
            get;
            set;
        }
    }
}
