using Godot;
using System;
using System.Collections.Generic;

namespace GodotPageNavigation;

/// <summary>
/// Manages a stack of pages and ensures only the current page is attached to the host node.
/// </summary>
public sealed class PageStack
{
    #region Private Fields

    /// <summary>
    /// The node that hosts the currently visible page.
    /// </summary>
    private readonly Node _host;

    /// <summary>
    /// The internal stack of page instances.
    /// </summary>
    private readonly Stack<Node> _pages = new();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PageStack"/> class.
    /// </summary>
    /// <param name="host">The node that will contain the current page.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is <see langword="null"/>.</exception>
    public PageStack(Node host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when the current page changes.
    /// </summary>
    public event Action<Node?>? CurrentPageChanged;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the number of pages currently on the stack.
    /// </summary>
    public int Count => _pages.Count;

    /// <summary>
    /// Gets a value indicating whether a pop operation can be performed.
    /// </summary>
    public bool CanPop => _pages.Count > 1;

    /// <summary>
    /// Gets the page currently on top of the stack.
    /// </summary>
    public Node? CurrentPage => _pages.Count > 0 ? _pages.Peek() : null;

    #endregion

    #region Public Methods

    /// <summary>
    /// Pushes a scene loaded from path and returns the instantiated typed page.
    /// </summary>
    /// <typeparam name="T">The expected page node type.</typeparam>
    /// <param name="scenePath">The resource path to a <see cref="PackedScene"/>.</param>
    /// <returns>The instantiated page.</returns>
    public T Push<T>(string scenePath) where T : Node
    {
        return Push<T>(LoadScene(scenePath));
    }

    /// <summary>
    /// Pushes a scene loaded from path and returns the instantiated page.
    /// </summary>
    /// <param name="scenePath">The resource path to a <see cref="PackedScene"/>.</param>
    /// <returns>The instantiated page.</returns>
    public Node Push(string scenePath)
    {
        return Push(LoadScene(scenePath));
    }

    /// <summary>
    /// Pushes a typed page created from a packed scene.
    /// </summary>
    /// <typeparam name="T">The expected page node type.</typeparam>
    /// <param name="scene">The packed scene to instantiate.</param>
    /// <returns>The instantiated page.</returns>
    public T Push<T>(PackedScene scene) where T : Node
    {
        var page = scene.Instantiate<T>();
        PushNode(page);
        return page;
    }

    /// <summary>
    /// Pushes a page created from a packed scene.
    /// </summary>
    /// <param name="scene">The packed scene to instantiate.</param>
    /// <returns>The instantiated page.</returns>
    public Node Push(PackedScene scene)
    {
        var page = scene.Instantiate();
        PushNode(page);
        return page;
    }

    /// <summary>
    /// Pops the current page and restores the previous page.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a page was popped; otherwise <see langword="false"/> when the stack is at root.
    /// </returns>
    public bool Pop()
    {
        if (!CanPop)
        {
            return false;
        }

        var current = _pages.Pop();

        if (current is INavigationAware currentAware)
        {
            currentAware.Navigation = null;
        }

        NotifyNavigatedFrom(current);

        if (current.GetParent() == _host)
        {
            _host.RemoveChild(current);
        }

        current.QueueFree();

        var restored = _pages.Peek();
        AddAsCurrent(restored);
        CurrentPageChanged?.Invoke(restored);

        return true;
    }

    /// <summary>
    /// Removes all pages above the root page.
    /// </summary>
    public void ClearToRoot()
    {
        while (_pages.Count > 1)
        {
            var page = _pages.Pop();

            if (page is INavigationAware aware)
            {
                aware.Navigation = null;
            }

            if (page.GetParent() == _host)
            {
                _host.RemoveChild(page);
            }

            page.QueueFree();
        }

        if (_pages.Count == 1)
        {
            var root = _pages.Peek();
            if (root.GetParent() != _host)
            {
                AddAsCurrent(root);
            }
        }

        CurrentPageChanged?.Invoke(CurrentPage);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Pushes a page instance onto the stack and makes it current.
    /// </summary>
    /// <param name="page">The page to push.</param>
    private void PushNode(Node page)
    {
        if (CurrentPage is { } current)
        {
            NotifyNavigatedFrom(current);
            if (current.GetParent() == _host)
            {
                _host.RemoveChild(current);
            }
        }

        _pages.Push(page);
        AddAsCurrent(page);
        CurrentPageChanged?.Invoke(page);
    }

    /// <summary>
    /// Adds the page to the host and notifies lifecycle hooks.
    /// </summary>
    /// <param name="page">The page to make current.</param>
    private void AddAsCurrent(Node page)
    {
        if (page is INavigationAware aware)
        {
            aware.Navigation = this;
        }

        if (page.GetParent() is Node parent && parent != _host)
        {
            parent.RemoveChild(page);
        }

        if (page.GetParent() != _host)
        {
            _host.AddChild(page);
        }

        NotifyNavigatedTo(page);
    }

    /// <summary>
    /// Loads a packed scene from the given path.
    /// </summary>
    /// <param name="scenePath">The resource path to load.</param>
    /// <returns>The loaded packed scene.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="scenePath"/> is blank.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the scene cannot be loaded.</exception>
    private static PackedScene LoadScene(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            throw new ArgumentException("Scene path is required.", nameof(scenePath));
        }

        return GD.Load<PackedScene>(scenePath)
            ?? throw new InvalidOperationException($"Unable to load scene at '{scenePath}'.");
    }

    /// <summary>
    /// Notifies a page that it has become active.
    /// </summary>
    /// <param name="page">The page being navigated to.</param>
    private static void NotifyNavigatedTo(Node page)
    {
        if (page is INavigationPage navigationPage)
        {
            navigationPage.OnNavigatedTo();
        }
    }

    /// <summary>
    /// Notifies a page that it is no longer active.
    /// </summary>
    /// <param name="page">The page being navigated from.</param>
    private static void NotifyNavigatedFrom(Node page)
    {
        if (page is INavigationPage navigationPage)
        {
            navigationPage.OnNavigatedFrom();
        }
    }

    #endregion
}

