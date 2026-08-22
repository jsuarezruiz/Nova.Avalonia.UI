using System;
using Avalonia;

namespace Nova.Avalonia.UI.CodeViewer;

internal sealed class AvaloniaPropertyChangedObserver : IObserver<AvaloniaPropertyChangedEventArgs>
{
    private readonly Action<AvaloniaPropertyChangedEventArgs> _onChanged;

    public AvaloniaPropertyChangedObserver(Action<AvaloniaPropertyChangedEventArgs> onChanged)
    {
        _onChanged = onChanged;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(AvaloniaPropertyChangedEventArgs value) => _onChanged(value);
}
