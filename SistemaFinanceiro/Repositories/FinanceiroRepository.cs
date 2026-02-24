using MySqlConnector;
using SistemaFinanceiro.Database;
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace SistemaFinanceiro.Repositories
{
    public class FinanceiroRepository
    {
        public List<Cobranca> ObterTodasCobrancas()
        {
            var listaCobrancas = new List<Cobranca>();

            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();

                string query = @"
                    SELECT 
                        c.id_cobranca AS IdCobranca,
                        c.valor_base AS ValorBase,
                        c.data_vencimento AS DataVencimento,
                        c.id_entidade AS AlunoId,
                        c.status_id AS StatusId,
                        CASE WHEN c.status_id = 2 THEN c.updated_at ELSE NULL END AS DataPagamento,
                        e.nome AS NomeAluno,
                        e.cpf_atleta AS CpfAluno,
                        e.bolsista_id,
                        e.status AS StatusAluno,
                        IFNULL(cat.descricao, 'Sem Categoria') AS CategoriaDescricao
                    FROM Cobrancas c
                    INNER JOIN Entidades e ON c.id_entidade = e.id_entidade
                    LEFT JOIN Categorias cat ON e.categoria_id = cat.categoria_id 
                    ORDER BY 
                        CASE WHEN c.status_id = 2 THEN 1 ELSE 0 END, 
                        c.data_vencimento DESC";

                using (var comando = new MySqlCommand(query, conexao))
                using (var leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        var cobranca = new Cobranca();
                        cobranca.IdCobranca = Convert.ToInt32(leitor["IdCobranca"]);
                        cobranca.AlunoId = Convert.ToInt32(leitor["AlunoId"]);
                        cobranca.BolsistaId = leitor["bolsista_id"] != DBNull.Value ? Convert.ToInt32(leitor["bolsista_id"]) : 0;
                        cobranca.ValorBase = Convert.ToDecimal(leitor["ValorBase"]);
                        cobranca.DataVencimento = Convert.ToDateTime(leitor["DataVencimento"]);

                        int statusId = leitor["StatusId"] != DBNull.Value ? Convert.ToInt32(leitor["StatusId"]) : 1;
                        cobranca.StatusId = statusId;

                        if (statusId == 2 && leitor["DataPagamento"] != DBNull.Value)
                            cobranca.DataPagamento = Convert.ToDateTime(leitor["DataPagamento"]);

                        cobranca.NomeAluno = leitor["NomeAluno"].ToString();
                        cobranca.CpfAluno = leitor["CpfAluno"].ToString();
                        cobranca.StatusAluno = leitor["StatusAluno"].ToString();
                        cobranca.CategoriaDescricao = leitor["CategoriaDescricao"].ToString();

                        listaCobrancas.Add(cobranca);
                    }
                }
            }
            return listaCobrancas;
        }

        public void GerarCobrancasMensais()
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string queryAlunos = "SELECT id_entidade, IFNULL(valor_mensalidade, 100) as Valor, IFNULL(dia_vencimento, 10) as Dia FROM Entidades WHERE status = 'Ativo'";
                var listaAlunosAtivos = new List<dynamic>();

                using (var cmd = new MySqlCommand(queryAlunos, conexao))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listaAlunosAtivos.Add(new { Id = Convert.ToInt32(reader["id_entidade"]), Valor = Convert.ToDecimal(reader["Valor"]), Dia = Convert.ToInt32(reader["Dia"]) });
                    }
                }

                foreach (var aluno in listaAlunosAtivos)
                {
                    DateTime dataAtual = DateTime.Now;
                    int diasNoMes = DateTime.DaysInMonth(dataAtual.Year, dataAtual.Month);
                    int diaVencimentoValidado = Math.Min((int)aluno.Dia, diasNoMes);
                    DateTime dataVencimento = new DateTime(dataAtual.Year, dataAtual.Month, diaVencimentoValidado);
                    string referenciaMes = dataVencimento.ToString("MM/yyyy");

                    string queryVerifica = "SELECT COUNT(*) FROM Cobrancas WHERE entidade_id = @id AND mes_referencia = @mesRef";
                    using (var cmdVerifica = new MySqlCommand(queryVerifica, conexao))
                    {
                        cmdVerifica.Parameters.AddWithValue("@id", (int)aluno.Id);
                        cmdVerifica.Parameters.AddWithValue("@mesRef", referenciaMes);

                        if (Convert.ToInt32(cmdVerifica.ExecuteScalar()) == 0)
                        {
                            string queryInsert = "INSERT INTO Cobrancas (entidade_id, valor_base, data_vencimento, mes_referencia, status_id, created_at) VALUES (@id, @valor, @dataVenc, @mesRef, 1, NOW())";
                            using (var cmdInsert = new MySqlCommand(queryInsert, conexao))
                            {
                                cmdInsert.Parameters.AddWithValue("@id", (int)aluno.Id);
                                cmdInsert.Parameters.AddWithValue("@valor", (decimal)aluno.Valor);
                                cmdInsert.Parameters.AddWithValue("@dataVenc", dataVencimento);
                                cmdInsert.Parameters.AddWithValue("@mesRef", referenciaMes);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        public void ReceberMensalidade(int idCobranca)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                using (var comando = new MySqlCommand("UPDATE Cobrancas SET status_id = 2, updated_at = NOW() WHERE id_cobranca = @id", conexao))
                {
                    comando.Parameters.AddWithValue("@id", idCobranca);
                    comando.ExecuteNonQuery();
                }
            }
        }

        public TotaisDTO ObterTotaisMes(string mesAnoReferencia)
        {
            var partesData = mesAnoReferencia.Split('/');
            int mes = int.Parse(partesData[0]);
            int ano = int.Parse(partesData[1]);
            var totais = new TotaisDTO();

            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();

                string queryRecebido = @"
                    SELECT IFNULL(SUM(c.valor_base), 0) 
                    FROM Cobrancas c
                    WHERE c.status_id = 2 
                    AND MONTH(c.updated_at) = @m AND YEAR(c.updated_at) = @a";

                string queryPendente = @"
                    SELECT IFNULL(SUM(c.valor_base), 0) 
                    FROM Cobrancas c
                    INNER JOIN Entidades e ON c.id_entidade = e.id_entidade
                    WHERE c.status_id = 1 
                    AND e.status = 'Ativo' 
                    AND MONTH(c.data_vencimento) = @m AND YEAR(c.data_vencimento) = @a";

                using (var cmd = new MySqlCommand(queryRecebido, conexao))
                {
                    cmd.Parameters.AddWithValue("@m", mes);
                    cmd.Parameters.AddWithValue("@a", ano);
                    totais.Recebido = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                using (var cmd = new MySqlCommand(queryPendente, conexao))
                {
                    cmd.Parameters.AddWithValue("@m", mes);
                    cmd.Parameters.AddWithValue("@a", ano);
                    totais.Pendente = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return totais;
        }
    }

    public class TotaisDTO
    {
        public decimal Recebido { get; set; }
        public decimal Pendente { get; set; }
    }
}