# Native WPF implementation

This directory is self-contained. It adds a native WPF implementation without changing the repository's existing WinForms library, WinForms demo, legacy WPF host demo, or original solution.

## Projects

- `ST.Library.UI.WPF`: native WPF controls rendered with `SkiaSharp.Views.WPF.SKElement`.
- `WpfNodeEditorDemo`: WPF-only sample with the node canvas, context menus, automatic layout, property editing, save/open actions, and execution command.
- `STNodeEditor.Wpf.sln`: standalone solution for the two projects above.

Neither project references `System.Windows.Forms`, `WindowsFormsIntegration`, `WindowsFormsHost`, or `SkiaSharp.Views.WindowsForms`.

The demo uses direct cursor-centered wheel zoom in `0.05` steps across the editor's full `0.2` to `5.0` scale range. The canvas lock button controls blank-area left-drag explicitly: unlocked pans the canvas, locked draws a selection rectangle, and middle-button drag always pans. Manual lock state is not changed by clicking a node.

The demo keeps the historical `WpfNodeEdittorDemo` assembly name so existing STND files retain the same node module identity; only the project folder and UI namespace use the corrected spelling.

## Run

Open `STNodeEditor.Wpf.sln`, select `WpfNodeEditorDemo`, and press F5. From PowerShell, the equivalent commands are:

```powershell
dotnet build .\STNodeEditor.Wpf.sln
dotnet run --project .\WpfNodeEditorDemo\WpfNodeEditorDemo.csproj
```

The projects target `net10.0-windows` to match the current upstream WinForms library target. This does not alter the target framework or source of any existing project.
