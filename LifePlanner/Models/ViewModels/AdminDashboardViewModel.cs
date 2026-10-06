using LifePlanner.Data.Entities;

namespace LifePlanner.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUtilizadores { get; set; }
        public int TotalTarefas { get; set; }
        public int TotalEventos { get; set; }
        public int TotalObjetivos { get; set; }
        public int TotalTags { get; set; }

        // Estado das tarefas
        public int TarefasPendentes { get; set; }
        public int TarefasEmProgresso { get; set; }
        public int TarefasConcluidas { get; set; }

        // Utilizadores
        public IEnumerable<User> Utilizadores { get; set; }
            = new List<User>();
    }
}