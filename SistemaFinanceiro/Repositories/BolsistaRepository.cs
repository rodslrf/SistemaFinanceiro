using System.Collections.Generic;
using System.Data.SqlClient;
using SistemaFinanceiro.Models;
using System.Data;

namespace SistemaFinanceiro.Repositories
{
    public class BolsistaRepository
    {
        private string stringConexao = "Server=localhost;Database=sistemaFinanceiro;Trusted_Connection=True;";

        public List<Bolsista> ObterTodosAtivos()
        {
            var lista = new List<Bolsista>();
            using (var conn = new SqlConnection(stringConexao))
            {
                conn.Open();
                // REMOVIDO "WHERE ativo = 1"
                string sql = "SELECT * FROM Bolsistas ORDER BY descricao";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Bolsista
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Descricao = reader.GetString(reader.GetOrdinal("descricao")),
                                Percentual = reader.GetDecimal(reader.GetOrdinal("percentual"))
                                // Removemos a linha que lia o 'ativo'
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}