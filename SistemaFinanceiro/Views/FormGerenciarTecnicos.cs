using SistemaFinanceiro.Models;
using SistemaFinanceiro.Repositories;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaFinanceiro.Views
{
    // AQUI ESTAVA O ERRO: Adicionei 'partial' para funcionar com o Visual Studio
    public partial class FormCadastroTecnico : Form
    {
        // Controles da Tela (criados via código)
        private TextBox txtNome;
        private TextBox txtObservacao;
        private Button btnSalvar;
        private Button btnCancelar;

        // Variável para controlar se é Edição
        private int? _idEdicao = null;

        // CONSTRUTOR 1: Para novo cadastro
        public FormCadastroTecnico()
        {
            // InitializeComponent(); // Se der erro, pode remover ou descomentar essa linha, mas ConfigurarLayout já faz o trabalho.
            ConfigurarLayout();
        }

        // CONSTRUTOR 2: Para edição (recebe o ID)
        public FormCadastroTecnico(int id) : this()
        {
            _idEdicao = id;
            this.Load += (s, e) => CarregarDados(id); // Carrega os dados ao abrir
        }

        private void ConfigurarLayout()
        {
            this.Text = _idEdicao.HasValue ? "Editar Técnico" : "Novo Técnico";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // -- Verifica se os controles já não foram criados pelo Designer para evitar duplicidade --

            // Label e Input Nome
            Label lblNome = new Label { Text = "Nome Completo *", Top = 20, Left = 20, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtNome = new TextBox { Top = 45, Left = 20, Width = 340, Font = new Font("Segoe UI", 10) };

            // Label e Input Observação
            Label lblObs = new Label { Text = "Observação", Top = 90, Left = 20, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtObservacao = new TextBox { Top = 115, Left = 20, Width = 340, Height = 100, Multiline = true, Font = new Font("Segoe UI", 10) };

            // Botões
            btnSalvar = new Button { Text = "Salvar", Top = 240, Left = 190, Width = 80, Height = 35, BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancelar = new Button { Text = "Cancelar", Top = 240, Left = 280, Width = 80, Height = 35, BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

            btnSalvar.Click += Salvar;
            btnCancelar.Click += (s, e) => this.Close();

            // Adiciona na tela
            this.Controls.Add(lblNome);
            this.Controls.Add(txtNome);
            this.Controls.Add(lblObs);
            this.Controls.Add(txtObservacao);
            this.Controls.Add(btnSalvar);
            this.Controls.Add(btnCancelar);
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
                else
                {
                    MessageBox.Show("Técnico não encontrado!");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar dados: " + ex.Message);
            }
        }

        private void Salvar(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("O nome é obrigatório!");
                return;
            }

            var tecnico = new Tecnico
            {
                Nome = txtNome.Text.Trim(),
                Observacao = txtObservacao.Text.Trim()
            };

            var repo = new TecnicoRepository();

            try
            {
                if (_idEdicao.HasValue)
                {
                    // É EDIÇÃO
                    tecnico.Id = _idEdicao.Value;
                    repo.Atualizar(tecnico);
                    MessageBox.Show("Técnico atualizado com sucesso!");
                }
                else
                {
                    // É NOVO
                    repo.Inserir(tecnico);
                    MessageBox.Show("Técnico cadastrado com sucesso!");
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }
    }
}