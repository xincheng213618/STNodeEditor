using System.Drawing;
using SkiaSharp;

namespace ST.Library.UI.NodeEditor;

/// <summary>
/// Drawing resources passed to nodes and node controls.
/// </summary>
/// <remarks>
/// <see cref="Canvas"/> is the canonical drawing surface. The pen and brush are
/// retained for source compatibility with the WinForms implementation; WPF
/// rendering translates them to Skia paints internally.
/// </remarks>
public struct DrawingTools
{
	public SKCanvas Canvas;

	public Pen Pen;

	public SolidBrush SolidBrush;

	internal SkiaDrawingContext Context;
}
