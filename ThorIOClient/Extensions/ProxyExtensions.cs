using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThorIOClient.Serialization;

namespace ThorIOClient.Extensions {
    internal static class ProxyExtensions {
    internal static void InvokeProxyMethod<T>(this T proxy, MethodInfo methodInfo, dynamic[] parameters)
            where T : ProxyBase, IProxyBase
        {
            if (methodInfo.ReturnType == typeof(void))
            {
                proxy.InvokeWithVoid(methodInfo.Name, parameters);
            }else
            {
                    throw new NotImplementedException();
            }
        }
 
        internal static void InvokeWithVoid<T>(this T proxy, string key, params dynamic[] p) 
        where T : ProxyBase, IProxyBase
        {
            if (proxy.Delegates == null)
                proxy.CreateDelegates();
            if (p == null)
            {            
                proxy.Delegates[key]();
                return;
            }
            switch (p.Length)
            {
                case 0:
                    proxy.Delegates[key]();
                    break;
                case 1:
                    proxy.Delegates[key](p[0]);
                    break;
                case 2:
                    proxy.Delegates[key](p[0], p[1]);
                    break;
                case 3:
                    proxy.Delegates[key](p[0], p[1], p[2]);
                    break;
                case 4:
                    proxy.Delegates[key](p[0], p[1], p[2], p[3]);
                    break;
                case 5:
                    proxy.Delegates[key](p[0], p[1], p[2], p[3], p[4]);
                    break;
                case 6:
                    proxy.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5]);
                    break;
                case 7:
                    proxy.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
                    break;                
            }
        }
 
        internal static object InvokeWithReturnValue<T>(this T proxy, string key, params dynamic[] p) 
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
 
        internal static void InvokeProxyMethod<T>(this T proxy, ProxyCustomMethodInfo proxyMethodInfo, string data)
            where T : ProxyBase, IProxyBase
        {
            try
            {
                proxy.InvokeProxyMethod(proxyMethodInfo.MethodInfo,
                                          proxyMethodInfo.ParameterInfo != null
                                              ?
                                              proxyMethodInfo.ParameterInfo.ExtractMethodParameters(data)
                                              : null);
            }
            catch (Exception ex)
            {
              
            }
        }
 
        internal static void InvokeProxyMethod<T>(this T proxy, ProxyCustomMethodInfo proxyMethodInfo)
            where T : ProxyBase, IProxyBase
        {
            proxy.InvokeProxyMethod(proxyMethodInfo.MethodInfo, null);
        }
 
        internal static void InvokeProxyMethod<T>(this T proxy, ProxyCustomMethodInfo proxyMethodInfo, ThorIOClient.Models.Message e)
            where T : ProxyBase, IProxyBase
        {
 
            if (proxyMethodInfo.ParameterInfo.Length == 0)
                proxy.InvokeProxyMethod(proxyMethodInfo);

            else if (proxyMethodInfo.ParameterInfo.First().ParameterType == typeof(ThorIOClient.Models.Message))
            {
                proxy.InvokeProxyMethod(proxyMethodInfo.MethodInfo,
                                          proxyMethodInfo.ParameterInfo != null
                                              ? new[]
                                                    {
                                                        e
                                                    }
                                              : null);
            }
            else
            {
                proxy.InvokeProxyMethod(proxyMethodInfo, e.Data);
            }
        }
    


        internal static object GetObject(this System.Type targetType, string json)
        {
              var serializer = new NewtonJsonSerialization();
            return serializer.Deserialize(json, targetType);
        }
      internal static bool IsBuiltIn(this System.Type type)
        {
            if (type.Namespace.StartsWith("System") && (type.Module.ScopeName == "CommonLanguageRuntimeLibrary" || type.Module.ScopeName == "mscorlib.dll"))
            {
                return true;
            }
            return false;
        }
        internal static dynamic[] ExtractMethodParameters(this ParameterInfo[] parameterInfo, string json)
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
                        methodParameters.Add(serializer.Deserialize(json, parameterInfo[0].ParameterType));
                    }
                    else
                    {
                        var param = serializer.Deserialize(json, parameterInfo.Select(p => p.Name).ToArray());
 
                        methodParameters.Add(serializer.Deserialize(param[parameterInfo[0].Name],
                                                                              parameterInfo[0].ParameterType));
                    }
                }
                else
                {
                    var pp = serializer.Deserialize(json, parameterInfo[0].ParameterType);
                    methodParameters.Add(pp);
                }                
            }
            else
            {
                //    //More than one parameter...
                var parameters = serializer.Deserialize(json, parameterInfo.Select(p => p.Name).ToArray());
 
                foreach (var pi in parameterInfo)
                {
                    methodParameters.Add(serializer.Deserialize(parameters[pi.Name], pi.ParameterType));
                }
            }

            return methodParameters.ToArray();  
        }
 
      
    }
}