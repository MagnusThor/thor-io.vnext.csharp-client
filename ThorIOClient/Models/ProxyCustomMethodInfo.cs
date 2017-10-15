using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using thorio.csharp.ThorIOClient.Interfaces;

namespace thorio.csharp.ThorIOClient.Models
{
    public class ProxyCustomMethodInfo : IProxyCustomMethodInfo
    {
        public ProxyCustomMethodInfo(MethodInfo methodInfo, string alias)
        {
            MethodInfo = methodInfo;
            ParameterInfo = methodInfo.GetParameters();
            this.Alias = alias;
        }

        public string Alias { get; set; }

        public MethodInfo MethodInfo
        {
            get;
            set;
        }
        public ParameterInfo[] ParameterInfo
        {
            get;
            private set;
        }
    }
}
