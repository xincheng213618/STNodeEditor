using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using ST.Library.UI.NodeEditor;
using SkiaSharp;

namespace WinNodeEditorDemo
{
    public class ToolStripRendererEx : ToolStripRenderer
    {
        private SolidBrush m_brush = new SolidBrush(Color.FromArgb(255, 52, 86, 141));
        private StringFormat m_sf = new StringFormat();

        public ToolStripRendererEx() {
            m_sf.LineAlignment = StringAlignment.Center;
        }

        protected override void InitializeItem(ToolStripItem item) {
            base.InitializeItem(item);
            item.AutoSize = false;
            item.Size = new Size(item.Width, 30);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
            base.OnRenderToolStripBackground(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
            base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            e.TextColor = e.Item.Selected ? Color.White : Color.LightGray;
            e.TextRectangle = new Rectangle(e.TextRectangle.Left, e.TextRectangle.Top, e.TextRectangle.Width, 30);
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            e.ArrowColor = e.Item.Selected ? Color.White : Color.LightGray;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
            base.OnRenderSeparator(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            base.OnRenderMenuItemBackground(e);
        }

    }
}
