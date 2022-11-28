using System.Diagnostics;

namespace BigFileSort.System;

public class Measure : IDisposable
{
    private static readonly AsyncLocal<Measure?> _threadCurrentMeasure = new();

    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
    private readonly Measure? _parent;
    private readonly int _level;
    private readonly string _actionName;
    private readonly (int Left, int Top) _cursorStartPosition;
    private readonly Stopwatch _sw;
    
    private string _info;

    public Measure(string actionName)
    {
        if (_threadCurrentMeasure.Value == null)
        {
            _threadCurrentMeasure.Value = this;
            _parent = null;
            _level = 0;
        }
        else
        {
            _parent = _threadCurrentMeasure.Value;
            _level = _parent._level + 1;
            _threadCurrentMeasure.Value = this;
        }
        _actionName = actionName;
        _sw = Stopwatch.StartNew();
        var initialLine = $"{GetTab()}{_actionName} Started";
        
        _semaphore.Wait(1000);
        _cursorStartPosition = Console.GetCursorPosition();
        Console.WriteLine(initialLine);
        _semaphore.Release();
    }

    private string GetTab() => new (Enumerable.Repeat('─', _level*2).ToArray());
    
    public void AddInfo(string info)
    {
        _info = info;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _semaphore.Wait(1000);
        var (left, top) = Console.GetCursorPosition();
        Console.SetCursorPosition(_cursorStartPosition.Left, _cursorStartPosition.Top);
        Console.Write("{0}{1} {2} Elapsed: {3}", GetTab(), _actionName, _info, _sw.Elapsed);
        Console.SetCursorPosition(left, top);
        _semaphore.Release();
        
        _threadCurrentMeasure.Value = _parent;
    }
}