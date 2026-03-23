using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ST.Library.UI.NodeEditor;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using SkiaSharp;

namespace WinNodeEditorDemo.ImageNode
{

    [STNode("Image", "Crystal_lz", "2212233137@qq.com", "st233.com", "Image Node")]
    public class ImageInputNode : ImageBaseNode
    {
        private string _FileName;//默认的DescriptorType不支持文件路径的选择 所以需要扩展
        [STNodeProperty("InputImage", "Click to select a image", DescriptorType = typeof(OpenFileDescriptor))]
        public string FileName {
            get { return _FileName; }
            set {
                Image img = null;                       //当文件名被设置时 加载图片并 向输出节点输出
                if (!string.IsNullOrEmpty(value)) {
                    img = Image.FromFile(value);
                }
                if (m_img_draw != null) m_img_draw.Dispose();
                m_img_draw = img;
                _FileName = value;
                m_op_img_out.TransferData(m_img_draw, true);
                this.Invalidate();
            }
        }

        protected override void OnCreate() {
            base.OnCreate();
            this.Title = "ImageInput";
        }

        protected override void OnDrawBody(DrawingTools dt) {
            base.OnDrawBody(dt);
            Rectangle rect = new Rectangle(this.Left + 10, this.Top + 30, 140, 80);
            SkiaDrawingHelper.RenderToCanvas(dt.Canvas, canvas => { using (var bg = new SKPaint { Color = SKColors.Gray, Style = SKPaintStyle.Fill, IsAntialias = true }) { canvas.DrawRect(rect.Left, rect.Top, rect.Width, rect.Height, bg); if (m_img_draw != null) { using (var bmp = SkiaDrawingHelper.ToSKBitmap(m_img_draw)) { if (bmp != null) canvas.DrawBitmap(bmp, new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom)); } } } });
        }
    }
    /// <summary>
    /// 对默认Descriptor进行扩展 使得支持文件路径选择
    /// </summary>
    public class OpenFileDescriptor : STNodePropertyDescriptor
    {
        private Rectangle m_rect_open;  //需要绘制"打开"按钮的区域
        private StringFormat m_sf;

        public OpenFileDescriptor() {
            m_sf = new StringFormat();
            m_sf.Alignment = StringAlignment.Center;
            m_sf.LineAlignment = StringAlignment.Center;
        }

        protected override void OnSetItemLocation() {   //当在STNodePropertyGrid上确定此属性需要显示的区域时候
            base.OnSetItemLocation();                   //计算出"打开"按钮需要绘制的区域
            m_rect_open = new Rectangle(
                this.RectangleR.Right - 20,
                this.RectangleR.Top,
                20, 
                this.RectangleR.Height);
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e) {
            if (m_rect_open.Contains(e.Location)) {     //点击在"打开"区域 则弹出文件选择框
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "*.jpg|*.jpg|*.png|*.png";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                this.SetValue(ofd.FileName);
            } else base.OnMouseClick(e);                //否则默认处理方式 弹出文本输入框
        }

        protected override void OnDrawValueRectangle(DrawingTools dt) {
            base.OnDrawValueRectangle(dt);              //在STNodePropertyGrid绘制此属性区域时候将"打开"按钮绘制上去
            SkiaDrawingHelper.RenderToCanvas(dt.Canvas, canvas => { using (var bg = new SKPaint { Color = SKColors.Gray, Style = SKPaintStyle.Fill, IsAntialias = true }) using (var tx = new SKPaint { Color = SKColors.White, TextSize = Math.Max(10f, this.Control.Font.Size), IsAntialias = true }) { canvas.DrawRect(m_rect_open.Left, m_rect_open.Top, m_rect_open.Width, m_rect_open.Height, bg); var fm = tx.FontMetrics; float y = m_rect_open.Top + (m_rect_open.Height - (fm.Descent - fm.Ascent)) / 2 - fm.Ascent; float x = m_rect_open.Left + (m_rect_open.Width - tx.MeasureText("+")) / 2; canvas.DrawText("+", x, y, tx);} });
        }
    }
}
