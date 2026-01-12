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
        private Label _lblRec, _lblPend;
        private DataGridView _grid;
        private Panel _pnlCards;
        private TextBox _inputSearch;
        private DarkComboBox _cmbCat, _cmbStatus;
        private List<Cobranca> _data = new List<Cobranca>();
        private FinanceiroRepository _repo = new FinanceiroRepository();

        public TelaFinanceiro()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            if (!DesignMode) { SetupUI(); ApplyTheme(); Load += (s, e) => LoadData(); }
        }

        public void ApplyTheme()
        {
            BackColor = TemaGlobal.CorFundo;
            foreach (Control c in _pnlCards.Controls) if (c is Panel p) { p.BackColor = TemaGlobal.CorSidebar; foreach (Control l in p.Controls) if (l is Label && l.Font.Size > 12) l.ForeColor = TemaGlobal.CorTexto; }
            foreach (Control c in Controls) if (c is Panel && c.Height == 80) c.Invalidate(true);
            if (_grid != null) { _grid.BackgroundColor = _grid.DefaultCellStyle.BackColor = _grid.DefaultCellStyle.SelectionBackColor = TemaGlobal.CorFundo; _grid.GridColor = TemaGlobal.CorBorda; _grid.DefaultCellStyle.ForeColor = _grid.DefaultCellStyle.SelectionForeColor = _grid.ColumnHeadersDefaultCellStyle.ForeColor = TemaGlobal.CorTexto; _grid.ColumnHeadersDefaultCellStyle.BackColor = _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = TemaGlobal.CorSidebar; }
            Invalidate();
        }

        private void SetupUI()
        {
            Controls.Clear();

            // Header
            var h = new Panel { Dock = DockStyle.Top, Height = 60 };
            var t = new Label { Text = "Gestão Financeira", Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };
            var b = new Button { Text = "⚡ Gerar Cobranças", BackColor = ColorTranslator.FromHtml("#2f81f7"), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 40, Width = 180, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(Width - 220, 10) };
            b.FlatAppearance.BorderSize = 0; b.Click += OnGen;
            h.Controls.Add(t); h.Controls.Add(b); h.Resize += (s, e) => b.Left = h.Width - 190;

            // Cards
            _pnlCards = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 10, 0, 20) };
            _lblPend = Card("PENDENTE (MÊS)", "R$ 0,00", ColorTranslator.FromHtml("#da3633"), 260);
            _lblRec = Card("RECEBIDO (MÊS)", "R$ 0,00", ColorTranslator.FromHtml("#238636"), 0);
            _pnlCards.Controls.Add(_lblPend.Parent); _pnlCards.Controls.Add(_lblRec.Parent);

            // Filters
            var f = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 15, 0, 15) };
            var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            _inputSearch = Input("Pesquisar Aluno"); _inputSearch.TextChanged += (s, e) => Filter();
            _cmbCat = new DarkComboBox(); LoadCats(); _cmbCat.SelectedIndexChanged += (s, e) => Filter();
            _cmbStatus = new DarkComboBox(); _cmbStatus.Items.AddRange(new[] { "Todos Status", "Pendentes", "Pagos", "Atrasados" }); _cmbStatus.SelectedIndex = 0; _cmbStatus.SelectedIndexChanged += (s, e) => Filter();
            var bClr = Btn("Limpar Filtros"); bClr.Click += (s, e) => { _inputSearch.Text = "Pesquisar Aluno"; _cmbCat.SelectedIndex = 0; _cmbStatus.SelectedIndex = 0; };

            tl.Controls.Add(Wrap(_inputSearch), 0, 0); tl.Controls.Add(Wrap(_cmbCat), 1, 0); tl.Controls.Add(Wrap(_cmbStatus), 2, 0); tl.Controls.Add(WrapBtn(bClr), 3, 0);
            f.Controls.Add(tl);

            // Grid
            var gp = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            _grid = new DataGridView(); SetupGrid(_grid); SetupCols(_grid); gp.Controls.Add(_grid);

            Controls.Add(gp); Controls.Add(f); Controls.Add(_pnlCards); Controls.Add(h);
            AddFooter();
        }

        private void AddFooter()
        {
            var f = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Transparent, Padding = new Padding(0, 0, 20, 10) };
            f.Controls.Add(new Label { Text = "@rodrigolopes_rf", Font = new Font("Segoe UI", 10), ForeColor = TemaGlobal.CorTexto, AutoSize = true, Dock = DockStyle.Right, Padding = new Padding(5, 2, 0, 0) });
            f.Controls.Add(new Label { Text = "📷", Font = new Font("Segoe UI Emoji", 12), ForeColor = TemaGlobal.CorTexto, AutoSize = true, Dock = DockStyle.Right });
            Controls.Add(f);
        }

        // Wrappers
        private Panel Wrap(Control c)
        {
            var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 10, 0), BackColor = Color.Transparent };
            var b = new Panel { Dock = DockStyle.Top, Height = 45 };
            b.Paint += (s, e) => { b.BackColor = TemaGlobal.CorSidebar; ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, TemaGlobal.CorBorda, ButtonBorderStyle.Solid); };
            if (c is ComboBox)
            {
                var m = new Panel { BackColor = TemaGlobal.CorSidebar, Width = b.Width - 12, Height = c.Height - 6, Location = new Point(6, (b.Height - c.Height + 6) / 2) };
                c.Dock = DockStyle.None; c.Location = new Point(-2, -3); c.Width = m.Width + 20; m.Controls.Add(c); b.Controls.Add(m);
                b.Resize += (s, e) => { m.Width = b.Width - 12; m.Location = new Point(6, (b.Height - m.Height) / 2); c.Width = m.Width + 20; };
            }
            else
            {
                c.Dock = DockStyle.None; c.Anchor = AnchorStyles.Left | AnchorStyles.Right; c.Width = b.Width - 20; c.Location = new Point(10, (b.Height - c.Height) / 2);
                if (c is TextBox t) { t.BorderStyle = BorderStyle.None; t.BackColor = TemaGlobal.CorSidebar; t.ForeColor = TemaGlobal.CorTexto; }
                b.Controls.Add(c);
            }
            p.Controls.Add(b); return p;
        }
        private Panel WrapBtn(Control c) { var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }; var b = new Panel { Dock = DockStyle.Top, Height = 45 }; b.Paint += (s, e) => { b.BackColor = TemaGlobal.CorSidebar; ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, TemaGlobal.CorBorda, ButtonBorderStyle.Solid); c.BackColor = TemaGlobal.CorSidebar; c.ForeColor = TemaGlobal.CorTexto; }; c.Dock = DockStyle.Fill; b.Controls.Add(c); p.Controls.Add(b); return p; }

        // Helpers
        private TextBox Input(string ph) { var t = new TextBox { Font = new Font("Segoe UI", 11), Text = ph, TextAlign = HorizontalAlignment.Center }; t.GotFocus += (s, e) => { if (t.Text == ph) { t.Text = ""; t.ForeColor = TemaGlobal.CorTexto; } }; t.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(t.Text)) { t.Text = ph; t.ForeColor = Color.Gray; } }; return t; }
        private Button Btn(string t) { var b = new Button { Text = t, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Fill, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) }; b.FlatAppearance.BorderSize = 0; return b; }
        private Label Card(string t, string v, Color c, int l) { var p = new Panel { Width = 240, Height = 80, BackColor = TemaGlobal.CorSidebar, Location = new Point(l, 0) }; var f = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = c }; var lt = new Label { Text = t, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true }; var lv = new Label { Text = v, ForeColor = TemaGlobal.CorTexto, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(12, 35), AutoSize = true }; p.Controls.Add(lt); p.Controls.Add(lv); p.Controls.Add(f); return lv; }
        private void LoadCats() { try { var c = new EntidadeRepository().ObterCategorias(); _cmbCat.Items.Clear(); _cmbCat.Items.Add("Todas Categorias"); foreach (var i in c) _cmbCat.Items.Add(i.GetType().GetProperty("Descricao").GetValue(i, null)); _cmbCat.SelectedIndex = 0; } catch { } }

        // Logic
        private void Filter() { if (_data == null) return; string t = _inputSearch.Text.ToLower().Contains("pesquisar") ? "" : _inputSearch.Text.ToLower(); string c = _cmbCat.SelectedItem?.ToString() ?? "Todas Categorias"; string s = _cmbStatus.SelectedItem?.ToString() ?? "Todos Status"; var hj = DateTime.Today; _grid.DataSource = _data.Where(x => (x.StatusAluno == "Ativo") && (string.IsNullOrEmpty(t) || x.NomeAluno.ToLower().Contains(t)) && (c == "Todas Categorias" || x.CategoriaDescricao == c) && (s == "Todos Status" || (s == "Pagos" && x.StatusId == 2) || (s == "Pendentes" && x.StatusId == 1 && x.DataVencimento.Date >= hj) || (s == "Atrasados" && x.StatusId == 1 && x.DataVencimento.Date < hj))).ToList(); }
        private void OnGen(object s, EventArgs e) { if (DarkBox.Confirmar("Gerar cobranças do mês?", out bool _)) { try { _repo.GerarCobrancasMensais(); DarkBox.Mostrar("Sucesso!", "Ok"); LoadData(); } catch (Exception ex) { DarkBox.Mostrar(ex.Message); } } }
        private void LoadData() { try { _data = _repo.ObterTodasCobrancas(); Filter(); var t = _repo.ObterTotaisMes(DateTime.Now.ToString("MM/yyyy")); _lblRec.Text = t.Recebido.ToString("C2"); _lblPend.Text = t.Pendente.ToString("C2"); } catch (Exception ex) { DarkBox.Mostrar(ex.Message); } }
        private void OnPay(Cobranca c) { if (DarkBox.Confirmar($"Receber {c.ValorBase:C2}?", out bool _)) try { _repo.ReceberMensalidade(c.IdCobranca); LoadData(); } catch (Exception ex) { DarkBox.Mostrar(ex.Message); } }

        // Grid
        private void SetupGrid(DataGridView g) { g.Dock = DockStyle.Fill; g.BackgroundColor = TemaGlobal.CorFundo; g.BorderStyle = BorderStyle.None; g.CellBorderStyle = DataGridViewCellBorderStyle.Single; g.GridColor = TemaGlobal.CorBorda; g.EnableHeadersVisualStyles = false; g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single; g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; g.ColumnHeadersHeight = 50; g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 14, FontStyle.Bold); g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10); g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; g.AutoGenerateColumns = false; g.ReadOnly = true; g.SelectionMode = DataGridViewSelectionMode.FullRowSelect; g.RowHeadersVisible = false; g.AllowUserToResizeRows = false; g.AllowUserToResizeColumns = false; g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; g.RowTemplate.Height = 45; g.DefaultCellStyle.Font = new Font("Segoe UI", 10); g.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0); g.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; g.DefaultCellStyle.SelectionBackColor = TemaGlobal.CorFundo; g.DefaultCellStyle.SelectionForeColor = TemaGlobal.CorTexto; }
        private void SetupCols(DataGridView g)
        {
            g.Columns.Clear(); g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "IdCobranca", Visible = false });
            var n = new DataGridViewTextBoxColumn { HeaderText = "Aluno", DataPropertyName = "NomeAluno", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }; n.DefaultCellStyle.Font = new Font("Segoe UI", 12); n.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter; g.Columns.Add(n);
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoria", DataPropertyName = "CategoriaDescricao", Width = 150 });
            var v = new DataGridViewTextBoxColumn { HeaderText = "Vencimento", DataPropertyName = "DataVencimento", Width = 140 }; v.DefaultCellStyle.Format = "dd/MM/yyyy"; v.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); g.Columns.Add(v);
            var val = new DataGridViewTextBoxColumn { HeaderText = "Valor", DataPropertyName = "ValorBase", Width = 140 }; val.DefaultCellStyle.Format = "C2"; val.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); g.Columns.Add(val);
            g.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Status", DataPropertyName = "StatusDescricao", Width = 150 }); g.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Ação", Width = 120 });

            g.CellPainting += (s, e) => {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;
                var i = g.Rows[e.RowIndex].DataBoundItem as Cobranca; if (i == null) return;
                bool st = g.Columns[e.ColumnIndex].HeaderText == "Status", ac = g.Columns[e.ColumnIndex].HeaderText == "Ação";
                if (st || ac)
                {
                    using (var b = new SolidBrush(TemaGlobal.CorFundo)) e.Graphics.FillRectangle(b, e.CellBounds);
                    using (var p = new Pen(TemaGlobal.CorBorda)) e.Graphics.DrawRectangle(p, e.CellBounds);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    if (st)
                    {
                        var c = i.StatusDescricao == "Pago" ? ColorTranslator.FromHtml("#d1e7dd") : i.StatusDescricao == "Atrasado" ? ColorTranslator.FromHtml("#f8d7da") : ColorTranslator.FromHtml("#fff8c5");
                        var t = i.StatusDescricao == "Pago" ? ColorTranslator.FromHtml("#0f5132") : i.StatusDescricao == "Atrasado" ? ColorTranslator.FromHtml("#842029") : ColorTranslator.FromHtml("#d29922");
                        var r = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 12, e.CellBounds.Width - 30, e.CellBounds.Height - 24);
                        var path = new GraphicsPath(); path.AddArc(r.X, r.Y, r.Height, r.Height, 90, 180); path.AddArc(r.Right - r.Height, r.Y, r.Height, r.Height, 270, 180); path.CloseFigure();
                        using (var b = new SolidBrush(c)) e.Graphics.FillPath(b, path); TextRenderer.DrawText(e.Graphics, i.StatusDescricao.ToUpper(), new Font("Segoe UI", 9, FontStyle.Bold), r, t, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else if (ac && i.StatusDescricao != "Pago")
                    {
                        var r = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 12, e.CellBounds.Width - 20, e.CellBounds.Height - 24);
                        var c = ColorTranslator.FromHtml("#2f81f7"); using (var b = new SolidBrush(Color.FromArgb(40, c))) e.Graphics.FillRectangle(b, r); using (var p = new Pen(c, 1)) e.Graphics.DrawRectangle(p, r); TextRenderer.DrawText(e.Graphics, "💲 BAIXAR", new Font("Segoe UI", 8, FontStyle.Bold), r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    e.Handled = true;
                }
            };
            g.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && g.Columns[e.ColumnIndex].HeaderText == "Ação") { var i = g.Rows[e.RowIndex].DataBoundItem as Cobranca; if (i != null && i.StatusDescricao != "Pago") OnPay(i); } };
        }
    }
}