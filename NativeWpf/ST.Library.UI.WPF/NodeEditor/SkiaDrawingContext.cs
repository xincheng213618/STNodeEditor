using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SkiaSharp;

namespace ST.Library.UI.NodeEditor;

/// <summary>
/// Internal bridge for the library's legacy System.Drawing geometry helpers.
/// The bridge never owns a GDI drawing surface: every operation is rendered to
/// the current <see cref="SKCanvas"/>.
/// </summary>
internal sealed class SkiaDrawingContext
{
	private readonly SKCanvas _canvas;
	private readonly SKMatrix _baseMatrix;

	public SmoothingMode SmoothingMode { get; set; } = SmoothingMode.HighQuality;

	public System.Drawing.Text.TextRenderingHint TextRenderingHint { get; set; }

	public SkiaDrawingContext(SKCanvas canvas)
	{
		_canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
		_baseMatrix = canvas.TotalMatrix;
	}

	public int Save()
	{
		return _canvas.Save();
	}

	public void Restore(int saveCount)
	{
		_canvas.RestoreToCount(saveCount);
	}

	public void ResetTransform()
	{
		_canvas.SetMatrix(_baseMatrix);
	}

	public void TranslateTransform(float dx, float dy)
	{
		_canvas.Translate(dx, dy);
	}

	public void ScaleTransform(float sx, float sy)
	{
		_canvas.Scale(sx, sy);
	}

	public void Clear(Color color)
	{
		_canvas.Clear(SkiaDrawingHelper.ToSKColor(color));
	}

	public void SetClip(GraphicsPath path, CombineMode combineMode)
	{
		using SKPath skPath = ToSKPath(path);
		SKClipOperation operation = combineMode == CombineMode.Exclude
			? SKClipOperation.Difference
			: SKClipOperation.Intersect;
		_canvas.ClipPath(skPath, operation, IsAntialias);
	}

	public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
	{
		using SKPaint paint = CreateStrokePaint(pen);
		_canvas.DrawLine(x1, y1, x2, y2, paint);
	}

	public void DrawRectangle(Pen pen, Rectangle rectangle)
	{
		DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void DrawRectangle(Pen pen, float x, float y, float width, float height)
	{
		using SKPaint paint = CreateStrokePaint(pen);
		_canvas.DrawRect(x, y, width, height, paint);
	}

	public void FillRectangle(Brush brush, Rectangle rectangle)
	{
		FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void FillRectangle(Brush brush, RectangleF rectangle)
	{
		FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void FillRectangle(Brush brush, float x, float y, float width, float height)
	{
		using SKPaint paint = CreateFillPaint(brush);
		_canvas.DrawRect(x, y, width, height, paint);
	}

	public void DrawRoundedShadow(
		RectangleF rectangle,
		float cornerRadius,
		Color color,
		float blurSigma,
		float offsetX,
		float offsetY)
	{
		if (rectangle.Width <= 0f || rectangle.Height <= 0f || color.A == 0)
		{
			return;
		}

		using SKMaskFilter blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, Math.Max(0.1f, blurSigma));
		using SKPaint paint = new SKPaint
		{
			Color = SkiaDrawingHelper.ToSKColor(color),
			Style = SKPaintStyle.Fill,
			IsAntialias = true,
			MaskFilter = blur
		};
		SKRect shadowRectangle = new SKRect(
			rectangle.Left + offsetX,
			rectangle.Top + offsetY,
			rectangle.Right + offsetX,
			rectangle.Bottom + offsetY);
		float radius = Math.Max(0f, Math.Min(cornerRadius, Math.Min(rectangle.Width, rectangle.Height) / 2f));
		_canvas.DrawRoundRect(shadowRectangle, radius, radius, paint);
	}

	public void DrawEllipse(Pen pen, float x, float y, float width, float height)
	{
		using SKPaint paint = CreateStrokePaint(pen);
		_canvas.DrawOval(new SKRect(x, y, x + width, y + height), paint);
	}

	public void FillEllipse(Brush brush, Rectangle rectangle)
	{
		using SKPaint paint = CreateFillPaint(brush);
		_canvas.DrawOval(SkiaDrawingHelper.ToSKRect(rectangle), paint);
	}

	public void FillPolygon(Brush brush, Point[] points)
	{
		if (points == null || points.Length == 0)
		{
			return;
		}

		using SKPath path = new SKPath();
		path.MoveTo(points[0].X, points[0].Y);
		for (int i = 1; i < points.Length; i++)
		{
			path.LineTo(points[i].X, points[i].Y);
		}
		path.Close();
		using SKPaint paint = CreateFillPaint(brush);
		_canvas.DrawPath(path, paint);
	}

	public void DrawPath(Pen pen, GraphicsPath path)
	{
		using SKPath skPath = ToSKPath(path);
		using SKPaint paint = CreateStrokePaint(pen);
		_canvas.DrawPath(skPath, paint);
	}

	public void FillPath(Brush brush, GraphicsPath path)
	{
		using SKPath skPath = ToSKPath(path);
		using SKPaint paint = CreateFillPaint(brush);
		_canvas.DrawPath(skPath, paint);
	}

	public SizeF MeasureString(string text, Font font)
	{
		return MeasureString(text, font, int.MaxValue);
	}

	public SizeF MeasureString(string text, Font font, int width)
	{
		text ??= string.Empty;
		font ??= SystemFonts.DefaultFont;
		using SKTypeface typeface = CreateTypeface(font);
		using SKPaint paint = CreateTextPaint(font, Color.Black, typeface);
		SKFontMetrics metrics = paint.FontMetrics;
		float lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;
		string[] lines = text.Replace("\r", string.Empty).Split('\n');
		float maxWidth = 0f;
		int lineCount = 0;
		foreach (string line in lines)
		{
			float lineWidth = paint.MeasureText(line);
			if (width > 0 && width != int.MaxValue && lineWidth > width)
			{
				int wrappedLines = Math.Max(1, (int)Math.Ceiling(lineWidth / width));
				lineCount += wrappedLines;
				maxWidth = Math.Max(maxWidth, width);
			}
			else
			{
				lineCount++;
				maxWidth = Math.Max(maxWidth, lineWidth);
			}
		}
		return new SizeF(maxWidth, Math.Max(1, lineCount) * lineHeight);
	}

	public void DrawString(string text, Font font, Brush brush, Rectangle rectangle, StringFormat format)
	{
		DrawString(text, font, brush, (RectangleF)rectangle, format);
	}

	public void DrawString(string text, Font font, Brush brush, RectangleF rectangle, StringFormat format)
	{
		text ??= string.Empty;
		font ??= SystemFonts.DefaultFont;
		using SKTypeface typeface = CreateTypeface(font);
		using SKPaint paint = CreateTextPaint(font, GetBrushColor(brush), typeface);
		SKFontMetrics metrics = paint.FontMetrics;
		float textWidth = paint.MeasureText(text);
		float x = rectangle.Left;
		if (format?.Alignment == StringAlignment.Center)
		{
			x += (rectangle.Width - textWidth) / 2f;
		}
		else if (format?.Alignment == StringAlignment.Far)
		{
			x = rectangle.Right - textWidth;
		}

		float baseline = rectangle.Top - metrics.Ascent;
		float textHeight = metrics.Descent - metrics.Ascent;
		if (format?.LineAlignment == StringAlignment.Center)
		{
			baseline = rectangle.Top + (rectangle.Height - textHeight) / 2f - metrics.Ascent;
		}
		else if (format?.LineAlignment == StringAlignment.Far)
		{
			baseline = rectangle.Bottom - metrics.Descent;
		}

		int saveCount = _canvas.Save();
		_canvas.ClipRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
		_canvas.DrawText(text, x, baseline, paint);
		_canvas.RestoreToCount(saveCount);
	}

	private bool IsAntialias => SmoothingMode != SmoothingMode.None
		&& SmoothingMode != SmoothingMode.HighSpeed;

	private SKPaint CreateStrokePaint(Pen pen)
	{
		SKPaint paint = new SKPaint
		{
			Color = SkiaDrawingHelper.ToSKColor(pen?.Color ?? Color.Black),
			Style = SKPaintStyle.Stroke,
			StrokeWidth = pen?.Width ?? 1f,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round,
			IsAntialias = IsAntialias
		};
		if (pen?.DashStyle == DashStyle.Dash)
		{
			paint.PathEffect = SKPathEffect.CreateDash(new float[] { 4f, 3f }, 0f);
		}
		else if (pen?.DashStyle == DashStyle.Dot)
		{
			paint.PathEffect = SKPathEffect.CreateDash(new float[] { 1f, 2f }, 0f);
		}
		return paint;
	}

	private SKPaint CreateFillPaint(Brush brush)
	{
		return new SKPaint
		{
			Color = SkiaDrawingHelper.ToSKColor(GetBrushColor(brush)),
			Style = SKPaintStyle.Fill,
			IsAntialias = IsAntialias
		};
	}

	private static SKTypeface CreateTypeface(Font font)
	{
		SKFontStyleWeight weight = font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
		SKFontStyleSlant slant = font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
		return SKTypeface.FromFamilyName(font.FontFamily.Name, weight, SKFontStyleWidth.Normal, slant);
	}

	private SKPaint CreateTextPaint(Font font, Color color, SKTypeface typeface)
	{
		return new SKPaint
		{
			Color = SkiaDrawingHelper.ToSKColor(color),
			Typeface = typeface,
			TextSize = Math.Max(1f, font.SizeInPoints * 96f / 72f),
			IsAntialias = true,
			SubpixelText = true
		};
	}

	private static Color GetBrushColor(Brush brush)
	{
		return brush is SolidBrush solidBrush ? solidBrush.Color : Color.Black;
	}

	private static SKPath ToSKPath(GraphicsPath path)
	{
		SKPath result = new SKPath
		{
			FillType = path.FillMode == FillMode.Alternate ? SKPathFillType.EvenOdd : SKPathFillType.Winding
		};
		PointF[] points = path.PathPoints;
		byte[] types = path.PathTypes;
		int index = 0;
		while (index < points.Length)
		{
			byte type = (byte)(types[index] & (byte)PathPointType.PathTypeMask);
			bool close = (types[index] & (byte)PathPointType.CloseSubpath) != 0;
			switch ((PathPointType)type)
			{
			case PathPointType.Start:
				result.MoveTo(points[index].X, points[index].Y);
				index++;
				break;
			case PathPointType.Line:
				result.LineTo(points[index].X, points[index].Y);
				index++;
				break;
			case PathPointType.Bezier3 when index + 2 < points.Length:
				result.CubicTo(
					points[index].X,
					points[index].Y,
					points[index + 1].X,
					points[index + 1].Y,
					points[index + 2].X,
					points[index + 2].Y);
				close = (types[index + 2] & (byte)PathPointType.CloseSubpath) != 0;
				index += 3;
				break;
			default:
				index++;
				break;
			}
			if (close)
			{
				result.Close();
			}
		}
		return result;
	}
}
