using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    // --- COMBOBOX ESCURO ---
    public class DarkComboBox : ComboBox
    {
        public Color ParentBackColor { get; set; } = TemaGlobal.CorSidebar;
        public Color BorderColor { get; set; } = TemaGlobal.CorBorda;

        public DarkComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            Font = new Font("Segoe UI", 11);
            ItemHeight = 26;
            IntegralHeight = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(ParentBackColor)) e.Graphics.FillRectangle(b, ClientRectangle);

            string t = SelectedItem?.ToString() ?? Text;
            if (Items.Count > 0 && SelectedIndex == -1 && !string.IsNullOrEmpty(Text)) t = Text;

            var r = new Rectangle(3, 1, Width - 20, Height - 2);
            TextRenderer.DrawText(e.Graphics, t, Font, r, TemaGlobal.CorTexto, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

            int x = Width - 15, y = (Height - 6) / 2;
            Point[] p = { new Point(x, y), new Point(x + 8, y), new Point(x + 4, y + 5) };
            using (var b = new SolidBrush(Color.Gray)) e.Graphics.FillPolygon(b, p);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var c = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? BorderColor : ParentBackColor;
            using (var b = new SolidBrush(c)) e.Graphics.FillRectangle(b, e.Bounds);
            using (var b = new SolidBrush(TemaGlobal.CorTexto)) e.Graphics.DrawString(Items[e.Index].ToString(), Font, b, new Point(e.Bounds.X + 2, e.Bounds.Y + 4));
        }
    }

    // --- CAIXA DE MENSAGEM PADRONIZADA ---
    public static class DarkBox
    {
        public static void Mostrar(string msg, string title = "Aviso")
        {
            var f = BaseForm(400, 200);

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.Transparent };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            var lbl = CriarLabel(msg);

            var btn = Btn("OK", ColorTranslator.FromHtml("#238636"), 100, 35);
            btn.Anchor = AnchorStyles.None; // Centraliza na célula
            btn.Click += (s, e) => f.DialogResult = DialogResult.OK;

            mainLayout.Controls.Add(lbl, 0, 0);
            mainLayout.Controls.Add(btn, 0, 1);

            f.Controls.Add(mainLayout);
            AdicionarFechar(f);
            f.ShowDialog();
        }

        public static bool Confirmar(string msg, out bool dontAsk)
        {
            dontAsk = false;
            var f = BaseForm(420, 230); // Tamanho compacto

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Color.Transparent };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); // Texto
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Checkbox
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); // Botões

            var lbl = CriarLabel(msg);

            var chk = new CheckBox
            {
                Text = "Não perguntar hoje",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Cursor = Cursors.Hand
            };

            // Painel FLUTUANTE para os botões (Isso centraliza o grupo de botões)
            var flowBtns = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0)
            };

            // Botões com tamanho fixo e normal
            var bNo = Btn("CANCELAR", ColorTranslator.FromHtml("#ff7b72"), 110, 40);
            bNo.DialogResult = DialogResult.No;
            bNo.Margin = new Padding(0, 0, 15, 0); // Espaço entre eles

            var bYes = Btn("CONFIRMAR", ColorTranslator.FromHtml("#238636"), 110, 40);
            bYes.DialogResult = DialogResult.Yes;
            bYes.Margin = new Padding(0);

            flowBtns.Controls.Add(bNo);
            flowBtns.Controls.Add(bYes);

            mainLayout.Controls.Add(lbl, 0, 0);
            mainLayout.Controls.Add(chk, 0, 1);
            mainLayout.Controls.Add(flowBtns, 0, 2);

            f.Controls.Add(mainLayout);
            AdicionarFechar(f);

            var res = f.ShowDialog();
            dontAsk = chk.Checked;
            return res == DialogResult.Yes;
        }

        public static bool Confirmar(string msg)
        {
            return Confirmar(msg, out _);
        }

        private static Form BaseForm(int w, int h)
        {
            var f = new Form
            {
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                Size = new Size(w, h),
                BackColor = TemaGlobal.CorSidebar,
                Padding = new Padding(2)
            };
            f.Paint += (s, e) => { using (var p = new Pen(TemaGlobal.CorBorda, 2)) e.Graphics.DrawRectangle(p, 0, 0, f.Width - 1, f.Height - 1); };
            return f;
        }

        private static void AdicionarFechar(Form f)
        {
            var x = new Label
            {
                Text = "✕",
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand,
                Location = new Point(f.Width - 30, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            x.Click += (s, e) => f.DialogResult = DialogResult.Cancel;
            x.MouseEnter += (s, e) => x.ForeColor = Color.White;
            x.MouseLeave += (s, e) => x.ForeColor = Color.Gray;
            f.Controls.Add(x);
            x.BringToFront();
        }

        private static Label CriarLabel(string msg)
        {
            return new Label
            {
                Text = msg,
                ForeColor = TemaGlobal.CorTexto,
                Font = new Font("Segoe UI", 12),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 20, 15, 0)
            };
        }

        public static void AplicarMarcaDagua(Control c, Image img)
        {
            if (c is DataGridView grid)
            {
                grid.DefaultCellStyle.BackColor = Color.Transparent;
            }

            if (c is ScrollableControl sc)
            {
                sc.Scroll += (s, e) => c.Invalidate();
                sc.MouseWheel += (s, e) => c.Invalidate();
            }

            c.Paint += (s, e) =>
            {
                try
                {
                    if (img == null) return;

                    int tamanho = 400;
                    int scrollX = 0, scrollY = 0;

                    if (c is ScrollableControl sc2)
                    {
                        scrollX = -sc2.AutoScrollPosition.X;
                        scrollY = -sc2.AutoScrollPosition.Y;
                    }

                    int x = scrollX + (c.ClientRectangle.Width - tamanho) / 2;
                    int y = scrollY + (c.ClientRectangle.Height - tamanho) / 2;
                    
                    float opacidade = TemaGlobal.ModoEscuro ? 0.03f : 0.07f;

                    var cm = new System.Drawing.Imaging.ColorMatrix { Matrix33 = opacidade };
                    var ia = new System.Drawing.Imaging.ImageAttributes();
                    ia.SetColorMatrix(cm);

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    e.Graphics.DrawImage(img,
                        new Rectangle(x, y, tamanho, tamanho),
                        0, 0, img.Width, img.Height,
                        GraphicsUnit.Pixel, ia);
                }
                catch { }
            };
        }

        private static Button Btn(string txt, Color c, int w, int h)
        {
            var b = new Button
            {
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Text = "",
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
                using (var br = new SolidBrush(c)) e.Graphics.FillRectangle(br, r);
                // Leve borda para destaque
                using (var p = new Pen(ControlPaint.Light(c), 1)) e.Graphics.DrawRectangle(p, r);
                TextRenderer.DrawText(e.Graphics, txt, new Font("Segoe UI", 10, FontStyle.Bold), r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            return b;
        }
    }
}