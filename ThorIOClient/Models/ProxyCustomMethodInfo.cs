using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using  ThorIOClient.Interfaces;

namespace ThorIOClient.Models
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
