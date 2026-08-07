using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SkiaSharp;

namespace ST.Library.UI.NodeEditor;

/// <summary>
/// Conversion helpers shared by the editor and custom node renderers.
/// </summary>
public static class SkiaDrawingHelper
{
	public static SKColor ToSKColor(Color color)
	{
		return new SKColor(color.R, color.G, color.B, color.A);
	}

	public static SKRect ToSKRect(Rectangle rectangle)
	{
		return new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
	}

	public static SKRect ToSKRect(RectangleF rectangle)
	{
		return new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
	}

	public static SKBitmap ToSKBitmap(Image image)
	{
		if (image == null)
		{
			return null;
		}

		using MemoryStream stream = new MemoryStream();
		image.Save(stream, ImageFormat.Png);
		stream.Position = 0;
		return SKBitmap.Decode(stream);
	}

	public static void RenderToCanvas(SKCanvas canvas, Action<SKCanvas> renderAction)
	{
		if (canvas != null && renderAction != null)
		{
			renderAction(canvas);
		}
	}
}
