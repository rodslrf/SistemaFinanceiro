using System;

namespace SistemaFinanceiro.Models
{
    public class FluxoCaixa
    {
        public int Id { get; set; }

        public int? CobrancaId { get; set; }

        public DateTime DataMovimentacao { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }

        // Define a direção da operação: "ENTRADA" ou "SAIDA"
        public string Tipo { get; set; }
        public int CategoriaId { get; set; }

        // Dinheiro, Pix, Cartão, etc
        public string FormaPagamento { get; set; }
    }
}