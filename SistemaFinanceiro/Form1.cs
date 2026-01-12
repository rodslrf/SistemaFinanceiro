using SistemaFinanceiro.Repositories;
using SistemaFinanceiro.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaFinanceiro
{
    public partial class Form1 : Form
    {
        // ================= CORES =================
        private readonly Color CorFundo = ColorTranslator.FromHtml("#0d1117");
        private readonly Color CorSidebar = ColorTranslator.FromHtml("#161b22");
        private readonly Color CorTexto = ColorTranslator.FromHtml("#c9d1d9");
        private readonly Color CorHover = ColorTranslator.FromHtml("#21262d");
        private readonly Color CorSelecionado = ColorTranslator.FromHtml("#30363d"); // Cor do botão ativo

        private bool sidebarExpandida = true;
        private const int LarguraExpandida = 250;
        private const int LarguraRecolhida = 70;

        private TableLayoutPanel mainLayout;
        private Panel contentPanel;
        private Panel sidebar;

        // Botões Globais para controle de cor
        private Button btnFinan, btnLista, btnNovo;

        public Form1()
        {
            InitializeComponent();
            ConfigurarJanelaPrincipal();

            // Inicia na lista por padrão
            NavegarParaLista();
        }

        private void ConfigurarJanelaPrincipal()
        {
            this.Text = "Sistema de Gestão";
            this.Size = new Size(1280, 800);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = CorFundo;
            this.ForeColor = CorTexto;

            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LarguraExpandida));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            ConfigurarSidebar();

            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = CorFundo };

            mainLayout.Controls.Add(sidebar, 0, 0);
            mainLayout.Controls.Add(contentPanel, 1, 0);
            this.Controls.Add(mainLayout);
        }

        private void ConfigurarSidebar()
        {
            sidebar = new Panel { Dock = DockStyle.Fill, BackColor = CorSidebar, Padding = new Padding(0) };

            Button btnMenu = new Button
            {
                Text = "☰",
                Dock = DockStyle.Top,
                Height = 60,
                FlatStyle = FlatStyle.Flat,
                ForeColor = CorTexto,
                Font = new Font("Segoe UI", 16),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Click += (s, e) => AlternarSidebar();

            // Instancia os botões
            btnFinan = CriarBotaoSidebar("Financeiro", "💲");
            btnLista = CriarBotaoSidebar("Listar Alunos", "📄");
            btnNovo = CriarBotaoSidebar("Novo Aluno", "➕");

            // Eventos de Navegação
            btnLista.Click += (s, e) => NavegarParaLista();
            btnNovo.Click += (s, e) => NavegarParaCadastro();
            btnFinan.Click += (s, e) => NavegarParaFinanceiro(); // <--- AQUI ESTÁ A LIGAÇÃO

            // Adiciona na ordem inversa (pois é Dock.Top)
            sidebar.Controls.Add(btnFinan);
            sidebar.Controls.Add(btnLista);
            sidebar.Controls.Add(btnNovo);
            sidebar.Controls.Add(btnMenu);
        }

        // =================================================================================
        //                              GERENCIADOR DE NAVEGAÇÃO
        // =================================================================================

        private void NavegarParaLista()
        {
            AtualizarBotoesSidebar(btnLista); // Marca visualmente
            contentPanel.Controls.Clear();
            var tela = new TelaListaAlunos();

            tela.IrParaCadastro += (s, e) => NavegarParaCadastro();
            tela.EditarAluno += (s, idAluno) => NavegarParaEdicao(idAluno);
            tela.ExcluirAluno += (s, idAluno) => ProcessarExclusao(idAluno);

            tela.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(tela);
        }

        private void NavegarParaCadastro()
        {
            AtualizarBotoesSidebar(btnNovo);
            contentPanel.Controls.Clear();
            var tela = new TelaCadastroAluno();
            tela.VoltarParaLista += (s, e) => NavegarParaLista();

            tela.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(tela);
        }

        // --- NOVA TELA FINANCEIRO ---
        private void NavegarParaFinanceiro()
        {
            AtualizarBotoesSidebar(btnFinan);
            contentPanel.Controls.Clear();
            var tela = new TelaFinanceiro(); // Instancia a tela que criamos

            tela.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(tela);
        }

        private void NavegarParaEdicao(int idAluno)
        {
            // Na edição, mantemos o botão "Lista" ou "Novo" aceso, ou nenhum
            contentPanel.Controls.Clear();
            var telaEdicao = new TelaCadastroAluno(idAluno);
            telaEdicao.VoltarParaLista += (s, e) => NavegarParaLista();

            telaEdicao.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(telaEdicao);
        }

        private void ProcessarExclusao(int idAluno)
        {
            try
            {
                var repo = new EntidadeRepository();
                repo.Excluir(idAluno); // Certifique-se que esse método existe no Repo
                MessageBox.Show("Aluno excluído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NavegarParaLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =================================================================================
        //                              LÓGICA VISUAL
        // =================================================================================

        private void AtualizarBotoesSidebar(Button botaoAtivo)
        {
            // Reseta cores
            btnFinan.BackColor = Color.Transparent;
            btnLista.BackColor = Color.Transparent;
            btnNovo.BackColor = Color.Transparent;

            // Marca o ativo
            if (botaoAtivo != null)
                botaoAtivo.BackColor = CorSelecionado;
        }

        private void AlternarSidebar()
        {
            sidebarExpandida = !sidebarExpandida;
            mainLayout.ColumnStyles[0].Width = sidebarExpandida ? LarguraExpandida : LarguraRecolhida;

            foreach (Control c in sidebar.Controls)
            {
                if (c is Button btn)
                {
                    if (btn.Text.Trim() == "☰")
                    {
                        btn.TextAlign = sidebarExpandida ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleCenter;
                        btn.Padding = sidebarExpandida ? new Padding(20, 0, 0, 0) : new Padding(0);
                        continue;
                    }
                    if (sidebarExpandida)
                    {
                        if (btn.Tag != null) { string iconeAtual = btn.Text.Substring(0, 2).Trim(); btn.Text = $"{iconeAtual}   {btn.Tag.ToString()}"; }
                        btn.TextAlign = ContentAlignment.MiddleLeft; btn.Padding = new Padding(20, 0, 0, 0); btn.Font = new Font("Segoe UI", 10);
                    }
                    else
                    {
                        if (btn.Tag == null) { string[] partes = btn.Text.Split(new string[] { "   " }, StringSplitOptions.None); if (partes.Length > 1) btn.Tag = partes[1]; }
                        string textoAtual = btn.Text.Trim(); if (textoAtual.Length >= 2) btn.Text = textoAtual.Substring(0, 2).Trim();
                        btn.TextAlign = ContentAlignment.MiddleCenter; btn.Padding = new Padding(0); btn.Font = new Font("Segoe UI", 14);
                    }
                }
            }
        }

        private Button CriarBotaoSidebar(string texto, string icone)
        {
            Button btn = new Button
            {
                Text = $"{icone}   {texto}",
                Tag = texto,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = CorTexto,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                Padding = new Padding(20, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            // Hover simples
            btn.MouseEnter += (s, e) => { if (btn.BackColor != CorSelecionado) btn.BackColor = CorHover; };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != CorSelecionado) btn.BackColor = Color.Transparent; };
            return btn;
        }
    }
}