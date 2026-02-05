using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SistemaFinanceiro.Models;
using System.Data;

namespace SistemaFinanceiro.Repositories
{
    public class TecnicoRepository
    {
        private string stringConexao = "Server=localhost;Database=sistemaFinanceiro;Trusted_Connection=True;";

        // Mantive o seu Inserir
        public void Inserir(Tecnico tecnico)
        {
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                string sql = "INSERT INTO Tecnicos (nome, status, observacao) VALUES (@Nome, 'Ativo', @Observacao)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nome", tecnico.Nome);
                    cmd.Parameters.AddWithValue("@Observacao", tecnico.Observacao ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Mantive o seu ObterTodosAtivos (Usado nos ComboBoxes)
        public List<Tecnico> ObterTodosAtivos()
        {
            var lista = new List<Tecnico>();
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                string sql = "SELECT * FROM Tecnicos WHERE status = 'Ativo' ORDER BY nome";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(Mapear(reader));
                        }
                    }
                }
            }
            return lista;
        }

        // NOVO: Para a Grid de Gerenciamento (Traz ativos e inativos)
        public List<Tecnico> ObterTodos()
        {
            var lista = new List<Tecnico>();
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                string sql = "SELECT * FROM Tecnicos ORDER BY nome";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(Mapear(reader));
                        }
                    }
                }
            }
            return lista;
        }

        // NOVO: Para carregar dados na edição
        public Tecnico ObterPorId(int id)
        {
            Tecnico tecnico = null;
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                string sql = "SELECT * FROM Tecnicos WHERE id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
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

        // NOVO: Para salvar edições de nome/observação
        public void Atualizar(Tecnico tecnico)
        {
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                string sql = "UPDATE Tecnicos SET nome = @Nome, observacao = @Observacao WHERE id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nome", tecnico.Nome);
                    cmd.Parameters.AddWithValue("@Observacao", tecnico.Observacao ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", tecnico.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // NOVO: Lógica de Ativar/Desativar
        public void TrocarStatus(int id)
        {
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                // SQL que inverte o status atual
                string sql = @"UPDATE Tecnicos 
                               SET status = CASE WHEN status = 'Ativo' THEN 'Inativo' ELSE 'Ativo' END 
                               WHERE id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private Tecnico Mapear(SqlDataReader reader)
        {
            return new Tecnico
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nome = reader.GetString(reader.GetOrdinal("nome")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                Observacao = reader.IsDBNull(reader.GetOrdinal("observacao")) ? null : reader.GetString(reader.GetOrdinal("observacao"))
            };
        }
    }
}