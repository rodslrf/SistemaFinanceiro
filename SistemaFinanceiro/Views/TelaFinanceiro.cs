using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public partial class TelaFinanceiro : UserControl
    {
        // ================= CORES =================
        private readonly Color CorFundo = ColorTranslator.FromHtml("#0d1117");
        private readonly Color CorSidebar = ColorTranslator.FromHtml("#161b22");
        private readonly Color CorTexto = ColorTranslator.FromHtml("#c9d1d9");
        private readonly Color CorBorda = ColorTranslator.FromHtml("#30363d");

        // Cores Cards
        private readonly Color CorVerde = ColorTranslator.FromHtml("#238636");
        private readonly Color CorVermelho = ColorTranslator.FromHtml("#da3633");
        private readonly Color CorAzul = ColorTranslator.FromHtml("#2f81f7");
        private readonly Color CorAzulSuave = Color.FromArgb(40, 47, 129, 247);

        // ================= CORES DOS STATUS =================

        // 1. PENDENTE (Dourado #d29922)
        private readonly Color CorPendenteTexto = ColorTranslator.FromHtml("#d29922");
        private readonly Color CorPendenteFundo = ColorTranslator.FromHtml("#fff8c5");

        // 2. PAGO (Verde)
        private readonly Color CorPagoTexto = ColorTranslator.FromHtml("#0f5132");
        private readonly Color CorPagoFundo = ColorTranslator.FromHtml("#d1e7dd");

        // 3. ATRASADO (Vermelho)
        private readonly Color CorAtrasadoTexto = ColorTranslator.FromHtml("#842029");
        private readonly Color CorAtrasadoFundo = ColorTranslator.FromHtml("#f8d7da");

        // COMPONENTES
        private Label lblTotalRecebido, lblTotalPendente;
        private DataGridView grid;
        private Panel pnlCards;
        private TextBox txtBusca;
        private DarkComboBox cmbFiltroCategoria;
        private DarkComboBox cmbFiltroStatus;

        private List<Cobranca> _listaOriginal = new List<Cobranca>();
        private FinanceiroRepository _repo = new FinanceiroRepository();

        public TelaFinanceiro()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = CorFundo;
            this.Padding = new Padding(40);
            if (!this.DesignMode) { ConfigurarLayout(); this.Load += (s, e) => CarregarDados(); }
        }

        private void ConfigurarLayout()
        {
            this.Controls.Clear();

            // HEADER
            Panel header = new Panel { Dock = DockStyle.Top, Height = 60 };
            Label lblTitulo = new Label { Text = "Gestão Financeira", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = CorTexto, AutoSize = true, Location = new Point(0, 10) };
            Button btnGerar = new Button { Text = "⚡ Gerar Cobranças", BackColor = CorAzul, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 40, Width = 180, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(this.Width - 220, 10) };
            btnGerar.FlatAppearance.BorderSize = 0; btnGerar.Click += BtnGerar_Click;
            header.Controls.Add(lblTitulo); header.Controls.Add(btnGerar); header.Resize += (s, e) => btnGerar.Left = header.Width - 190;

            // CARDS
            pnlCards = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 10, 0, 20) };
            lblTotalPendente = CriarCard("PENDENTE (MÊS)", "R$ 0,00", CorVermelho, 260, 0);
            lblTotalRecebido = CriarCard("RECEBIDO (MÊS)", "R$ 0,00", CorVerde, 0, 0);
            pnlCards.Controls.Add(lblTotalPendente.Parent); pnlCards.Controls.Add(lblTotalRecebido.Parent);

            // FILTROS
            Panel pnlFiltros = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 15, 0, 15) };
            TableLayoutPanel layoutFiltros = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
            layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            txtBusca = CriarInputBusca("Pesquisar Aluno..."); txtBusca.TextChanged += (s, e) => AplicarFiltros();
            cmbFiltroCategoria = new DarkComboBox { ParentBackColor = CorSidebar, ForeColor = CorTexto, BorderColor = CorBorda }; CarregarCategoriasFiltro(); cmbFiltroCategoria.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cmbFiltroStatus = new DarkComboBox { ParentBackColor = CorSidebar, ForeColor = CorTexto, BorderColor = CorBorda }; cmbFiltroStatus.Items.AddRange(new object[] { "Todos Status", "Pendentes", "Pagos", "Atrasados" }); cmbFiltroStatus.SelectedIndex = 0; cmbFiltroStatus.SelectedIndexChanged += (s, e) => AplicarFiltros();

            Button btnLimpar = new Button { Text = "Limpar", ForeColor = CorTexto, BackColor = CorSidebar, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Fill, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnLimpar.FlatAppearance.BorderSize = 0; btnLimpar.Click += (s, e) => { txtBusca.Text = "Pesquisar Aluno..."; cmbFiltroCategoria.SelectedIndex = 0; cmbFiltroStatus.SelectedIndex = 0; };

            layoutFiltros.Controls.Add(CriarWrapperFiltro(txtBusca), 0, 0); layoutFiltros.Controls.Add(CriarWrapperFiltro(cmbFiltroCategoria), 1, 0); layoutFiltros.Controls.Add(CriarWrapperFiltro(cmbFiltroStatus), 2, 0); layoutFiltros.Controls.Add(CriarWrapperBotao(btnLimpar), 3, 0);
            pnlFiltros.Controls.Add(layoutFiltros);

            // GRID
            Panel gridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            grid = new DataGridView(); ConfigurarEstiloGrid(grid); ConfigurarColunas(grid);
            gridContainer.Controls.Add(grid);

            this.Controls.Add(gridContainer); this.Controls.Add(pnlFiltros); this.Controls.Add(pnlCards); this.Controls.Add(header);
        }

        // ================= CONFIGURAÇÃO VISUAL DAS COLUNAS =================
        private void ConfigurarColunas(DataGridView grid)
        {
            grid.Columns.Clear();

            // ID
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "IdCobranca", Visible = false });

            // Aluno (CENTRALIZADO)
            var colNome = new DataGridViewTextBoxColumn { HeaderText = "Aluno", DataPropertyName = "NomeAluno" };
            colNome.DefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            colNome.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // <--- AGORA CENTRALIZADO
            colNome.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(colNome);

            // Categoria
            var colCat = new DataGridViewTextBoxColumn { HeaderText = "Categoria", DataPropertyName = "CategoriaDescricao", Width = 150 };
            colCat.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(colCat);

            // Vencimento (Negrito + Centro)
            var colVenc = new DataGridViewTextBoxColumn { HeaderText = "Vencimento", DataPropertyName = "DataVencimento", Width = 140 };
            colVenc.DefaultCellStyle.Format = "dd/MM/yyyy";
            colVenc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colVenc.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.Columns.Add(colVenc);

            // Valor (Negrito + Centro)
            var colValor = new DataGridViewTextBoxColumn { HeaderText = "Valor", DataPropertyName = "ValorBase", Width = 140 };
            colValor.DefaultCellStyle.Format = "C2";
            colValor.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colValor.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.Columns.Add(colValor);

            // Status e Ação
            grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Status", DataPropertyName = "StatusDescricao", Width = 150 });
            grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Ação", Width = 120 });

            ConfigurarPinturaGrid();
        }

        // ================= PINTURA CUSTOMIZADA =================
        private void ConfigurarPinturaGrid()
        {
            grid.CellPainting += (s, e) => {
                if (e.RowIndex < 0) return;
                if (e.RowIndex >= grid.Rows.Count) return;
                var item = grid.Rows[e.RowIndex].DataBoundItem as Cobranca;
                if (item == null) return;

                bool isStatus = (grid.Columns[e.ColumnIndex].HeaderText == "Status");
                bool isAcao = (grid.Columns[e.ColumnIndex].HeaderText == "Ação");

                if (isStatus || isAcao)
                {
                    using (Brush b = new SolidBrush(CorFundo)) e.Graphics.FillRectangle(b, e.CellBounds);
                    using (Pen p = new Pen(CorBorda)) e.Graphics.DrawRectangle(p, e.CellBounds);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // STATUS
                    if (isStatus)
                    {
                        string status = item.StatusDescricao;
                        Color bg = CorPendenteFundo;
                        Color txt = CorPendenteTexto;

                        if (status == "Pago") { bg = CorPagoFundo; txt = CorPagoTexto; }
                        else if (status == "Atrasado") { bg = CorAtrasadoFundo; txt = CorAtrasadoTexto; }

                        Rectangle rect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 12, e.CellBounds.Width - 30, e.CellBounds.Height - 24);
                        GraphicsPath path = new GraphicsPath();
                        path.AddArc(rect.X, rect.Y, rect.Height, rect.Height, 90, 180);
                        path.AddArc(rect.Right - rect.Height, rect.Y, rect.Height, rect.Height, 270, 180);
                        path.CloseFigure();

                        using (Brush b = new SolidBrush(bg)) e.Graphics.FillPath(b, path);
                        TextRenderer.DrawText(e.Graphics, status.ToUpper(), new Font("Segoe UI", 9, FontStyle.Bold), rect, txt, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    // AÇÃO
                    else if (isAcao)
                    {
                        if (item.StatusDescricao != "Pago")
                        {
                            Rectangle rect = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 12, e.CellBounds.Width - 20, e.CellBounds.Height - 24);
                            using (Brush brushSuave = new SolidBrush(CorAzulSuave)) e.Graphics.FillRectangle(brushSuave, rect);
                            using (Pen pen = new Pen(CorAzul, 1)) e.Graphics.DrawRectangle(pen, rect);
                            TextRenderer.DrawText(e.Graphics, "💲 BAIXAR", new Font("Segoe UI", 8, FontStyle.Bold), rect, CorAzul, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    e.Handled = true;
                }
            };

            grid.CellContentClick += (s, e) => {
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
                if (grid.Columns[e.ColumnIndex].HeaderText == "Ação")
                {
                    var item = grid.Rows[e.RowIndex].DataBoundItem as Cobranca;
                    if (item != null && item.StatusDescricao != "Pago") ConfirmarRecebimento(item);
                }
            };
        }

        // --- MÉTODOS DE CONFIGURAÇÃO DO GRID (VISUAL) ---
        private void ConfigurarEstiloGrid(DataGridView grid)
        {
            grid.Dock = DockStyle.Fill; grid.BackgroundColor = CorFundo; grid.BorderStyle = BorderStyle.None; grid.CellBorderStyle = DataGridViewCellBorderStyle.Single; grid.GridColor = CorBorda;
            grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle.BackColor = CorSidebar; grid.ColumnHeadersDefaultCellStyle.ForeColor = CorTexto; grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = CorSidebar;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 14, FontStyle.Bold); grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10); grid.ColumnHeadersHeight = 60; grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.AutoGenerateColumns = false; grid.ReadOnly = true; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.RowHeadersVisible = false; grid.AllowUserToResizeRows = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.DefaultCellStyle.BackColor = CorFundo; grid.DefaultCellStyle.ForeColor = CorTexto; grid.DefaultCellStyle.Font = new Font("Segoe UI", 10); grid.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0); grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; grid.RowTemplate.Height = 55; grid.DefaultCellStyle.SelectionBackColor = CorFundo; grid.DefaultCellStyle.SelectionForeColor = CorTexto;
        }

        // --- HELPERS (Cards, Wrappers, Inputs) ---
        private Label CriarCard(string t, string v, Color c, int l, int top) { Panel p = new Panel { Width = 240, Height = 80, BackColor = CorSidebar, Location = new Point(l, top) }; Panel f = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = c }; Label lt = new Label { Text = t, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true }; Label lv = new Label { Text = v, ForeColor = CorTexto, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(12, 35), AutoSize = true }; p.Controls.Add(lt); p.Controls.Add(lv); p.Controls.Add(f); return lv; }
        private Panel CriarWrapperFiltro(Control c) { Panel p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 15, 0), BackColor = Color.Transparent }; Panel b = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = CorSidebar, Padding = new Padding(10, 11, 5, 5) }; b.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, CorBorda, ButtonBorderStyle.Solid); c.Dock = DockStyle.Top; c.BackColor = CorSidebar; c.ForeColor = CorTexto; if (c is TextBox t) t.BorderStyle = BorderStyle.None; b.Controls.Add(c); p.Controls.Add(b); return p; }
        private Panel CriarWrapperBotao(Control c) { Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }; Panel b = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = CorSidebar }; b.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, CorBorda, ButtonBorderStyle.Solid); c.Dock = DockStyle.Fill; b.Controls.Add(c); p.Controls.Add(b); return p; }
        private TextBox CriarInputBusca(string ph) { var t = new TextBox { Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Text = ph }; t.GotFocus += (s, e) => { if (t.Text == ph) { t.Text = ""; t.ForeColor = CorTexto; } }; t.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(t.Text)) { t.Text = ph; t.ForeColor = Color.Gray; } }; return t; }
        private void CarregarCategoriasFiltro() { try { var cats = new EntidadeRepository().ObterCategorias(); cmbFiltroCategoria.Items.Clear(); cmbFiltroCategoria.Items.Add("Todas Categorias"); foreach (var c in cats) cmbFiltroCategoria.Items.Add(c.GetType().GetProperty("Descricao").GetValue(c, null)); cmbFiltroCategoria.SelectedIndex = 0; } catch { } }

        // --- AÇÕES ---
        private void BtnGerar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Gerar cobranças do mês atual?", "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try { _repo.GerarCobrancasMensais(); MessageBox.Show("Processo concluído!", "Sucesso"); CarregarDados(); }
                catch (Exception ex) { MessageBox.Show("Erro ao gerar: " + ex.Message); }
            }
        }

        private void CarregarDados() { try { _listaOriginal = _repo.ObterTodasCobrancas(); AplicarFiltros(); var t = _repo.ObterTotaisMes(DateTime.Now.ToString("MM/yyyy")); lblTotalRecebido.Text = t.Recebido.ToString("C2"); lblTotalPendente.Text = t.Pendente.ToString("C2"); } catch (Exception ex) { MessageBox.Show("Erro ao carregar: " + ex.Message); } }
        private void ConfirmarRecebimento(Cobranca c) { if (MessageBox.Show($"Receber {c.ValorBase:C2}?", "Baixa", MessageBoxButtons.YesNo) == DialogResult.Yes) try { _repo.ReceberMensalidade(c.IdCobranca); CarregarDados(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private void AplicarFiltros()
        {
            if (_listaOriginal == null) return;
            string termo = txtBusca.Text.ToLower().Contains("pesquisar") ? "" : txtBusca.Text.ToLower();
            string cat = cmbFiltroCategoria.SelectedItem?.ToString() ?? "Todas Categorias";
            string st = cmbFiltroStatus.SelectedItem?.ToString() ?? "Todos Status";
            grid.DataSource = _listaOriginal.Where(c =>
                (string.IsNullOrEmpty(termo) || c.NomeAluno.ToLower().Contains(termo)) &&
                (cat == "Todas Categorias" || c.CategoriaDescricao == cat) &&
                (st == "Todos Status" || (st == "Pagos" && c.StatusDescricao == "Pago") || (st == "Pendentes" && c.StatusDescricao == "Pendente") || (st == "Atrasados" && c.StatusDescricao == "Atrasado"))
            ).ToList();
        }
    }
}