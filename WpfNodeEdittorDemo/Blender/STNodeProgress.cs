using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ST.Library.UI.NodeEditor;
using System.Drawing;
using SkiaSharp;

namespace WinNodeEditorDemo.Blender
{
    /// <summary>
    /// 此类仅演示 作为MixRGB节点的进度条控件
    /// </summary>
    public class STNodeProgress : STNodeControl
    {
        private int _Value = 50;

        public int Value {
            get { return _Value; }
            set { 
                _Value = value;
                this.Invalidate();
            }
        }

        private bool m_bMouseDown;

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged(EventArgs e) {
            if (this.ValueChanged != null) this.ValueChanged(this, e);
        }

        protected override void OnPaint(DrawingTools dt) {
            base.OnPaint(dt);
            float progress=(float)this._Value/100; SkiaDrawingHelper.RenderToCanvas(dt.Canvas, canvas => { using (var bg = new SKPaint { Color = SKColors.Gray, Style = SKPaintStyle.Fill, IsAntialias = true }) using (var fg = new SKPaint { Color = SKColors.CornflowerBlue, Style = SKPaintStyle.Fill, IsAntialias = true }) using (var text = new SKPaint { Color = SKColors.White, TextSize = Math.Max(10f, this.Font.Size), IsAntialias = true }) { canvas.DrawRect(0,0,this.Width,this.Height,bg); canvas.DrawRect(0,0,this.Width*progress,this.Height,fg); var fm=text.FontMetrics; float y=(this.Height-(fm.Descent-fm.Ascent))/2-fm.Ascent; canvas.DrawText(this.Text ?? string.Empty,2,y,text); var pct=progress.ToString("F2"); canvas.DrawText(pct,this.Width-text.MeasureText(pct)-2,y,text);} });

        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);
            m_bMouseDown = true;
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            m_bMouseDown = false;
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);
            if (!m_bMouseDown) return;
            int v = (int)((float)e.X / this.Width * 100);
            if (v < 0) v = 0;
            if (v > 100) v = 100;
            this._Value = v;
            this.OnValueChanged(new EventArgs());
            this.Invalidate();
        }
    }
}
