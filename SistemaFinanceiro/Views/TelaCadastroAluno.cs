using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace SistemaFinanceiro.Views
{
    public partial class TelaCadastroAluno : UserControl
    {
        private Color CorFundo;
        private Color CorSidebar;
        private Color CorTexto;
        private Color CorLabel;
        private Color CorBorda;
        private Color CorSalvar;
        private Color CorSalvarSuave;
        private Color CorCancelar;
        private Color CorCancelarSuave;

        public event EventHandler VoltarParaLista;

        private int? _idEdicao = null;
        private TextBox txtNome, txtEmail, txtValorMensalidade, txtDiaVencimento;
        private MaskedTextBox mskCpf, mskCpfPais, txtTelefone;
        private DateTimePicker dtpDataNasc;
        private ComboBox cmbCategoria;
        private Label lblTitulo;

        public TelaCadastroAluno()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            AplicarTema();

            if (!this.DesignMode) { ConfigurarLayout(); CarregarCategorias(); }
        }

        public TelaCadastroAluno(int id) : this()
        {
            _idEdicao = id;
            this.Load += (s, e) => CarregarDados(id);
        }

        public void AplicarTema()
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
                // Paleta Modo Claro
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

            // Aplica as cores ao Container Principal
            this.BackColor = CorFundo;

            // Atualiza os controles já criados
            if (this.Controls.Count > 0)
            {
                AtualizarCoresRecursivo(this);
                this.Invalidate(true);
            }
        }

        private void AtualizarCoresRecursivo(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox || c is MaskedTextBox)
                {
                    c.BackColor = CorSidebar;
                    c.ForeColor = CorTexto;
                }
                else if (c is ComboBox)
                {
                    c.BackColor = CorSidebar;
                    c.ForeColor = CorTexto;
                }
                else if (c is DateTimePicker dtp)
                {
                    dtp.CalendarForeColor = CorTexto;
                    dtp.CalendarMonthBackground = CorSidebar;
                }
                else if (c is Label lbl)
                {
                    // Diferencia o Título dos labels comuns
                    if (lbl == lblTitulo) lbl.ForeColor = CorTexto;
                    else lbl.ForeColor = CorLabel;
                }
                // Se for os Paineis "Wrapper", atualiza a cor
                else if (c is Panel && c.Tag != null && c.Tag.ToString() == "WRAPPER")
                {
                    c.BackColor = CorSidebar;
                    c.Invalidate();
                }

                if (c.HasChildren) AtualizarCoresRecursivo(c);
            }
        }

        private void ConfigurarLayout()
        {
            this.Controls.Clear();
            Panel mainPanel = new Panel { AutoSize = true, Anchor = AnchorStyles.None, BackColor = Color.Transparent };

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80 };
            lblTitulo = new Label { Text = "Novo Cadastro", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = CorTexto, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
            Label lblFechar = new Label { Text = "✕", AutoSize = true, ForeColor = Color.Gray, BackColor = Color.Transparent, Font = new Font("Segoe UI", 18, FontStyle.Regular), Cursor = Cursors.Hand, Location = new Point(pnlHeader.Width - 50, 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            lblFechar.MouseEnter += (s, e) => lblFechar.ForeColor = CorLabel;
            lblFechar.MouseLeave += (s, e) => lblFechar.ForeColor = Color.Gray;
            lblFechar.Click += (s, e) => VoltarParaLista?.Invoke(this, EventArgs.Empty);
            pnlHeader.Controls.Add(lblFechar); pnlHeader.Controls.Add(lblTitulo); lblFechar.BringToFront();

            // Grid
            TableLayoutPanel formGrid = new TableLayoutPanel();
            formGrid.AutoSize = true; formGrid.BackColor = Color.Transparent;
            formGrid.Padding = new Padding(0, 10, 0, 20);
            formGrid.ColumnCount = 2;
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            formGrid.Dock = DockStyle.Top;

            // Campos
            txtTelefone = CriarInputMask("(00) 00000-0000");
            mskCpf = CriarInputMask(@"000\.000\.000-00");
            mskCpfPais = CriarInputMask(@"000\.000\.000-00");

            txtValorMensalidade = CriarInputTexto();
            txtValorMensalidade.Text = "0,00";
            txtValorMensalidade.TextAlign = HorizontalAlignment.Right;
            txtValorMensalidade.TextChanged += TxtValor_TextChanged;
            txtValorMensalidade.KeyPress += TxtValor_KeyPress; // Bloqueia letras
            txtValorMensalidade.ShortcutsEnabled = false;
            txtValorMensalidade.Enter += (s, e) => this.BeginInvoke((MethodInvoker)(() => txtValorMensalidade.Select(txtValorMensalidade.Text.Length, 0)));

            txtDiaVencimento = CriarInputTexto();
            txtDiaVencimento.MaxLength = 2;
            txtDiaVencimento.Text = "10";
            txtDiaVencimento.TextAlign = HorizontalAlignment.Center;
            txtDiaVencimento.KeyPress += TxtNumerico_KeyPress;
            txtDiaVencimento.Validating += TxtDia_Validating;

            // Grid
            AdicionarCampo(formGrid, "Nome Completo", txtNome = CriarInputTexto(), 0, 2);
            AdicionarCampo(formGrid, "CPF do Aluno", mskCpf, 1, 1, 0);
            AdicionarCampo(formGrid, "CPF do Responsável", mskCpfPais, 1, 1, 1);
            AdicionarCampo(formGrid, "E-mail dos Pais", txtEmail = CriarInputTexto(), 2, 1, 0);
            AdicionarCampo(formGrid, "Telefone dos Pais", txtTelefone, 2, 1, 1);

            dtpDataNasc = new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill, Height = 38, Font = new Font("Segoe UI", 12), CalendarMonthBackground = CorSidebar, CalendarForeColor = CorTexto };
            AdicionarCampoCustom(formGrid, "Data de Nascimento", dtpDataNasc, 3, 0);

            cmbCategoria = new ComboBox { Dock = DockStyle.Fill, Height = 38, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = CorSidebar, ForeColor = CorTexto, FlatStyle = FlatStyle.Flat };
            AdicionarCampoCustom(formGrid, "Categoria", cmbCategoria, 3, 1);

            AdicionarCampo(formGrid, "Valor Mensalidade (R$)", txtValorMensalidade, 4, 1, 0);
            AdicionarCampo(formGrid, "Dia Vencimento", txtDiaVencimento, 4, 1, 1);

            // Botões
            Panel pnlBotoes = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 40, 0, 0) };
            Button btnCancelar = new Button { Cursor = Cursors.Hand, Width = 280, Height = 50, Dock = DockStyle.Left };
            ConfigurarBotao(btnCancelar, () => CorCancelar, () => CorCancelarSuave, "✕", "CANCELAR");
            btnCancelar.Click += (s, e) => VoltarParaLista?.Invoke(this, EventArgs.Empty);

            Button btnSalvar = new Button { Cursor = Cursors.Hand, Width = 280, Height = 50, Dock = DockStyle.Right };
            ConfigurarBotao(btnSalvar, () => CorSalvar, () => CorSalvarSuave, "✓", "SALVAR");
            btnSalvar.Click += BtnSalvar_Click;

            pnlBotoes.Controls.Add(btnCancelar); pnlBotoes.Controls.Add(btnSalvar);
            mainPanel.Controls.Add(pnlBotoes); mainPanel.Controls.Add(formGrid); mainPanel.Controls.Add(pnlHeader);
            this.Controls.Add(mainPanel);

            this.Resize += (s, e) => {
                int w = Math.Min(this.Width - 80, 1000);
                mainPanel.Width = w > 650 ? w : 650;
                mainPanel.Left = (this.Width - mainPanel.Width) / 2;
                mainPanel.Top = (this.Height - mainPanel.Height) / 2;
            };
        }

        private void TxtValor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void TxtValor_TextChanged(object sender, EventArgs e)
        {
            txtValorMensalidade.TextChanged -= TxtValor_TextChanged;
            try
            {
                string originalText = txtValorMensalidade.Text;
                string digitsOnly = Regex.Replace(originalText, "[^0-9]", "");
                if (string.IsNullOrEmpty(digitsOnly)) digitsOnly = "0";
                int cursorPositionFromEnd = originalText.Length - txtValorMensalidade.SelectionStart;
                decimal valor = decimal.Parse(digitsOnly) / 100m;
                txtValorMensalidade.Text = valor.ToString("N2");
                int newCursorPosition = txtValorMensalidade.Text.Length - cursorPositionFromEnd;
                if (newCursorPosition < 0) newCursorPosition = 0;
                if (newCursorPosition > txtValorMensalidade.Text.Length) newCursorPosition = txtValorMensalidade.Text.Length;
                txtValorMensalidade.SelectionStart = newCursorPosition;
            }
            catch { }
            txtValorMensalidade.TextChanged += TxtValor_TextChanged;
        }

        private void TxtNumerico_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void TxtDia_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDiaVencimento.Text)) return;
            if (int.TryParse(txtDiaVencimento.Text, out int dia))
            {
                if (dia < 1) dia = 1; else if (dia > 30) dia = 30;
                txtDiaVencimento.Text = dia.ToString("D2");
            }
            else txtDiaVencimento.Text = "10";
        }

        private void CarregarDados(int id)
        {
            try
            {
                var repo = new EntidadeRepository();
                var aluno = repo.ObterPorId(id);
                if (aluno != null)
                {
                    lblTitulo.Text = "Editar Aluno";
                    txtNome.Text = aluno.Nome;
                    txtEmail.Text = aluno.EmailPais;
                    mskCpf.Text = ""; mskCpfPais.Text = ""; txtTelefone.Text = "";
                    if (!string.IsNullOrEmpty(aluno.CpfAtleta)) mskCpf.Text = LimparNumeros(aluno.CpfAtleta);
                    if (!string.IsNullOrEmpty(aluno.CpfPais)) mskCpfPais.Text = LimparNumeros(aluno.CpfPais);
                    string tel = LimparNumeros(aluno.TelefonePais);
                    if (!string.IsNullOrEmpty(tel)) { txtTelefone.Mask = (tel.Length > 10) ? "(00) 00000-0000" : "(00) 0000-0000"; txtTelefone.Text = tel; }
                    dtpDataNasc.Value = aluno.DataNascimento;
                    cmbCategoria.SelectedValue = aluno.Categoria_id;
                    txtValorMensalidade.Text = aluno.ValorMensalidade.ToString("N2");
                    txtDiaVencimento.Text = aluno.DiaVencimento.ToString("D2");
                    this.BeginInvoke((MethodInvoker)delegate {
                        mskCpf.SelectionStart = 0; mskCpf.SelectionLength = 0;
                        mskCpfPais.SelectionStart = 0; mskCpfPais.SelectionLength = 0;
                        txtTelefone.SelectionStart = 0; txtTelefone.SelectionLength = 0;
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro ao carregar dados: " + ex.Message); }
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text)) { MessageBox.Show("Preencha o nome!"); return; }
            decimal valorFinal = 0;
            if (!decimal.TryParse(txtValorMensalidade.Text, out valorFinal)) valorFinal = 0;
            int.TryParse(txtDiaVencimento.Text, out int diaVencimento);
            if (diaVencimento < 1) diaVencimento = 1;
            var aluno = new Entidade
            {
                Id = _idEdicao ?? 0,
                Nome = txtNome.Text.Trim(),
                CpfAtleta = LimparNumeros(mskCpf.Text),
                CpfPais = LimparNumeros(mskCpfPais.Text),
                EmailPais = txtEmail.Text,
                TelefonePais = LimparNumeros(txtTelefone.Text),
                TipoVinculo = "Aluno",
                Status = "Ativo",
                DataNascimento = dtpDataNasc.Value,
                Categoria_id = cmbCategoria.SelectedValue != null ? (int)cmbCategoria.SelectedValue : 1,
                ValorMensalidade = valorFinal,
                DiaVencimento = diaVencimento
            };
            try
            {
                var repo = new EntidadeRepository();
                if (_idEdicao.HasValue) { repo.Atualizar(aluno); MessageBox.Show("Atualizado!"); }
                else { repo.Inserir(aluno); MessageBox.Show("Cadastrado!"); }
                VoltarParaLista?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) { MessageBox.Show("Erro ao salvar: " + ex.Message); }
        }

        private string LimparNumeros(string texto) => string.IsNullOrEmpty(texto) ? "" : Regex.Replace(texto, "[^0-9]", "");

        private MaskedTextBox CriarInputMask(string m)
        {
            return new MaskedTextBox
            {
                BorderStyle = BorderStyle.None,
                Mask = m,
                TextMaskFormat = MaskFormat.ExcludePromptAndLiterals,
                CutCopyMaskFormat = MaskFormat.ExcludePromptAndLiterals,
                ResetOnSpace = false
            };
        }
        private TextBox CriarInputTexto() => new TextBox { BorderStyle = BorderStyle.None };

        private void AdicionarCampo(TableLayoutPanel grid, string label, Control input, int row, int colspan = 1, int col = 0)
        {
            Panel container = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 20, 20), Height = 80 };
            Label lbl = new Label { Text = label, Dock = DockStyle.Top, ForeColor = CorLabel, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 25, TextAlign = ContentAlignment.BottomLeft };
            Panel wrapper = new Panel { Dock = DockStyle.Bottom, Height = 45, BackColor = CorSidebar, Padding = new Padding(10, 5, 10, 5), Tag = "WRAPPER" }; // Tag para identificar na troca de tema
            wrapper.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, wrapper.ClientRectangle, CorBorda, ButtonBorderStyle.Solid);
            input.BackColor = CorSidebar; input.ForeColor = CorTexto; input.Font = new Font("Segoe UI", 12F);
            if (input is TextBox t) { t.BorderStyle = BorderStyle.None; t.Multiline = true; t.WordWrap = false; t.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) e.SuppressKeyPress = true; }; }
            if (input is MaskedTextBox m) { m.BorderStyle = BorderStyle.None; m.Click += (s, e) => { }; }
            input.Dock = DockStyle.None; input.Anchor = AnchorStyles.Left | AnchorStyles.Right; input.Height = 30; input.Width = wrapper.Width - 25;
            input.Location = new Point(10, (wrapper.Height - input.Height) / 2); wrapper.Resize += (s, e) => input.Width = wrapper.Width - 25;
            wrapper.Controls.Add(input); container.Controls.Add(wrapper); container.Controls.Add(lbl);
            grid.Controls.Add(container, col, row); if (colspan > 1) grid.SetColumnSpan(container, colspan);
        }

        private void AdicionarCampoCustom(TableLayoutPanel grid, string label, Control input, int row, int col)
        {
            Panel container = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 20, 20), Height = 80 };
            Label lbl = new Label { Text = label, Dock = DockStyle.Top, ForeColor = CorLabel, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 25, TextAlign = ContentAlignment.BottomLeft };
            Panel w = new Panel { Dock = DockStyle.Bottom, Height = 45, BackColor = CorSidebar, Tag = "WRAPPER" };
            w.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, w.ClientRectangle, CorBorda, ButtonBorderStyle.Solid);
            input.Dock = DockStyle.None; input.Anchor = AnchorStyles.Left | AnchorStyles.Right; input.Width = w.Width - 10; input.Location = new Point(5, (w.Height - input.Height) / 2);
            w.Controls.Add(input); container.Controls.Add(w); container.Controls.Add(lbl); grid.Controls.Add(container, col, row);
        }

        private void ConfigurarBotao(Button btn, Func<Color> getCorSolida, Func<Color> getCorSuave, string icone, string texto)
        {
            btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 0; btn.FlatAppearance.MouseDownBackColor = Color.Transparent; btn.FlatAppearance.MouseOverBackColor = Color.Transparent; btn.BackColor = Color.Transparent; btn.Text = "";
            btn.Paint += (s, e) => {
                Color cSolida = getCorSolida();
                Color cSuave = getCorSuave();
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; Rectangle rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (Brush brushSuave = new SolidBrush(cSuave)) e.Graphics.FillRectangle(brushSuave, rect);
                using (Pen pen = new Pen(cSolida, 1)) e.Graphics.DrawRectangle(pen, rect);
                string textoCompleto = $"{icone}  {texto}"; TextRenderer.DrawText(e.Graphics, textoCompleto, new Font("Segoe UI", 11, FontStyle.Bold), rect, cSolida, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
        }
        private void CarregarCategorias() { try { var r = new EntidadeRepository(); cmbCategoria.DataSource = r.ObterCategorias(); cmbCategoria.DisplayMember = "Descricao"; cmbCategoria.ValueMember = "Id"; } catch { } }
    }
}