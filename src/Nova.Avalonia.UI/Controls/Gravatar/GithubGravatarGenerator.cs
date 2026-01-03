using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Generates GitHub-style 5x5 pixel identicon avatars from an identifier.
/// </summary>
public class GithubGravatarGenerator : IGravatarGenerator
{
    private const int RenderDataMaxLength = 15;

    /// <inheritdoc />
    public object? GenerateAvatar(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        id = id.Trim().ToLowerInvariant();

        var hashcode = GetHashCode(id);
        var renderData = GetRenderData(hashcode);
        var renderBrush = GetRenderBrush(hashcode);
        var geometryGroup = new GeometryGroup();

        void AddRectangle(int i, int j, bool hidden = false)
        {
            var rect = new RectangleGeometry
            {
                Rect = new Rect(new Point(i, j), hidden ? new Size() : new Size(1, 1))
            };
            geometryGroup.Children.Add(rect);
        }

        var index = 0;
        
        // Left columns (0-1)
        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 5; j++, index++)
            {
                AddRectangle(i, j, renderData[index] == 0);
            }
        }

        // Center column
        for (var j = 0; j < 5; j++, index++)
        {
            AddRectangle(2, j, renderData[index] == 0);
        }

        // Right columns (mirrored from left)
        index -= 10;
        for (var i = 3; i < 5; i++)
        {
            for (var j = 0; j < 5; j++, index++)
            {
                AddRectangle(i, j, renderData[index] == 0);
            }
            index -= 10;
        }

        return new Path
        {
            Data = geometryGroup,
            Fill = renderBrush,
            Stretch = Stretch.Uniform
        };
    }

    private static string GetHashCode(string id)
    {
        var bytes = Encoding.ASCII.GetBytes(id);
        var hash = MD5.HashData(bytes);
        var sb = new StringBuilder();
        foreach (var item in hash)
        {
            sb.Append(item.ToString("X2"));
        }
        return sb.ToString();
    }

    private static int[] GetRenderData(string hashcode)
    {
        var arr = new int[RenderDataMaxLength];
        for (var i = 0; i < RenderDataMaxLength; i++)
        {
            var c = hashcode[i];
            arr[i] = c % 2;
        }
        return arr;
    }

    private static IBrush GetRenderBrush(string hashcode)
    {
        var hexValue = hashcode.Substring(hashcode.Length - 7);
        var v = (double)int.Parse(hexValue, NumberStyles.HexNumber);
        var scale = v / 0xfffffff;
        var color = Hsl2Rgb(scale);
        return new SolidColorBrush(color);
    }

    private static Color Hsl2Rgb(double h, double s = 0.7, double b = 0.5)
    {
        h *= 6;
        var arr = new[]
        {
            b += s *= b < .5 ? b : 1 - b,
            b - h % 1 * s * 2,
            b -= s *= 2,
            b,
            b + h % 1 * s,
            b + s
        };

        var hValue = (int)Math.Floor(h);
        return Color.FromArgb(
            255,
            (byte)(arr[hValue % 6] * 255),
            (byte)(arr[(hValue | 16) % 6] * 255),
            (byte)(arr[(hValue | 8) % 6] * 255));
    }
}
