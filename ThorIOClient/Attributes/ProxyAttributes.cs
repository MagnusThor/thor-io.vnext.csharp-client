using System;

namespace ThorIOClient.Attributes{

 [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Invokable: Attribute {
        public string Alias {
            get;
            set;
        }
        public Invokable(string alias) {
            this.Alias = alias;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProxyProperties: Attribute {
        public string Alias {
            get;
            private set;
        }
        public ProxyProperties(string alias) {
            this.Alias = alias;
        }
    }


}