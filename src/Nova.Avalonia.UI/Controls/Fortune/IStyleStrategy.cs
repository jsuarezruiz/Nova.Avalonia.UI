namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines a strategy for styling fortune items based on their position.
/// </summary>
public interface IStyleStrategy
{
    /// <summary>
    /// Gets the style for an item at a specific index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <param name="totalCount">The total number of items.</param>
    /// <param name="itemStyle">The item's custom style, if any.</param>
    /// <returns>The style to apply to the item.</returns>
    FortuneItemStyle GetStyle(int index, int totalCount, FortuneItemStyle? itemStyle);
}
