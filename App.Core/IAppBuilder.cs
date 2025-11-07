
namespace App.Core;

public interface IAppBuilder<TContext>
{
    IServiceProvider AppServices { get; }

    IAppBuilder<TContext> Use(Func<Func<TContext,Task>, Func<TContext, Task>> middleware);

    Func<TContext, Task> Build();
}
