using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public class DarkComboBox : ComboBox
    {
        public Color ParentBackColor { get; set; } = TemaGlobal.CorSidebar;
        public Color BorderColor { get; set; } = TemaGlobal.CorBorda;
        public DarkComboBox() { SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true); DrawMode = DrawMode.OwnerDrawFixed; DropDownStyle = ComboBoxStyle.DropDownList; FlatStyle = FlatStyle.Flat; Font = new Font("Segoe UI", 11); ItemHeight = 26; IntegralHeight = false; }
        protected override void OnPaint(PaintEventArgs e) { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var b = new SolidBrush(ParentBackColor)) e.Graphics.FillRectangle(b, ClientRectangle); string t = SelectedItem?.ToString() ?? Text; if (Items.Count > 0 && SelectedIndex == -1 && !string.IsNullOrEmpty(Text)) t = Text; var r = new Rectangle(3, 1, Width - 20, Height - 2); TextRenderer.DrawText(e.Graphics, t, Font, r, TemaGlobal.CorTexto, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine); int x = Width - 15, y = (Height - 6) / 2; Point[] p = { new Point(x, y), new Point(x + 8, y), new Point(x + 4, y + 5) }; using (var b = new SolidBrush(Color.Gray)) e.Graphics.FillPolygon(b, p); }
        protected override void OnDrawItem(DrawItemEventArgs e) { if (e.Index < 0) return; var c = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? BorderColor : ParentBackColor; using (var b = new SolidBrush(c)) e.Graphics.FillRectangle(b, e.Bounds); using (var b = new SolidBrush(TemaGlobal.CorTexto)) e.Graphics.DrawString(Items[e.Index].ToString(), Font, b, new Point(e.Bounds.X + 2, e.Bounds.Y + 4)); }
    }
}