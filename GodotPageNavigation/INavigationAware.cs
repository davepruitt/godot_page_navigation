namespace GodotPageNavigation;

/// <summary>
/// Represents a page that can receive a navigation stack reference.
/// </summary>
public interface INavigationAware
{
    #region Public Properties

    /// <summary>
    /// Gets or sets the navigation stack controlling the page.
    /// </summary>
    PageStack? Navigation { get; set; }

    #endregion
}

