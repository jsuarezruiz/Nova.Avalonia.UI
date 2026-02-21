using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Xunit;
using System;
using System.Linq;
using System.Reflection;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ApiInspectionTests
{
    [Fact]
    public void InspectItemContainerGenerator()
    {
        var type = typeof(ItemContainerGenerator);
        Console.WriteLine("Methods of ItemContainerGenerator:");
        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            var paramsStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"{m.Name}({paramsStr})");
        }

        var interfaces = type.GetInterfaces();
        foreach (var i in interfaces)
        {
             Console.WriteLine($"Interface: {i.Name}");
             var iMethods = i.GetMethods().Select(m => m.Name);
             foreach (var m in iMethods) Console.WriteLine($"  - {m}");
        }
    }
}
