using System;
using System.Collections.Generic;
using System.Text;
using ThorIOClient;
using ThorIOClient.Attributes;

namespace ThorIOClient
{
    [ProxyProperties("generic")]
    public class GenericProxy : ProxyBase
    {
        public GenericProxy(string alias)
        {
            this.alias = alias;
        }
    }
}
