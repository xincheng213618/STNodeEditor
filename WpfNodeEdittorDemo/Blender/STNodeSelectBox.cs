using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using SkiaSharp;
using ST.Library.UI.NodeEditor;

namespace WinNodeEditorDemo.Blender
{
    /// <summary>
    /// 此类仅演示 作为MixRGB节点的下拉框控件
    /// </summary>
    public class STNodeSelectEnumBox : STNodeControl
    {
        private Enum _Enum;
        public Enum Enum {
            get { return _Enum; }
            set {
                _Enum = value;
                this.Invalidate();
            }
        }

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged(EventArgs e) {
            if (this.ValueChanged != null) this.ValueChanged(this, e);
        }

        protected override void OnPaint(DrawingTools dt) {
            SkiaDrawingHelper.RenderToCanvas(dt.Canvas, canvas => { using (var bg = new SKPaint { Color = new SKColor(0,0,0,80), Style = SKPaintStyle.Fill, IsAntialias = true }) using (var text = new SKPaint { Color = SKColors.White, TextSize = Math.Max(10f, this.Font.Size), IsAntialias = true }) using (var arrow = new SKPaint { Color = SKColors.Gray, Style = SKPaintStyle.Fill, IsAntialias = true }) { canvas.DrawRect(0,0,this.Width,this.Height,bg); var fm=text.FontMetrics; float y=(this.Height-(fm.Descent-fm.Ascent))/2-fm.Ascent; canvas.DrawText(this.Enum == null ? string.Empty : this.Enum.ToString(),2,y,text); using (var path = new SKPath()) { path.MoveTo(this.Width-25,7); path.LineTo(this.Width-15,7); path.LineTo(this.Width-20,12); path.Close(); canvas.DrawPath(path,arrow);} } });
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseClick(e);
            Point pt = new Point(this.Left + this.Owner.Left, this.Top + this.Owner.Top + this.Owner.TitleHeight);
            pt = this.Owner.Owner.CanvasToControl(pt);
            pt = this.Owner.Owner.PointToScreen(pt);
            FrmEnumSelect frm = new FrmEnumSelect(this.Enum, pt, this.Width, this.Owner.Owner.CanvasScale);
            var v = frm.ShowDialog();
            if (v != System.Windows.Forms.DialogResult.OK) return;
            this.Enum = frm.Enum;
            this.OnValueChanged(new EventArgs());
        }
    }
}
