using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using ST.Library.UI.NodeEditor;
using SkiaSharp;

namespace WinNodeEditorDemo.Blender
{
    /// <summary>
    /// 此类仅演示 作为MixRGB节点的复选框控件
    /// </summary>
    public class STNodeCheckBox : STNodeControl
    {
        private bool _Checked;

        public bool Checked {
            get { return _Checked; }
            set {
                _Checked = value;
                this.Invalidate();
            }
        }

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged(EventArgs e) {
            if (this.ValueChanged != null) this.ValueChanged(this, e);
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseClick(e);
            this.Checked = !this.Checked;
            this.OnValueChanged(new EventArgs());
        }

        protected override void OnPaint(DrawingTools dt) {
            SkiaDrawingHelper.RenderToCanvas(dt.Canvas, canvas => {
                using (var gray = new SKPaint { Color = SKColors.Gray, Style = SKPaintStyle.Fill, IsAntialias = true })
                using (var black = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill, IsAntialias = true })
                using (var textPaint = new SKPaint { Color = SKColors.LightGray, TextSize = Math.Max(10f, this.Font.Size), IsAntialias = true }) {
                    canvas.DrawRect(0, 5, 10, 10, gray);
                    var metrics = textPaint.FontMetrics;
                    float textY = (20 - (metrics.Descent - metrics.Ascent)) / 2 - metrics.Ascent;
                    canvas.DrawText(this.Text ?? string.Empty, 15, textY, textPaint);
                    if (this.Checked) canvas.DrawRect(2, 7, 6, 6, black);
                }
            });
        }
    }
}
