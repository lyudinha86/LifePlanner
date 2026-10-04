using LifePlanner.Data;
using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using LifePlanner.Models.ViewModels;
using LifePlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LifePlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGenericRepository<TaskItem> _taskRepository;
        private readonly IGenericRepository<Goal> _goalRepository;
        private readonly IGenericRepository<PlannerEvent> _eventRepository;
        private readonly IGenericRepository<FinancialTransaction> _financeRepository;
        private readonly IUserHelper _userHelper;

        public HomeController(
        IGenericRepository<TaskItem> taskRepository,
        IGenericRepository<Goal> goalRepository,
        IGenericRepository<PlannerEvent> eventRepository,
        IGenericRepository<FinancialTransaction> financeRepository,
        IUserHelper userHelper)
        {
            _taskRepository = taskRepository;
            _goalRepository = goalRepository;
            _eventRepository = eventRepository;
            _financeRepository = financeRepository;
            _userHelper = userHelper;
        }

        public async Task<IActionResult> Index()
        {
            // Se o utilizador não estiver autenticado,
            // mostrar a página inicial normal
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View();
            }

            var user = await _userHelper
                .GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return View();
            }

            var hoje = DateTime.Today;

            var tarefas = _taskRepository
                .GetAll()
                .Where(t => t.UserId == user.Id);

            var objetivos = _goalRepository
                .GetAll()
                .Where(g => g.UserId == user.Id);

            var eventos = _eventRepository
                .GetAll()
                .Where(e => e.UserId == user.Id);

            var financas = _financeRepository
                .GetAll()
                .Where(f => f.UserId == user.Id);


            var model = new DashboardViewModel
            {
                TarefasPendentes = await tarefas
                    .CountAsync(t => t.Status != "Concluída"),

                ProximosEventos = await eventos
                    .CountAsync(e => e.StartDate >= hoje),

                ObjetivosEmProgresso = await objetivos
                    .CountAsync(g => g.Status == "Em progresso"),

                TotalReceitas = await financas
                    .Where(f => f.Type == "Receita")
                    .SumAsync(f => (decimal?)f.Amount) ?? 0,

                TotalDespesas = await financas
                    .Where(f => f.Type == "Despesa")
                    .SumAsync(f => (decimal?)f.Amount) ?? 0,

                TarefasRecentes = await tarefas
                    .Where(t =>
                        t.Status != "Concluída" &&
                        t.DueDate != null)
                    .OrderBy(t => t.DueDate)
                    .Take(5)
                    .ToListAsync(),

                EventosProximos = await eventos
                    .Where(e => e.StartDate >= hoje)
                    .OrderBy(e => e.StartDate)
                    .Take(5)
                    .ToListAsync(),

                ObjetivosRecentes = await objetivos
                    .Where(g => g.Status == "Em progresso")
                    .OrderBy(g => g.DueDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
