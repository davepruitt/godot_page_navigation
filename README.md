# Godot Page Navigation

Provides a simple stack-based page navigation system for Godot (C#), inspired by mobile app frameworks such as .NET MAUI.

The goal is to make page-style navigation easy:
- Push a new page onto a stack.
- Pop back to the previous page.
- Keep only the current page attached to the root host node.

## Repository Structure

- `GodotPageNavigation/`
  - Reusable library with the core navigation stack (`PageStack`) and navigation interfaces.
- `godot-page-navigation-example/`
  - Runnable Godot example that demonstrates how to use the library in a project.

## Core Concepts

- `PageStack`
  - Manages a stack of instantiated pages (`Node`).
  - Handles page switching by removing the previous page from the host and adding the current page.
  - Supports `Push(...)`, `Pop()`, and `ClearToRoot()`.
- `INavigationAware`
  - Optional interface for pages that want a reference to the active `PageStack`.
- `INavigationPage`
  - Optional interface for lifecycle hooks: `OnNavigatedTo()` and `OnNavigatedFrom()`.

## How to Use

1. Reference `GodotPageNavigation` from your Godot C# project.
2. Create a root host scene/script (for example, a `Control`) that initializes `PageStack`.
3. Push your initial page in `_Ready()`.
4. In each page, call `Navigation.Push(...)` and `Navigation.Pop()` as needed.

### Minimal Example

```csharp
using Godot;
using GodotPageNavigation;

public partial class AppRoot : Control
{
    public PageStack Navigation { get; private set; } = null!;

    public override void _Ready()
    {
        Navigation = new PageStack(this);
        Navigation.Push("res://Scenes/MainPage.tscn");
    }
}
```

```csharp
using Godot;
using GodotPageNavigation;

public partial class MainPage : PanelContainer, INavigationAware
{
    public PageStack? Navigation { get; set; }

    private void OnOpenDetailsPressed()
    {
        Navigation?.Push("res://Scenes/DetailsPage.tscn");
    }

    private void OnBackPressed()
    {
        Navigation?.Pop();
    }
}
```

## Running the Example

1. Open `godot-page-navigation-example` in Godot 4.5+ with .NET support enabled.
2. Build or run the project.
3. Use the buttons to push and pop pages and observe stack depth updates.

## Notes

- `Pop()` is root-safe: it returns `false` when there is only one page left on the stack.
- The included example is intentionally simple and focused on core navigation behavior.
