using MySqlConnector;
using SistemaFinanceiro.Database;
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;

namespace SistemaFinanceiro.Repositories
{
    // Classe auxiliar para o ComboBox não travar
    public class TecnicoItem
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    public class TecnicoRepository
    {
        // Removida a string de conexão fixa "localhost". Agora usa o DbConnection central.

        public void Inserir(Tecnico tecnico)
        {
            using (var conn = DbConnection.GetConnection()) // Usa a conexão correta da KingHost
            {
                conn.Open();
                string sql = "INSERT INTO Tecnicos (nome, status, observacao) VALUES (@Nome, 'Ativo', @Observacao)";
                using (var cmd = new MySqlCommand(sql, conn)) // Mudou para MySqlCommand
                {
                    cmd.Parameters.AddWithValue("@Nome", tecnico.Nome);
                    cmd.Parameters.AddWithValue("@Observacao", tecnico.Observacao ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Método otimizado para preencher o ComboBox (Retorna TecnicoItem)
        public List<TecnicoItem> ObterTodosAtivos()
        {
            var lista = new List<TecnicoItem>();
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT id, nome FROM Tecnicos WHERE status = 'Ativo' ORDER BY nome";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new TecnicoItem
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Nome = reader["nome"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        // Para a Grid de Gerenciamento (Traz ativos e inativos)
        public List<Tecnico> ObterTodos()
        {
            var lista = new List<Tecnico>();
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Tecnicos ORDER BY nome";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(Mapear(reader));
                    }
                }
            }
            return lista;
        }

        public Tecnico ObterPorId(int id)
        {
            Tecnico tecnico = null;
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Tecnicos WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tecnico = Mapear(reader);
                        }
                    }
                }
            }
            return tecnico;
        }

        public void Atualizar(Tecnico tecnico)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Tecnicos SET nome = @Nome, observacao = @Observacao WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nome", tecnico.Nome);
                    cmd.Parameters.AddWithValue("@Observacao", tecnico.Observacao ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", tecnico.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void TrocarStatus(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Tecnicos 
                               SET status = CASE WHEN status = 'Ativo' THEN 'Inativo' ELSE 'Ativo' END 
                               WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private Tecnico Mapear(MySqlDataReader reader)
        {
            return new Tecnico
            {
                Id = Convert.ToInt32(reader["id"]),
                Nome = reader["nome"].ToString(),
                Status = reader["status"].ToString(),
                Observacao = reader["observacao"] != DBNull.Value ? reader["observacao"].ToString() : null
            };
        }
    }
}