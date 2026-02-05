using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    public class FormCadastroTecnico : Form
    {
        private TextBox txtNome;
        private TextBox txtObservacao;
        private Button btnSalvar;
        private Button btnCancelar;
        private int? _idEdicao = null;

        // Cores do Tema
        private Color cFundo, cTexto, cInput, cBorda;

        public FormCadastroTecnico()
        {
            CarregarTema();
            ConfigurarLayout();
        }

        public FormCadastroTecnico(int id) : this()
        {
            _idEdicao = id;
            this.Load += (s, e) => CarregarDados(id);
        }

        private void CarregarTema()
        {
            try
            {
                // Pega as cores do TemaGlobal
                cFundo = TemaGlobal.CorFundo; // Ex: Preto/Cinza Escuro
                cTexto = TemaGlobal.CorTexto; // Ex: Branco/Cinza Claro
                cInput = TemaGlobal.CorSidebar; // Ex: Cinza Médio (para inputs)
                cBorda = TemaGlobal.CorBorda;
            }
            catch
            {
                // Fallback
                cFundo = Color.White;
                cTexto = Color.Black;
                cInput = Color.WhiteSmoke;
                cBorda = Color.Gray;
            }
        }

        private void ConfigurarLayout()
        {
            this.Text = _idEdicao.HasValue ? "Editar Técnico" : "Novo Técnico";
            this.Size = new Size(400, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // APLICA A COR DE FUNDO DO TEMA
            this.BackColor = cFundo;

            // Nome
            Label lblNome = CriarLabel("Nome Completo *", 20, 20);
            txtNome = CriarInput(20, 45, 340, 30);

            // Observação
            Label lblObs = CriarLabel("Observação", 20, 90);
            txtObservacao = CriarInput(20, 115, 340, 100);
            txtObservacao.Multiline = true;

            // Botões
            btnSalvar = new Button { Text = "Salvar", Top = 260, Left = 190, Width = 80, Height = 35, BackColor = ColorTranslator.FromHtml("#238636"), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancelar = new Button { Text = "Cancelar", Top = 260, Left = 280, Width = 80, Height = 35, BackColor = ColorTranslator.FromHtml("#da3633"), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

            btnSalvar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatAppearance.BorderSize = 0;

            btnSalvar.Click += Salvar;
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.Add(lblNome);
            this.Controls.Add(txtNome);
            this.Controls.Add(lblObs);
            this.Controls.Add(txtObservacao);
            this.Controls.Add(btnSalvar);
            this.Controls.Add(btnCancelar);
        }

        // Helpers para criar componentes já estilizados
        private Label CriarLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = cTexto // Cor do texto dinâmica
            };
        }

        private TextBox CriarInput(int x, int y, int w, int h)
        {
            var t = new TextBox
            {
                Location = new Point(x, y),
                Width = w,
                Height = h,
                Font = new Font("Segoe UI", 10),
                BackColor = cInput, // Fundo do input dinâmico
                ForeColor = cTexto, // Texto do input dinâmico
                BorderStyle = BorderStyle.FixedSingle
            };
            return t;
        }

        private void CarregarDados(int id)
        {
            try
            {
                var repo = new TecnicoRepository();
                var tecnico = repo.ObterPorId(id);

                if (tecnico != null)
                {
                    txtNome.Text = tecnico.Nome;
                    txtObservacao.Text = tecnico.Observacao;
                    this.Text = "Editar Técnico - " + tecnico.Nome;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void Salvar(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text)) { MessageBox.Show("Nome obrigatório!"); return; }

            var tecnico = new Tecnico { Nome = txtNome.Text.Trim(), Observacao = txtObservacao.Text.Trim() };
            var repo = new TecnicoRepository();

            try
            {
                if (_idEdicao.HasValue)
                {
                    tecnico.Id = _idEdicao.Value;
                    repo.Atualizar(tecnico);
                }
                else
                {
                    repo.Inserir(tecnico);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }
    }
}