using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThorIOClient.Serialization;

namespace ThorIOClient.Extensions {
    public static class ProxyExtensions {
        
    public static void InvokePluginMethod<T>(this T proxy, MethodInfo methodInfo, dynamic[] parameters)
            where T : ProxyBase, IProxyBase
        {
            if (methodInfo.ReturnType == typeof(void))
            {
                proxy.InvokeWithVoid(methodInfo.Name, parameters);
            }
        }
 
        public static void InvokeWithVoid<T>(this T plugin, string key, params dynamic[] p) 
        where T : ProxyBase, IProxyBase
        {
            if (plugin.Delegates == null)
                plugin.CreateDelegates();
            if (p == null)
            {            
                plugin.Delegates[key]();
                return;
            }
            switch (p.Length)
            {
                case 0:
                    plugin.Delegates[key]();
                    break;
                case 1:
                    plugin.Delegates[key](p[0]);
                    break;
                case 2:
                    plugin.Delegates[key](p[0], p[1]);
                    break;
                case 3:
                    plugin.Delegates[key](p[0], p[1], p[2]);
                    break;
                case 4:
                    plugin.Delegates[key](p[0], p[1], p[2], p[3]);
                    break;
                case 5:
                    plugin.Delegates[key](p[0], p[1], p[2], p[3], p[4]);
                    break;
                case 6:
                    plugin.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5]);
                    break;
                case 7:
                    plugin.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
                    break;                
            }
        }
 
        public static object InvokeWithReturnValue<T>(this T proxy, string key, params dynamic[] p) 
        where T : ProxyBase, IProxyBase
        {
            if (proxy.Delegates == null)
                proxy.CreateDelegates();
                
            if (p == null)
                return proxy.Delegates[key]();
 
            switch (p.Length)
            {
                case 0:
                    return proxy.Delegates[key]();
                case 1:
                    return proxy.Delegates[key](p[0]);
                case 2:
                    return proxy.Delegates[key](p[0], p[1]);
                case 3:
                    return proxy.Delegates[key](p[0], p[1], p[2]);
                case 4:
                    return proxy.Delegates[key](p[0], p[1], p[2], p[3]);
                case 5:
                    return proxy.Delegates[key](p[0], p[1], p[2], p[3], p[4]);
                case 6:
                    return proxy.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5]);
                case 7:
                    return proxy.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
                default:
                    return null;
            }
        }
 
        public static void InvokePluginMethod<T>(this T proxy, PluginCustomEventInfo pluginMethodInfo, string data)
            where T : ProxyBase, IProxyBase
        {
            try
            {
                proxy.InvokePluginMethod(pluginMethodInfo.MethodInfo,
                                          pluginMethodInfo.ParameterInfo != null
                                              ?
                                              pluginMethodInfo.ParameterInfo.ExtractMethodParameters(data)
                                              : null);
            }
            catch (Exception ex)
            {
              
            }
        }
 
        public static void InvokePluginMethod<T>(this T plugin, PluginCustomEventInfo pluginMethodInfo)
            where T : ProxyBase, IProxyBase
        {
            plugin.InvokePluginMethod(pluginMethodInfo.MethodInfo, null);
        }
 
        public static void InvokePluginMethod<T>(this T plugin, PluginCustomEventInfo pluginMethodInfo, ThorIOClient.Models.Message e)
            where T : ProxyBase, IProxyBase
        {
 
            if (pluginMethodInfo.ParameterInfo.Length == 0)
                plugin.InvokePluginMethod(pluginMethodInfo);

            else if (pluginMethodInfo.ParameterInfo.First().ParameterType == typeof(ThorIOClient.Models.Message))
            {
                plugin.InvokePluginMethod(pluginMethodInfo.MethodInfo,
                                          pluginMethodInfo.ParameterInfo != null
                                              ? new[]
                                                    {
                                                        e
                                                    }
                                              : null);
            }
            else
            {
                plugin.InvokePluginMethod(pluginMethodInfo, e.Data);
            }
        }
    


        static object GetObject(this System.Type targetType, string json)
        {
              var serializer = new NewtonJsonSerialization();
            return serializer.DeserializeFromString(json, targetType);
        }
      private static bool IsBuiltIn(this System.Type type)
        {
            if (type.Namespace.StartsWith("System") && (type.Module.ScopeName == "CommonLanguageRuntimeLibrary" || type.Module.ScopeName == "mscorlib.dll"))
            {
                return true;
            }
            return false;
        }
        public static dynamic[] ExtractMethodParameters(this ParameterInfo[] parameterInfo, string json)
        {
            var methodParameters = new List<dynamic>();
            if (parameterInfo.Length == 0)
                return null;
 
            var serializer = new NewtonJsonSerialization();


            if (parameterInfo.Length == 1)
            {
                if (parameterInfo[0].ParameterType.IsBuiltIn())
                {
                    if (parameterInfo[0].ParameterType.IsGenericType)
                    {
                        methodParameters.Add(serializer.DeserializeFromString(json, parameterInfo[0].ParameterType));
                    }
                    else
                    {
                        var param = serializer.DeserializeFromString(json, parameterInfo.Select(p => p.Name).ToArray());
 
                        methodParameters.Add(serializer.DeserializeFromString(param[parameterInfo[0].Name],
                                                                              parameterInfo[0].ParameterType));
                    }
                }
                else
                {
                    var pp = serializer.DeserializeFromString(json, parameterInfo[0].ParameterType);
                    methodParameters.Add(pp);
                }                
            }
            else
            {
                //    //More than one parameter...
                var parameters = serializer.DeserializeFromString(json, parameterInfo.Select(p => p.Name).ToArray());
 
                foreach (var pi in parameterInfo)
                {
                    methodParameters.Add(serializer.DeserializeFromString(parameters[pi.Name], pi.ParameterType));
                }
            }

            return methodParameters.ToArray();  
        }
 
      
    }
}