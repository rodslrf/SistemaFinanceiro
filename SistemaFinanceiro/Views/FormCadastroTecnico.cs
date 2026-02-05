using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public partial class FormCadastroTecnico : Form
    {
        private Color CorFundo, CorSidebar, CorTexto, CorLabel, CorBorda;
        private Color CorSalvar, CorSalvarSuave, CorCancelar, CorCancelarSuave;

        public TextBox txtNome, txtObservacao;
        private Label lblTitulo;

        public FormCadastroTecnico()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 400);
            this.Padding = new Padding(2);

            AplicarTema();
            ConfigurarLayout();
        }

        private void AplicarTema()
        {
            bool modoEscuro = TemaGlobal.ModoEscuro;
            if (modoEscuro)
            {
                CorFundo = ColorTranslator.FromHtml("#0d1117");
                CorSidebar = ColorTranslator.FromHtml("#161b22");
                CorTexto = ColorTranslator.FromHtml("#c9d1d9");
                CorLabel = Color.White;
                CorBorda = ColorTranslator.FromHtml("#30363d");
                CorSalvar = ColorTranslator.FromHtml("#238636");
                CorSalvarSuave = Color.FromArgb(40, 35, 134, 54);
                CorCancelar = ColorTranslator.FromHtml("#ff7b72");
                CorCancelarSuave = Color.FromArgb(40, 255, 123, 114);
            }
            else
            {
                CorFundo = Color.White;
                CorSidebar = ColorTranslator.FromHtml("#f6f8fa");
                CorTexto = ColorTranslator.FromHtml("#24292f");
                CorLabel = Color.DimGray;
                CorBorda = ColorTranslator.FromHtml("#d0d7de");
                CorSalvar = ColorTranslator.FromHtml("#1f883d");
                CorSalvarSuave = Color.FromArgb(40, 31, 136, 61);
                CorCancelar = ColorTranslator.FromHtml("#cf222e");
                CorCancelarSuave = Color.FromArgb(40, 207, 34, 46);
            }
            this.BackColor = CorBorda;
        }

        private void ConfigurarLayout()
        {
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = CorFundo };

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = CorSidebar };
            lblTitulo = new Label { Text = "Novo Técnico", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = CorTexto, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pnlHeader.Controls.Add(lblTitulo);

            // Container dos Campos
            Panel pnlCampos = new Panel { Dock = DockStyle.Top, Height = 220, Padding = new Padding(20) };

            // --- ALTERAÇÃO AQUI: Adicionamos a Observação PRIMEIRO para ela ficar EMBAIXO ---
            // Campo Observação
            txtObservacao = CriarInputTexto();
            txtObservacao.Multiline = true;
            txtObservacao.Height = 60;
            AdicionarCampo(pnlCampos, "Observação (Opcional)", txtObservacao, 85);

            // Campo Nome (Adicionado POR ÚLTIMO para ficar no TOPO devido ao BringToFront)
            txtNome = CriarInputTexto();
            AdicionarCampo(pnlCampos, "Nome do Técnico *", txtNome, 0);
            // -------------------------------------------------------------------------------

            // Botões
            Panel pnlBotoes = new Panel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(20, 10, 20, 10), BackColor = CorFundo };

            Button btnCancelar = new Button { Dock = DockStyle.Left, Width = 150, Cursor = Cursors.Hand };
            ConfigurarBotao(btnCancelar, () => CorCancelar, () => CorCancelarSuave, "✕", "CANCELAR");
            btnCancelar.Click += (s, e) => this.Close();

            Button btnSalvar = new Button { Dock = DockStyle.Right, Width = 150, Cursor = Cursors.Hand };
            ConfigurarBotao(btnSalvar, () => CorSalvar, () => CorSalvarSuave, "✓", "SALVAR");
            btnSalvar.Click += BtnSalvar_Click;

            pnlBotoes.Controls.Add(btnCancelar);
            pnlBotoes.Controls.Add(btnSalvar);

            mainPanel.Controls.Add(pnlCampos);
            mainPanel.Controls.Add(pnlBotoes);
            mainPanel.Controls.Add(pnlHeader);
            this.Controls.Add(mainPanel);
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("O nome é obrigatório!");
                return;
            }

            try
            {
                var repo = new TecnicoRepository();
                repo.Inserir(new Tecnico
                {
                    Nome = txtNome.Text.Trim(),
                    Observacao = txtObservacao.Text.Trim(),
                    Status = "Ativo"
                });

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar técnico: " + ex.Message);
            }
        }

        private TextBox CriarInputTexto() => new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11F), BackColor = CorSidebar, ForeColor = CorTexto };

        private void AdicionarCampo(Panel parent, string labelText, Control input, int topY)
        {
            Panel container = new Panel { Height = labelText.Contains("Observação") ? 100 : 75, Dock = DockStyle.Top };

            // Este comando joga o painel para o topo da pilha visual. 
            // Por isso adicionamos o Nome por último na função ConfigurarLayout.
            container.BringToFront();

            Label lbl = new Label { Text = labelText, Dock = DockStyle.Top, ForeColor = CorLabel, Font = new Font("Segoe UI", 9, FontStyle.Bold), Height = 25 };

            Panel wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CorSidebar, Padding = new Padding(10, 5, 10, 5) };
            wrapper.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, wrapper.ClientRectangle, CorBorda, ButtonBorderStyle.Solid);

            input.Width = 400;
            input.Dock = DockStyle.Fill;

            wrapper.Controls.Add(input);
            container.Controls.Add(wrapper);
            container.Controls.Add(lbl);

            parent.Controls.Add(container);
            // container.Location = new Point(20, topY); // Location é ignorado devido ao DockStyle.Top
        }

        private void ConfigurarBotao(Button btn, Func<Color> getCorSolida, Func<Color> getCorSuave, string icone, string texto)
        {
            btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) => {
                Color cSolida = getCorSolida();
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (Brush brush = new SolidBrush(getCorSuave())) e.Graphics.FillRectangle(brush, rect);
                using (Pen pen = new Pen(cSolida, 1)) e.Graphics.DrawRectangle(pen, rect);
                TextRenderer.DrawText(e.Graphics, $"{icone}  {texto}", new Font("Segoe UI", 10, FontStyle.Bold), rect, cSolida, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
        }
    }
}