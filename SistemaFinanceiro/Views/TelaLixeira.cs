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
    public partial class TelaLixeira : UserControl
    {
        private TextBox _search;
        private DarkComboBox _cmbCat;
        private DataGridView _grid;
        private List<dynamic> _data = new List<dynamic>();
        private EntidadeRepository _repo = new EntidadeRepository();
        private bool _sortAsc = true;

        public TelaLixeira()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            if (!DesignMode)
            {
                SetupUI();
                ApplyTheme();
                LoadData();
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

            foreach (Control c in Controls)
                RecursiveUpdate(c);

            Invalidate();
        }

        private void RecursiveUpdate(Control c)
        {
            if (c is Label) c.ForeColor = TemaGlobal.CorTexto;
            if (c is TextBox)
            {
                c.BackColor = TemaGlobal.CorSidebar;
                c.ForeColor = TemaGlobal.CorTexto;
            }
            if (c is DarkComboBox cmb)
            {
                cmb.ParentBackColor = TemaGlobal.CorSidebar;
                cmb.Invalidate();
            }
            if (c is Panel) c.Invalidate();

            foreach (Control k in c.Controls)
                RecursiveUpdate(k);
        }

        private void SetupUI()
        {
            Controls.Clear();

            var h = new Panel { Dock = DockStyle.Top, Height = 80 };
            var t = new Label { Text = "Lixeira de Alunos", Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };

            // Botão Voltar ou Ação extra se necessário (opcional)
            // h.Controls.Add(...);
            h.Controls.Add(t);

            var f = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 15, 0, 15) };
            var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };

            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            _search = Input("Pesquisar por Nome ou CPF...");
            _search.TextChanged += (s, e) => Filter();

            _cmbCat = new DarkComboBox();
            LoadCats();
            _cmbCat.SelectedIndexChanged += (s, e) => Filter();

            var bClr = Btn("Limpar Filtros");
            bClr.Click += (s, e) =>
            {
                _search.Text = "Pesquisar por Nome ou CPF...";
                _search.ForeColor = Color.Gray;
                _cmbCat.SelectedIndex = 0;
            };

            tl.Controls.Add(Wrap(_search), 0, 0);
            tl.Controls.Add(Wrap(_cmbCat), 1, 0);
            tl.Controls.Add(WrapBtn(bClr), 2, 0);
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
        }

        private void LoadData()
        {
            try
            {
                _data = _repo.ObterLixeira();
                Filter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar: " + ex.Message);
            }
        }

        private void Filter()
        {
            if (_data == null) return;

            string t = _search.Text.ToLower().Contains("pesquisar") ? "" : _search.Text.ToLower();
            string c = _cmbCat.SelectedItem?.ToString() ?? "Todas Categorias";

            var filtered = _data.Where(x =>
                (string.IsNullOrEmpty(t) || (x.Nome != null && x.Nome.ToLower().Contains(t)) || (x.CpfAtleta != null && x.CpfAtleta.Contains(t))) &&
                (c == "Todas Categorias" || (x.GetType().GetProperty("CategoriaDescricao") != null && x.CategoriaDescricao == c))
            ).ToList();

            // Ordena alfabeticamente por padrão
            _grid.DataSource = filtered.OrderBy(x => x.Nome).ToList();
        }

        private void RestaurarAluno(int id)
        {
            if (MessageBox.Show("Deseja restaurar este aluno para a lista ativa?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _repo.Restaurar(id);
                    LoadData();
                    MessageBox.Show("Aluno restaurado com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: " + ex.Message);
                }
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
        }

        private void SetupCols(DataGridView g)
        {
            g.Columns.Clear();
            var id = new DataGridViewTextBoxColumn { Name = "id", DataPropertyName = "id_entidade", Visible = false };
            g.Columns.Add(id);

            var n = new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome do Aluno", FillWeight = 250 };
            n.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            g.Columns.Add(n);

            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CpfAtleta", HeaderText = "CPF", Width = 140 });

            var btn = new DataGridViewButtonColumn { HeaderText = "Ação", Width = 150 };
            g.Columns.Add(btn);

            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;

                bool isBtn = g.Columns[e.ColumnIndex].HeaderText == "Ação";

                if (isBtn)
                {
                    using (var b = new SolidBrush(TemaGlobal.CorFundo))
                        e.Graphics.FillRectangle(b, e.CellBounds);

                    using (var p = new Pen(TemaGlobal.CorBorda))
                        e.Graphics.DrawRectangle(p, e.CellBounds);

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Estilo do botão Restaurar (Verde)
                    var c = ColorTranslator.FromHtml("#238636");
                    var bg = Color.FromArgb(25, 35, 134, 54);
                    var r = new Rectangle(e.CellBounds.X + 20, e.CellBounds.Y + 10, e.CellBounds.Width - 40, e.CellBounds.Height - 20);

                    using (var b = new SolidBrush(bg))
                        e.Graphics.FillRectangle(b, r);

                    using (var p = new Pen(c, 1))
                        e.Graphics.DrawRectangle(p, r);

                    TextRenderer.DrawText(e.Graphics, "♻️ Restaurar", new Font("Segoe UI", 9, FontStyle.Bold), r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

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

                if (g.Columns[e.ColumnIndex].HeaderText == "Ação")
                {
                    var v = g.Rows[e.RowIndex].Cells["id"].Value;
                    if (v != null)
                        RestaurarAluno(Convert.ToInt32(v));
                }
            };
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
            t.GotFocus += (s, e) =>
            {
                if (t.Text == ph)
                {
                    t.Text = "";
                    t.ForeColor = TemaGlobal.CorTexto;
                }
            };
            t.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(t.Text))
                {
                    t.Text = ph;
                    t.ForeColor = Color.Gray;
                }
            };
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