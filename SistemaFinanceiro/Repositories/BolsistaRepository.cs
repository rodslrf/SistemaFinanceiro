using MySqlConnector;
using SistemaFinanceiro.Database;
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;

namespace SistemaFinanceiro.Repositories
{
    // Classe auxiliar para o ComboBox de Bolsas
    public class BolsistaItem
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public decimal Percentual { get; set; }

        // Propriedade extra para facilitar a exibição no Combo (Ex: "Bolsa Parcial - 50%")
        public string DescricaoCompleta => $"{Descricao} ({Percentual:F0}%)";
    }

    public class BolsistaRepository
    {
        public void Inserir(Bolsista bolsista)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Bolsistas (descricao, percentual) VALUES (@Descricao, @Percentual)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Descricao", bolsista.Descricao);
                    cmd.Parameters.AddWithValue("@Percentual", bolsista.Percentual);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Método otimizado para preencher o ComboBox de Cadastro de Alunos
        public List<BolsistaItem> ObterTodosAtivos()
        {
            var lista = new List<BolsistaItem>();
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Trazendo todas as bolsas cadastradas
                string sql = "SELECT id, descricao, percentual FROM Bolsistas ORDER BY descricao";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new BolsistaItem
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Descricao = reader["descricao"].ToString(),
                            Percentual = Convert.ToDecimal(reader["percentual"])
                        });
                    }
                }
            }
            return lista;
        }

        // Para a Grid de Gerenciamento de Bolsas (se houver)
        public List<Bolsista> ObterTodos()
        {
            var lista = new List<Bolsista>();
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Bolsistas ORDER BY descricao";
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

        public Bolsista ObterPorId(int id)
        {
            Bolsista bolsista = null;
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Bolsistas WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bolsista = Mapear(reader);
                        }
                    }
                }
            }
            return bolsista;
        }

        public void Atualizar(Bolsista bolsista)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Bolsistas SET descricao = @Descricao, percentual = @Percentual WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Descricao", bolsista.Descricao);
                    cmd.Parameters.AddWithValue("@Percentual", bolsista.Percentual);
                    cmd.Parameters.AddWithValue("@Id", bolsista.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Excluir(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Bolsistas WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private Bolsista Mapear(MySqlDataReader reader)
        {
            return new Bolsista
            {
                Id = Convert.ToInt32(reader["id"]),
                Descricao = reader["descricao"].ToString(),
                Percentual = Convert.ToDecimal(reader["percentual"])
            };
        }
    }
}