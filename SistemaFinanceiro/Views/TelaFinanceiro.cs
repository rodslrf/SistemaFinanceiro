using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Globalization;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public partial class TelaFinanceiro : UserControl
    {
        private Label _lblRec, _lblPend, _lblTotalBolsistas;
        private DataGridView _grid;
        private Panel _pnlCards, _pnlStats;
        private TextBox _inputSearch;
        private DarkComboBox _cmbCat, _cmbStatus, _cmbBolsa, _cmbMes;

        private List<Cobranca> _dataRaw = new List<Cobranca>();

        // NOVA LISTA: Vai guardar os IDs dos alunos que realmente têm bolsa
        private List<int> _idsBolsistasReais = new List<int>();

        private FinanceiroRepository _repo = new FinanceiroRepository();
        private bool _sortAsc = true;

        private static bool _ignorarMsgGerar = false;
        private static bool _ignorarMsgReceber = false;

        public TelaFinanceiro()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            if (!DesignMode)
            {
                SetupUI();
                ApplyTheme();
                Load += (s, e) => LoadData();
            }
        }

        public void ApplyTheme()
        {
            BackColor = TemaGlobal.CorFundo;

            foreach (Control c in _pnlCards.Controls)
            {
                if (c is Panel p)
                {
                    p.BackColor = TemaGlobal.CorSidebar;
                    foreach (Control l in p.Controls)
                        if (l is Label && l.Font.Size > 12)
                            l.ForeColor = TemaGlobal.CorTexto;
                }
            }

            if (_grid != null)
            {
                _grid.BackgroundColor = _grid.DefaultCellStyle.BackColor =
                    _grid.DefaultCellStyle.SelectionBackColor = TemaGlobal.CorFundo;

                _grid.GridColor = TemaGlobal.CorBorda;

                _grid.DefaultCellStyle.ForeColor =
                    _grid.DefaultCellStyle.SelectionForeColor =
                    _grid.ColumnHeadersDefaultCellStyle.ForeColor = TemaGlobal.CorTexto;

                _grid.ColumnHeadersDefaultCellStyle.BackColor =
                    _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = TemaGlobal.CorSidebar;
            }

            if (_pnlStats != null)
            {
                bool escuro = TemaGlobal.ModoEscuro;
                _pnlStats.BackColor = escuro
                    ? ColorTranslator.FromHtml("#21262d")
                    : ColorTranslator.FromHtml("#F7F7F8");
                _pnlStats.Invalidate();
            }
            Invalidate();
        }

        private void SetupUI()
        {
            Controls.Clear();

            // --- Header ---
            var h = new Panel { Dock = DockStyle.Top, Height = 80 };
            var t = new Label
            {
                Text = "Gestão Financeira",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 10)
            };

            _pnlStats = new Panel { Size = new Size(200, 35) };
            _pnlStats.Paint += (s, e) =>
            {
                var corBorda = TemaGlobal.ModoEscuro ? ColorTranslator.FromHtml("#30363d") : Color.Silver;
                using (var p = new Pen(corBorda))
                    e.Graphics.DrawRectangle(p, 0, 0, _pnlStats.Width - 1, _pnlStats.Height - 1);
            };

            _lblTotalBolsistas = new Label
            {
                Text = "🎓 Bolsistas: 0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#58a6ff")
            };
            _pnlStats.Controls.Add(_lblTotalBolsistas);

            var b = new Button
            {
                Text = "⚡ Gerar Cobranças",
                BackColor = ColorTranslator.FromHtml("#2f81f7"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 40,
                Width = 180,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Width - 220, 15)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += OnGen;

            h.Resize += (s, e) =>
            {
                b.Left = h.Width - 190;
                _pnlStats.Left = b.Left - _pnlStats.Width - 20;
                _pnlStats.Top = 18;
            };

            h.Controls.Add(t);
            h.Controls.Add(_pnlStats);
            h.Controls.Add(b);

            // --- Cards ---
            _pnlCards = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 10, 0, 20) };
            _lblPend = Card("PENDENTE (MÊS)", "R$ 0,00", ColorTranslator.FromHtml("#da3633"), 260);
            _lblRec = Card("RECEBIDO (MÊS)", "R$ 0,00", ColorTranslator.FromHtml("#238636"), 0);
            _pnlCards.Controls.Add(_lblPend.Parent);
            _pnlCards.Controls.Add(_lblRec.Parent);

            // --- Filtros ---
            var f = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 15, 0, 15) };

            var tl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));

            _inputSearch = Input("Pesquisar Aluno");
            _inputSearch.TextChanged += (s, e) => Filter();

            _cmbMes = new DarkComboBox();
            _cmbMes.Items.Add("Todos os Meses");
            var meses = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrEmpty(m))
                .Select(m => m.ToUpper())
                .ToArray();
            _cmbMes.Items.AddRange(meses);
            _cmbMes.SelectedIndex = 0;
            _cmbMes.SelectedIndexChanged += (s, e) => Filter();

            _cmbCat = new DarkComboBox();
            LoadCats();
            _cmbCat.SelectedIndexChanged += (s, e) => Filter();

            _cmbStatus = new DarkComboBox();
            _cmbStatus.Items.AddRange(new[] { "Todos Status", "Pendentes", "Pagos", "Atrasados" });
            _cmbStatus.SelectedIndex = 0;
            _cmbStatus.SelectedIndexChanged += (s, e) => Filter();

            _cmbBolsa = new DarkComboBox();
            _cmbBolsa.Items.AddRange(new[] { "Todos Alunos", "Somente Bolsistas", "Não Bolsistas" });
            _cmbBolsa.SelectedIndex = 0;
            _cmbBolsa.SelectedIndexChanged += (s, e) => Filter();

            var bClr = Btn("Limpar");
            bClr.Click += (s, e) =>
            {
                _inputSearch.Text = "Pesquisar Aluno";
                _cmbCat.SelectedIndex = 0;
                _cmbStatus.SelectedIndex = 0;
                _cmbBolsa.SelectedIndex = 0;
                _cmbMes.SelectedIndex = 0;
            };

            tl.Controls.Add(Wrap(_inputSearch), 0, 0);
            tl.Controls.Add(Wrap(_cmbMes), 1, 0);
            tl.Controls.Add(Wrap(_cmbCat), 2, 0);
            tl.Controls.Add(Wrap(_cmbStatus), 3, 0);
            tl.Controls.Add(Wrap(_cmbBolsa), 4, 0);
            tl.Controls.Add(WrapBtn(bClr), 5, 0);

            f.Controls.Add(tl);

            // --- Grid ---
            var gp = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            _grid = new DataGridView();
            SetupGrid(_grid);
            SetupCols(_grid);
            gp.Controls.Add(_grid);

            Controls.Add(gp);
            Controls.Add(f);
            Controls.Add(_pnlCards);
            Controls.Add(h);
            AddFooter();
        }

        private void LoadData()
        {
            try
            {
                // 1. Carrega as cobranças do banco
                _dataRaw = _repo.ObterTodasCobrancas();

                // 2. MAGIA: Carrega a lista de alunos para descobrir quem são os bolsistas DE VERDADE
                // Isso usa a mesma lógica da tela de lista de alunos ("Sem Bolsa" vs Nome da Bolsa)
                var repoEntidade = new EntidadeRepository();
                var todosAlunos = repoEntidade.ObterTodos();

                _idsBolsistasReais.Clear();
                foreach (var aluno in todosAlunos)
                {
                    // Usamos reflection pois ObterTodos retorna dynamic
                    var propId = aluno.GetType().GetProperty("id_entidade");
                    var propBolsa = aluno.GetType().GetProperty("BolsaDescricao");

                    if (propId != null && propBolsa != null)
                    {
                        int id = (int)propId.GetValue(aluno, null);
                        string bolsa = propBolsa.GetValue(aluno, null)?.ToString();

                        // Se a descrição NÃO for "Sem Bolsa", guardamos o ID como bolsista
                        if (bolsa != null && !bolsa.Equals("Sem Bolsa", StringComparison.OrdinalIgnoreCase))
                        {
                            _idsBolsistasReais.Add(id);
                        }
                    }
                }

                // 3. Atualiza o contador usando a contagem oficial do banco
                int qtdBolsistas = repoEntidade.ContarBolsistasAtivos();
                _lblTotalBolsistas.Text = $"🎓 Bolsistas: {qtdBolsistas}";

                // 4. Aplica filtros
                Filter();

                // 5. Atualiza Cards
                var t = _repo.ObterTotaisMes(DateTime.Now.ToString("MM/yyyy"));
                _lblRec.Text = t.Recebido.ToString("C2");
                _lblPend.Text = t.Pendente.ToString("C2");
            }
            catch (Exception ex)
            {
                DarkBox.Mostrar(ex.Message);
            }
        }

        private void Filter()
        {
            if (_dataRaw == null) return;

            string t = _inputSearch.Text.ToLower().Contains("pesquisar") ? "" : _inputSearch.Text.ToLower();
            string c = _cmbCat.SelectedItem?.ToString() ?? "Todas Categorias";
            string s = _cmbStatus.SelectedItem?.ToString() ?? "Todos Status";
            string b = _cmbBolsa.SelectedItem?.ToString() ?? "Todos Alunos";
            int selMes = _cmbMes.SelectedIndex;

            var hj = DateTime.Today;

            var filtered = _dataRaw.Where(x =>
                (x.StatusAluno.Equals("Ativo", StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(t) || x.NomeAluno.ToLower().Contains(t)) &&
                (c == "Todas Categorias" || x.CategoriaDescricao == c) &&
                (s == "Todos Status" ||
                    (s == "Pagos" && x.StatusId == 2) ||
                    (s == "Pendentes" && x.StatusId == 1 && x.DataVencimento.Date >= hj) ||
                    (s == "Atrasados" && x.StatusId == 1 && x.DataVencimento.Date < hj)) &&
                (b == "Todos Alunos" || IsBolsistaMatch(x, b)) &&
                (selMes == 0 || x.DataVencimento.Month == selMes)
            ).ToList();

            var sorted = filtered
                .OrderByDescending(x => x.DataVencimento.Year)
                .ThenByDescending(x => x.DataVencimento.Month)
                .ThenBy(x => x.NomeAluno)
                .ToList();

            var listaVisual = new List<Cobranca>();
            string ultimoGrupo = "";

            foreach (var item in sorted)
            {
                string grupoAtual = item.DataVencimento.ToString("MMMM 'de' yyyy", CultureInfo.CurrentCulture);
                if (grupoAtual != ultimoGrupo)
                {
                    listaVisual.Add(new Cobranca
                    {
                        IdCobranca = -1,
                        NomeAluno = char.ToUpper(grupoAtual[0]) + grupoAtual.Substring(1)
                    });
                    ultimoGrupo = grupoAtual;
                }
                listaVisual.Add(item);
            }

            _grid.DataSource = listaVisual;
        }

        private bool IsBolsistaMatch(Cobranca x, string filtro)
        {
            // CORREÇÃO FINAL: Verifica se o ID do aluno está na lista de bolsistas reais
            bool ehBolsista = _idsBolsistasReais.Contains(x.AlunoId);

            if (filtro == "Somente Bolsistas") return ehBolsista;
            if (filtro == "Não Bolsistas") return !ehBolsista;
            return true;
        }

        private void SetupGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.BackgroundColor = TemaGlobal.CorFundo;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 60;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10);
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.AutoGenerateColumns = false;
            g.ReadOnly = true;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;
            g.AllowUserToResizeRows = false;
            g.AllowUserToResizeColumns = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.RowTemplate.Height = 45;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            g.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            g.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.DefaultCellStyle.SelectionBackColor = TemaGlobal.CorFundo;
            g.DefaultCellStyle.SelectionForeColor = TemaGlobal.CorTexto;

            DarkBox.AplicarMarcaDagua(g, Properties.Resources.diviminas);

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, g, new object[] { true });

            g.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < g.Rows.Count)
                {
                    var item = g.Rows[e.RowIndex].DataBoundItem as Cobranca;
                    if (item != null && item.IdCobranca == -1) // É Header
                        g.Rows[e.RowIndex].Height = 70;
                    else
                        g.Rows[e.RowIndex].Height = 45;
                }
            };

            g.RowPostPaint += (s, e) =>
            {
                var item = g.Rows[e.RowIndex].DataBoundItem as Cobranca;
                if (item != null && item.IdCobranca == -1)
                {
                    string texto = item.NomeAluno;
                    var fonte = new Font("Segoe UI", 14, FontStyle.Bold);
                    var corTexto = Color.White;
                    var rectTexto = new Rectangle(e.RowBounds.Left + 20, e.RowBounds.Top, g.Width - 40, e.RowBounds.Height);
                    TextRenderer.DrawText(e.Graphics, texto, fonte, rectTexto, corTexto,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            };
        }

        private void SetupCols(DataGridView g)
        {
            g.Columns.Clear();
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "IdCobranca", Visible = false });

            var n = new DataGridViewTextBoxColumn
            {
                HeaderText = "Aluno",
                DataPropertyName = "NomeAluno",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            n.DefaultCellStyle.Font = new Font("Segoe UI", 12);
            n.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.Columns.Add(n);

            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoria", DataPropertyName = "CategoriaDescricao", Width = 130 });

            var colVenc = new DataGridViewTextBoxColumn();
            colVenc.HeaderText = "Vencimento";
            colVenc.DataPropertyName = "DataVencimento";
            colVenc.Width = 110;
            colVenc.DefaultCellStyle.Format = "dd/MM/yyyy";
            colVenc.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            g.Columns.Add(colVenc);

            var colVal = new DataGridViewTextBoxColumn();
            colVal.HeaderText = "Valor";
            colVal.DataPropertyName = "ValorBase";
            colVal.Width = 110;
            colVal.DefaultCellStyle.Format = "C2";
            colVal.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            g.Columns.Add(colVal);

            g.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Status", DataPropertyName = "StatusDescricao", Width = 130 });
            g.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Ação", Width = 150 });

            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;
                var i = g.Rows[e.RowIndex].DataBoundItem as Cobranca;
                if (i == null) return;

                if (i.IdCobranca == -1)
                {
                    using (var b = new SolidBrush(TemaGlobal.CorFundo)) e.Graphics.FillRectangle(b, e.CellBounds);
                    int gap = 20;
                    var rectBarra = new Rectangle(e.CellBounds.Left, e.CellBounds.Top + gap, e.CellBounds.Width, e.CellBounds.Height - gap);
                    using (var b = new SolidBrush(ColorTranslator.FromHtml("#161b22"))) e.Graphics.FillRectangle(b, rectBarra);
                    e.Handled = true;
                    return;
                }

                bool st = g.Columns[e.ColumnIndex].HeaderText == "Status";
                bool ac = g.Columns[e.ColumnIndex].HeaderText == "Ação";

                if (st || ac)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.None;
                    using (var b = new SolidBrush(TemaGlobal.CorFundo)) e.Graphics.FillRectangle(b, e.CellBounds);
                    using (var p = new Pen(TemaGlobal.CorBorda)) e.Graphics.DrawRectangle(p, e.CellBounds);
                    var r = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 10, e.CellBounds.Width - 20, e.CellBounds.Height - 20);

                    if (st)
                    {
                        string status = i.StatusDescricao;
                        bool atrasado = status != "Pago" && i.DataVencimento.Date < DateTime.Today;
                        if (atrasado) status = "ATRASADO";

                        Color bg = Color.Gray, border = Color.Black, text = Color.White;

                        if (status == "Pago") { bg = ColorTranslator.FromHtml("#d1e7dd"); border = ColorTranslator.FromHtml("#0f5132"); text = ColorTranslator.FromHtml("#0f5132"); }
                        else if (status == "ATRASADO") { bg = ColorTranslator.FromHtml("#f8d7da"); border = ColorTranslator.FromHtml("#842029"); text = ColorTranslator.FromHtml("#842029"); }
                        else { bg = ColorTranslator.FromHtml("#fff8c5"); border = ColorTranslator.FromHtml("#d29922"); text = ColorTranslator.FromHtml("#d29922"); }

                        using (var b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, r);
                        using (var p = new Pen(border, 2)) e.Graphics.DrawRectangle(p, r);
                        TextRenderer.DrawText(e.Graphics, status.ToUpper(), new Font("Segoe UI", 9, FontStyle.Bold), r, text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else if (ac)
                    {
                        if (i.StatusDescricao != "Pago")
                        {
                            var cAzul = ColorTranslator.FromHtml("#2f81f7");
                            using (var b = new SolidBrush(Color.FromArgb(30, cAzul))) e.Graphics.FillRectangle(b, r);
                            using (var p = new Pen(cAzul, 2)) e.Graphics.DrawRectangle(p, r);
                            TextRenderer.DrawText(e.Graphics, "💲 BAIXAR", new Font("Segoe UI", 8, FontStyle.Bold), r, cAzul, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                        else
                        {
                            bool pagouAtrasado = false;
                            if (i.DataPagamento.HasValue) pagouAtrasado = i.DataPagamento.Value.Date > i.DataVencimento.Date;

                            string textoAcao; Color bgAcao, borderAcao, textAcao;
                            if (pagouAtrasado) { textoAcao = "PAGO C/ ATRASO"; bgAcao = ColorTranslator.FromHtml("#fff3cd"); borderAcao = ColorTranslator.FromHtml("#ffc107"); textAcao = ColorTranslator.FromHtml("#856404"); }
                            else { textoAcao = "PAGO NO PRAZO"; bgAcao = ColorTranslator.FromHtml("#c3e6cb"); borderAcao = ColorTranslator.FromHtml("#155724"); textAcao = ColorTranslator.FromHtml("#155724"); }

                            using (var b = new SolidBrush(bgAcao)) e.Graphics.FillRectangle(b, r);
                            using (var p = new Pen(borderAcao, 2)) e.Graphics.DrawRectangle(p, r);
                            TextRenderer.DrawText(e.Graphics, textoAcao, new Font("Segoe UI", 7, FontStyle.Bold), r, textAcao, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    e.Handled = true;
                }
            };

            g.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && g.Columns[e.ColumnIndex].HeaderText == "Ação")
                {
                    var i = g.Rows[e.RowIndex].DataBoundItem as Cobranca;
                    if (i != null && i.IdCobranca != -1 && i.StatusDescricao != "Pago")
                        OnPay(i);
                }
            };
        }

        private void OnGen(object s, EventArgs e)
        {
            if (!_ignorarMsgGerar)
            {
                if (!DarkBox.Confirmar("Gerar cobranças do mês?", out bool dontAsk)) return;
                if (dontAsk) _ignorarMsgGerar = true;
            }
            try
            {
                _repo.GerarCobrancasMensais();
                DarkBox.Mostrar("Sucesso!", "Ok");
                LoadData();
            }
            catch (Exception ex) { DarkBox.Mostrar(ex.Message); }
        }

        private void OnPay(Cobranca c)
        {
            if (!_ignorarMsgReceber)
            {
                if (!DarkBox.Confirmar($"Receber {c.ValorBase:C2}?", out bool dontAsk)) return;
                if (dontAsk) _ignorarMsgReceber = true;
            }
            try
            {
                _repo.ReceberMensalidade(c.IdCobranca);
                LoadData();
            }
            catch (Exception ex) { DarkBox.Mostrar(ex.Message); }
        }

        private void AddFooter()
        {
            var f = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Transparent, Padding = new Padding(0, 0, 20, 10) };
            f.Controls.Add(new Label { Text = "@rodrigolopes_rf", Font = new Font("Segoe UI", 10), ForeColor = TemaGlobal.CorTexto, AutoSize = true, Dock = DockStyle.Right, Padding = new Padding(5, 2, 0, 0) });
            f.Controls.Add(new Label { Text = "📷", Font = new Font("Segoe UI Emoji", 12), ForeColor = TemaGlobal.CorTexto, AutoSize = true, Dock = DockStyle.Right });
            Controls.Add(f);
        }

        private Panel Wrap(Control c)
        {
            var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 10, 0), BackColor = Color.Transparent };
            var b = new Panel { Dock = DockStyle.Top, Height = 45 };
            b.Paint += (s, e) =>
            {
                b.BackColor = TemaGlobal.CorSidebar;
                ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, TemaGlobal.CorBorda, ButtonBorderStyle.Solid);
            };

            if (c is ComboBox)
            {
                var m = new Panel { BackColor = TemaGlobal.CorSidebar, Width = b.Width - 12, Height = c.Height - 6, Location = new Point(6, (b.Height - c.Height + 6) / 2) };
                c.Dock = DockStyle.None;
                c.Location = new Point(-2, -3);
                c.Width = m.Width + 20;
                m.Controls.Add(c);
                b.Controls.Add(m);
                b.Resize += (s, e) =>
                {
                    m.Width = b.Width - 12;
                    m.Location = new Point(6, (b.Height - m.Height) / 2);
                    c.Width = m.Width + 20;
                };
            }
            else
            {
                c.Dock = DockStyle.None;
                c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                c.Width = b.Width - 20;
                c.Location = new Point(10, (b.Height - c.Height) / 2);
                if (c is TextBox t)
                {
                    t.BorderStyle = BorderStyle.None;
                    t.BackColor = TemaGlobal.CorSidebar;
                    t.ForeColor = TemaGlobal.CorTexto;
                }
                b.Controls.Add(c);
            }
            p.Controls.Add(b);
            return p;
        }

        private Panel WrapBtn(Control c)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var b = new Panel { Dock = DockStyle.Top, Height = 45 };
            b.Paint += (s, e) =>
            {
                b.BackColor = TemaGlobal.CorSidebar;
                ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, TemaGlobal.CorBorda, ButtonBorderStyle.Solid);
                c.BackColor = TemaGlobal.CorSidebar;
                c.ForeColor = TemaGlobal.CorTexto;
            };
            c.Dock = DockStyle.Fill;
            b.Controls.Add(c);
            p.Controls.Add(b);
            return p;
        }

        private TextBox Input(string ph)
        {
            var t = new TextBox { Font = new Font("Segoe UI", 11), Text = ph, TextAlign = HorizontalAlignment.Center };
            t.GotFocus += (s, e) => { if (t.Text == ph) { t.Text = ""; t.ForeColor = TemaGlobal.CorTexto; } };
            t.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(t.Text)) { t.Text = ph; t.ForeColor = Color.Gray; } };
            return t;
        }

        private Button Btn(string t)
        {
            var b = new Button { Text = t, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Fill, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Label Card(string t, string v, Color c, int l)
        {
            var p = new Panel { Width = 240, Height = 80, BackColor = TemaGlobal.CorSidebar, Location = new Point(l, 0) };
            var f = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = c };
            var lt = new Label { Text = t, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            var lv = new Label { Text = v, ForeColor = TemaGlobal.CorTexto, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(12, 35), AutoSize = true };
            p.Controls.Add(lt);
            p.Controls.Add(lv);
            p.Controls.Add(f);
            return lv;
        }

        private void LoadCats()
        {
            try
            {
                var c = new EntidadeRepository().ObterCategorias();
                _cmbCat.Items.Clear();
                _cmbCat.Items.Add("Todas Categorias");
                foreach (var i in c)
                    _cmbCat.Items.Add(i.GetType().GetProperty("Descricao").GetValue(i, null));
                _cmbCat.SelectedIndex = 0;
            }
            catch { }
        }
    }
}