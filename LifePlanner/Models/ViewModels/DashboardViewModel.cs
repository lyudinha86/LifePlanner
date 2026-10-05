using LifePlanner.Data.Entities;

namespace LifePlanner.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        // Tarefas
        public int TarefasPendentes { get; set; }

        public int TarefasConcluidas { get; set; }

        // Eventos
        public int ProximosEventos { get; set; }

        // Objetivos
        public int ObjetivosEmProgresso { get; set; }

        // Finanças
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }

        public decimal TotalPorPagar { get; set; }

        public decimal Saldo => TotalReceitas - TotalDespesas;

        // Listas para mostrar no Dashboard
        public IEnumerable<TaskItem> TarefasRecentes { get; set; }
            = new List<TaskItem>();

        public IEnumerable<PlannerEvent> EventosProximos { get; set; }
            = new List<PlannerEvent>();

        public IEnumerable<Goal> ObjetivosRecentes { get; set; }
            = new List<Goal>();

        public IEnumerable<FinancialTransaction> PagamentosProximos { get; set; }
           = new List<FinancialTransaction>();

        // Despesas agrupadas por categoria
        public IEnumerable<DashboardCategoryViewModel> DespesasPorCategoria { get; set; }
            = new List<DashboardCategoryViewModel>();
    }

    public class DashboardCategoryViewModel
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentagem { get; set; }
    }
}
