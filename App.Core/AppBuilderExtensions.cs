using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace App.Core;

public static class AppBuilderExtensions
{
    public static IAppBuilder<TContext> UseMiddleware<TMiddleware, TContext>(
        this IAppBuilder<TContext> builder
    )
        where TMiddleware : class
    {
        var constructor = typeof(TMiddleware).GetConstructors().First();
        var parameters = constructor.GetParameters();
        var method = typeof(TMiddleware).GetMethod("InvokeAsync");

        if (method == null)
        {
            throw new InvalidOperationException("中间件必须包含 InvokeAsync 方法。");
        }

        return builder.Use(next =>
        {
            var args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(Func<TContext, Task>))
                {
                    args[i] = next;
                }
                else
                {
                    //从 Ioc容器 获取服务实例
                    //当标注了 [FromKeyedServices] 时,需要特殊处理

                    if (
                        parameters[i]
                            .GetCustomAttributes(typeof(FromKeyedServicesAttribute), false)
                            .FirstOrDefault()
                        is FromKeyedServicesAttribute fromKeyedServicesAttribute
                    )
                    {
                        //获取 Key 的值
                        var key = fromKeyedServicesAttribute.Key;

                        // 使用反射调用泛型扩展方法 GetKeyedService<T>
                        var getKeyedServiceMethod = typeof(ServiceProviderKeyedServiceExtensions)
                            .GetMethod(
                                nameof(ServiceProviderKeyedServiceExtensions.GetKeyedService),
                                BindingFlags.Public | BindingFlags.Static,
                                null,
                                [typeof(IServiceProvider), typeof(object)],
                                null
                            )
                            ?.MakeGenericMethod(parameters[i].ParameterType);

                        if (getKeyedServiceMethod == null)
                        {
                            throw new InvalidOperationException(
                                $"无法找到 GetKeyedService 方法。"
                            );
                        }

                        var service = getKeyedServiceMethod.Invoke(null, [builder.AppServices, key]);

                        if (service == null)
                        {
                            throw new InvalidOperationException(
                                $"尝试激活 '{typeof(TMiddleware)}' 时,无法解析类型 '{parameters[i].ParameterType}' 且 Key 为 '{key}' 的服务。"
                            );
                        }
                        args[i] = service;
                    }
                    else
                    {
                        args[i] =
                            builder.AppServices.GetService(parameters[i].ParameterType)
                            ?? throw new InvalidOperationException(
                                $"尝试激活 '{typeof(TMiddleware)}' 时,无法解析类型 '{parameters[i].ParameterType}' 的服务。"
                            );
                    }
                }
            }

            var middleware = (TMiddleware)constructor.Invoke(args);

            return context => (Task)method.Invoke(middleware, [context])!;
        });
    }
}
