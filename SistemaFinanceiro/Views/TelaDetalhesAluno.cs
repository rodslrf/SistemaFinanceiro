using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public partial class TelaDetalhesAluno : UserControl
    {
        private readonly Color CorFundo = ColorTranslator.FromHtml("#0d1117");
        private readonly Color CorTexto = ColorTranslator.FromHtml("#c9d1d9");
        private readonly Color CorBorda = ColorTranslator.FromHtml("#30363d");
        // Novas Cores
        private readonly Color CorBtnEditar = ColorTranslator.FromHtml("#d9a648");
        private readonly Color CorBtnOutline = ColorTranslator.FromHtml("#d9486a");

        public event EventHandler Voltar;
        public event EventHandler<int> Editar; // Novo evento
        public event EventHandler<int> Excluir; // Novo evento

        private int _idAlunoAtual;

        public TelaDetalhesAluno() { InitializeComponent(); this.BackColor = CorFundo; }

        public TelaDetalhesAluno(int idAluno) : this()
        {
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;
            _idAlunoAtual = idAluno;

            if (!this.DesignMode) ConfigurarLayout(idAluno);
        }

        private void ConfigurarLayout(int id)
        {
            var repo = new EntidadeRepository();
            var aluno = repo.ObterPorId(id);

            if (aluno == null) { MessageBox.Show("Aluno não encontrado!"); return; }

            // Header
            Panel header = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(40, 20, 40, 0) };
            Label lblTitulo = new Label { Text = "Detalhes do Aluno", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft };

            // Painel de Botões na direita
            Panel pnlBotoes = new Panel { Dock = DockStyle.Right, Width = 400, Height = 60 };

            // Botão Voltar (Estilo Outline Vermelho - Imagem 9)
            Button btnVoltar = CriarBotaoOutline("← VOLTAR", CorBtnOutline);
            btnVoltar.Click += (s, e) => Voltar?.Invoke(this, EventArgs.Empty);

            // Botão Excluir (Estilo Outline Vermelho)
            Button btnExcluir = CriarBotaoOutline("EXCLUIR", CorBtnOutline);
            btnExcluir.Click += (s, e) => ConfirmarExclusao();

            // Botão Editar (Estilo Sólido Amarelo - Imagem 8)
            Button btnEditar = new Button
            {
                Text = "EDITAR ✎",
                Width = 120,
                Height = 40,
                Dock = DockStyle.Right,
                BackColor = CorBtnEditar,
                ForeColor = CorFundo,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += (s, e) => Editar?.Invoke(this, _idAlunoAtual);

            // Adiciona na ordem inversa (por causa do DockStyle.Right)
            pnlBotoes.Controls.Add(btnVoltar);
            pnlBotoes.Controls.Add(btnExcluir);
            pnlBotoes.Controls.Add(btnEditar);

            header.Controls.Add(pnlBotoes);
            header.Controls.Add(lblTitulo);
            this.Controls.Add(header);

            // Grid de Dados (Mantido igual)
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Top; grid.AutoSize = true; grid.Padding = new Padding(40);
            grid.ColumnCount = 2; grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            AdicionarItem(grid, "Nome Completo", aluno.Nome);
            AdicionarItem(grid, "Status", aluno.Status);
            AdicionarItem(grid, "Categoria", aluno.CategoriaDescricao);
            AdicionarItem(grid, "Data de Nascimento", aluno.DataNascimento.ToString("dd/MM/yyyy"));
            AdicionarItem(grid, "CPF do Aluno", aluno.CpfAtleta);
            AdicionarItem(grid, "CPF do Responsável", aluno.CpfPais);
            grid.Controls.Add(new Panel { Height = 30 }, 0, grid.RowCount++);
            AdicionarItem(grid, "E-mail dos Pais", aluno.EmailPais);
            AdicionarItem(grid, "Telefone dos Pais", aluno.TelefonePais);
            AdicionarItem(grid, "Tipo de Vínculo", aluno.TipoVinculo);

            this.Controls.Add(grid);
        }

        // Helper para criar botões com contorno (Outline)
        private Button CriarBotaoOutline(string texto, Color cor)
        {
            Button btn = new Button
            {
                Text = texto,
                Width = 120,
                Height = 40,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent,
                ForeColor = cor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = cor;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, cor.R, cor.G, cor.B); // Efeito hover sutil
            return btn;
        }

        private void ConfirmarExclusao()
        {
            // Mesma lógica de dupla confirmação da lista
            DialogResult r1 = MessageBox.Show("Tem certeza que deseja excluir este aluno?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r1 == DialogResult.Yes)
            {
                DialogResult r2 = MessageBox.Show("Essa ação NÃO pode ser desfeita.\nDeseja realmente continuar?", "Atenção Absoluta", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (r2 == DialogResult.Yes)
                {
                    Excluir?.Invoke(this, _idAlunoAtual);
                }
            }
        }

        private void AdicionarItem(TableLayoutPanel grid, string titulo, string valor)
        {
            Panel p = new Panel { Height = 70, Dock = DockStyle.Top, Margin = new Padding(0, 0, 20, 20) };
            Label lblTitulo = new Label { Text = titulo.ToUpper(), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Top };
            Label lblValor = new Label { Text = string.IsNullOrWhiteSpace(valor) ? "-" : valor, ForeColor = CorTexto, Font = new Font("Segoe UI", 12), Dock = DockStyle.Top, Height = 30, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            Panel linha = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = CorBorda };
            p.Controls.Add(linha); p.Controls.Add(lblValor); p.Controls.Add(lblTitulo);
            grid.Controls.Add(p);
        }
    }
}