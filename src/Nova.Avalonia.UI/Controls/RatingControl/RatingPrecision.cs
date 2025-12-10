namespace Nova.Avalonia.UI.Controls
{
    /// <summary>
    /// Specifies the precision level of the RatingControl.
    /// </summary>
    public enum RatingPrecision
    {
        /// <summary>
        /// Allows only full integer ratings (e.g., 1, 2, 3).
        /// </summary>
        Full,

        /// <summary>
        /// Allows half-star ratings (e.g., 2.5, 3.5).
        /// </summary>
        Half,

        /// <summary>
        /// Allows exact/continuous ratings (e.g., 3.7, 4.2).
        /// </summary>
        Exact
    }
}
