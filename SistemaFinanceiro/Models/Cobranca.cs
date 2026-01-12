using System;

namespace SistemaFinanceiro.Models
{
    public class Cobranca
    {
        public int IdCobranca { get; set; }
        public int AlunoId { get; set; }
        public string NomeAluno { get; set; }
        public string CpfAluno { get; set; }
        public string CategoriaDescricao { get; set; }
        public decimal ValorBase { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public int StatusId { get; set; } // 1=Pendente, 2=Pago, 3=Atrasado

        // Propriedade para exibir no Grid (Texto bonitinho)
        public string StatusDescricao
        {
            get
            {
                if (StatusId == 2) return "Pago";
                if (DataVencimento.Date < DateTime.Today) return "Atrasado";
                return "Pendente";
            }
        }
    }
}