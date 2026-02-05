using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public partial class TelaConfiguracoes : UserControl
    {
        public event EventHandler<bool> AoMudarTema;

        private Panel _cardTema;
        private Label _lblTitulo, _lblDesc, _lblStatus;
        private ToggleSwitch _btnSwitch;

        public TelaConfiguracoes()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            if (!this.DesignMode)
            {
                ConstruirInterfaceVisual();
                AplicarCoresAtuais();
            }
        }

        public void AplicarCoresAtuais()
        {
            this.BackColor = TemaGlobal.CorFundo;
            if (_lblTitulo != null) _lblTitulo.ForeColor = TemaGlobal.CorTexto;
            if (_lblDesc != null) _lblDesc.ForeColor = TemaGlobal.CorTexto;

            if (_cardTema != null)
            {
                _cardTema.BackColor = TemaGlobal.CorSidebar;
                _cardTema.Invalidate();
            }
            if (_btnSwitch != null) _btnSwitch.Invalidate();
        }

        private void ConstruirInterfaceVisual()
        {
            this.Controls.Clear();

            _lblTitulo = new Label { Text = "Configurações", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(40, 30) };

            _cardTema = new Panel { Size = new Size(500, 100), Location = new Point(40, 100) };
            _cardTema.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(TemaGlobal.CorBorda, 1))
                {
                    Rectangle r = new Rectangle(0, 0, _cardTema.Width - 1, _cardTema.Height - 1);
                    e.Graphics.DrawRectangle(p, r);
                }
            };

            _lblDesc = new Label { Text = "Modo Escuro", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 25), AutoSize = true };
            _lblStatus = new Label { Text = TemaGlobal.ModoEscuro ? "Ativado" : "Desativado", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(20, 55), AutoSize = true };

            _btnSwitch = new ToggleSwitch { Location = new Point(420, 35), Size = new Size(50, 26), Checked = TemaGlobal.ModoEscuro };
            _btnSwitch.CheckedChanged += (s, e) =>
            {
                TemaGlobal.ModoEscuro = _btnSwitch.Checked;
                _lblStatus.Text = TemaGlobal.ModoEscuro ? "Ativado" : "Desativado";
                AoMudarTema?.Invoke(this, TemaGlobal.ModoEscuro);
                AplicarCoresAtuais();
            };

            _cardTema.Controls.Add(_lblDesc);
            _cardTema.Controls.Add(_lblStatus);
            _cardTema.Controls.Add(_btnSwitch);

            this.Controls.Add(_cardTema);
            this.Controls.Add(_lblTitulo);

            DarkBox.AplicarMarcaDagua(this, Properties.Resources.diviminas);
        }

    }

    public class ToggleSwitch : CheckBox
    {
        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Cursor = Cursors.Hand;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.Parent.BackColor);

            Color corFundo = this.Checked ? ColorTranslator.FromHtml("#238636") : ColorTranslator.FromHtml("#30363d");
            int d = this.Height;

            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, d, d, 90, 180);
            path.AddArc(this.Width - d, 0, d, d, 270, 180);
            path.CloseFigure();

            using (var b = new SolidBrush(corFundo)) e.Graphics.FillPath(b, path);

            int r = this.Height - 6;
            int x = this.Checked ? (this.Width - r - 3) : 3;
            using (var b = new SolidBrush(Color.White)) e.Graphics.FillEllipse(b, x, 3, r, r);
        }
    }
}