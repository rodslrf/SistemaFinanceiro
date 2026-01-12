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

        public string StatusAluno { get; set; }

        public string StatusDescricao
        {
            get
            {
                if (StatusId == 2) return "Pago";
                // Lógica corrigida: Se for Pendente (1) e venceu antes de hoje, é Atrasado
                if (StatusId == 1 && DataVencimento.Date < DateTime.Today) return "Atrasado";
                return "Pendente";
            }
        }
    }
}