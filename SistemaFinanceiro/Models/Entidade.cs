using System;

namespace SistemaFinanceiro.Models
{
    public class Entidade
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string TipoVinculo { get; set; }
        public string CpfAtleta { get; set; }
        public string email_atleta { get; set; }
        public string modalidade { get; set; }
        public string nome_responsavel { get; set; }
        public string nome_responsavel2 { get; set; }

        public string CpfPais { get; set; }
        public string EmailPais { get; set; }
        public string TelefonePais { get; set; }
        public DateTime DataNascimento { get; set; }
        public int Categoria_id { get; set; }
        public string CategoriaDescricao { get; set; }
        public string BolsaDescricao { get; set; }
        public int id_tecnico { get; set; }
        public int? bolsista_id { get; set; }
        public string Status { get; set; }
        public decimal ValorMensalidade { get; set; }
        public int DiaVencimento { get; set; }
        public string telefone_aluno { get; set; }
        public string cpf_pais2 { get; set; }
        public string email_pais2 { get; set; }
        public string telefone_pais2 { get; set; }
        public string observacao { get; set; }
    }
}