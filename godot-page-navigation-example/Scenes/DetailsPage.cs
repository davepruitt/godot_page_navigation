using Godot;
using GodotPageNavigation;

namespace GodotPageNavigationExample;

/// <summary>
/// Example details page that demonstrates recursive push and pop operations.
/// </summary>
public partial class DetailsPage : PanelContainer, INavigationAware, INavigationPage
{
    #region Private Fields

    /// <summary>
    /// Displays the current stack status.
    /// </summary>
    private Label _statusLabel = null!;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the navigation stack controlling this page.
    /// </summary>
    public PageStack? Navigation { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Wires up UI events.
    /// </summary>
    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("MarginContainer/VBoxContainer/StatusLabel");

        var pushButton = GetNode<Button>("MarginContainer/VBoxContainer/PushButton");
        pushButton.Pressed += OnPushPressed;

        var popButton = GetNode<Button>("MarginContainer/VBoxContainer/PopButton");
        popButton.Pressed += OnPopPressed;
    }

    /// <summary>
    /// Updates UI when this page becomes active.
    /// </summary>
    public void OnNavigatedTo()
    {
        UpdateStatus();
    }

    /// <summary>
    /// Called when this page is no longer the current page.
    /// </summary>
    public void OnNavigatedFrom()
    {
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Pushes another details page onto the stack.
    /// </summary>
    private void OnPushPressed()
    {
        Navigation?.Push("res://Scenes/DetailsPage.tscn");
    }

    /// <summary>
    /// Pops this page from the stack.
    /// </summary>
    private void OnPopPressed()
    {
        Navigation?.Pop();
    }

    /// <summary>
    /// Refreshes the stack depth text.
    /// </summary>
    private void UpdateStatus()
    {
        var count = Navigation?.Count ?? 0;
        _statusLabel.Text = $"Stack depth: {count}";
    }

    #endregion
}

