using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.Controls;

internal sealed class ShowcaseAutomationIsolation
{
    private readonly Dictionary<Control, IDisposable> _overrides = [];
    private Showcase? _showcase;
    private ShowcaseInteractionMode _interactionMode;
    private Control? _target;

    public void Update(Showcase showcase, ShowcaseInteractionMode interactionMode, Control? target)
    {
        _showcase = showcase;
        _interactionMode = interactionMode;
        _target = target;
        Refresh();
    }

    public void Refresh()
    {
        if (_showcase == null ||
            _interactionMode == ShowcaseInteractionMode.Passthrough ||
            _showcase.GetVisualRoot() is not Visual root)
        {
            ClearOverrides();
            return;
        }

        var allowedControls = _showcase
            .GetSelfAndVisualDescendants()
            .OfType<Control>()
            .ToHashSet();

        if (_interactionMode == ShowcaseInteractionMode.TargetOnly && _target != null)
        {
            allowedControls.UnionWith(
                _target
                    .GetSelfAndVisualDescendants()
                    .OfType<Control>());
        }

        var controlsToIsolate = root
            .GetVisualDescendants()
            .OfType<Control>()
            .Where(control => !allowedControls.Contains(control))
            .ToHashSet();

        foreach (var control in _overrides.Keys.Where(x => !controlsToIsolate.Contains(x)).ToArray())
        {
            if (_overrides.Remove(control, out var restore))
            {
                restore.Dispose();
            }
        }

        foreach (var control in controlsToIsolate)
        {
            if (_overrides.ContainsKey(control))
            {
                continue;
            }

            var restore = control.SetValue(
                AutomationProperties.AccessibilityViewProperty,
                AccessibilityView.Raw,
                BindingPriority.Animation);

            if (restore != null)
            {
                _overrides.Add(control, restore);
            }
        }
    }

    public void Clear()
    {
        _showcase = null;
        _target = null;
        ClearOverrides();
    }

    private void ClearOverrides()
    {
        foreach (var restore in _overrides.Values.Reverse())
        {
            restore.Dispose();
        }

        _overrides.Clear();
    }
}
