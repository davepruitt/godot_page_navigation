namespace GodotPageNavigation;

/// <summary>
/// Defines lifecycle callbacks for page navigation.
/// </summary>
public interface INavigationPage
{
    #region Public Methods

    /// <summary>
    /// Called when the page becomes the current page.
    /// </summary>
    void OnNavigatedTo();

    /// <summary>
    /// Called when the page stops being the current page.
    /// </summary>
    void OnNavigatedFrom();

    #endregion
}

