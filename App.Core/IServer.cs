

namespace App.Core;

public interface IServer<TContext> : IAsyncDisposable
{
    Task StartAsync(Func<TContext, Task> handler);
}
