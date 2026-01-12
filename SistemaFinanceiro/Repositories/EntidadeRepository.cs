using SistemaFinanceiro.Database;
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SistemaFinanceiro.Repositories
{
    public class EntidadeRepository
    {
        // Cadastro e Atualizações
        public void Inserir(Entidade entidade)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();

                string query = @"INSERT INTO Entidades 
                      (nome, tipo_vinculo, cpf_atleta, cpf_pais, email_pais, telefone_pais, data_nascimento, categoria_id, status, valor_mensalidade, dia_vencimento) 
                      VALUES 
                      (@Nome, @TipoVinculo, @CpfAtleta, @CpfPais, @EmailPais, @TelefonePais, @DataNascimento, @CategoriaId, @Status, @Valor, @Dia)";

                using (var comando = new SqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@Nome", entidade.Nome);
                    comando.Parameters.AddWithValue("@TipoVinculo", entidade.TipoVinculo);
                    comando.Parameters.AddWithValue("@CpfAtleta", (object)entidade.CpfAtleta ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@CpfPais", (object)entidade.CpfPais ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@EmailPais", (object)entidade.EmailPais ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@TelefonePais", (object)entidade.TelefonePais ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@DataNascimento", entidade.DataNascimento);
                    comando.Parameters.AddWithValue("@CategoriaId", entidade.Categoria_id);
                    comando.Parameters.AddWithValue("@Status", entidade.Status);

                    // Dados financeiros
                    comando.Parameters.AddWithValue("@Valor", entidade.ValorMensalidade);
                    comando.Parameters.AddWithValue("@Dia", entidade.DiaVencimento);

                    comando.ExecuteNonQuery();
                }
            }
        }

        public void Atualizar(Entidade entidade)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();

                string query = @"UPDATE Entidades SET 
                      nome=@Nome, 
                      cpf_atleta=@CpfAtleta, 
                      cpf_pais=@CpfPais, 
                      email_pais=@EmailPais, 
                      telefone_pais=@TelefonePais, 
                      data_nascimento=@DataNascimento, 
                      categoria_id=@CategoriaId,
                      valor_mensalidade=@Valor, 
                      dia_vencimento=@Dia 
                      WHERE id_entidade=@Id";

                using (var comando = new SqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@Id", entidade.Id);
                    comando.Parameters.AddWithValue("@Nome", entidade.Nome);
                    comando.Parameters.AddWithValue("@CpfAtleta", (object)entidade.CpfAtleta ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@CpfPais", (object)entidade.CpfPais ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@EmailPais", (object)entidade.EmailPais ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@TelefonePais", (object)entidade.TelefonePais ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@DataNascimento", entidade.DataNascimento);
                    comando.Parameters.AddWithValue("@CategoriaId", entidade.Categoria_id);

                    // Dados financeiros
                    comando.Parameters.AddWithValue("@Valor", entidade.ValorMensalidade);
                    comando.Parameters.AddWithValue("@Dia", entidade.DiaVencimento);

                    comando.ExecuteNonQuery();
                }
            }
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

                                // Verifica nulos para campos numéricos opcionais
                                ValorMensalidade = leitor["valor_mensalidade"] != DBNull.Value ? (decimal)leitor["valor_mensalidade"] : 0,
                                DiaVencimento = leitor["dia_vencimento"] != DBNull.Value ? (int)leitor["dia_vencimento"] : 10
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

                // Traz a descrição da categoria junto com o aluno
                string query = @"
                    SELECT e.id_entidade, e.nome, e.cpf_atleta, e.status, c.descricao as CategoriaDescricao 
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
                            CategoriaDescricao = leitor["CategoriaDescricao"]
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
                string query = @"
                    UPDATE Entidades 
                    SET status = CASE WHEN status = 'Ativo' THEN 'Inativo' ELSE 'Ativo' END 
                    WHERE id_entidade = @id";

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