using System;

namespace SistemaFinanceiro.Models
{
    public class Entidade
    {
        public decimal ValorMensalidade { get; set; }
        public int DiaVencimento { get; set; }
        // Mapeia para id_entidade
        public int Id { get; set; }
        public string Nome { get; set; }
        public string TipoVinculo { get; set; }
        public string CpfAtleta { get; set; }
        public string CpfPais { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Status { get; set; }
        public int Categoria_id { get; set; }

        // Novos Campos
        public string EmailPais { get; set; }
        public string TelefonePais { get; set; }

        // Auxiliar (não grava no banco, vem do JOIN)
        public string CategoriaDescricao { get; set; }
    }
}