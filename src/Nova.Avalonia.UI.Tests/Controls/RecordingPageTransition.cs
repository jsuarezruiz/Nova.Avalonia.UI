using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;

namespace Nova.Avalonia.UI.Tests.Controls;

internal sealed class RecordingPageTransition : IPageTransition
{
    private readonly List<CancellationToken> _tokens = [];

    public int StartCount { get; private set; }

    public bool? LastDirection { get; private set; }

    public Visual? LastFrom { get; private set; }

    public Visual? LastTo { get; private set; }

    public IReadOnlyList<CancellationToken> Tokens => _tokens;

    public Func<CancellationToken, Task>? OnStart { get; set; }

    public Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        StartCount++;
        LastDirection = forward;
        LastFrom = from;
        LastTo = to;
        _tokens.Add(cancellationToken);
        return OnStart?.Invoke(cancellationToken) ?? Task.CompletedTask;
    }
}
