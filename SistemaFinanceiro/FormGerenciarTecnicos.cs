using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public class FormGerenciarTecnicos : Form
    {
        private TextBox _search;
        private DarkComboBox _cmbStatus;
        private DataGridView _grid;
        private Button _btnNovo;
        private TecnicoRepository _repository;
        private List<Tecnico> _data = new List<Tecnico>();

        public FormGerenciarTecnicos()
        {
            _repository = new TecnicoRepository();

            this.Text = "Gerenciar Técnicos";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            SetupUI();
            ApplyTheme();

            this.Load += (s, e) => LoadData();
        }

        public void ApplyTheme()
        {
            try
            {
                this.BackColor = TemaGlobal.CorFundo;

                if (_grid != null)
                {
                    _grid.BackgroundColor = TemaGlobal.CorFundo;
                    _grid.DefaultCellStyle.BackColor = TemaGlobal.CorFundo;
                    _grid.DefaultCellStyle.SelectionBackColor = TemaGlobal.CorFundo;
                    _grid.DefaultCellStyle.ForeColor = TemaGlobal.CorTexto;
                    _grid.DefaultCellStyle.SelectionForeColor = TemaGlobal.CorTexto;
                    _grid.GridColor = Color.FromArgb(100, 100, 100);

                    _grid.ColumnHeadersDefaultCellStyle.BackColor = TemaGlobal.CorSidebar;
                    _grid.ColumnHeadersDefaultCellStyle.ForeColor = TemaGlobal.CorTexto;
                    _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = TemaGlobal.CorSidebar;
                }

                foreach (Control c in Controls) RecursiveUpdate(c);
                this.Invalidate();
            }
            catch { }
        }

        private void RecursiveUpdate(Control c)
        {
            try
            {
                if (c is Label) c.ForeColor = TemaGlobal.CorTexto;
                if (c is Panel) c.Invalidate();
                if (c is DarkComboBox) return;
                foreach (Control k in c.Controls) RecursiveUpdate(k);
            }
            catch { }
        }

        private void SetupUI()
        {
            var h = new Panel { Dock = DockStyle.Top, Height = 80 };

            var t = new Label
            {
                Text = "Técnicos Cadastrados",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            var btnNovo = new Button
            {
                Text = "+ Novo Técnico",
                BackColor = ColorTranslator.FromHtml("#238636"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 40,
                Width = 150,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnNovo.Location = new Point(800, 20);

            btnNovo.FlatAppearance.BorderSize = 0;
            btnNovo.Click += (s, e) => AbrirCadastro();

            h.Controls.Add(t);
            h.Controls.Add(btnNovo);

            btnNovo.BringToFront();

            h.Resize += (s, e) => { btnNovo.Left = h.Width - 170; };
            btnNovo.Left = 950 - 170;

            var f = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(30, 5, 30, 5) };
            var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            _search = Input("Pesquisar por Nome...");
            _search.TextChanged += (s, e) => Filter();

            _cmbStatus = new DarkComboBox();
            _cmbStatus.Items.AddRange(new[] { "Todos Status", "Ativos", "Inativos" });
            _cmbStatus.SelectedIndex = 0;
            _cmbStatus.SelectedIndexChanged += (s, e) => Filter();

            tl.Controls.Add(Wrap(_search), 0, 0);
            tl.Controls.Add(Wrap(_cmbStatus), 1, 0);
            f.Controls.Add(tl);

            // --- 3. GRID ---
            var gc = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 10, 30, 20) };
            _grid = new DataGridView();
            SetupGrid(_grid);
            SetupCols(_grid);
            gc.Controls.Add(_grid);

            this.Controls.Add(gc);
            this.Controls.Add(f);
            this.Controls.Add(h);
        }

        private void SetupGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 60;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.AutoGenerateColumns = false;
            g.ReadOnly = true;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;
            g.AllowUserToResizeRows = false;
            g.AllowUserToAddRows = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.RowTemplate.Height = 50;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, g, new object[] { true });

            try { DarkBox.AplicarMarcaDagua(g, Properties.Resources.diviminas); } catch { }
        }

        private void SetupCols(DataGridView g)
        {
            g.Columns.Clear();
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", Visible = false });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nome", DataPropertyName = "Nome", HeaderText = "Nome do Técnico", FillWeight = 300 });
            g.Columns.Add(new DataGridViewButtonColumn { Name = "Status", HeaderText = "Status", Width = 150 });
            g.Columns.Add(new DataGridViewButtonColumn { Name = "Editar", HeaderText = "Editar", Width = 100 });

            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;

                bool st = g.Columns[e.ColumnIndex].Name == "Status";
                bool ed = g.Columns[e.ColumnIndex].Name == "Editar";

                if (st || ed)
                {
                    using (var b = new SolidBrush(TemaGlobal.CorFundo))
                        e.Graphics.FillRectangle(b, e.CellBounds);

                    using (var p = new Pen(Color.FromArgb(100, 100, 100)))
                        e.Graphics.DrawLine(p, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    if (ed)
                    {
                        var c = ColorTranslator.FromHtml("#d9a648");
                        var bg = Color.FromArgb(25, 217, 166, 72);
                        var r = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 10, e.CellBounds.Width - 20, e.CellBounds.Height - 20);

                        using (var brush = new SolidBrush(bg)) e.Graphics.FillRectangle(brush, r);
                        using (var pen = new Pen(c, 1)) e.Graphics.DrawRectangle(pen, r);
                        TextRenderer.DrawText(e.Graphics, "✏️", new Font("Segoe UI Emoji", 10), r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    else if (st)
                    {
                        var list = (List<Tecnico>)g.DataSource;
                        string status = list[e.RowIndex].Status;
                        bool at = status == "Ativo";
                        var bg = at ? ColorTranslator.FromHtml("#d1e7dd") : ColorTranslator.FromHtml("#f8d7da");
                        var fg = at ? ColorTranslator.FromHtml("#0f5132") : ColorTranslator.FromHtml("#842029");

                        var r = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 12, e.CellBounds.Width - 30, e.CellBounds.Height - 24);

                        using (var path = new GraphicsPath())
                        {
                            path.AddArc(r.X, r.Y, r.Height, r.Height, 90, 180);
                            path.AddArc(r.Right - r.Height, r.Y, r.Height, r.Height, 270, 180);
                            path.CloseFigure();
                            using (var brush = new SolidBrush(bg)) e.Graphics.FillPath(brush, path);
                        }

                        TextRenderer.DrawText(e.Graphics, status.ToUpper(), new Font("Segoe UI", 9, FontStyle.Bold), r, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    e.Handled = true;
                }
            };

            g.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var list = (List<Tecnico>)g.DataSource;
                int id = list[e.RowIndex].Id;

                if (g.Columns[e.ColumnIndex].Name == "Status")
                    ToggleStatus(id, list[e.RowIndex].Status);
                else if (g.Columns[e.ColumnIndex].Name == "Editar")
                    AbrirCadastro(id);
            };
        }

        private void LoadData()
        {
            try { _data = _repository.ObterTodos(); Filter(); } catch { }
        }

        private void Filter()
        {
            if (_data == null) return;
            string t = _search.Text.ToLower().Contains("pesquisar") ? "" : _search.Text.ToLower();
            string s = _cmbStatus.SelectedItem?.ToString() ?? "Todos Status";

            _grid.DataSource = _data.Where(x =>
                (string.IsNullOrEmpty(t) || x.Nome.ToLower().Contains(t)) &&
                (s == "Todos Status" || x.Status == (s == "Ativos" ? "Ativo" : "Inativo"))
            ).OrderBy(x => x.Status == "Ativo" ? 0 : 1)
             .ThenBy(x => x.Nome)
             .ToList();

            _grid.ClearSelection();
        }

        private void ToggleStatus(int id, string currentStatus)
        {
            string msg = currentStatus == "Ativo" ? "DESATIVAR" : "ATIVAR";

            if (DarkBox.Confirmar($"Deseja realmente {msg} este técnico?"))
            {
                try
                {
                    _repository.TrocarStatus(id);
                    LoadData();
                }
                catch (Exception ex)
                {
                    DarkBox.Mostrar("Erro ao alterar status: " + ex.Message);
                }
            }
        }

        private void AbrirCadastro(int? id = null)
        {
            Form form = id.HasValue ? new FormCadastroTecnico(id.Value) : new FormCadastroTecnico();
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private Panel Wrap(Control c)
        {
            var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 10, 0), BackColor = Color.Transparent };
            var b = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = TemaGlobal.CorSidebar };

            b.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, b.ClientRectangle, TemaGlobal.CorBorda, ButtonBorderStyle.Solid);
            };

            c.Location = new Point(10, (b.Height - c.Height) / 2);
            c.Width = b.Width - 20;

            b.Resize += (s, e) =>
            {
                c.Width = b.Width - 20;
                c.Location = new Point(10, (b.Height - c.Height) / 2);
            };

            if (c is TextBox txt)
            {
                txt.BorderStyle = BorderStyle.None;
                txt.BackColor = TemaGlobal.CorSidebar;
                txt.ForeColor = TemaGlobal.CorTexto;
            }

            b.Controls.Add(c);
            p.Controls.Add(b);
            return p;
        }

        private TextBox Input(string ph)
        {
            var t = new TextBox { Font = new Font("Segoe UI", 11), Text = ph, ForeColor = Color.Gray };
            t.GotFocus += (s, e) => { if (t.Text == ph) { t.Text = ""; t.ForeColor = TemaGlobal.CorTexto; } };
            t.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(t.Text)) { t.Text = ph; t.ForeColor = Color.Gray; } };
            return t;
        }
    }
}