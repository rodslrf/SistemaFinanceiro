using System;

namespace SistemaFinanceiro.Models
{
    public class FluxoCaixa
    {
        public int Id { get; set; }
        public int? CobrancaId { get; set; } // Pode ser nulo (ex: conta de luz não tem cobrança de aluno)
        public DateTime DataMovimentacao { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; } // "ENTRADA" ou "SAIDA"
        public int CategoriaId { get; set; } // Ex: 1-Mensalidade, 2-Manutenção
        public string FormaPagamento { get; set; } // Dinheiro, Pix, Cartão
    }
}