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

    public T? GetCurrent<T>() => Current != null ? (T)Current : default;
    
    public T? GetPrevious<T>() => Previous != null ? (T)Previous : default;
}