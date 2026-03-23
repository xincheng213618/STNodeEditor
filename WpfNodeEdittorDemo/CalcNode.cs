using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ST.Library.UI.NodeEditor;
using System.Drawing;
using SkiaSharp;

namespace WinNodeEditorDemo
{
    /// <summary>
    /// 此节点仅演示UI自定义以及控件 并不包含功能
    /// </summary>
    [STNode("/", "DebugST", "2212233137@qq.com", "st233.com", "此节点仅演示UI自定义以及控件,并不包含功能.")]
    public class CalcNode : STNode
    {
        private StringFormat m_f;

        protected override void OnCreate() {
            base.OnCreate();
            m_sf = new StringFormat();
            m_sf.LineAlignment = StringAlignment.Center;
            this.Title = "Calculator";
            this.AutoSize = false;          //注意需要先设置AutoSize=false 才能够进行大小设置
            this.Size = new Size(218, 308);

            var ctrl = new STNodeControl();
            ctrl.Text = "";                 //此控件为显示屏幕
            ctrl.Location = new Point(13, 31);
            ctrl.Size = new Size(190, 50);
            this.Controls.Add(ctrl);

            ctrl.Paint += (s, e) => {
                m_sf.Alignment = StringAlignment.Far;
                STNodeControl c = s as STNodeControl;
                SkiaDrawingHelper.RenderToCanvas(e.DrawingTools.Canvas, canvas => { using (var text = new SKPaint { Color = SKColors.White, TextSize = Math.Max(10f, ctrl.Font.Size), IsAntialias = true }) { var fm = text.FontMetrics; float y = (c.Height - (fm.Descent - fm.Ascent)) / 2 - fm.Ascent; float w = text.MeasureText("0"); canvas.DrawText("0", c.Width - w - 2, y, text); } });
            };

            string[] strs = { //按钮文本
                                "MC", "MR", "MS", "M+",
                                "M-", "←",  "CE", "C", "+", "√", 
                                "7",  "8",  "9",  "/", "%",
                                "4",  "5",  "6",  "*", "1/x",
                                "1",  "2",  "3",  "-", "=",
                                "0",  " ",  ".",  "+" };
            Point p = new Point(13, 86);
            for (int i = 0; i < strs.Length; i++) {
                if (strs[i] == " ") continue;
                ctrl = new STNodeControl();
                ctrl.Text = strs[i];
                ctrl.Size = new Size(34, 27);
                ctrl.Left = 13 + (i % 5) * 39;
                ctrl.Top = 86 + (i / 5) * 32;
                if (ctrl.Text == "=") ctrl.Height = 59;
                if (ctrl.Text == "0") ctrl.Width = 73;
                this.Controls.Add(ctrl);
                if (i == 8) ctrl.Paint += (s, e) => {
                    m_sf.Alignment = StringAlignment.Center;
                    STNodeControl c = s as STNodeControl;
                    SkiaDrawingHelper.RenderToCanvas(e.DrawingTools.Canvas, canvas => { using (var text = new SKPaint { Color = SKColors.White, TextSize = Math.Max(10f, ctrl.Font.Size), IsAntialias = true }) { var fm = text.FontMetrics; float y = (c.Height - (fm.Descent - fm.Ascent)) / 2 - fm.Ascent; float w = text.MeasureText("_"); canvas.DrawText("_", (c.Width - w) / 2, y, text); } });
                };
                ctrl.MouseClick += (s, e) => System.Windows.Forms.MessageBox.Show(((STNodeControl)s).Text);
            }

            this.OutputOptions.Add("Result", typeof(int), false);
        }
    }
}
