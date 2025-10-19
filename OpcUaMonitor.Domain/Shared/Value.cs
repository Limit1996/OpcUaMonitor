namespace OpcUaMonitor.Domain.Shared;

public record Value
{
    public object? Current => _current;
    private object? _current;

    public object? Previous => _previous;
    private object? _previous;

    public void Update(object? value)
    {
        _previous = _current;
        _current = value;
    }

    public T? GetCurrent<T>(Action<object>? calcCallback = null)
    {
        if(_current == null || calcCallback == null)
            return default;
        calcCallback(_current);
        return (T)_current;
    }

    public T? GetPrevious<T>(Action<object>? calcCallback = null)
    {
        if(_previous == null || calcCallback == null)
            return default;
        calcCallback(_previous);
        return (T)_previous;
    }
}
