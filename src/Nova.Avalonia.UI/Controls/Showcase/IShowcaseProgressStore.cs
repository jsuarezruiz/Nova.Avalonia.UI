using System.Threading;
using System.Threading.Tasks;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Stores and restores showcase progress.
/// </summary>
public interface IShowcaseProgressStore
{
    /// <summary>
    /// Loads persisted progress for a showcase key.
    /// </summary>
    Task<ShowcaseProgressState?> LoadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves persisted progress for a showcase key.
    /// </summary>
    Task SaveAsync(string key, ShowcaseProgressState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears persisted progress for a showcase key.
    /// </summary>
    Task ClearAsync(string key, CancellationToken cancellationToken = default);
}
