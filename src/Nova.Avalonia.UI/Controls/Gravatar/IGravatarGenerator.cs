namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Interface for gravatar/identicon generators.
/// </summary>
public interface IGravatarGenerator
{
    /// <summary>
    /// Generates an avatar visual from the given identifier.
    /// </summary>
    /// <param name="id">The identifier (email, username, etc.) to generate from.</param>
    /// <returns>A visual element representing the avatar.</returns>
    object? GenerateAvatar(string? id);
}
