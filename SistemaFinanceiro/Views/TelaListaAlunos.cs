using SistemaFinanceiro.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public partial class TelaListaAlunos : UserControl
    {
        // ================= CORES (PADRÃO DARK) =================
        private readonly Color CorFundo = ColorTranslator.FromHtml("#0d1117");
        private readonly Color CorSidebar = ColorTranslator.FromHtml("#161b22");
        private readonly Color CorTexto = ColorTranslator.FromHtml("#c9d1d9");
        private readonly Color CorBorda = ColorTranslator.FromHtml("#30363d");

        // Cores Botões Principais
        private readonly Color CorBotaoNovo = ColorTranslator.FromHtml("#238636");
        // private readonly Color CorBotaoLimpar = ColorTranslator.FromHtml("#30363d"); // Não usado diretamente, mas parte do tema

        // Cores Ação (Editar/Excluir)
        private readonly Color CorEditar = ColorTranslator.FromHtml("#d9a648"); // Amarelo
        private readonly Color CorExcluir = ColorTranslator.FromHtml("#ff7b72"); // Vermelho
        private readonly Color CorEditarSuave = Color.FromArgb(25, 217, 166, 72);
        private readonly Color CorExcluirSuave = Color.FromArgb(25, 255, 123, 114);

        // Cores Badge Status
        private readonly Color CorAtivoFundo = ColorTranslator.FromHtml("#d1e7dd"); // Verde Claro
        private readonly Color CorAtivoTexto = ColorTranslator.FromHtml("#0f5132"); // Verde Escuro
        private readonly Color CorInativoFundo = ColorTranslator.FromHtml("#f8d7da"); // Vermelho Claro
        private readonly Color CorInativoTexto = ColorTranslator.FromHtml("#842029"); // Vermelho Escuro

        private static DateTime? _confirmacaoDesativadaAte = null;

        // CONTROLES
        private TextBox txtBusca;
        private DarkComboBox cmbFiltroCategoria;
        private DarkComboBox cmbFiltroStatus;
        private DataGridView _grid;

        // DADOS
        private List<dynamic> _listaOriginal = new List<dynamic>();

        // EVENTOS
        public event EventHandler IrParaCadastro;
        public event EventHandler<int> EditarAluno;
        public event EventHandler<int> ExcluirAluno;

        public TelaListaAlunos()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = CorFundo;
            this.Padding = new Padding(40);
            if (!this.DesignMode) ConfigurarLayout();
        }

        private void ConfigurarLayout()
        {
            this.Controls.Clear();

            // 1. HEADER
            Panel header = new Panel { Dock = DockStyle.Top, Height = 60 };
            Label lblTitulo = new Label { Text = "Alunos Cadastrados", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = CorTexto, AutoSize = true, Location = new Point(0, 10) };

            Button btnNovo = new Button { Text = "+  Novo Aluno", BackColor = CorBotaoNovo, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 40, Width = 150, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(this.Width - 190, 10) };
            btnNovo.FlatAppearance.BorderSize = 0;
            btnNovo.Click += (s, e) => IrParaCadastro?.Invoke(this, EventArgs.Empty);
            header.Resize += (s, e) => btnNovo.Left = header.Width - 160;
            header.Controls.Add(lblTitulo); header.Controls.Add(btnNovo);

            // 2. FILTROS (4 Colunas agora)
            Panel pnlFiltros = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 15, 0, 15) };
            TableLayoutPanel layoutFiltros = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
            layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Busca
            layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Categoria
            layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Status
            layoutFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Limpar

            // Inputs
            txtBusca = CriarInputBusca("Pesquisar por Nome ou CPF...");
            txtBusca.TextChanged += (s, e) => AplicarFiltros();

            cmbFiltroCategoria = new DarkComboBox { ParentBackColor = CorSidebar, ForeColor = CorTexto, BorderColor = CorBorda };
            CarregarCategoriasFiltro();
            cmbFiltroCategoria.SelectedIndexChanged += (s, e) => AplicarFiltros();

            cmbFiltroStatus = new DarkComboBox { ParentBackColor = CorSidebar, ForeColor = CorTexto, BorderColor = CorBorda };
            cmbFiltroStatus.Items.AddRange(new object[] { "Todos Status", "Ativos", "Inativos" });
            cmbFiltroStatus.SelectedIndex = 0;
            cmbFiltroStatus.SelectedIndexChanged += (s, e) => AplicarFiltros();

            Button btnLimpar = CriarBotaoLimpar();
            btnLimpar.Click += (s, e) => LimparFiltros();

            // Adiciona wrappers
            layoutFiltros.Controls.Add(CriarWrapperFiltro(txtBusca), 0, 0);
            layoutFiltros.Controls.Add(CriarWrapperFiltro(cmbFiltroCategoria), 1, 0);
            layoutFiltros.Controls.Add(CriarWrapperFiltro(cmbFiltroStatus), 2, 0);
            layoutFiltros.Controls.Add(CriarWrapperBotao(btnLimpar), 3, 0);
            pnlFiltros.Controls.Add(layoutFiltros);

            // 3. GRID
            Panel gridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            _grid = new DataGridView();
            ConfigurarEstiloGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            gridContainer.Controls.Add(_grid);

            this.Controls.Add(gridContainer);
            this.Controls.Add(pnlFiltros);
            this.Controls.Add(header);

            CarregarDadosNoGrid();
        }

        // ================= WRAPPERS E HELPERS =================
        private Panel CriarWrapperFiltro(Control ctrl)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 15, 0), BackColor = Color.Transparent };
            Panel b = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = CorSidebar, Padding = new Padding(10, 11, 5, 5) };
            b.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, CorBorda, ButtonBorderStyle.Solid);
            ctrl.Dock = DockStyle.Top; ctrl.BackColor = CorSidebar; ctrl.ForeColor = CorTexto;
            if (ctrl is TextBox t) t.BorderStyle = BorderStyle.None;
            b.Controls.Add(ctrl); p.Controls.Add(b); return p;
        }

        private Panel CriarWrapperBotao(Control ctrl)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Panel b = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = CorSidebar };
            b.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, CorBorda, ButtonBorderStyle.Solid);
            ctrl.Dock = DockStyle.Fill; b.Controls.Add(ctrl); p.Controls.Add(b); return p;
        }

        private TextBox CriarInputBusca(string ph)
        {
            var txt = new TextBox { Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Text = ph };
            txt.GotFocus += (s, e) => { if (txt.Text == ph) { txt.Text = ""; txt.ForeColor = CorTexto; } };
            txt.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = ph; txt.ForeColor = Color.Gray; } };
            return txt;
        }

        private Button CriarBotaoLimpar()
        {
            Button btn = new Button { Text = "Limpar", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = CorTexto, BackColor = CorSidebar, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Dock = DockStyle.Fill };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ================= GRID =================
        private void ConfigurarEstiloGrid(DataGridView grid)
        {
            grid.AllowUserToResizeColumns = false; grid.AllowUserToResizeRows = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false; grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = CorFundo;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            grid.GridColor = CorBorda;

            // Correção do Bug Azul
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle.BackColor = CorSidebar;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = CorTexto;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = CorSidebar; // Remove azul
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10);
            grid.ColumnHeadersHeight = 60;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            grid.DefaultCellStyle.BackColor = CorFundo;
            grid.DefaultCellStyle.ForeColor = CorTexto;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            grid.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.RowTemplate.Height = 55;
            grid.DefaultCellStyle.SelectionBackColor = CorFundo; // Remove azul
            grid.DefaultCellStyle.SelectionForeColor = CorTexto;
        }

        private void CarregarDadosNoGrid()
        {
            try
            {
                if (_grid.Columns.Count == 0)
                {
                    _grid.AutoGenerateColumns = false;
                    AdicionarColuna(_grid, "id_entidade", "ID", 0).Visible = false;

                    var colNome = AdicionarColuna(_grid, "Nome", "Nome do Aluno");
                    colNome.FillWeight = 250;
                    colNome.DefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Regular);
                    colNome.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; // Alinhado esquerda

                    AdicionarColuna(_grid, "CpfAtleta", "CPF", 140);
                    AdicionarColuna(_grid, "CategoriaDescricao", "Categoria", 160);

                    _grid.Columns.Add(new DataGridViewButtonColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 150 });
                    _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Editar", Width = 100 });
                    _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Excluir", Width = 100 });

                    ConfigurarEventosGrid();
                }

                var repo = new EntidadeRepository();
                _listaOriginal = repo.ObterTodos();
                AplicarFiltros();
            }
            catch { }
        }

        private void ConfigurarEventosGrid()
        {
            _grid.CellPainting += (s, e) => {
                if (e.RowIndex < 0) return;

                // Proteção contra linhas vazias
                if (e.RowIndex >= _grid.Rows.Count) return;

                bool isStatus = (_grid.Columns[e.ColumnIndex].HeaderText == "Status");
                bool isEditar = (_grid.Columns[e.ColumnIndex].HeaderText == "Editar");
                bool isExcluir = (_grid.Columns[e.ColumnIndex].HeaderText == "Excluir");

                if (isStatus || isEditar || isExcluir)
                {
                    using (Brush b = new SolidBrush(CorFundo)) e.Graphics.FillRectangle(b, e.CellBounds);
                    using (Pen p = new Pen(CorBorda)) e.Graphics.DrawRectangle(p, e.CellBounds);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    if (isEditar || isExcluir)
                    {
                        Color corBorda = isEditar ? CorEditar : CorExcluir;
                        Color corFundo = isEditar ? CorEditarSuave : CorExcluirSuave;
                        string icone = isEditar ? "✏️" : "🗑️";

                        Rectangle rect = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 12, e.CellBounds.Width - 20, e.CellBounds.Height - 24);
                        using (Brush brushSuave = new SolidBrush(corFundo)) e.Graphics.FillRectangle(brushSuave, rect);
                        using (Pen pen = new Pen(corBorda, 1)) e.Graphics.DrawRectangle(pen, rect);
                        TextRenderer.DrawText(e.Graphics, icone, new Font("Segoe UI Emoji", 10), rect, corBorda, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else if (isStatus)
                    {
                        string statusValor = e.FormattedValue.ToString();
                        bool ativo = statusValor == "Ativo";
                        Color corFundo = ativo ? CorAtivoFundo : CorInativoFundo;
                        Color corTexto = ativo ? CorAtivoTexto : CorInativoTexto;

                        Rectangle rect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 12, e.CellBounds.Width - 30, e.CellBounds.Height - 24);
                        GraphicsPath path = new GraphicsPath();

                        // CORREÇÃO DO ERRO DA VARIÁVEL RADIUS:
                        // Declaramos a variável ANTES de usá-la no path.AddArc
                        int radius = rect.Height;

                        path.AddArc(rect.X, rect.Y, radius, radius, 90, 180);
                        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 180);
                        path.CloseFigure();

                        using (Brush b = new SolidBrush(corFundo)) e.Graphics.FillPath(b, path);
                        TextRenderer.DrawText(e.Graphics, statusValor.ToUpper(), new Font("Segoe UI", 9, FontStyle.Bold), rect, corTexto, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    e.Handled = true;
                }
            };

            _grid.CellFormatting += (s, e) => {
                if (e.Value != null && _grid.Columns[e.ColumnIndex].DataPropertyName == "CpfAtleta")
                {
                    string val = Regex.Replace(e.Value.ToString(), "[^0-9]", "");
                    if (val.Length == 11) { e.Value = Convert.ToUInt64(val).ToString(@"000\.000\.000\-00"); e.FormattingApplied = true; }
                }
            };

            _grid.CellContentClick += (s, e) => {
                if (e.RowIndex < 0) return;
                var idVal = _grid.Rows[e.RowIndex].Cells["id_entidade"].Value;
                if (idVal == null) return;
                int id = Convert.ToInt32(idVal);

                if (_grid.Columns[e.ColumnIndex].HeaderText == "Status") ProcessarTrocaStatus(id, _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                else if (_grid.Columns[e.ColumnIndex].HeaderText == "Editar") EditarAluno?.Invoke(this, id);
                else if (_grid.Columns[e.ColumnIndex].HeaderText == "Excluir") ConfirmarExclusao(id);
            };
        }

        // ================= LÓGICA E FILTROS =================
        private void LimparFiltros()
        {
            txtBusca.Text = "Pesquisar por Nome ou CPF..."; txtBusca.ForeColor = Color.Gray;
            if (cmbFiltroCategoria.Items.Count > 0) cmbFiltroCategoria.SelectedIndex = 0;
            if (cmbFiltroStatus.Items.Count > 0) cmbFiltroStatus.SelectedIndex = 0;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_listaOriginal == null) return;
            string termo = txtBusca.Text.ToLower().Contains("pesquisar") ? "" : txtBusca.Text.ToLower();
            string cat = cmbFiltroCategoria.SelectedItem?.ToString() ?? "Todas Categorias";
            string st = cmbFiltroStatus.SelectedItem?.ToString() ?? "Todos Status";

            var lista = _listaOriginal.Where(aluno =>
                (string.IsNullOrEmpty(termo) || (aluno.Nome != null && aluno.Nome.ToLower().Contains(termo)) || (aluno.CpfAtleta != null && aluno.CpfAtleta.Contains(termo))) &&
                (cat == "Todas Categorias" || (aluno.CategoriaDescricao != null && aluno.CategoriaDescricao == cat)) &&
                (st == "Todos Status" || (st == "Ativos" && aluno.Status == "Ativo") || (st == "Inativos" && aluno.Status == "Inativo"))
            ).ToList();
            _grid.DataSource = lista;
        }

        private void ProcessarTrocaStatus(int id, string statusAtual)
        {
            if (_confirmacaoDesativadaAte.HasValue && DateTime.Now > _confirmacaoDesativadaAte.Value) _confirmacaoDesativadaAte = null;
            if (_confirmacaoDesativadaAte == null)
            {
                string novoStatus = statusAtual == "Ativo" ? "INATIVAR" : "ATIVAR";
                if (!MostrarConfirmacaoVisual($"Deseja realmente {novoStatus} este aluno?", out bool naoPerguntarHoje)) return;
                if (naoPerguntarHoje) _confirmacaoDesativadaAte = DateTime.Today.AddDays(1);
            }
            try { new EntidadeRepository().AlternarStatus(id); CarregarDadosNoGrid(); } catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void CarregarCategoriasFiltro() { try { var cats = new EntidadeRepository().ObterCategorias(); cmbFiltroCategoria.Items.Clear(); cmbFiltroCategoria.Items.Add("Todas Categorias"); foreach (var c in cats) cmbFiltroCategoria.Items.Add(c.GetType().GetProperty("Descricao").GetValue(c, null)); cmbFiltroCategoria.SelectedIndex = 0; } catch { } }
        private void ConfirmarExclusao(int id) { if (MostrarConfirmacaoVisual("Tem certeza que deseja excluir este aluno?", out bool _)) ExcluirAluno?.Invoke(this, id); }
        private DataGridViewTextBoxColumn AdicionarColuna(DataGridView grid, string prop, string titulo, int largura = 0) { DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn(); if (!string.IsNullOrEmpty(prop)) { col.DataPropertyName = prop; col.Name = prop; } col.HeaderText = titulo; if (largura > 0) { col.Width = largura; col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; } grid.Columns.Add(col); return col; }

        private bool MostrarConfirmacaoVisual(string mensagem, out bool dontAskAgain)
        {
            dontAskAgain = false;
            Form form = new Form { StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.None, Size = new Size(450, 250), BackColor = CorSidebar, Padding = new Padding(20) };
            form.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, form.ClientRectangle, CorTexto, ButtonBorderStyle.Solid);
            Label lblFechar = new Label { Text = "✕", AutoSize = true, ForeColor = Color.Gray, BackColor = Color.Transparent, Font = new Font("Segoe UI", 14), Cursor = Cursors.Hand, Location = new Point(form.Width - 35, 10) };
            lblFechar.Click += (s, e) => form.DialogResult = DialogResult.Cancel;
            form.Controls.Add(lblFechar);
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            Label lblMsg = new Label { Text = mensagem, ForeColor = CorTexto, Font = new Font("Segoe UI", 14), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
            CheckBox chk = new CheckBox { Text = "Não perguntar novamente hoje", ForeColor = ColorTranslator.FromHtml("#8b949e"), Font = new Font("Segoe UI", 10), AutoSize = true, Anchor = AnchorStyles.None };
            Panel pnlBotoes = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 20, 0, 0) };
            Button btnNao = new Button { Cursor = Cursors.Hand, Size = new Size(150, 50), DialogResult = DialogResult.No, Dock = DockStyle.Left }; AplicarEstiloBotaoDialogo(btnNao, CorExcluir, Color.White, "CANCELAR");
            Button btnSim = new Button { Cursor = Cursors.Hand, Size = new Size(150, 50), DialogResult = DialogResult.Yes, Dock = DockStyle.Right }; AplicarEstiloBotaoDialogo(btnSim, CorBotaoNovo, Color.White, "CONFIRMAR");
            pnlBotoes.Controls.Add(btnNao); pnlBotoes.Controls.Add(btnSim); layout.Controls.Add(lblMsg, 0, 0); layout.Controls.Add(chk, 0, 1); layout.Controls.Add(pnlBotoes, 0, 2); form.Controls.Add(layout);
            var result = form.ShowDialog(); dontAskAgain = chk.Checked; return result == DialogResult.Yes;
        }
        private void AplicarEstiloBotaoDialogo(Button btn, Color c, Color t, string txt) { btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 0; btn.BackColor = Color.Transparent; btn.ForeColor = t; btn.Text = ""; btn.Paint += (s, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1); using (Brush b = new SolidBrush(c)) e.Graphics.FillRectangle(b, r); using (Pen p = new Pen(Color.White, 1)) e.Graphics.DrawRectangle(p, r); TextRenderer.DrawText(e.Graphics, txt, new Font("Segoe UI", 11, FontStyle.Bold), r, t, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }; }
    }

    public class DarkComboBox : ComboBox
    {
        public Color ParentBackColor { get; set; } = ColorTranslator.FromHtml("#161b22");
        public Color BorderColor { get; set; } = ColorTranslator.FromHtml("#30363d");
        public DarkComboBox() { SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true); DrawMode = DrawMode.OwnerDrawFixed; DropDownStyle = ComboBoxStyle.DropDownList; FlatStyle = FlatStyle.Flat; Font = new Font("Segoe UI", 11); ItemHeight = 26; IntegralHeight = false; }
        protected override void OnPaint(PaintEventArgs e) { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var b = new SolidBrush(ParentBackColor)) e.Graphics.FillRectangle(b, this.ClientRectangle); string t = this.SelectedItem != null ? this.SelectedItem.ToString() : this.Text; if (this.Items.Count > 0 && this.SelectedIndex == -1 && !string.IsNullOrEmpty(Text)) t = Text; TextRenderer.DrawText(e.Graphics, t, this.Font, new Point(0, 4), this.ForeColor, TextFormatFlags.Left); int x = this.Width - 15, y = (this.Height - 6) / 2; Point[] p = { new Point(x, y), new Point(x + 8, y), new Point(x + 4, y + 5) }; using (var b = new SolidBrush(Color.Gray)) e.Graphics.FillPolygon(b, p); }
        protected override void OnDrawItem(DrawItemEventArgs e) { if (e.Index < 0) return; bool s = (e.State & DrawItemState.Selected) == DrawItemState.Selected; Color c = s ? BorderColor : ParentBackColor; using (var b = new SolidBrush(c)) e.Graphics.FillRectangle(b, e.Bounds); using (var b = new SolidBrush(this.ForeColor)) e.Graphics.DrawString(this.Items[e.Index].ToString(), this.Font, b, new Point(e.Bounds.X + 2, e.Bounds.Y + 4)); }
    }
}