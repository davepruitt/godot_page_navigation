using Godot;
using GodotPageNavigation;

namespace GodotPageNavigationExample;

/// <summary>
/// Root control that owns the <see cref="PageStack"/> and loads the initial page.
/// </summary>
public partial class AppRoot : Control
{
    #region Public Properties

    /// <summary>
    /// Gets or sets the scene path of the first page to push.
    /// </summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string InitialPagePath { get; set; } = "res://Scenes/MainPage.tscn";

    /// <summary>
    /// Gets the navigation stack instance for this app root.
    /// </summary>
    public PageStack Navigation { get; private set; } = null!;

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the page stack and pushes the configured initial page.
    /// </summary>
    public override void _Ready()
    {
        Navigation = new PageStack(this);
        Navigation.Push(InitialPagePath);
    }

    #endregion
}

