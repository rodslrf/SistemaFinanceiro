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
        private Color corFundo, corCardFundo, corInputFundo, corTexto, corRotulo, corBorda, corDestaque, corSalvar, corSalvarSuave, corCancelar, corCancelarSuave;
        public event EventHandler VoltarParaLista;
        private int? idEdicao = null;

        private TextBox campoNome, campoEmail1, campoEmail2, campoValorMensalidade, campoDiaVencimento, campoObservacao;
        private MaskedTextBox campoCpfAluno, campoTelefoneAluno, campoCpfPais1, campoTelefonePais1, campoCpfPais2, campoTelefonePais2;
        private DateTimePicker seletorDataNasc;
        private ComboBox comboCategoria, comboTecnico, comboBolsista;
        private Button botaoAdicionarTecnico;
        private Label labelTitulo;
        private FlowLayoutPanel painelRolagem;

        // Variáveis do Aviso
        private Panel _pnlAvisoFinanceiro;
        private Panel _cardFinanceiroPanel;
        private Label _lblAvisoTexto;
        private string _valorOriginal = "";
        private string _diaOriginal = "";
        private bool _carregandoDados = false;

        public TelaCadastroAluno()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();

            AplicarTema();

            if (!this.DesignMode)
            {
                ConfigurarLayout();
                CarregarListas();

                if (idEdicao == null)
                    ConfigurarMascarasPadrao();
            }
        }

        public TelaCadastroAluno(int id) : this()
        {
            idEdicao = id;
            this.Load += (s, e) => CarregarDados(id);
        }

        private void ConfigurarMascarasPadrao()
        {
            campoCpfAluno.Mask = @"000\.000\.000-00";
            campoCpfPais1.Mask = @"000\.000\.000-00";
            campoCpfPais2.Mask = @"000\.000\.000-00";
            campoTelefoneAluno.Mask = "(00) 00000-0000";
            campoTelefonePais1.Mask = "(00) 00000-0000";
            campoTelefonePais2.Mask = "(00) 00000-0000";
        }

        public void AplicarTema()
        {
            bool modoEscuro = TemaGlobal.ModoEscuro;

            if (modoEscuro)
            {
                corFundo = ColorTranslator.FromHtml("#0d1117");
                corCardFundo = ColorTranslator.FromHtml("#161b22");
                corInputFundo = ColorTranslator.FromHtml("#21262d");
                corTexto = ColorTranslator.FromHtml("#e6edf3");
                corRotulo = ColorTranslator.FromHtml("#8b949e");
                corBorda = ColorTranslator.FromHtml("#30363d");
                corDestaque = ColorTranslator.FromHtml("#58a6ff");
                corSalvar = ColorTranslator.FromHtml("#238636");
                corSalvarSuave = Color.FromArgb(40, 35, 134, 54);
                corCancelar = ColorTranslator.FromHtml("#ff7b72");
                corCancelarSuave = Color.FromArgb(40, 255, 123, 114);
            }
            else
            {
                corFundo = ColorTranslator.FromHtml("#f6f8fa");
                corCardFundo = Color.White;
                corInputFundo = Color.White;
                corTexto = ColorTranslator.FromHtml("#24292f");
                corRotulo = Color.DimGray;
                corBorda = ColorTranslator.FromHtml("#d0d7de");
                corDestaque = ColorTranslator.FromHtml("#0969da");
                corSalvar = ColorTranslator.FromHtml("#1f883d");
                corSalvarSuave = Color.FromArgb(40, 31, 136, 61);
                corCancelar = ColorTranslator.FromHtml("#cf222e");
                corCancelarSuave = Color.FromArgb(40, 207, 34, 46);
            }

            this.BackColor = corFundo;
            if (this.Controls.Count > 0)
                AtualizarCoresRecursivo(this);
        }

        private void AtualizarCoresRecursivo(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox || c is MaskedTextBox || c is ComboBox)
                {
                    c.BackColor = corInputFundo;
                    c.ForeColor = corTexto;
                }
                else if (c is DateTimePicker dtp)
                {
                    dtp.CalendarForeColor = corTexto;
                    dtp.CalendarMonthBackground = corInputFundo;
                }
                else if (c is Panel && c.Tag?.ToString() == "WRAPPER")
                {
                    c.BackColor = corInputFundo;
                    c.Invalidate();
                }

                if (c.HasChildren)
                    AtualizarCoresRecursivo(c);
            }
        }

        private void ConfigurarLayout()
        {
            this.Controls.Clear();
            this.SuspendLayout();

            Panel painelCabecalho = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = corFundo, Padding = new Padding(20, 0, 20, 0) };
            labelTitulo = new Label { Text = "Novo Cadastro", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = corTexto, AutoSize = true, Location = new Point(20, 15) };
            Label labelFechar = new Label { Text = "✕", AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 16), Cursor = Cursors.Hand, Dock = DockStyle.Right, TextAlign = ContentAlignment.TopRight };
            labelFechar.Click += (s, e) => VoltarParaLista?.Invoke(this, EventArgs.Empty);
            painelCabecalho.Controls.Add(labelTitulo);
            painelCabecalho.Controls.Add(labelFechar);

            // --- 2. RODAPÉ (BOTÕES AJUSTADOS) ---
            Panel painelBotoes = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = corFundo,
                // Padding ajustado para dar espaço nas laterais, mas deixamos o topo/base livres para o Flow controlar
                Padding = new Padding(0, 0, 30, 0)
            };

            FlowLayoutPanel flowBotoes = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                // AQUI ESTÁ O AJUSTE: 20px de margem superior empurra os botões para baixo
                Padding = new Padding(0, 20, 0, 0)
            };

            Button btnCancelar = new Button { Text = "CANCELAR", Width = 150, Height = 50, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0, 0, 5, 0) };
            EstilizarBotao(btnCancelar, corCancelar, corCancelarSuave);
            btnCancelar.Click += (s, e) => VoltarParaLista?.Invoke(this, EventArgs.Empty);

            Button btnSalvar = new Button { Text = "SALVAR DADOS", Width = 150, Height = 50, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0) };
            EstilizarBotao(btnSalvar, corSalvar, corSalvarSuave);
            btnSalvar.Click += EventoSalvar;

            flowBotoes.Controls.Add(btnCancelar);
            flowBotoes.Controls.Add(btnSalvar);
            painelBotoes.Controls.Add(flowBotoes);

            painelRolagem = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(30, 10, 30, 40), BackColor = corFundo };
            DarkBox.AplicarMarcaDagua(painelRolagem, Properties.Resources.diviminas);

            InicializarCampos();

            var cardAluno = CriarCard("DADOS DO ALUNO", 360);
            AdicionarCampoGrid(cardAluno.Grid, "Nome Completo", campoNome, 0, 3);
            AdicionarCampoGrid(cardAluno.Grid, "Data de Nascimento", seletorDataNasc, 1, 1, 0);
            AdicionarCampoGrid(cardAluno.Grid, "CPF do Aluno", campoCpfAluno, 1, 1, 1);
            AdicionarCampoGrid(cardAluno.Grid, "Telefone do Aluno", campoTelefoneAluno, 1, 1, 2);

            var cardResp = CriarCard("CONTATO E RESPONSÁVEIS", 360);
            AdicionarCampoGrid(cardResp.Grid, "CPF Responsável 1 *", campoCpfPais1, 0, 1, 0);
            AdicionarCampoGrid(cardResp.Grid, "Telefone Resp. 1 *", campoTelefonePais1, 0, 1, 1);
            AdicionarCampoGrid(cardResp.Grid, "E-mail Resp. 1", campoEmail1, 0, 1, 2);
            AdicionarCampoGrid(cardResp.Grid, "CPF Responsável 2 (Opcional)", campoCpfPais2, 1, 1, 0);
            AdicionarCampoGrid(cardResp.Grid, "Telefone Resp. 2 (Opcional)", campoTelefonePais2, 1, 1, 1);
            AdicionarCampoGrid(cardResp.Grid, "E-mail Resp. 2 (Opcional)", campoEmail2, 1, 1, 2);

            var cardFinTuple = CriarCard("MATRÍCULA E FINANCEIRO", 360);
            _cardFinanceiroPanel = cardFinTuple.Panel;

            Panel pnlTecnico = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0) };
            botaoAdicionarTecnico.Dock = DockStyle.Right;
            botaoAdicionarTecnico.Width = 45;
            Panel pnlComboWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 5, 0), BackColor = Color.Transparent };
            comboTecnico.Dock = DockStyle.None;
            pnlComboWrapper.Controls.Add(comboTecnico);
            pnlTecnico.Controls.Add(pnlComboWrapper);
            pnlTecnico.Controls.Add(botaoAdicionarTecnico);
            pnlComboWrapper.Resize += (s, e) => {
                comboTecnico.Width = pnlComboWrapper.Width - 5;
                comboTecnico.Top = (pnlComboWrapper.Height - comboTecnico.Height) / 2;
            };

            AdicionarCampoPersonalizado(cardFinTuple.Grid, "Técnico Responsável", pnlTecnico, 0, 0);
            AdicionarCampoPersonalizado(cardFinTuple.Grid, "Categoria Esportiva", comboCategoria, 0, 1);
            AdicionarCampoPersonalizado(cardFinTuple.Grid, "Tipo de Bolsa", comboBolsista, 0, 2);
            AdicionarCampoGrid(cardFinTuple.Grid, "Dia Vencimento", campoDiaVencimento, 1, 1, 0);
            AdicionarCampoGrid(cardFinTuple.Grid, "Valor Mensalidade (R$)", campoValorMensalidade, 1, 1, 1);

            _pnlAvisoFinanceiro = AdicionarAvisoFinanceiro();
            _pnlAvisoFinanceiro.Visible = false;
            cardFinTuple.Grid.Controls.Add(_pnlAvisoFinanceiro, 0, 2);
            cardFinTuple.Grid.SetColumnSpan(_pnlAvisoFinanceiro, 3);

            var cardObs = CriarCard("OBSERVAÇÕES", 260);
            AdicionarCampoGrid(cardObs.Grid, "Informações Adicionais", campoObservacao, 0, 3);

            painelRolagem.Controls.Add(cardAluno.Panel);
            painelRolagem.Controls.Add(cardResp.Panel);
            painelRolagem.Controls.Add(_cardFinanceiroPanel);
            painelRolagem.Controls.Add(cardObs.Panel);

            this.Controls.Add(painelRolagem);
            this.Controls.Add(painelCabecalho);
            this.Controls.Add(painelBotoes);
            painelRolagem.BringToFront();

            this.SizeChanged += (s, e) => AjustarLarguraCards();
            AjustarLarguraCards();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Panel AdicionarAvisoFinanceiro()
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, Height = 40, Margin = new Padding(0, 5, 0, 0), Visible = false };

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Ícone
            Label lblIcone = new Label
            {
                Text = "⚠️",
                ForeColor = Color.Yellow,
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Margin = new Padding(0),
                Anchor = AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Texto
            _lblAvisoTexto = new Label
            {
                Text = "",
                ForeColor = Color.Yellow,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(5, 0, 0, 0),
                Anchor = AnchorStyles.Left, 
                TextAlign = ContentAlignment.MiddleLeft
            };

            grid.Controls.Add(lblIcone, 0, 0);
            grid.Controls.Add(_lblAvisoTexto, 1, 0);
            pnl.Controls.Add(grid);

            return pnl;
        }

        private void VerificarAlteracaoFinanceira(object sender, EventArgs e)
        {
            if (_carregandoDados || !idEdicao.HasValue) return;

            string valAtual = campoValorMensalidade.Text.Trim();
            string diaAtual = campoDiaVencimento.Text.Trim();

            bool mudouValor = valAtual != _valorOriginal;
            bool mudouDia = diaAtual != _diaOriginal;
            bool houveAlgumaAlteracao = mudouValor || mudouDia;

            if (mudouValor && mudouDia)
                _lblAvisoTexto.Text = "A alteração na data e no valor de pagamento só será efetuada no próximo mês.";
            else if (mudouDia)
                _lblAvisoTexto.Text = "A alteração na data de pagamento só será efetuada no próximo mês.";
            else if (mudouValor)
                _lblAvisoTexto.Text = "A alteração no valor do pagamento só será efetuada no próximo mês.";

            if (_pnlAvisoFinanceiro.Visible != houveAlgumaAlteracao)
            {
                _pnlAvisoFinanceiro.Visible = houveAlgumaAlteracao;
                _cardFinanceiroPanel.Height = houveAlgumaAlteracao ? 420 : 360;
            }
        }

        private void AjustarLarguraCards()
        {
            if (painelRolagem == null) return;
            int larguraUtil = Math.Max(600, this.Width - 80);
            foreach (Control c in painelRolagem.Controls)
                if (c is Panel p) p.Width = larguraUtil;
        }

        private void InicializarCampos()
        {
            campoNome = CriarInputTexto();
            campoCpfAluno = CriarInputMask();
            campoTelefoneAluno = CriarInputMask();
            campoTelefonePais1 = CriarInputMask();
            campoCpfPais1 = CriarInputMask();
            campoTelefonePais2 = CriarInputMask();
            campoCpfPais2 = CriarInputMask();
            campoEmail1 = CriarInputTexto();
            campoEmail2 = CriarInputTexto();
            campoObservacao = CriarInputTexto();
            campoObservacao.Multiline = true;
            campoObservacao.Height = 100;

            seletorDataNasc = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 11), Dock = DockStyle.Fill };

            campoValorMensalidade = CriarInputTexto();
            campoValorMensalidade.TextAlign = HorizontalAlignment.Right;
            campoValorMensalidade.Text = "0,00";
            campoValorMensalidade.TextChanged += EventoMudarValor;
            campoValorMensalidade.Enter += (s, e) => campoValorMensalidade.SelectAll();

            campoDiaVencimento = CriarInputTexto();
            campoDiaVencimento.TextAlign = HorizontalAlignment.Center;
            campoDiaVencimento.Text = "10";
            campoDiaVencimento.MaxLength = 2;
            campoDiaVencimento.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            campoDiaVencimento.Validating += (s, e) => {
                if (int.TryParse(campoDiaVencimento.Text, out int d)) campoDiaVencimento.Text = Math.Clamp(d, 1, 30).ToString("D2");
                else campoDiaVencimento.Text = "10";
            };

            campoValorMensalidade.TextChanged += VerificarAlteracaoFinanceira;
            campoDiaVencimento.TextChanged += VerificarAlteracaoFinanceira;

            comboCategoria = CriarCombo();
            comboBolsista = CriarCombo();
            comboTecnico = CriarCombo();

            botaoAdicionarTecnico = new Button
            {
                Text = "⚙️",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = corRotulo,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = 45
            };

            botaoAdicionarTecnico.FlatAppearance.BorderSize = 0;
            botaoAdicionarTecnico.MouseEnter += (s, e) => botaoAdicionarTecnico.ForeColor = corDestaque;
            botaoAdicionarTecnico.MouseLeave += (s, e) => botaoAdicionarTecnico.ForeColor = corRotulo;

            botaoAdicionarTecnico.Click += EventoGerenciarTecnicos;
        }

        private (Panel Panel, TableLayoutPanel Grid) CriarCard(string titulo, int altura)
        {
            Panel card = new Panel { Height = altura, BackColor = corCardFundo, Margin = new Padding(0, 0, 0, 30), Padding = new Padding(25) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, corBorda, ButtonBorderStyle.Solid);
            Label lblTitulo = new Label { Text = titulo, Dock = DockStyle.Top, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = corDestaque, Height = 35 };
            Panel linha = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = corBorda, Margin = new Padding(0, 0, 0, 20) };
            TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(0, 20, 0, 0), BackColor = Color.Transparent };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            card.Controls.Add(grid);
            card.Controls.Add(linha);
            card.Controls.Add(lblTitulo);
            return (card, grid);
        }

        private void EventoSalvar(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(campoNome.Text)) { MessageBox.Show("Nome obrigatório!"); return; }
            if (comboTecnico.SelectedValue == null) { MessageBox.Show("Selecione um técnico!"); return; }

            decimal.TryParse(campoValorMensalidade.Text, out decimal valor);
            int.TryParse(campoDiaVencimento.Text, out int dia);

            var aluno = new Entidade
            {
                Id = idEdicao ?? 0,
                Nome = campoNome.Text.Trim(),
                CpfAtleta = LimparNumeros(campoCpfAluno.Text),
                telefone_aluno = LimparNumeros(campoTelefoneAluno.Text),
                DataNascimento = seletorDataNasc.Value,
                EmailPais = campoEmail1.Text,
                TelefonePais = LimparNumeros(campoTelefonePais1.Text),
                CpfPais = LimparNumeros(campoCpfPais1.Text),
                email_pais2 = campoEmail2.Text,
                telefone_pais2 = LimparNumeros(campoTelefonePais2.Text),
                cpf_pais2 = LimparNumeros(campoCpfPais2.Text),
                observacao = campoObservacao.Text.Trim(),
                Categoria_id = (int)(comboCategoria.SelectedValue ?? 1),
                id_tecnico = (int)comboTecnico.SelectedValue,
                bolsista_id = (int?)comboBolsista.SelectedValue,
                ValorMensalidade = valor,
                DiaVencimento = dia > 0 ? dia : 10,
                Status = "Ativo",
                TipoVinculo = "Aluno"
            };

            try
            {
                var repo = new EntidadeRepository();
                if (idEdicao.HasValue) repo.Atualizar(aluno); else repo.Inserir(aluno);
                VoltarParaLista?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void CarregarDados(int id)
        {
            _carregandoDados = true;

            var aluno = new EntidadeRepository().ObterPorId(id);
            if (aluno == null) return;

            labelTitulo.Text = "Editar Aluno";
            campoNome.Text = aluno.Nome;
            PreencherMascara(campoCpfAluno, aluno.CpfAtleta, @"000\.000\.000-00");
            string telA = LimparNumeros(aluno.telefone_aluno);
            PreencherMascara(campoTelefoneAluno, telA, (telA.Length > 10) ? "(00) 00000-0000" : "(00) 0000-0000");
            seletorDataNasc.Value = aluno.DataNascimento;
            PreencherMascara(campoCpfPais1, aluno.CpfPais, @"000\.000\.000-00");
            string telP1 = LimparNumeros(aluno.TelefonePais);
            PreencherMascara(campoTelefonePais1, telP1, (telP1.Length > 10) ? "(00) 00000-0000" : "(00) 0000-0000");
            campoEmail1.Text = aluno.EmailPais;
            PreencherMascara(campoCpfPais2, aluno.cpf_pais2, @"000\.000\.000-00");
            string telP2 = LimparNumeros(aluno.telefone_pais2);
            PreencherMascara(campoTelefonePais2, telP2, (telP2.Length > 10) ? "(00) 00000-0000" : "(00) 0000-0000");
            campoEmail2.Text = aluno.email_pais2 ?? "";
            campoObservacao.Text = aluno.observacao;

            campoValorMensalidade.Text = aluno.ValorMensalidade.ToString("N2");
            campoDiaVencimento.Text = aluno.DiaVencimento.ToString("D2");

            comboCategoria.SelectedValue = aluno.Categoria_id;
            comboTecnico.SelectedValue = aluno.id_tecnico;
            comboBolsista.SelectedValue = aluno.bolsista_id;

            _valorOriginal = campoValorMensalidade.Text.Trim();
            _diaOriginal = campoDiaVencimento.Text.Trim();

            _carregandoDados = false;
        }

        private void PreencherMascara(MaskedTextBox msk, string valor, string mascara)
        {
            if (string.IsNullOrEmpty(valor)) { msk.Mask = mascara; msk.Text = ""; return; }
            msk.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            msk.Mask = ""; msk.Text = valor; msk.Mask = mascara;
        }

        private void CarregarListas()
        {
            comboCategoria.DataSource = new EntidadeRepository().ObterCategorias();
            comboCategoria.DisplayMember = "Descricao"; comboCategoria.ValueMember = "Id";
            comboTecnico.DataSource = new TecnicoRepository().ObterTodosAtivos();
            comboTecnico.DisplayMember = "Nome"; comboTecnico.ValueMember = "Id";
            comboBolsista.DataSource = new BolsistaRepository().ObterTodosAtivos();
            comboBolsista.DisplayMember = "Descricao"; comboBolsista.ValueMember = "Id";
            comboTecnico.SelectedIndex = comboBolsista.SelectedIndex = -1;
        }

        private void EventoGerenciarTecnicos(object s, EventArgs e)
        {
            using (var formGerenciar = new FormGerenciarTecnicos())
            {
                formGerenciar.ShowDialog();

                CarregarListas();
            }
        }

        private void EventoMudarValor(object s, EventArgs e)
        {
            campoValorMensalidade.TextChanged -= EventoMudarValor;
            string d = Regex.Replace(campoValorMensalidade.Text, "[^0-9]", "");
            if (decimal.TryParse(d, out decimal v)) campoValorMensalidade.Text = (v / 100m).ToString("N2");
            else campoValorMensalidade.Text = "0,00";
            campoValorMensalidade.SelectionStart = campoValorMensalidade.Text.Length;
            campoValorMensalidade.TextChanged += EventoMudarValor;
            VerificarAlteracaoFinanceira(s, e);
        }

        private string LimparNumeros(string t) => Regex.Replace(t ?? "", "[^0-9]", "");
        private TextBox CriarInputTexto() => new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 12), BackColor = corInputFundo, ForeColor = corTexto };
        private MaskedTextBox CriarInputMask() => new MaskedTextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 12), BackColor = corInputFundo, ForeColor = corTexto, ResetOnSpace = false };
        private ComboBox CriarCombo() => new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = corInputFundo, ForeColor = corTexto };

        private void AdicionarCampoGrid(TableLayoutPanel grid, string t, Control c, int lin, int span = 1, int col = 0)
        {
            int alturaPainel = (c is TextBox tb && tb.Multiline) ? 160 : 110;
            Panel p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 20, 15), Height = alturaPainel };
            Label l = new Label { Text = t, Dock = DockStyle.Top, ForeColor = corRotulo, Font = new Font("Segoe UI", 9, FontStyle.Bold), Height = 30 };
            int h = (c is TextBox tb2 && tb2.Multiline) ? 120 : 45;
            Panel w = new Panel { Dock = DockStyle.Top, Height = h, Tag = "WRAPPER", BackColor = corInputFundo, Padding = new Padding(10, 0, 10, 0) };
            w.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, w.ClientRectangle, corBorda, ButtonBorderStyle.Solid);
            if (!(c is TextBox tbm && tbm.Multiline)) { c.Dock = DockStyle.None; c.Anchor = AnchorStyles.Left | AnchorStyles.Right; c.Width = w.Width - 20; c.Location = new Point(10, (h - c.Height) / 2); }
            else c.Dock = DockStyle.Fill;
            w.Controls.Add(c); p.Controls.Add(w); p.Controls.Add(l);
            grid.Controls.Add(p, col, lin); if (span > 1) grid.SetColumnSpan(p, span);
        }

        private void AdicionarCampoPersonalizado(TableLayoutPanel grid, string t, Control c, int lin, int col)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 20, 15), Height = 110 };
            Label l = new Label { Text = t, Dock = DockStyle.Top, ForeColor = corRotulo, Font = new Font("Segoe UI", 9, FontStyle.Bold), Height = 30 };
            Panel w = new Panel { Dock = DockStyle.Top, Height = 45, Tag = "WRAPPER", BackColor = corInputFundo, Padding = new Padding(1) };
            w.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, w.ClientRectangle, corBorda, ButtonBorderStyle.Solid);
            if (c is Panel) c.Dock = DockStyle.Fill;
            else if (c is ComboBox) { c.Dock = DockStyle.None; c.Anchor = AnchorStyles.Left | AnchorStyles.Right; c.Width = w.Width - 20; c.Location = new Point(10, (45 - c.Height) / 2); }
            else c.Dock = DockStyle.Fill;
            w.Controls.Add(c); p.Controls.Add(w); p.Controls.Add(l);
            grid.Controls.Add(p, col, lin);
        }

        private void EstilizarBotao(Button b, Color bg, Color hover)
        {
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; b.BackColor = bg; b.ForeColor = Color.White;
            b.MouseEnter += (s, e) => b.BackColor = hover; b.MouseLeave += (s, e) => b.BackColor = bg;
        }
    }
}