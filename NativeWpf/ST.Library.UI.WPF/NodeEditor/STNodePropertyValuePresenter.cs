using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using DrawingColor = System.Drawing.Color;
using DrawingPen = System.Drawing.Pen;
using DrawingSolidBrush = System.Drawing.SolidBrush;
using MediaFontFamily = System.Windows.Media.FontFamily;

namespace ST.Library.UI.NodeEditor;

/// <summary>
/// Hosts the Skia-rendered value surface used by custom property descriptors
/// and a native WPF editor when the descriptor delegates to its base click.
/// </summary>
internal sealed class STNodePropertyValueHost : Grid
{
	private readonly STNodePropertyGrid _grid;
	private readonly STNodePropertyDescriptor _descriptor;
	private readonly Grid _row;
	private readonly STNodePropertyValuePresenter _presenter;
	private readonly bool _readOnly;
	private FrameworkElement _editor;
	private bool _closingEditor;

	public STNodePropertyValueHost(
		STNodePropertyGrid grid,
		STNodePropertyDescriptor descriptor,
		Grid row,
		bool readOnly)
	{
		_grid = grid;
		_descriptor = descriptor;
		_row = row;
		_readOnly = readOnly;
		Margin = new Thickness(4, 3, 4, 3);
		ClipToBounds = true;
		_presenter = new STNodePropertyValuePresenter(this, grid, descriptor);
		Children.Add(_presenter);
		Loaded += OnLayoutChanged;
		SizeChanged += OnLayoutChanged;
		AutomationProperties.SetName(_presenter, descriptor.Name ?? descriptor.PropertyInfo?.Name ?? "Property value");
	}

	internal bool IsReadOnly => _readOnly;

	internal void UpdateDescriptorLayout()
	{
		_grid.UpdateDescriptorLayout(_descriptor, _row, this);
	}

	internal void InvalidatePresenter()
	{
		_presenter.InvalidateVisual();
	}

	internal void BeginEdit()
	{
		if (_readOnly || _editor != null)
		{
			return;
		}

		_presenter.ReleaseMouseCapture();
		Type propertyType = _descriptor.PropertyInfo.PropertyType;
		if (propertyType == typeof(bool) || propertyType.IsEnum)
		{
			BeginSelectionEdit(propertyType);
			return;
		}

		var textBox = new TextBox
		{
			Text = _descriptor.GetStringFromValue() ?? string.Empty,
			VerticalContentAlignment = VerticalAlignment.Center,
			BorderThickness = new Thickness(1),
			Padding = new Thickness(3, 0, 3, 0),
			Background = CreateValueBrush(),
			Foreground = _grid.Foreground,
			FontFamily = new MediaFontFamily(_grid.Font.Name),
			FontSize = Math.Max(1d, _grid.Font.SizeInPoints * 96d / 72d)
		};
		textBox.KeyDown += OnTextEditorKeyDown;
		textBox.LostKeyboardFocus += OnEditorLostKeyboardFocus;
		ShowEditor(textBox);
		textBox.SelectAll();
	}

	private void BeginSelectionEdit(Type propertyType)
	{
		string[] items = propertyType == typeof(bool)
			? new[] { bool.TrueString, bool.FalseString }
			: Enum.GetNames(propertyType);
		var comboBox = new ComboBox
		{
			ItemsSource = items,
			SelectedItem = _descriptor.GetValue(null)?.ToString(),
			Background = CreateValueBrush(),
			Foreground = _grid.Foreground,
			FontFamily = new MediaFontFamily(_grid.Font.Name),
			FontSize = Math.Max(1d, _grid.Font.SizeInPoints * 96d / 72d)
		};
		comboBox.SelectionChanged += OnSelectionChanged;
		comboBox.KeyDown += OnSelectionEditorKeyDown;
		comboBox.LostKeyboardFocus += OnEditorLostKeyboardFocus;
		ShowEditor(comboBox);
		_ = comboBox.Dispatcher.BeginInvoke(
			DispatcherPriority.Input,
			new Action(() =>
			{
				if (ReferenceEquals(_editor, comboBox))
				{
					comboBox.IsDropDownOpen = true;
				}
			}));
	}

	private void ShowEditor(FrameworkElement editor)
	{
		_editor = editor;
		Panel.SetZIndex(editor, 1);
		Children.Add(editor);
		editor.Focus();
	}

	private void OnTextEditorKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			CloseEditor(commit: true);
			e.Handled = true;
		}
		else if (e.Key == Key.Escape)
		{
			CloseEditor(commit: false);
			e.Handled = true;
		}
	}

	private void OnSelectionEditorKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			CloseEditor(commit: false);
			e.Handled = true;
		}
		else if (e.Key == Key.Enter)
		{
			CloseEditor(commit: true);
			e.Handled = true;
		}
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_editor != null && !_closingEditor)
		{
			CloseEditor(commit: true);
		}
	}

	private void OnEditorLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender is not FrameworkElement editor)
		{
			return;
		}
		_ = editor.Dispatcher.BeginInvoke(
			DispatcherPriority.Input,
			new Action(() =>
			{
				if (ReferenceEquals(_editor, editor) && !editor.IsKeyboardFocusWithin)
				{
					CloseEditor(commit: true);
				}
			}));
	}

	private void CloseEditor(bool commit)
	{
		if (_editor == null || _closingEditor)
		{
			return;
		}

		_closingEditor = true;
		FrameworkElement editor = _editor;
		try
		{
			if (commit)
			{
				string text = editor switch
				{
					TextBox textBox => textBox.Text,
					ComboBox comboBox => comboBox.SelectedItem?.ToString(),
					_ => null
				};
				if (text != null)
				{
					_grid.CommitText(_descriptor, text);
				}
			}
		}
		finally
		{
			if (editor is TextBox textBox)
			{
				textBox.KeyDown -= OnTextEditorKeyDown;
				textBox.LostKeyboardFocus -= OnEditorLostKeyboardFocus;
			}
			else if (editor is ComboBox comboBox)
			{
				comboBox.SelectionChanged -= OnSelectionChanged;
				comboBox.KeyDown -= OnSelectionEditorKeyDown;
				comboBox.LostKeyboardFocus -= OnEditorLostKeyboardFocus;
			}
			Children.Remove(editor);
			_editor = null;
			_closingEditor = false;
			InvalidatePresenter();
			_presenter.Focus();
		}
	}

	private SolidColorBrush CreateValueBrush()
	{
		DrawingColor color = _grid.ItemValueBackColor;
		return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
	}

	private void OnLayoutChanged(object sender, EventArgs e)
	{
		UpdateDescriptorLayout();
	}
}

internal sealed class STNodePropertyValuePresenter : SKElement
{
	private readonly STNodePropertyValueHost _host;
	private readonly STNodePropertyGrid _grid;
	private readonly STNodePropertyDescriptor _descriptor;
	private System.Drawing.Point? _mouseDownLocation;

	public STNodePropertyValuePresenter(
		STNodePropertyValueHost host,
		STNodePropertyGrid grid,
		STNodePropertyDescriptor descriptor)
	{
		_host = host;
		_grid = grid;
		_descriptor = descriptor;
		Focusable = true;
		Cursor = Cursors.Arrow;
	}

	protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
	{
		base.OnPaintSurface(e);
		_host.UpdateDescriptorLayout();
		SKCanvas canvas = e.Surface.Canvas;
		canvas.Clear(SkiaDrawingHelper.ToSKColor(_grid.ItemValueBackColor));
		if (ActualWidth <= 0d || ActualHeight <= 0d)
		{
			return;
		}

		float scaleX = e.Info.Width / (float)ActualWidth;
		float scaleY = e.Info.Height / (float)ActualHeight;
		int saveCount = canvas.Save();
		canvas.Scale(scaleX, scaleY);
		canvas.Translate(-_descriptor.RectangleR.Left, -_descriptor.RectangleR.Top);
		try
		{
			using DrawingPen pen = new DrawingPen(DrawingColor.Black, 1f);
			using DrawingSolidBrush brush = new DrawingSolidBrush(DrawingColor.Black);
			var drawingTools = new DrawingTools
			{
				Canvas = canvas,
				Pen = pen,
				SolidBrush = brush,
				Context = new SkiaDrawingContext(canvas)
			};
			_descriptor.OnDrawValueRectangle(drawingTools);
			if (_host.IsReadOnly)
			{
				using SKPaint overlay = new SKPaint
				{
					Color = new SKColor(125, 125, 125, 125),
					Style = SKPaintStyle.Fill,
					IsAntialias = true
				};
				canvas.DrawRect(SkiaDrawingHelper.ToSKRect(_descriptor.RectangleR), overlay);
			}
		}
		catch (Exception ex)
		{
			_descriptor.OnSetValueError(ex);
		}
		finally
		{
			canvas.RestoreToCount(saveCount);
		}
	}

	protected override void OnMouseEnter(MouseEventArgs e)
	{
		base.OnMouseEnter(e);
		Dispatch(() => _descriptor.OnMouseEnter(EventArgs.Empty));
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		Dispatch(() => _descriptor.OnMouseLeave(EventArgs.Empty));
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		STNodeMouseEventArgs args = CreateMouseArgs(e, GetPressedButtons(e), clicks: 0);
		Dispatch(() => _descriptor.OnMouseMove(args));
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);
		if (_host.IsReadOnly)
		{
			return;
		}

		Focus();
		STNodeMouseEventArgs args = CreateMouseArgs(e, ToMouseButton(e.ChangedButton), e.ClickCount);
		_mouseDownLocation = args.Location;
		CaptureMouse();
		Dispatch(() => _descriptor.OnMouseDown(args));
		e.Handled = true;
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);
		if (_host.IsReadOnly || _mouseDownLocation == null)
		{
			return;
		}

		STNodeMouseEventArgs args = CreateMouseArgs(e, ToMouseButton(e.ChangedButton), e.ClickCount);
		System.Drawing.Point downLocation = _mouseDownLocation.Value;
		_mouseDownLocation = null;
		Dispatch(() => _descriptor.OnMouseUp(args));
		ReleaseMouseCapture();
		if (downLocation == args.Location)
		{
			Dispatch(() => _descriptor.OnMouseClick(args));
		}
		_grid.RefreshDescriptor(_descriptor);
		e.Handled = true;
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (!_host.IsReadOnly && (e.Key == Key.Enter || e.Key == Key.F2 || e.Key == Key.Space))
		{
			_host.BeginEdit();
			e.Handled = true;
		}
	}

	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		_mouseDownLocation = null;
	}

	private STNodeMouseEventArgs CreateMouseArgs(MouseEventArgs e, STMouseButtons buttons, int clicks)
	{
		_host.UpdateDescriptorLayout();
		System.Windows.Point point = e.GetPosition(this);
		return new STNodeMouseEventArgs(
			buttons,
			clicks,
			_descriptor.RectangleR.Left + (int)Math.Round(point.X),
			_descriptor.RectangleR.Top + (int)Math.Round(point.Y),
			e is MouseWheelEventArgs wheel ? wheel.Delta : 0);
	}

	private void Dispatch(Action action)
	{
		try
		{
			action();
			InvalidateVisual();
		}
		catch (Exception ex)
		{
			_descriptor.OnSetValueError(ex);
		}
	}

	private static STMouseButtons GetPressedButtons(MouseEventArgs e)
	{
		STMouseButtons buttons = STMouseButtons.None;
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			buttons |= STMouseButtons.Left;
		}
		if (e.RightButton == MouseButtonState.Pressed)
		{
			buttons |= STMouseButtons.Right;
		}
		if (e.MiddleButton == MouseButtonState.Pressed)
		{
			buttons |= STMouseButtons.Middle;
		}
		if (e.XButton1 == MouseButtonState.Pressed)
		{
			buttons |= STMouseButtons.XButton1;
		}
		if (e.XButton2 == MouseButtonState.Pressed)
		{
			buttons |= STMouseButtons.XButton2;
		}
		return buttons;
	}

	private static STMouseButtons ToMouseButton(MouseButton button)
	{
		return button switch
		{
			MouseButton.Left => STMouseButtons.Left,
			MouseButton.Right => STMouseButtons.Right,
			MouseButton.Middle => STMouseButtons.Middle,
			MouseButton.XButton1 => STMouseButtons.XButton1,
			MouseButton.XButton2 => STMouseButtons.XButton2,
			_ => STMouseButtons.None
		};
	}
}
