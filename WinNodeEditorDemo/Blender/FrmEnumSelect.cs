using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ST.Library.UI.NodeEditor;
using SkiaSharp;

namespace WinNodeEditorDemo.Blender
{
    public class FrmEnumSelect : Form
    {
        private Point m_pt;
        private int m_nWidth;
        private float m_scale;
        private List<object> m_lst = new List<object>();
        public Enum Enum { get; set; }
        private bool m_bClosed;

        public FrmEnumSelect(Enum e, Point pt, int nWidth, float scale) {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            foreach (var v in Enum.GetValues(e.GetType())) m_lst.Add(v);
            this.Enum = e;
            m_pt = pt;
            m_scale = scale;
            m_nWidth = nWidth;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(255, 34, 34, 34);
            this.FormBorderStyle = FormBorderStyle.None;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            this.Location = m_pt;
            this.Width = (int)(m_nWidth * m_scale);
            this.Height = (int)(m_lst.Count * 20 * m_scale);
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);
            int nIndex = e.Y / (int)(20 * m_scale);
            if (nIndex >= 0 && nIndex < m_lst.Count) this.Enum = (Enum)m_lst[nIndex];
            this.DialogResult = DialogResult.OK;
            m_bClosed = true;
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            if (m_bClosed) return;
            this.Close();
        }
    }
}
