using LifePlanner.Data.Entities;

namespace LifePlanner.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Tarefas
        public int TarefasPendentes { get; set; }

        // Eventos
        public int ProximosEventos { get; set; }

        // Objetivos
        public int ObjetivosEmProgresso { get; set; }

        // Finanças
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }

        public decimal Saldo => TotalReceitas - TotalDespesas;

        // Listas para mostrar no Dashboard
        public IEnumerable<TaskItem> TarefasRecentes { get; set; }
            = new List<TaskItem>();

        public IEnumerable<PlannerEvent> EventosProximos { get; set; }
            = new List<PlannerEvent>();

        public IEnumerable<Goal> ObjetivosRecentes { get; set; }
            = new List<Goal>();
    }
}
