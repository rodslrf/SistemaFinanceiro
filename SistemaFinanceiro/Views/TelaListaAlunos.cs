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
        private DarkComboBox _cmbCat, _cmbStatus;
        private DataGridView _grid;
        private List<dynamic> _data = new List<dynamic>();

        public event EventHandler IrParaCadastro;
        public event EventHandler<int> EditarAluno;
        public event EventHandler<int> ExcluirAluno;

        public TelaListaAlunos()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            if (!DesignMode) { SetupUI(); ApplyTheme(); }
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
            foreach (Control c in Controls) RecursiveUpdate(c);
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

        // Layout
        private void SetupUI()
        {
            Controls.Clear();

            // Header
            var h = new Panel { Dock = DockStyle.Top, Height = 60 };
            var t = new Label { Text = "Alunos Cadastrados", Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };
            var b = new Button { Text = "+  Novo Aluno", BackColor = ColorTranslator.FromHtml("#238636"), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 40, Width = 150, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(Width - 190, 10) };
            b.FlatAppearance.BorderSize = 0; b.Click += (s, e) => IrParaCadastro?.Invoke(this, EventArgs.Empty);
            h.Resize += (s, e) => b.Left = h.Width - 160; h.Controls.Add(t); h.Controls.Add(b);

            // Filters
            var f = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 15, 0, 15) };
            var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            _search = Input("Pesquisar por Nome ou CPF..."); _search.TextChanged += (s, e) => Filter();
            _cmbCat = new DarkComboBox(); LoadCats(); _cmbCat.SelectedIndexChanged += (s, e) => Filter();
            _cmbStatus = new DarkComboBox(); _cmbStatus.Items.AddRange(new[] { "Todos Status", "Ativos", "Inativos" }); _cmbStatus.SelectedIndex = 0; _cmbStatus.SelectedIndexChanged += (s, e) => Filter();
            var bClr = Btn("Limpar"); bClr.Click += (s, e) => { _search.Text = "Pesquisar por Nome ou CPF..."; _search.ForeColor = Color.Gray; _cmbCat.SelectedIndex = 0; _cmbStatus.SelectedIndex = 0; };

            tl.Controls.Add(Wrap(_search), 0, 0); tl.Controls.Add(Wrap(_cmbCat), 1, 0); tl.Controls.Add(Wrap(_cmbStatus), 2, 0); tl.Controls.Add(WrapBtn(bClr), 3, 0);
            f.Controls.Add(tl);

            // Grid
            var gc = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            _grid = new DataGridView(); SetupGrid(_grid); SetupCols(_grid); gc.Controls.Add(_grid);

            Controls.Add(gc); Controls.Add(f); Controls.Add(h);
            AddFooter(); LoadData();
        }

        private void AddFooter()
        {
            var f = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Transparent, Padding = new Padding(0, 0, 20, 10) };
            f.Controls.Add(new Label { Text = "@rodrigolopes_rf", Font = new Font("Segoe UI", 10), ForeColor = TemaGlobal.CorTexto, AutoSize = true, Dock = DockStyle.Right, Padding = new Padding(5, 2, 0, 0) });
            f.Controls.Add(new Label { Text = "📷", Font = new Font("Segoe UI Emoji", 12), ForeColor = TemaGlobal.CorTexto, AutoSize = true, Dock = DockStyle.Right });
            Controls.Add(f);
        }

        // Wrappers & Inputs
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
                if (c is TextBox t) t.BorderStyle = BorderStyle.None; b.Controls.Add(c);
            }
            p.Controls.Add(b); return p;
        }
        private Panel WrapBtn(Control c) { var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }; var b = new Panel { Dock = DockStyle.Top, Height = 45 }; b.Paint += (s, e) => { b.BackColor = TemaGlobal.CorSidebar; ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, TemaGlobal.CorBorda, ButtonBorderStyle.Solid); c.BackColor = TemaGlobal.CorSidebar; c.ForeColor = TemaGlobal.CorTexto; }; c.Dock = DockStyle.Fill; b.Controls.Add(c); p.Controls.Add(b); return p; }
        private TextBox Input(string ph) { var t = new TextBox { Font = new Font("Segoe UI", 11), Text = ph, TextAlign = HorizontalAlignment.Center }; t.GotFocus += (s, e) => { if (t.Text == ph) { t.Text = ""; t.ForeColor = TemaGlobal.CorTexto; } }; t.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(t.Text)) { t.Text = ph; t.ForeColor = Color.Gray; } }; return t; }
        private Button Btn(string t) { var b = new Button { Text = t, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Dock = DockStyle.Fill }; b.FlatAppearance.BorderSize = 0; return b; }

        // Grid
        private void SetupGrid(DataGridView g) { g.Dock = DockStyle.Fill; g.BorderStyle = BorderStyle.None; g.CellBorderStyle = DataGridViewCellBorderStyle.Single; g.EnableHeadersVisualStyles = false; g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single; g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; g.ColumnHeadersHeight = 50; g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 14, FontStyle.Bold); g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10); g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; g.AutoGenerateColumns = false; g.ReadOnly = true; g.SelectionMode = DataGridViewSelectionMode.FullRowSelect; g.RowHeadersVisible = false; g.AllowUserToResizeRows = false; g.AllowUserToResizeColumns = false; g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; g.RowTemplate.Height = 45; g.DefaultCellStyle.Font = new Font("Segoe UI", 10); g.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0); g.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; g.DefaultCellStyle.SelectionBackColor = TemaGlobal.CorFundo; g.DefaultCellStyle.SelectionForeColor = TemaGlobal.CorTexto; }
        private void SetupCols(DataGridView g)
        {
            g.Columns.Clear(); g.AutoGenerateColumns = false;
            var id = new DataGridViewTextBoxColumn { DataPropertyName = "id_entidade", Name = "id_entidade", HeaderText = "ID", Visible = false }; g.Columns.Add(id);
            var n = new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome do Aluno", FillWeight = 250 }; n.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; g.Columns.Add(n);
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CpfAtleta", HeaderText = "CPF", Width = 140 });
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoriaDescricao", HeaderText = "Categoria", Width = 160 });
            g.Columns.Add(new DataGridViewButtonColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 150 });
            g.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Editar", Width = 100 });
            g.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Excluir", Width = 100 });

            g.CellPainting += (s, e) => {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;
                bool st = g.Columns[e.ColumnIndex].HeaderText == "Status", ed = g.Columns[e.ColumnIndex].HeaderText == "Editar", ex = g.Columns[e.ColumnIndex].HeaderText == "Excluir";
                if (st || ed || ex)
                {
                    using (var b = new SolidBrush(TemaGlobal.CorFundo)) e.Graphics.FillRectangle(b, e.CellBounds);
                    using (var p = new Pen(TemaGlobal.CorBorda)) e.Graphics.DrawRectangle(p, e.CellBounds);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    if (ed || ex)
                    {
                        var c = ed ? ColorTranslator.FromHtml("#d9a648") : ColorTranslator.FromHtml("#ff7b72");
                        var bg = ed ? Color.FromArgb(25, 217, 166, 72) : Color.FromArgb(25, 255, 123, 114);
                        var r = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 10, e.CellBounds.Width - 20, e.CellBounds.Height - 20);
                        using (var b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, r); using (var p = new Pen(c, 1)) e.Graphics.DrawRectangle(p, r);
                        TextRenderer.DrawText(e.Graphics, ed ? "✏️" : "🗑️", new Font("Segoe UI Emoji", 10), r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else if (st)
                    {
                        bool at = e.FormattedValue.ToString() == "Ativo";
                        var bg = at ? ColorTranslator.FromHtml("#d1e7dd") : ColorTranslator.FromHtml("#f8d7da");
                        var fg = at ? ColorTranslator.FromHtml("#0f5132") : ColorTranslator.FromHtml("#842029");
                        var r = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 10, e.CellBounds.Width - 30, e.CellBounds.Height - 20);
                        var p = new GraphicsPath(); p.AddArc(r.X, r.Y, r.Height, r.Height, 90, 180); p.AddArc(r.Right - r.Height, r.Y, r.Height, r.Height, 270, 180); p.CloseFigure();
                        using (var b = new SolidBrush(bg)) e.Graphics.FillPath(b, p);
                        TextRenderer.DrawText(e.Graphics, e.FormattedValue.ToString().ToUpper(), new Font("Segoe UI", 9, FontStyle.Bold), r, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    e.Handled = true;
                }
            };
            g.CellFormatting += (s, e) => { if (e.Value != null && g.Columns[e.ColumnIndex].DataPropertyName == "CpfAtleta") { string v = Regex.Replace(e.Value.ToString(), "[^0-9]", ""); if (v.Length == 11) { e.Value = Convert.ToUInt64(v).ToString(@"000\.000\.000\-00"); e.FormattingApplied = true; } } };
            g.CellContentClick += (s, e) => { if (e.RowIndex < 0) return; var v = g.Rows[e.RowIndex].Cells["id_entidade"].Value; if (v == null) return; int id = Convert.ToInt32(v); if (g.Columns[e.ColumnIndex].HeaderText == "Status") ToggleStatus(id, g.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()); else if (g.Columns[e.ColumnIndex].HeaderText == "Editar") EditarAluno?.Invoke(this, id); else if (g.Columns[e.ColumnIndex].HeaderText == "Excluir") Del(id); };
        }

        // Logic
        private void LoadCats() { try { var c = new EntidadeRepository().ObterCategorias(); _cmbCat.Items.Clear(); _cmbCat.Items.Add("Todas Categorias"); foreach (var i in c) _cmbCat.Items.Add(i.GetType().GetProperty("Descricao").GetValue(i, null)); _cmbCat.SelectedIndex = 0; } catch { } }
        private void LoadData() { try { var r = new EntidadeRepository(); _data = r.ObterTodos(); Filter(); } catch { } }
        private void Filter() { if (_data == null) return; string t = _search.Text.ToLower().Contains("pesquisar") ? "" : _search.Text.ToLower(); string c = _cmbCat.SelectedItem?.ToString() ?? "Todas Categorias"; string s = _cmbStatus.SelectedItem?.ToString() ?? "Todos Status"; _grid.DataSource = _data.Where(x => (string.IsNullOrEmpty(t) || (x.Nome != null && x.Nome.ToLower().Contains(t)) || (x.CpfAtleta != null && x.CpfAtleta.Contains(t))) && (c == "Todas Categorias" || (x.CategoriaDescricao != null && x.CategoriaDescricao == c)) && (s == "Todos Status" || (s == "Ativos" && x.Status == "Ativo") || (s == "Inativos" && x.Status == "Inativo"))).ToList(); }

        private void ToggleStatus(int id, string s)
        {
            if (!DarkBox.Confirmar($"Deseja realmente {(s == "Ativo" ? "INATIVAR" : "ATIVAR")} este aluno?", out bool _)) return;
            try { new EntidadeRepository().AlternarStatus(id); LoadData(); } catch (Exception ex) { DarkBox.Mostrar("Erro: " + ex.Message); }
        }
        private void Del(int id) { if (DarkBox.Confirmar("Tem certeza que deseja excluir?", out bool _)) ExcluirAluno?.Invoke(this, id); }
    }
    public class DarkComboBox : ComboBox
    {
        public Color ParentBackColor { get; set; } = TemaGlobal.CorSidebar;
        public Color BorderColor { get; set; } = TemaGlobal.CorBorda;
        public DarkComboBox() { SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true); DrawMode = DrawMode.OwnerDrawFixed; DropDownStyle = ComboBoxStyle.DropDownList; FlatStyle = FlatStyle.Flat; Font = new Font("Segoe UI", 11); ItemHeight = 26; IntegralHeight = false; }
        protected override void OnPaint(PaintEventArgs e) { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var b = new SolidBrush(ParentBackColor)) e.Graphics.FillRectangle(b, ClientRectangle); string t = SelectedItem?.ToString() ?? Text; if (Items.Count > 0 && SelectedIndex == -1 && !string.IsNullOrEmpty(Text)) t = Text; var r = new Rectangle(3, 1, Width - 20, Height - 2); TextRenderer.DrawText(e.Graphics, t, Font, r, TemaGlobal.CorTexto, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine); int x = Width - 15, y = (Height - 6) / 2; Point[] p = { new Point(x, y), new Point(x + 8, y), new Point(x + 4, y + 5) }; using (var b = new SolidBrush(Color.Gray)) e.Graphics.FillPolygon(b, p); }
        protected override void OnDrawItem(DrawItemEventArgs e) { if (e.Index < 0) return; var c = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? BorderColor : ParentBackColor; using (var b = new SolidBrush(c)) e.Graphics.FillRectangle(b, e.Bounds); using (var b = new SolidBrush(TemaGlobal.CorTexto)) e.Graphics.DrawString(Items[e.Index].ToString(), Font, b, new Point(e.Bounds.X + 2, e.Bounds.Y + 4)); }
    }

    public static class DarkBox
    {
        public static void Mostrar(string msg, string title = "Aviso")
        {
            var f = BaseForm(msg, 350, 200); var b = Btn("OK", ColorTranslator.FromHtml("#238636"));
            b.Location = new Point((f.Width - b.Width) / 2, f.Height - 70); b.Click += (s, e) => f.DialogResult = DialogResult.OK;
            f.Controls.Add(b); f.ShowDialog();
        }
        public static bool Confirmar(string msg, out bool dontAsk)
        {
            dontAsk = false; var f = BaseForm(msg, 450, 250);
            var tlp = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 120, RowCount = 2, BackColor = Color.Transparent };
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            var chk = new CheckBox { Text = "Não perguntar hoje", ForeColor = Color.Gray, Font = new Font("Segoe UI", 10), AutoSize = true, Anchor = AnchorStyles.None };
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 10) };
            var bNo = Btn("CANCELAR", ColorTranslator.FromHtml("#ff7b72")); bNo.DialogResult = DialogResult.No; bNo.Dock = DockStyle.Left;
            var bYes = Btn("CONFIRMAR", ColorTranslator.FromHtml("#238636")); bYes.DialogResult = DialogResult.Yes; bYes.Dock = DockStyle.Right;
            pnl.Controls.Add(bNo); pnl.Controls.Add(bYes); tlp.Controls.Add(chk, 0, 0); tlp.Controls.Add(pnl, 0, 1);
            f.Controls.Add(tlp); var res = f.ShowDialog(); dontAsk = chk.Checked; return res == DialogResult.Yes;
        }
        private static Form BaseForm(string msg, int w, int h) { var f = new Form { StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.None, Size = new Size(w, h), BackColor = TemaGlobal.CorSidebar, Padding = new Padding(20) }; f.Paint += (s, e) => { using (var p = new Pen(TemaGlobal.CorBorda, 2)) e.Graphics.DrawRectangle(p, 0, 0, f.Width - 1, f.Height - 1); }; var x = new Label { Text = "✕", AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 12), Cursor = Cursors.Hand, Location = new Point(f.Width - 30, 10) }; x.Click += (s, e) => f.DialogResult = DialogResult.Cancel; f.Controls.Add(x); f.Controls.Add(new Label { Text = msg, ForeColor = TemaGlobal.CorTexto, Font = new Font("Segoe UI", 13), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, Padding = new Padding(0, 20, 0, 50) }); return f; }
        private static Button Btn(string txt, Color c) { var b = new Button { Size = new Size(150, 50), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White, Text = "", Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; b.Paint += (s, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; var r = new Rectangle(0, 0, b.Width - 1, b.Height - 1); using (var br = new SolidBrush(c)) e.Graphics.FillRectangle(br, r); TextRenderer.DrawText(e.Graphics, txt, new Font("Segoe UI", 11, FontStyle.Bold), r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }; return b; }
    }
}