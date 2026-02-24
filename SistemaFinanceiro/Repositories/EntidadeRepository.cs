using MySqlConnector;
using SistemaFinanceiro.Database;
using SistemaFinanceiro.Models;
using System;
using System.Collections.Generic;

namespace SistemaFinanceiro.Repositories
{
    public class CategoriaItem
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
    }

    public class EntidadeRepository
    {
        public void Inserir(Entidade entidade)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string query = @"
                    INSERT INTO Entidades 
                    (nome, tipo_vinculo, cpf_atleta, 
                     email_atleta, modalidade, nome_responsavel, nome_responsavel2,
                     cpf_pais, email_pais, telefone_pais, 
                     data_nascimento, categoria_id, id_tecnico, bolsista_id, status, 
                     valor_mensalidade, dia_vencimento, 
                     telefone_aluno, cpf_pais2, email_pais2, telefone_pais2, observacao, created_at) 
                    VALUES 
                    (@Nome, @TipoVinculo, @CpfAtleta, 
                     @EmailAtleta, @Modalidade, @NomeResponsavel, @NomeResponsavel2,
                     @CpfPais, @EmailPais, @TelefonePais, 
                     @DataNascimento, @CategoriaId, @IdTecnico, @BolsistaId, @Status, 
                     @Valor, @Dia, 
                     @TelefoneAluno, @CpfPais2, @EmailPais2, @TelefonePais2, @Observacao, NOW())";

                using (var comando = new MySqlCommand(query, conexao))
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
                    email_atleta=@EmailAtleta,
                    modalidade=@Modalidade,
                    nome_responsavel=@NomeResponsavel,
                    nome_responsavel2=@NomeResponsavel2,
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
                    updated_at = NOW()
                    WHERE id_entidade=@Id";

                using (var comando = new MySqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@Id", entidade.Id);
                    AdicionarParametros(comando, entidade);
                    comando.ExecuteNonQuery();
                }
            }
        }

        // Método auxiliar para evitar repetição de código
        private void AdicionarParametros(MySqlCommand comando, Entidade entidade)
        {
            comando.Parameters.AddWithValue("@Nome", entidade.Nome ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@TipoVinculo", "Aluno");
            comando.Parameters.AddWithValue("@CpfAtleta", entidade.CpfAtleta ?? (object)DBNull.Value);

            comando.Parameters.AddWithValue("@EmailAtleta", entidade.email_atleta ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@Modalidade", entidade.modalidade ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@NomeResponsavel", entidade.nome_responsavel ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@NomeResponsavel2", entidade.nome_responsavel2 ?? (object)DBNull.Value);

            comando.Parameters.AddWithValue("@CpfPais", entidade.CpfPais ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@EmailPais", entidade.EmailPais ?? (object)DBNull.Value);
            comando.Parameters.AddWithValue("@TelefonePais", entidade.TelefonePais ?? (object)DBNull.Value);

            // Garante que a data seja válida para o MySQL
            if (entidade.DataNascimento < new DateTime(1900, 1, 1))
                comando.Parameters.AddWithValue("@DataNascimento", DBNull.Value);
            else
                comando.Parameters.AddWithValue("@DataNascimento", entidade.DataNascimento);

            comando.Parameters.AddWithValue("@CategoriaId", entidade.Categoria_id);
            comando.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(entidade.Status) ? "Ativo" : entidade.Status);
            comando.Parameters.AddWithValue("@IdTecnico", entidade.id_tecnico);

            if (entidade.bolsista_id.HasValue && entidade.bolsista_id > 0)
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

        public bool ExisteCpf(string cpf, int idIgnorar)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                // A mágica acontece aqui: Remove pontos e traços do banco antes de comparar
                string query = @"
                    SELECT COUNT(*) 
                    FROM Entidades 
                    WHERE REPLACE(REPLACE(cpf_atleta, '.', ''), '-', '') = @cpf 
                    AND id_entidade != @id";

                using (var comando = new MySqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@cpf", cpf); // CPF já vem limpo da tela (apenas números)
                    comando.Parameters.AddWithValue("@id", idIgnorar);

                    int count = Convert.ToInt32(comando.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public Entidade ObterPorId(int id)
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string query = "SELECT * FROM Entidades WHERE id_entidade = @id";

                using (var comando = new MySqlCommand(query, conexao))
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

                                // Campos novos
                                email_atleta = leitor["email_atleta"] as string,
                                modalidade = leitor["modalidade"] as string,
                                nome_responsavel = leitor["nome_responsavel"] as string,
                                nome_responsavel2 = leitor["nome_responsavel2"] as string,

                                CpfPais = leitor["cpf_pais"] as string,
                                EmailPais = leitor["email_pais"] as string,
                                TelefonePais = leitor["telefone_pais"] as string,
                                DataNascimento = leitor["data_nascimento"] != DBNull.Value ? (DateTime)leitor["data_nascimento"] : DateTime.Now,
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
                    SELECT 
                        e.id_entidade, 
                        e.nome, 
                        e.cpf_atleta, 
                        e.status, 
                        e.bolsista_id, -- Necessário para contar manualmente se precisar
                        c.descricao as CategoriaDescricao, 
                        IFNULL(b.descricao, 'Sem Bolsa') as BolsaDescricao
                    FROM Entidades e
                    LEFT JOIN Categorias c ON e.categoria_id = c.categoria_id
                    LEFT JOIN Bolsistas b ON e.bolsista_id = b.id
                    ORDER BY e.nome";

                using (var comando = new MySqlCommand(query, conexao))
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
                            bolsista_id = leitor["bolsista_id"], // Usado para lógica de filtros
                            CategoriaDescricao = leitor["CategoriaDescricao"],
                            BolsaDescricao = leitor["BolsaDescricao"]
                        });
                    }
                }
            }
            return listaResultados;
        }

        public int ContarBolsistasAtivos()
        {
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                // Conta apenas quem tem percentual de desconto > 0 e está ativo
                string query = @"
                    SELECT COUNT(*) 
                    FROM Entidades e
                    INNER JOIN Bolsistas b ON e.bolsista_id = b.id
                    WHERE e.status = 'Ativo' AND b.percentual > 0";

                using (var comando = new MySqlCommand(query, conexao))
                {
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
        }

        public List<CategoriaItem> ObterCategorias()
        {
            var listaCategorias = new List<CategoriaItem>();
            using (var conexao = DbConnection.GetConnection())
            {
                conexao.Open();
                string query = "SELECT categoria_id, descricao FROM Categorias";

                using (var comando = new MySqlCommand(query, conexao))
                using (var leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        listaCategorias.Add(new CategoriaItem
                        {
                            Id = (int)leitor["categoria_id"],
                            Descricao = leitor["descricao"].ToString()
                        });
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
                string query = @"UPDATE Entidades SET status = CASE WHEN status = 'Ativo' THEN 'Inativo' ELSE 'Ativo' END WHERE id_entidade = @id";
                using (var comando = new MySqlCommand(query, conexao))
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
                using (var comando = new MySqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }
    }
}