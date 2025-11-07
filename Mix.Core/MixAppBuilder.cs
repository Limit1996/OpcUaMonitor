using App.Core;

using Mix.Core.Contexts;

namespace Mix.Core;

public class MixPrintBuilder : IAppBuilder<PrintContext>
{
    private readonly IList<Func<Func<PrintContext, Task>, Func<PrintContext, Task>>> _middlewares = [];


    public IServiceProvider AppServices { get; }

    public MixPrintBuilder(IServiceProvider serviceProvider)
    {
        AppServices = serviceProvider;
    }

    public Func<PrintContext, Task> Build()
    {
        Func<PrintContext, Task> app = context =>
        {
            return Task.CompletedTask;
        };
        foreach (var middleware in _middlewares.Reverse())
        {
            app = middleware(app);
        }
        return app;
    }

    public IAppBuilder<PrintContext> Use(Func<Func<PrintContext, Task>, Func<PrintContext, Task>> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }
}
