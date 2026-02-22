using Godot;
using System;
using System.Collections.Generic;

namespace GodotPageNavigation;

/// <summary>
/// Manages primary and modal page stacks and activates only the current page.
/// </summary>
public sealed class PageStack
{
    #region Private Fields

    /// <summary>
    /// The node that hosts visible pages.
    /// </summary>
    private readonly Node _host;

    /// <summary>
    /// Dedicated canvas layer for modal UI pages so they draw above in-scene canvas layers.
    /// </summary>
    private readonly CanvasLayer _modalCanvasLayer;

    /// <summary>
    /// The primary navigation stack.
    /// </summary>
    private readonly Stack<Node> _primaryPages = new();

    /// <summary>
    /// The modal navigation stack.
    /// </summary>
    private readonly Stack<Node> _modalPages = new();

    /// <summary>
    /// Tracks whether the top modal page is currently suspended.
    /// </summary>
    private bool _isTopModalSuspended;

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
        _modalCanvasLayer = new CanvasLayer
        {
            Name = "PageStackModalLayer",
            Layer = 100
        };
        _host.AddChild(_modalCanvasLayer);
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
    /// Gets the number of pages in the primary stack.
    /// </summary>
    public int Count => _primaryPages.Count;

    /// <summary>
    /// Gets the number of pages in the modal stack.
    /// </summary>
    public int ModalCount => _modalPages.Count;

    /// <summary>
    /// Gets a value indicating whether the top modal page is currently visible.
    /// </summary>
    public bool HasVisibleModal => _modalPages.Count > 0 && !_isTopModalSuspended;

    /// <summary>
    /// Gets a value indicating whether a pop operation can be performed.
    /// </summary>
    public bool CanPop => _modalPages.Count > 0 || _primaryPages.Count > 1;

    /// <summary>
    /// Gets the current page from the primary stack.
    /// </summary>
    public Node? CurrentPrimaryPage => _primaryPages.Count > 0 ? _primaryPages.Peek() : null;

    /// <summary>
    /// Gets the current page from the modal stack.
    /// </summary>
    public Node? CurrentModalPage => _modalPages.Count > 0 ? _modalPages.Peek() : null;

    /// <summary>
    /// Gets the effective current page.
    /// </summary>
    public Node? CurrentPage => HasVisibleModal ? CurrentModalPage : CurrentPrimaryPage;

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
        PushPrimaryNode(page);
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
        PushPrimaryNode(page);
        return page;
    }

    /// <summary>
    /// Pushes an existing page instance onto the primary stack.
    /// </summary>
    /// <typeparam name="T">The expected page node type.</typeparam>
    /// <param name="page">The page instance to push.</param>
    /// <returns>The same page instance.</returns>
    public T Push<T>(T page) where T : Node
    {
        PushPrimaryNode(page);
        return page;
    }

    /// <summary>
    /// Pushes an existing page instance onto the primary stack.
    /// </summary>
    /// <param name="page">The page instance to push.</param>
    /// <returns>The same page instance.</returns>
    public Node Push(Node page)
    {
        PushPrimaryNode(page);
        return page;
    }

    /// <summary>
    /// Pushes a scene loaded from path onto the modal stack and returns the instantiated typed page.
    /// </summary>
    /// <typeparam name="T">The expected page node type.</typeparam>
    /// <param name="scenePath">The resource path to a <see cref="PackedScene"/>.</param>
    /// <returns>The instantiated page.</returns>
    public T PushModal<T>(string scenePath) where T : Node
    {
        return PushModal<T>(LoadScene(scenePath));
    }

    /// <summary>
    /// Pushes a scene loaded from path onto the modal stack and returns the instantiated page.
    /// </summary>
    /// <param name="scenePath">The resource path to a <see cref="PackedScene"/>.</param>
    /// <returns>The instantiated page.</returns>
    public Node PushModal(string scenePath)
    {
        return PushModal(LoadScene(scenePath));
    }

    /// <summary>
    /// Pushes a typed page created from a packed scene onto the modal stack.
    /// </summary>
    /// <typeparam name="T">The expected page node type.</typeparam>
    /// <param name="scene">The packed scene to instantiate.</param>
    /// <returns>The instantiated page.</returns>
    public T PushModal<T>(PackedScene scene) where T : Node
    {
        var page = scene.Instantiate<T>();
        PushModalNode(page);
        return page;
    }

    /// <summary>
    /// Pushes a page created from a packed scene onto the modal stack.
    /// </summary>
    /// <param name="scene">The packed scene to instantiate.</param>
    /// <returns>The instantiated page.</returns>
    public Node PushModal(PackedScene scene)
    {
        var page = scene.Instantiate();
        PushModalNode(page);
        return page;
    }

    /// <summary>
    /// Pushes an existing page instance onto the modal stack.
    /// </summary>
    /// <typeparam name="T">The expected page node type.</typeparam>
    /// <param name="page">The page instance to push.</param>
    /// <returns>The same page instance.</returns>
    public T PushModal<T>(T page) where T : Node
    {
        PushModalNode(page);
        return page;
    }

    /// <summary>
    /// Pushes an existing page instance onto the modal stack.
    /// </summary>
    /// <param name="page">The page instance to push.</param>
    /// <returns>The same page instance.</returns>
    public Node PushModal(Node page)
    {
        PushModalNode(page);
        return page;
    }

    /// <summary>
    /// Pops the current page.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a page was popped; otherwise <see langword="false"/>.
    /// </returns>
    public bool Pop()
    {
        if (_modalPages.Count > 0)
        {
            return PopModal();
        }

        return PopPrimary();
    }

    /// <summary>
    /// Pops the current modal page.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a modal page was popped; otherwise <see langword="false"/>.
    /// </returns>
    public bool PopModal()
    {
        if (_modalPages.Count == 0)
        {
            return false;
        }

        var current = _modalPages.Pop();
        bool wasSuspended = _isTopModalSuspended;

        if (current is INavigationAware currentAware)
        {
            currentAware.Navigation = null;
        }

        if (!wasSuspended)
        {
            NotifyNavigatedFrom(current);
        }

        RemoveIfHosted(current);

        current.QueueFree();

        _isTopModalSuspended = false;

        if (_modalPages.Count > 0)
        {
            if (wasSuspended && CurrentPrimaryPage is { } activePrimary)
            {
                NotifyNavigatedFrom(activePrimary);
            }

            var restoredModal = _modalPages.Peek();
            AddAsCurrent(restoredModal, preferModalLayer: true);
            CurrentPageChanged?.Invoke(restoredModal);
            return true;
        }

        if (CurrentPrimaryPage is { } primary)
        {
            EnsureAttached(primary, preferModalLayer: false);
            ActivatePage(primary);
            if (!wasSuspended)
            {
                NotifyNavigatedTo(primary);
            }
        }

        CurrentPageChanged?.Invoke(CurrentPage);
        return true;
    }

    /// <summary>
    /// Temporarily suspends the top modal page while keeping it on the modal stack.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a modal page was suspended; otherwise <see langword="false"/>.
    /// </returns>
    public bool SuspendTopModal()
    {
        if (_modalPages.Count == 0 || _isTopModalSuspended)
        {
            return false;
        }

        var modal = _modalPages.Peek();
        NotifyNavigatedFrom(modal);
        DeactivatePage(modal);

        if (CurrentPrimaryPage is { } primary)
        {
            EnsureAttached(primary, preferModalLayer: false);
            ActivatePage(primary);
            NotifyNavigatedTo(primary);
        }

        _isTopModalSuspended = true;
        CurrentPageChanged?.Invoke(CurrentPage);
        return true;
    }

    /// <summary>
    /// Resumes the top modal page after suspension.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a modal page was resumed; otherwise <see langword="false"/>.
    /// </returns>
    public bool ResumeTopModal()
    {
        if (_modalPages.Count == 0 || !_isTopModalSuspended)
        {
            return false;
        }

        if (CurrentPrimaryPage is { } primary)
        {
            NotifyNavigatedFrom(primary);
        }

        _isTopModalSuspended = false;
        var modal = _modalPages.Peek();
        AddAsCurrent(modal, preferModalLayer: true);
        CurrentPageChanged?.Invoke(modal);

        return true;
    }

    /// <summary>
    /// Removes all modal pages.
    /// </summary>
    public void ClearModals()
    {
        while (_modalPages.Count > 0)
        {
            PopModal();
        }
    }

    /// <summary>
    /// Removes all pages above the primary root page and clears all modals.
    /// </summary>
    public void ClearToRoot()
    {
        ClearModals();

        while (_primaryPages.Count > 1)
        {
            var page = _primaryPages.Pop();

            if (page is INavigationAware aware)
            {
                aware.Navigation = null;
            }

            NotifyNavigatedFrom(page);
            RemoveIfHosted(page);

            page.QueueFree();
        }

        if (_primaryPages.Count == 1)
        {
            var root = _primaryPages.Peek();
            EnsureAttached(root, preferModalLayer: false);
            ActivatePage(root);
            NotifyNavigatedTo(root);
        }

        CurrentPageChanged?.Invoke(CurrentPage);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Pushes a page instance onto the primary stack and makes it current.
    /// </summary>
    /// <param name="page">The page to push.</param>
    private void PushPrimaryNode(Node page)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        if (_modalPages.Count > 0)
        {
            throw new InvalidOperationException(
                "Cannot push a primary page while modal pages are open. Pop or clear modals first.");
        }

        if (CurrentPrimaryPage is { } currentPrimary)
        {
            NotifyNavigatedFrom(currentPrimary);
            RemoveIfHosted(currentPrimary);
        }

        _primaryPages.Push(page);
        AddAsCurrent(page, preferModalLayer: false);
        CurrentPageChanged?.Invoke(page);
    }

    /// <summary>
    /// Pushes a page instance onto the modal stack and makes it current.
    /// </summary>
    /// <param name="page">The modal page to push.</param>
    private void PushModalNode(Node page)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        if (_modalPages.Count > 0)
        {
            if (!_isTopModalSuspended)
            {
                var currentModal = _modalPages.Peek();
                NotifyNavigatedFrom(currentModal);
                DeactivatePage(currentModal);
            }
        }
        else if (CurrentPrimaryPage is { } primary)
        {
            NotifyNavigatedFrom(primary);
        }

        _isTopModalSuspended = false;
        _modalPages.Push(page);
        AddAsCurrent(page, preferModalLayer: true);
        CurrentPageChanged?.Invoke(page);
    }

    /// <summary>
    /// Pops the current primary page and restores the previous primary page.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a primary page was popped; otherwise <see langword="false"/>.
    /// </returns>
    private bool PopPrimary()
    {
        if (_primaryPages.Count <= 1)
        {
            return false;
        }

        var current = _primaryPages.Pop();

        if (current is INavigationAware currentAware)
        {
            currentAware.Navigation = null;
        }

        NotifyNavigatedFrom(current);
        RemoveIfHosted(current);
        current.QueueFree();

        var restored = _primaryPages.Peek();
        AddAsCurrent(restored, preferModalLayer: false);
        CurrentPageChanged?.Invoke(restored);
        return true;
    }

    /// <summary>
    /// Adds the page to the host and notifies lifecycle hooks.
    /// </summary>
    /// <param name="page">The page to make current.</param>
    /// <param name="preferModalLayer">True to host canvas items on the modal canvas layer.</param>
    private void AddAsCurrent(Node page, bool preferModalLayer)
    {
        if (page is INavigationAware aware)
        {
            aware.Navigation = this;
        }

        EnsureAttached(page, preferModalLayer);
        ActivatePage(page);
        NotifyNavigatedTo(page);
    }

    /// <summary>
    /// Ensures the page is attached to the host node.
    /// </summary>
    /// <param name="page">The page to attach.</param>
    /// <param name="preferModalLayer">True to host canvas items on the modal canvas layer.</param>
    private void EnsureAttached(Node page, bool preferModalLayer)
    {
        Node targetParent = ResolveTargetParent(page, preferModalLayer);

        if (page.GetParent() is Node parent && parent != targetParent)
        {
            parent.RemoveChild(page);
        }

        if (page.GetParent() != targetParent)
        {
            targetParent.AddChild(page);
        }
    }

    /// <summary>
    /// Removes the node from the host if it is attached there.
    /// </summary>
    /// <param name="page">The node to detach.</param>
    private void RemoveIfHosted(Node page)
    {
        if (page.GetParent() is Node parent)
        {
            parent.RemoveChild(page);
        }
    }

    /// <summary>
    /// Marks the page as visible and interactive.
    /// </summary>
    /// <param name="page">The page node.</param>
    private static void ActivatePage(Node page)
    {
        SetPageVisibility(page, isVisible: true);
    }

    /// <summary>
    /// Marks the page as hidden and non-interactive while it remains on the stack.
    /// </summary>
    /// <param name="page">The page node.</param>
    private static void DeactivatePage(Node page)
    {
        SetPageVisibility(page, isVisible: false);
    }

    /// <summary>
    /// Sets visibility for known visual page node types.
    /// </summary>
    /// <param name="page">The page node.</param>
    /// <param name="isVisible">True to show the page; false to hide it.</param>
    private static void SetPageVisibility(Node page, bool isVisible)
    {
        switch (page)
        {
            case CanvasItem canvasItem:
                canvasItem.Visible = isVisible;
                break;
            case Node3D node3D:
                node3D.Visible = isVisible;
                break;
            case Window window:
                window.Visible = isVisible;
                break;
        }
    }

    /// <summary>
    /// Resolves the target parent node for a page.
    /// </summary>
    /// <param name="page">The page to host.</param>
    /// <param name="preferModalLayer">True to host canvas items on the modal canvas layer.</param>
    /// <returns>The parent node to attach to.</returns>
    private Node ResolveTargetParent(Node page, bool preferModalLayer)
    {
        if (preferModalLayer && page is CanvasItem)
        {
            return _modalCanvasLayer;
        }

        return _host;
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

