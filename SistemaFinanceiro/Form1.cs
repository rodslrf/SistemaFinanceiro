using SistemaFinanceiro.Repositories;
using SistemaFinanceiro.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaFinanceiro
{
    public partial class Form1 : Form
    {
        private bool _menuLateralExpandido = true;
        private const int LARGURA_MENU_EXPANDIDO = 250;
        private const int LARGURA_MENU_RECOLHIDO = 70;

        private TableLayoutPanel _layoutPrincipal;
        private Panel _painelConteudo;
        private Panel _painelLateral;

        private Button _btnMenuFinanceiro, _btnMenuLista, _btnMenuNovo, _btnConfig;
        private UserControl _telaAtual;

        public Form1()
        {
            InitializeComponent();
            ConfigurarJanelaPrincipal();
            AplicarTemaGlobal();
            NavegarParaListaDeAlunos();
        }

        public void AplicarTemaGlobal()
        {
            this.BackColor = TemaGlobal.CorFundo;
            this.ForeColor = TemaGlobal.CorTexto;

            if (_painelLateral != null) _painelLateral.BackColor = TemaGlobal.CorSidebar;
            if (_painelConteudo != null) _painelConteudo.BackColor = TemaGlobal.CorFundo;

            if (_painelLateral != null)
            {
                foreach (Control c in _painelLateral.Controls)
                {
                    if (c is Button btn)
                    {
                        btn.ForeColor = TemaGlobal.CorTexto;
                        if (btn.BackColor != Color.Transparent)
                            btn.BackColor = TemaGlobal.CorBotaoAtivo;
                    }
                }
            }
        }

        private void ConfigurarJanelaPrincipal()
        {
            this.Text = "Sistema de Gestão Integrada";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            _layoutPrincipal = new TableLayoutPanel();
            _layoutPrincipal.Dock = DockStyle.Fill;
            _layoutPrincipal.ColumnCount = 2;
            _layoutPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LARGURA_MENU_EXPANDIDO));
            _layoutPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            ConfigurarMenuLateral();

            _painelConteudo = new Panel { Dock = DockStyle.Fill };

            _layoutPrincipal.Controls.Add(_painelLateral, 0, 0);
            _layoutPrincipal.Controls.Add(_painelConteudo, 1, 0);
            this.Controls.Add(_layoutPrincipal);
        }

        private void ConfigurarMenuLateral()
        {
            _painelLateral = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };

            Button btnAlternarMenu = new Button
            {
                Text = "☰",
                Dock = DockStyle.Top,
                Height = 60,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
            btnAlternarMenu.Click += (s, e) => AlternarVisibilidadeMenu();

            _btnConfig = CriarBotaoNavegacao("Configurações", "⚙️", DockStyle.Bottom);

            _btnMenuFinanceiro = CriarBotaoNavegacao("Financeiro", "💲", DockStyle.Top);
            _btnMenuLista = CriarBotaoNavegacao("Listar Alunos", "📄", DockStyle.Top);
            _btnMenuNovo = CriarBotaoNavegacao("Novo Aluno", "➕", DockStyle.Top);

            _btnMenuLista.Click += (s, e) => NavegarParaListaDeAlunos();
            _btnMenuNovo.Click += (s, e) => NavegarParaCadastroAluno();
            _btnMenuFinanceiro.Click += (s, e) => NavegarParaPainelFinanceiro();
            _btnConfig.Click += (s, e) => NavegarParaConfiguracoes();

            _painelLateral.Controls.Add(_btnConfig);

            // Adicionando em ordem inversa para Dock.Top
            _painelLateral.Controls.Add(_btnMenuFinanceiro);
            _painelLateral.Controls.Add(_btnMenuLista);
            _painelLateral.Controls.Add(_btnMenuNovo);

            _painelLateral.Controls.Add(btnAlternarMenu);
        }

        private void NavegarParaListaDeAlunos()
        {
            DestacarBotaoAtivo(_btnMenuLista);
            _painelConteudo.Controls.Clear();
            var tela = new TelaListaAlunos();
            _telaAtual = tela;
            tela.IrParaCadastro += (s, e) => NavegarParaCadastroAluno();
            tela.EditarAluno += (s, id) => NavegarParaEdicaoAluno(id);
            // Removida a ação de exclusão física, o controle agora é via Status na própria lista
            tela.Dock = DockStyle.Fill;
            _painelConteudo.Controls.Add(tela);
        }

        private void NavegarParaCadastroAluno()
        {
            DestacarBotaoAtivo(_btnMenuNovo);
            _painelConteudo.Controls.Clear();
            var tela = new TelaCadastroAluno();
            _telaAtual = tela;
            tela.VoltarParaLista += (s, e) => NavegarParaListaDeAlunos();
            tela.Dock = DockStyle.Fill;
            _painelConteudo.Controls.Add(tela);
        }

        private void NavegarParaPainelFinanceiro()
        {
            DestacarBotaoAtivo(_btnMenuFinanceiro);
            _painelConteudo.Controls.Clear();
            var tela = new TelaFinanceiro();
            _telaAtual = tela;
            tela.Dock = DockStyle.Fill;
            _painelConteudo.Controls.Add(tela);
        }

        private void NavegarParaConfiguracoes()
        {
            DestacarBotaoAtivo(_btnConfig);
            _painelConteudo.Controls.Clear();
            var tela = new TelaConfiguracoes();
            _telaAtual = tela;
            tela.AoMudarTema += (s, modo) => AplicarTemaGlobal();
            tela.Dock = DockStyle.Fill;
            _painelConteudo.Controls.Add(tela);
        }

        private void NavegarParaEdicaoAluno(int id)
        {
            _painelConteudo.Controls.Clear();
            var tela = new TelaCadastroAluno(id);
            _telaAtual = tela;
            tela.VoltarParaLista += (s, e) => NavegarParaListaDeAlunos();
            tela.Dock = DockStyle.Fill;
            _painelConteudo.Controls.Add(tela);
        }

        private void DestacarBotaoAtivo(Button botaoSelecionado)
        {
            _btnMenuFinanceiro.BackColor = Color.Transparent;
            _btnMenuLista.BackColor = Color.Transparent;
            _btnMenuNovo.BackColor = Color.Transparent;
            _btnConfig.BackColor = Color.Transparent;

            if (botaoSelecionado != null)
                botaoSelecionado.BackColor = TemaGlobal.CorBotaoAtivo;
        }

        private void AlternarVisibilidadeMenu()
        {
            _menuLateralExpandido = !_menuLateralExpandido;
            _layoutPrincipal.ColumnStyles[0].Width = _menuLateralExpandido ? LARGURA_MENU_EXPANDIDO : LARGURA_MENU_RECOLHIDO;

            foreach (Control controle in _painelLateral.Controls)
            {
                if (controle is Button btn)
                {
                    if (btn.Text.Trim() == "☰")
                    {
                        btn.TextAlign = _menuLateralExpandido ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleCenter;
                        btn.Padding = _menuLateralExpandido ? new Padding(20, 0, 0, 0) : new Padding(0);
                        continue;
                    }

                    if (_menuLateralExpandido)
                    {
                        if (btn.Tag != null)
                        {
                            string icone = "";
                            if (!string.IsNullOrEmpty(btn.Text) && btn.Text.Trim().Length > 0)
                            {
                                int len = Math.Min(2, btn.Text.Trim().Length);
                                icone = btn.Text.Substring(0, len).Trim();
                            }
                            else icone = "•";

                            btn.Text = $"{icone}   {btn.Tag.ToString()}";
                        }
                        btn.TextAlign = ContentAlignment.MiddleLeft;
                        btn.Padding = new Padding(20, 0, 0, 0);
                        btn.Font = new Font("Segoe UI", 10);
                    }
                    else
                    {
                        if (btn.Tag == null)
                        {
                            string[] partes = btn.Text.Split(new string[] { "   " }, StringSplitOptions.None);
                            if (partes.Length > 1) btn.Tag = partes[1];
                            else btn.Tag = btn.Text;
                        }

                        string[] partesTexto = btn.Text.Split(new string[] { "   " }, StringSplitOptions.None);
                        if (partesTexto.Length > 0) btn.Text = partesTexto[0].Trim();

                        btn.TextAlign = ContentAlignment.MiddleCenter;
                        btn.Padding = new Padding(0);
                        btn.Font = new Font("Segoe UI", 14);
                    }
                }
            }
        }

        private Button CriarBotaoNavegacao(string texto, string icone, DockStyle dock)
        {
            Button btn = new Button
            {
                Text = $"{icone}   {texto}",
                Tag = texto,
                Dock = dock,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = TemaGlobal.CorTexto,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                Padding = new Padding(20, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };

            btn.MouseEnter += (s, e) => { if (btn.BackColor != TemaGlobal.CorBotaoAtivo) btn.BackColor = TemaGlobal.CorBotaoHover; };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != TemaGlobal.CorBotaoAtivo) btn.BackColor = Color.Transparent; };
            return btn;
        }
    }
}