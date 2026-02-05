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
        private TextBox _search;
        private DarkComboBox _cmbCat, _cmbStatus, _cmbBolsa;
        private DataGridView _grid;
        private Label _lblTotalBolsistas;
        private Panel _pnlStats;
        private List<dynamic> _data = new List<dynamic>();
        private bool _sortAsc = true;

        private static bool _ignorarMsgStatus = false;

        public event EventHandler IrParaCadastro;
        public event EventHandler<int> EditarAluno;

        public TelaListaAlunos()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            if (!DesignMode)
            {
                SetupUI();
                ApplyTheme();
            }
        }

        public void ApplyTheme()
        {
            BackColor = TemaGlobal.CorFundo;

            if (_grid != null)
            {
                _grid.BackgroundColor = _grid.DefaultCellStyle.BackColor = _grid.DefaultCellStyle.SelectionBackColor = TemaGlobal.CorFundo;
                _grid.GridColor = TemaGlobal.CorBorda;
                _grid.DefaultCellStyle.ForeColor = _grid.DefaultCellStyle.SelectionForeColor = _grid.ColumnHeadersDefaultCellStyle.ForeColor = TemaGlobal.CorTexto;
                _grid.ColumnHeadersDefaultCellStyle.BackColor = _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = TemaGlobal.CorSidebar;
            }

            if (_pnlStats != null)
            {
                bool escuro = TemaGlobal.ModoEscuro;
                _pnlStats.BackColor = escuro ? ColorTranslator.FromHtml("#21262d") : ColorTranslator.FromHtml("#F7F7F8");
                _pnlStats.Invalidate();
            }

            foreach (Control c in Controls)
                RecursiveUpdate(c);

            Invalidate();
        }

        private void RecursiveUpdate(Control c)
        {
            if (c is Label) c.ForeColor = TemaGlobal.CorTexto;
            if (c is TextBox) { c.BackColor = TemaGlobal.CorSidebar; c.ForeColor = TemaGlobal.CorTexto; }
            if (c is DarkComboBox cmb) { cmb.ParentBackColor = TemaGlobal.CorSidebar; cmb.Invalidate(); }
            if (c is Panel) c.Invalidate();
            foreach (Control k in c.Controls) RecursiveUpdate(k);
        }

        private void SetupUI()
        {
            Controls.Clear();

            var h = new Panel { Dock = DockStyle.Top, Height = 80 };
            var t = new Label { Text = "Alunos Cadastrados", Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };

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
                ForeColor = ColorTranslator.FromHtml("#58a6ff"),
                AutoSize = false
            };
            _pnlStats.Controls.Add(_lblTotalBolsistas);

            var b = new Button
            {
                Text = "+  Novo Aluno",
                BackColor = ColorTranslator.FromHtml("#238636"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 40,
                Width = 150,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Width - 190, 15)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => IrParaCadastro?.Invoke(this, EventArgs.Empty);

            h.Resize += (s, e) =>
            {
                b.Left = h.Width - 160;
                _pnlStats.Left = b.Left - _pnlStats.Width - 20;
                _pnlStats.Top = 18;
            };

            h.Controls.Add(t);
            h.Controls.Add(_pnlStats);
            h.Controls.Add(b);

            var f = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 15, 0, 15) };
            var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, BackColor = Color.Transparent };
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));

            _search = Input("Pesquisar por Nome ou CPF...");
            _search.TextChanged += (s, e) => Filter();

            _cmbCat = new DarkComboBox();
            LoadCats();
            _cmbCat.SelectedIndexChanged += (s, e) => Filter();

            _cmbStatus = new DarkComboBox();
            // Mudei a ordem aqui para "Todos Status" ser o primeiro
            _cmbStatus.Items.AddRange(new[] { "Todos Status", "Ativos", "Inativos" });
            _cmbStatus.SelectedIndex = 0; // Padrão: Mostrar Todos
            _cmbStatus.SelectedIndexChanged += (s, e) => Filter();

            _cmbBolsa = new DarkComboBox();
            _cmbBolsa.Items.AddRange(new[] { "Todos Alunos", "Somente Bolsistas", "Não Bolsistas" });
            _cmbBolsa.SelectedIndex = 0;
            _cmbBolsa.SelectedIndexChanged += (s, e) => Filter();

            var bClr = Btn("Limpar");
            bClr.Click += (s, e) =>
            {
                _search.Text = "Pesquisar por Nome ou CPF...";
                _search.ForeColor = Color.Gray;
                _cmbCat.SelectedIndex = 0;
                _cmbStatus.SelectedIndex = 0;
                _cmbBolsa.SelectedIndex = 0;
            };

            tl.Controls.Add(Wrap(_search), 0, 0);
            tl.Controls.Add(Wrap(_cmbCat), 1, 0);
            tl.Controls.Add(Wrap(_cmbStatus), 2, 0);
            tl.Controls.Add(Wrap(_cmbBolsa), 3, 0);
            tl.Controls.Add(WrapBtn(bClr), 4, 0);
            f.Controls.Add(tl);

            var gc = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            _grid = new DataGridView();
            SetupGrid(_grid);
            SetupCols(_grid);
            gc.Controls.Add(_grid);

            Controls.Add(gc);
            Controls.Add(f);
            Controls.Add(h);
            AddFooter();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var r = new EntidadeRepository();
                _data = r.ObterTodos();

                int qtdBolsistas = 0;
                if (_data != null)
                {
                    qtdBolsistas = _data.Count(x =>
                    {
                        try
                        {
                            var pBolsa = x.GetType().GetProperty("bolsista_id");
                            var pStatus = x.GetType().GetProperty("Status");
                            if (pBolsa == null || pStatus == null) return false;
                            var vBolsa = pBolsa.GetValue(x, null);
                            var vStatus = pStatus.GetValue(x, null)?.ToString();

                            // Conta bolsistas apenas se estiverem ATIVOS
                            return vBolsa != null && Convert.ToInt32(vBolsa) != 6 && vStatus != null && vStatus.Equals("Ativo", StringComparison.OrdinalIgnoreCase);
                        }
                        catch { return false; }
                    });
                }
                _lblTotalBolsistas.Text = $"🎓 Bolsistas: {qtdBolsistas}";
                Filter();
            }
            catch { }
        }

        private void Filter()
        {
            if (_data == null) return;

            string t = _search.Text.ToLower().Contains("pesquisar") ? "" : _search.Text.ToLower();
            string c = _cmbCat.SelectedItem?.ToString() ?? "Todas Categorias";
            string s = _cmbStatus.SelectedItem?.ToString() ?? "Todos Status";
            string b = _cmbBolsa.SelectedItem?.ToString() ?? "Todos Alunos";

            var filtered = _data.Where(x =>
                (string.IsNullOrEmpty(t) || (x.Nome != null && x.Nome.ToLower().Contains(t)) || (x.CpfAtleta != null && x.CpfAtleta.Contains(t))) &&
                (c == "Todas Categorias" || (x.CategoriaDescricao != null && x.CategoriaDescricao == c)) &&
                // Lógica de Status: Se for "Todos Status", mostra tudo. Se não, filtra igual.
                (s == "Todos Status" || (x.Status != null && x.Status.Equals(s == "Ativos" ? "Ativo" : "Inativo", StringComparison.OrdinalIgnoreCase))) &&
                (b == "Todos Alunos" || IsBolsistaMatch(x, b))
            ).ToList();

            // Ordena: Ativos primeiro, depois Inativos, depois alfabeticamente
            _grid.DataSource = filtered.OrderBy(x => x.Status.Equals("Ativo", StringComparison.OrdinalIgnoreCase) ? 0 : 1).ThenBy(x => x.Nome).ToList();
        }

        private bool IsBolsistaMatch(dynamic x, string filtro)
        {
            try
            {
                var prop = x.GetType().GetProperty("bolsista_id");
                int? id = null;
                if (prop != null)
                {
                    var val = prop.GetValue(x, null);
                    if (val != null) id = Convert.ToInt32(val);
                }
                bool ehBolsista = (id != null && id != 6);
                if (filtro == "Somente Bolsistas") return ehBolsista;
                if (filtro == "Não Bolsistas") return !ehBolsista;
            }
            catch { return false; }
            return true;
        }

        private void ToggleStatus(int id, string s)
        {
            string novoStatus = s == "Ativo" ? "INATIVAR" : "ATIVAR";

            if (!_ignorarMsgStatus)
            {
                if (!DarkBox.Confirmar($"Deseja realmente {novoStatus} este aluno?", out bool dontAsk))
                    return;

                if (dontAsk) _ignorarMsgStatus = true;
            }

            try
            {
                new EntidadeRepository().AlternarStatus(id);
                LoadData();
            }
            catch (Exception ex)
            {
                DarkBox.Mostrar("Erro: " + ex.Message);
            }
        }

        private void SetupGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 60;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
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
        }

        private void SetupCols(DataGridView g)
        {
            g.Columns.Clear();
            var id = new DataGridViewTextBoxColumn { Name = "id_entidade", DataPropertyName = "id_entidade", HeaderText = "ID", Visible = false };
            g.Columns.Add(id);

            var n = new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome do Aluno", FillWeight = 250 };
            n.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            g.Columns.Add(n);

            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CpfAtleta", HeaderText = "CPF", Width = 140 });
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoriaDescricao", HeaderText = "Categoria", Width = 160 });

            var colStatus = new DataGridViewButtonColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 150 };
            g.Columns.Add(colStatus);

            g.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Editar", Width = 100 });

            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;

                bool st = g.Columns[e.ColumnIndex].HeaderText == "Status";
                bool ed = g.Columns[e.ColumnIndex].HeaderText == "Editar";

                if (st || ed)
                {
                    using (var b = new SolidBrush(TemaGlobal.CorFundo))
                        e.Graphics.FillRectangle(b, e.CellBounds);

                    using (var p = new Pen(TemaGlobal.CorBorda))
                        e.Graphics.DrawRectangle(p, e.CellBounds);

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    if (ed)
                    {
                        var c = ColorTranslator.FromHtml("#d9a648");
                        var bg = Color.FromArgb(25, 217, 166, 72);
                        var r = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 10, e.CellBounds.Width - 20, e.CellBounds.Height - 20);

                        using (var b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, r);
                        using (var p = new Pen(c, 1)) e.Graphics.DrawRectangle(p, r);
                        TextRenderer.DrawText(e.Graphics, "✏️", new Font("Segoe UI Emoji", 10), r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else if (st)
                    {
                        bool at = e.FormattedValue.ToString() == "Ativo";
                        var bg = at ? ColorTranslator.FromHtml("#d1e7dd") : ColorTranslator.FromHtml("#f8d7da");
                        var fg = at ? ColorTranslator.FromHtml("#0f5132") : ColorTranslator.FromHtml("#842029");
                        var r = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 10, e.CellBounds.Width - 30, e.CellBounds.Height - 20);
                        var p = new GraphicsPath();

                        p.AddArc(r.X, r.Y, r.Height, r.Height, 90, 180);
                        p.AddArc(r.Right - r.Height, r.Y, r.Height, r.Height, 270, 180);
                        p.CloseFigure();

                        using (var b = new SolidBrush(bg)) e.Graphics.FillPath(b, p);
                        TextRenderer.DrawText(e.Graphics, e.FormattedValue.ToString().ToUpper(), new Font("Segoe UI", 9, FontStyle.Bold), r, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    e.Handled = true;
                }
            };

            g.CellFormatting += (s, e) =>
            {
                if (e.Value != null && g.Columns[e.ColumnIndex].DataPropertyName == "CpfAtleta")
                {
                    string v = Regex.Replace(e.Value.ToString(), "[^0-9]", "");
                    if (v.Length == 11)
                    {
                        e.Value = Convert.ToUInt64(v).ToString(@"000\.000\.000\-00");
                        e.FormattingApplied = true;
                    }
                }
            };

            g.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var v = g.Rows[e.RowIndex].Cells["id_entidade"].Value;
                if (v == null) return;

                int id = Convert.ToInt32(v);

                if (g.Columns[e.ColumnIndex].HeaderText == "Status")
                    ToggleStatus(id, g.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                else if (g.Columns[e.ColumnIndex].HeaderText == "Editar")
                    EditarAluno?.Invoke(this, id);
            };
        }

        private void OrdenarGrid(string propriedade)
        {
            if (_grid.DataSource == null) return;
            var lista = (List<dynamic>)_grid.DataSource;
            if (_sortAsc) lista = lista.OrderBy(x => GetPropValue(x, propriedade)).ToList();
            else lista = lista.OrderByDescending(x => GetPropValue(x, propriedade)).ToList();
            _grid.DataSource = lista;
            _sortAsc = !_sortAsc;
        }

        private object GetPropValue(object src, string propName)
        {
            try { return src.GetType().GetProperty(propName).GetValue(src, null); } catch { return ""; }
        }

        private void LoadCats()
        {
            try
            {
                var c = new EntidadeRepository().ObterCategorias();
                _cmbCat.Items.Clear();
                _cmbCat.Items.Add("Todas Categorias");
                foreach (var i in c) _cmbCat.Items.Add(i.GetType().GetProperty("Descricao").GetValue(i, null));
                _cmbCat.SelectedIndex = 0;
            }
            catch { }
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
                if (c is TextBox t) { t.BorderStyle = BorderStyle.None; t.BackColor = TemaGlobal.CorSidebar; t.ForeColor = TemaGlobal.CorTexto; }
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
            var b = new Button { Text = t, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Dock = DockStyle.Fill };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}