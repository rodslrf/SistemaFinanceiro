using SistemaFinanceiro.Database; // Confirma que está usando a conexão correta
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SistemaFinanceiro.Repositories
{
    public class EntidadeRepository
    {
        // =====================================================================
        //                    INSERIR (CRIAR NOVO)
        // =====================================================================
        public void Inserir(Entidade entidade)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // CORREÇÃO: Nomes das colunas ajustados para minúsculo_com_underline (padrão do seu banco)
                string sql = @"INSERT INTO Entidades 
                     (nome, tipo_vinculo, cpf_atleta, cpf_pais, email_pais, telefone_pais, data_nascimento, categoria_id, status, valor_mensalidade, dia_vencimento) 
                     VALUES 
                     (@Nome, @TipoVinculo, @CpfAtleta, @CpfPais, @EmailPais, @TelefonePais, @DataNascimento, @Categoria_id, @Status, @Valor, @Dia)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nome", entidade.Nome);
                    cmd.Parameters.AddWithValue("@TipoVinculo", entidade.TipoVinculo);
                    cmd.Parameters.AddWithValue("@CpfAtleta", (object)entidade.CpfAtleta ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CpfPais", (object)entidade.CpfPais ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailPais", (object)entidade.EmailPais ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TelefonePais", (object)entidade.TelefonePais ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DataNascimento", entidade.DataNascimento);
                    cmd.Parameters.AddWithValue("@Categoria_id", entidade.Categoria_id);
                    cmd.Parameters.AddWithValue("@Status", entidade.Status);

                    // Financeiro
                    cmd.Parameters.AddWithValue("@Valor", entidade.ValorMensalidade);
                    cmd.Parameters.AddWithValue("@Dia", entidade.DiaVencimento);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =====================================================================
        //                    ATUALIZAR (EDITAR EXISTENTE)
        // =====================================================================
        public void Atualizar(Entidade entidade)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // CORREÇÃO: Nomes das colunas ajustados aqui também
                string sql = @"UPDATE Entidades SET 
                     nome=@Nome, 
                     cpf_atleta=@CpfAtleta, 
                     cpf_pais=@CpfPais, 
                     email_pais=@EmailPais, 
                     telefone_pais=@TelefonePais, 
                     data_nascimento=@DataNascimento, 
                     categoria_id=@Categoria_id,
                     valor_mensalidade=@Valor, 
                     dia_vencimento=@Dia 
                     WHERE id_entidade=@Id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", entidade.Id);
                    cmd.Parameters.AddWithValue("@Nome", entidade.Nome);
                    cmd.Parameters.AddWithValue("@CpfAtleta", (object)entidade.CpfAtleta ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CpfPais", (object)entidade.CpfPais ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailPais", (object)entidade.EmailPais ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TelefonePais", (object)entidade.TelefonePais ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DataNascimento", entidade.DataNascimento);
                    cmd.Parameters.AddWithValue("@Categoria_id", entidade.Categoria_id);

                    // Financeiro
                    cmd.Parameters.AddWithValue("@Valor", entidade.ValorMensalidade);
                    cmd.Parameters.AddWithValue("@Dia", entidade.DiaVencimento);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =====================================================================
        //                    OBTER POR ID (CARREGAR DADOS)
        // =====================================================================
        public Entidade ObterPorId(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Entidades WHERE id_entidade = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // CORREÇÃO: Ao ler do banco, usamos os nomes das colunas como estão lá (snake_case)
                            // e jogamos para as propriedades do C# (PascalCase)
                            return new Entidade
                            {
                                Id = (int)reader["id_entidade"],
                                Nome = reader["nome"].ToString(),
                                CpfAtleta = reader["cpf_atleta"] as string,
                                CpfPais = reader["cpf_pais"] as string,
                                EmailPais = reader["email_pais"] as string,
                                TelefonePais = reader["telefone_pais"] as string,
                                DataNascimento = (DateTime)reader["data_nascimento"],
                                Categoria_id = (int)reader["categoria_id"],
                                Status = reader["status"].ToString(),

                                // Lê os novos campos, verificando se são nulos
                                ValorMensalidade = reader["valor_mensalidade"] != DBNull.Value ? (decimal)reader["valor_mensalidade"] : 0,
                                DiaVencimento = reader["dia_vencimento"] != DBNull.Value ? (int)reader["dia_vencimento"] : 10
                            };
                        }
                    }
                }
            }
            return null;
        }

        // =====================================================================
        //                    OBTER TODOS (LISTAGEM)
        // =====================================================================
        public List<dynamic> ObterTodos()
        {
            var lista = new List<dynamic>();
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Join simples para pegar o nome da categoria
                string sql = @"
                    SELECT e.id_entidade, e.nome, e.cpf_atleta, e.status, c.descricao as CategoriaDescricao 
                    FROM Entidades e
                    LEFT JOIN Categorias c ON e.categoria_id = c.categoria_id";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Aqui usamos um objeto anônimo (dynamic) para alimentar o GridView
                        lista.Add(new
                        {
                            id_entidade = reader["id_entidade"],
                            Nome = reader["nome"],
                            CpfAtleta = reader["cpf_atleta"],
                            Status = reader["status"],
                            CategoriaDescricao = reader["CategoriaDescricao"]
                        });
                    }
                }
            }
            return lista;
        }

        // =====================================================================
        //                    OBTER CATEGORIAS (COMBOBOX)
        // =====================================================================
        public List<dynamic> ObterCategorias()
        {
            var lista = new List<dynamic>();
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT categoria_id, descricao FROM Categorias";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new { Id = (int)reader["categoria_id"], Descricao = reader["descricao"].ToString() });
                    }
                }
            }
            return lista;
        }

        // =====================================================================
        //                    ALTERNAR STATUS (ATIVO/INATIVO)
        // =====================================================================
        public void AlternarStatus(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = @"
                    UPDATE Entidades 
                    SET status = CASE WHEN status = 'Ativo' THEN 'Inativo' ELSE 'Ativo' END 
                    WHERE id_entidade = @id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void Excluir(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Cuidado: Se tiver cobranças vinculadas, vai dar erro de FK.
                // O ideal é Inativar (AlternarStatus), mas se quiser excluir mesmo:
                string sql = "DELETE FROM Entidades WHERE id_entidade = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}