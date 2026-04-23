using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Generates GitHub 5x5 pixel identicon avatars from an identifier.
/// </summary>
public class GithubGravatarGenerator : IGravatarGenerator
{
    private const int GridSize = 5;
    private const int CellCount = 15;

    /// <inheritdoc />
    public object? GenerateAvatar(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        id = id.Trim().ToLowerInvariant();

        var hash = ComputeHash(id);
        var cells = GetCellVisibility(hash);
        var brush = GetBrushFromHash(hash);
        var geometryGroup = new GeometryGroup();

        for (var col = 0; col < GridSize; col++)
        {
            var dataCol = col < 3 ? col : 4 - col;

            for (var row = 0; row < GridSize; row++)
            {
                if (cells[dataCol * GridSize + row])
                {
                    geometryGroup.Children.Add(new RectangleGeometry
                    {
                        Rect = new Rect(col, row, 1, 1)
                    });
                }
            }
        }

        return new Path
        {
            Data = geometryGroup,
            Fill = brush,
            Stretch = Stretch.Uniform
        };
    }

    private static string ComputeHash(string id)
    {
        var bytes = Encoding.UTF8.GetBytes(id);
        var hash = MD5.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    private static bool[] GetCellVisibility(string hash)
    {
        var cells = new bool[CellCount];
        for (var i = 0; i < CellCount; i++)
        {
            cells[i] = hash[i] % 2 == 1;
        }
        return cells;
    }

    private static IBrush GetBrushFromHash(string hash)
    {
        var hexValue = hash.Substring(hash.Length - 7);
        var value = (double)int.Parse(hexValue, NumberStyles.HexNumber);
        var hue = value / 0xFFFFFFF;
        var color = HslToRgb(hue, 0.7, 0.5);
        return new SolidColorBrush(color);
    }

    private static Color HslToRgb(double hue, double saturation, double lightness)
    {
        double chroma = lightness < 0.5
            ? 2.0 * saturation * lightness
            : 2.0 * saturation * (1.0 - lightness);

        double mid = lightness - chroma / 2.0;
        double high = mid + chroma;

        double h6 = hue * 6.0;
        int sector = (int)Math.Floor(h6);
        double fraction = h6 - sector;

        double rising = mid + chroma * fraction;
        double falling = high - chroma * fraction;

        double r, g, b;
        switch (sector % 6)
        {
            case 0: r = high; g = rising; b = mid; break;
            case 1: r = falling; g = high; b = mid; break;
            case 2: r = mid; g = high; b = rising; break;
            case 3: r = mid; g = falling; b = high; break;
            case 4: r = rising; g = mid; b = high; break;
            default: r = high; g = mid; b = falling; break;
        }

        return Color.FromArgb(
            255,
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255));
    }
}
