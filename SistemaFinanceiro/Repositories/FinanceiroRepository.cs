using Microsoft.Data.SqlClient;
using SistemaFinanceiro.Database;
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace SistemaFinanceiro.Repositories
{
    public class FinanceiroRepository
    {
        // ================= LEITURA (GRID) =================
        public List<Cobranca> ObterTodasCobrancas()
        {
            var lista = new List<Cobranca>();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                // CORREÇÃO AQUI: Troquei 'cat.id' por 'cat.id_categoria'
                // Assumindo que sua tabela Categorias segue o padrão id_tabela
                string sql = @"
                    SELECT 
                        c.id_cobranca AS IdCobranca,
                        c.valor_base AS ValorBase,
                        c.data_vencimento AS DataVencimento,
                        c.entidade_id AS AlunoId,
                        c.status_id AS StatusId,
                        CASE WHEN c.status_id = 2 THEN c.updated_at ELSE NULL END AS DataPagamento,
                        e.nome AS NomeAluno,
                        e.cpf_atleta AS CpfAluno,
                        ISNULL(cat.descricao, 'Sem Categoria') AS CategoriaDescricao
                    FROM Cobrancas c
                    INNER JOIN Entidades e ON c.entidade_id = e.id_entidade
                    LEFT JOIN Categorias cat ON e.categoria_id = cat.categoria_id  -- <--- CORREÇÃO AQUI
                    ORDER BY 
                        CASE WHEN c.status_id = 2 THEN 1 ELSE 0 END, 
                        c.data_vencimento DESC";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var obj = new Cobranca();
                        obj.IdCobranca = Convert.ToInt32(reader["IdCobranca"]);
                        obj.AlunoId = Convert.ToInt32(reader["AlunoId"]);
                        obj.ValorBase = Convert.ToDecimal(reader["ValorBase"]);
                        obj.DataVencimento = Convert.ToDateTime(reader["DataVencimento"]);

                        // StatusId: 1=Pendente, 2=Pago
                        int statusId = reader["StatusId"] != DBNull.Value ? Convert.ToInt32(reader["StatusId"]) : 1;
                        obj.StatusId = statusId;

                        if (statusId == 2 && reader["DataPagamento"] != DBNull.Value)
                            obj.DataPagamento = Convert.ToDateTime(reader["DataPagamento"]);

                        obj.NomeAluno = reader["NomeAluno"].ToString();
                        obj.CpfAluno = reader["CpfAluno"].ToString();
                        obj.CategoriaDescricao = reader["CategoriaDescricao"].ToString();

                        lista.Add(obj);
                    }
                }
            }
            return lista;
        }

        // ================= GERAÇÃO (CORRIGIDO PARA id_entidade) =================
        public void GerarCobrancasMensais()
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                // Busca alunos ativos. Se valor_mensalidade for NULL, assume 100.
                string sqlAlunos = @"
                    SELECT 
                        id_entidade, 
                        ISNULL(valor_mensalidade, 100) as Valor, 
                        ISNULL(dia_vencimento, 10) as Dia 
                    FROM Entidades 
                    WHERE status = 'Ativo'";

                var alunosParaGerar = new List<dynamic>();

                using (var cmd = new SqlCommand(sqlAlunos, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        alunosParaGerar.Add(new
                        {
                            Id = Convert.ToInt32(reader["id_entidade"]),
                            Valor = Convert.ToDecimal(reader["Valor"]),
                            Dia = Convert.ToInt32(reader["Dia"])
                        });
                    }
                }

                foreach (var aluno in alunosParaGerar)
                {
                    DateTime hoje = DateTime.Now;
                    int diasNoMes = DateTime.DaysInMonth(hoje.Year, hoje.Month);
                    int diaVencimento = Math.Min((int)aluno.Dia, diasNoMes);

                    DateTime dataVencimento = new DateTime(hoje.Year, hoje.Month, diaVencimento);
                    string mesReferencia = dataVencimento.ToString("MM/yyyy");

                    // Verifica duplicidade usando mes_referencia
                    string sqlCheck = @"
                        SELECT COUNT(*) FROM Cobrancas 
                        WHERE entidade_id = @id 
                        AND mes_referencia = @mesRef";

                    using (var cmdCheck = new SqlCommand(sqlCheck, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@id", (int)aluno.Id);
                        cmdCheck.Parameters.AddWithValue("@mesRef", mesReferencia);

                        int count = (int)cmdCheck.ExecuteScalar();

                        if (count == 0)
                        {
                            // status_id = 1 (Pendente)
                            string sqlInsert = @"
                                INSERT INTO Cobrancas (entidade_id, valor_base, data_vencimento, mes_referencia, status_id, created_at) 
                                VALUES (@id, @valor, @dataVenc, @mesRef, 1, GETDATE())";

                            using (var cmdInsert = new SqlCommand(sqlInsert, conn))
                            {
                                cmdInsert.Parameters.AddWithValue("@id", (int)aluno.Id);
                                cmdInsert.Parameters.AddWithValue("@valor", (decimal)aluno.Valor);
                                cmdInsert.Parameters.AddWithValue("@dataVenc", dataVencimento);
                                cmdInsert.Parameters.AddWithValue("@mesRef", mesReferencia);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        // ================= RECEBIMENTO =================
        public void ReceberMensalidade(int idCobranca)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sqlUpdate = "UPDATE Cobrancas SET status_id = 2, updated_at = GETDATE() WHERE id_cobranca = @id";
                using (var cmd = new SqlCommand(sqlUpdate, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idCobranca);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ================= TOTAIS =================
        public TotaisDTO ObterTotaisMes(string mesAnoString)
        {
            var partes = mesAnoString.Split('/');
            int mes = int.Parse(partes[0]);
            int ano = int.Parse(partes[1]);
            var totais = new TotaisDTO();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                // Status 2 = Pago
                string sqlRecebido = "SELECT ISNULL(SUM(valor_base), 0) FROM Cobrancas WHERE status_id = 2 AND MONTH(updated_at) = @m AND YEAR(updated_at) = @a";
                // Status 1 = Pendente
                string sqlPendente = "SELECT ISNULL(SUM(valor_base), 0) FROM Cobrancas WHERE status_id = 1 AND MONTH(data_vencimento) = @m AND YEAR(data_vencimento) = @a";

                using (var cmd = new SqlCommand(sqlRecebido, conn))
                {
                    cmd.Parameters.AddWithValue("@m", mes); cmd.Parameters.AddWithValue("@a", ano);
                    totais.Recebido = Convert.ToDecimal(cmd.ExecuteScalar());
                }
                using (var cmd = new SqlCommand(sqlPendente, conn))
                {
                    cmd.Parameters.AddWithValue("@m", mes); cmd.Parameters.AddWithValue("@a", ano);
                    totais.Pendente = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return totais;
        }
    }

    public class TotaisDTO { public decimal Recebido { get; set; } public decimal Pendente { get; set; } }
}