using SistemaFinanceiro.Database;
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SistemaFinanceiro.Repositories
{
    public class EntidadeRepository
    {
        public void Inserir(Entidade entidade)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();

                string query = @"
                    INSERT INTO Entidades 
                    (nome, tipo_vinculo, cpf_atleta, cpf_pais, email_pais, telefone_pais, 
                     data_nascimento, categoria_id, id_tecnico, bolsista_id, status, 
                     valor_mensalidade, dia_vencimento, 
                     telefone_aluno, cpf_pais2, email_pais2, telefone_pais2, observacao, created_at) 
                    VALUES 
                    (@Nome, @TipoVinculo, @CpfAtleta, @CpfPais, @EmailPais, @TelefonePais, 
                     @DataNascimento, @CategoriaId, @IdTecnico, @BolsistaId, @Status, 
                     @Valor, @Dia, 
                     @TelefoneAluno, @CpfPais2, @EmailPais2, @TelefonePais2, @Observacao, GETDATE())";

                using (var comando = new SqlCommand(query, conexao))
                {
                    AdicionarParametros(comando, entidade);
                    comando.ExecuteNonQuery();
                }
            }
        }

        public void Atualizar(Entidade entidade)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();

                string query = @"
                    UPDATE Entidades SET 
                    nome=@Nome, 
                    cpf_atleta=@CpfAtleta, 
                    cpf_pais=@CpfPais, 
                    email_pais=@EmailPais, 
                    telefone_pais=@TelefonePais, 
                    data_nascimento=@DataNascimento, 
                    categoria_id=@CategoriaId,
                    id_tecnico=@IdTecnico,
                    bolsista_id=@BolsistaId,
                    valor_mensalidade=@Valor, 
                    dia_vencimento=@Dia,
                    telefone_aluno=@TelefoneAluno,
                    cpf_pais2=@CpfPais2,
                    email_pais2=@EmailPais2,
                    telefone_pais2=@TelefonePais2,
                    observacao=@Observacao,
                    updated_at = GETDATE()
                    WHERE id_entidade=@Id";

                using (var comando = new SqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@Id", entidade.Id);
                    AdicionarParametros(comando, entidade);
                    comando.ExecuteNonQuery();
                }
            }
        }

        private void AdicionarParametros(SqlCommand comando, Entidade entidade)
        {
            comando.Parameters.AddWithValue("@Nome", entidade.Nome ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@TipoVinculo", entidade.TipoVinculo ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@CpfAtleta", entidade.CpfAtleta ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@CpfPais", entidade.CpfPais ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@EmailPais", entidade.EmailPais ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@TelefonePais", entidade.TelefonePais ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@DataNascimento", entidade.DataNascimento);
            comando.Parameters.AddWithValue("@CategoriaId", entidade.Categoria_id);

            // Verifica se o comando tem @Status (Insert)
            if (comando.Parameters.IndexOf("@Status") == -1 && !comando.CommandText.Contains("@Status"))
            {
            }
            else
            {
                comando.Parameters.AddWithValue("@Status", entidade.Status ?? "Ativo");
            }

            comando.Parameters.AddWithValue("@IdTecnico", entidade.id_tecnico);

            if (entidade.bolsista_id.HasValue)
                comando.Parameters.AddWithValue("@BolsistaId", entidade.bolsista_id.Value);
            else
                comando.Parameters.AddWithValue("@BolsistaId", DBNull.Value);

            comando.Parameters.AddWithValue("@Valor", entidade.ValorMensalidade);
            comando.Parameters.AddWithValue("@Dia", entidade.DiaVencimento);

            comando.Parameters.AddWithValue("@TelefoneAluno", entidade.telefone_aluno ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@CpfPais2", entidade.cpf_pais2 ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@EmailPais2", entidade.email_pais2 ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@TelefonePais2", entidade.telefone_pais2 ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@Observacao", entidade.observacao ?? (object)DBNull.Value);
        }

        public Entidade ObterPorId(int id)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string query = "SELECT * FROM Entidades WHERE id_entidade = @id";

                using (var comando = new SqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    using (var leitor = comando.ExecuteReader())
                    {
                        if (leitor.Read())
                        {
                            return new Entidade
                            {
                                Id = (int)leitor["id_entidade"],
                                Nome = leitor["nome"].ToString(),
                                CpfAtleta = leitor["cpf_atleta"] as string,
                                CpfPais = leitor["cpf_pais"] as string,
                                EmailPais = leitor["email_pais"] as string,
                                TelefonePais = leitor["telefone_pais"] as string,
                                DataNascimento = (DateTime)leitor["data_nascimento"],
                                Categoria_id = (int)leitor["categoria_id"],
                                Status = leitor["status"].ToString(),
                                id_tecnico = leitor["id_tecnico"] != DBNull.Value ? (int)leitor["id_tecnico"] : 0,
                                bolsista_id = leitor["bolsista_id"] != DBNull.Value ? (int?)leitor["bolsista_id"] : null,
                                ValorMensalidade = leitor["valor_mensalidade"] != DBNull.Value ? (decimal)leitor["valor_mensalidade"] : 0,
                                DiaVencimento = leitor["dia_vencimento"] != DBNull.Value ? (int)leitor["dia_vencimento"] : 10,
                                telefone_aluno = leitor["telefone_aluno"] as string,
                                cpf_pais2 = leitor["cpf_pais2"] as string,
                                email_pais2 = leitor["email_pais2"] as string,
                                telefone_pais2 = leitor["telefone_pais2"] as string,
                                observacao = leitor["observacao"] as string
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<dynamic> ObterTodos()
        {
            var listaResultados = new List<dynamic>();
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string query = @"
                    SELECT e.id_entidade, e.nome, e.cpf_atleta, e.status, c.descricao as CategoriaDescricao, e.bolsista_id as bolsista_id 
                    FROM Entidades e
                    LEFT JOIN Categorias c ON e.categoria_id = c.categoria_id";

                using (var comando = new SqlCommand(query, conexao))
                using (var leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        listaResultados.Add(new
                        {
                            id_entidade = leitor["id_entidade"],
                            Nome = leitor["nome"],
                            CpfAtleta = leitor["cpf_atleta"],
                            Status = leitor["status"],
                            CategoriaDescricao = leitor["CategoriaDescricao"],
                            bolsista_id = leitor["bolsista_id"]
                        });
                    }
                }
            }
            return listaResultados;
        }

        public List<dynamic> ObterCategorias()
        {
            var listaCategorias = new List<dynamic>();
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string query = "SELECT categoria_id, descricao FROM Categorias";

                using (var comando = new SqlCommand(query, conexao))
                using (var leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        listaCategorias.Add(new { Id = (int)leitor["categoria_id"], Descricao = leitor["descricao"].ToString() });
                    }
                }
            }
            return listaCategorias;
        }

        public void AlternarStatus(int id)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                // Alterna entre Ativo e Inativo
                string query = @"UPDATE Entidades SET status = CASE WHEN status = 'Ativo' THEN 'Inativo' ELSE 'Ativo' END WHERE id_entidade = @id";
                using (var comando = new SqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string query = "DELETE FROM Entidades WHERE id_entidade = @id";
                using (var comando = new SqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }
    }
}