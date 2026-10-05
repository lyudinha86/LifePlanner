using LifePlanner.Data;
using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using LifePlanner.Models;
using LifePlanner.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var inicioMes = new DateTime(
                hoje.Year,
                hoje.Month,
                1);

            var inicioProximoMes = inicioMes.AddMonths(1);


            // =====================================================
            // DADOS DO UTILIZADOR
            // =====================================================

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


            // =====================================================
            // FINANÇAS DO MÊS
            // Apenas movimentos pagos entram no saldo realizado.
            // =====================================================

            var financasMes = financas.Where(f =>
                f.TransactionDate >= inicioMes &&
                f.TransactionDate < inicioProximoMes &&
                f.IsPaid);


            var totalReceitas = await financasMes
                .Where(f => f.Type == "Receita")
                .SumAsync(f => (decimal?)f.Amount) ?? 0;


            var totalDespesas = await financasMes
                .Where(f => f.Type == "Despesa")
                .SumAsync(f => (decimal?)f.Amount) ?? 0;


            // =====================================================
            // PAGAMENTOS PENDENTES
            // =====================================================

            var pagamentosProximos = await financas
                .Where(f =>
                    f.Type == "Despesa" &&
                    !f.IsPaid &&
                    f.DueDate.HasValue &&
                    f.DueDate.Value >= hoje)
                .OrderBy(f => f.DueDate)
                .Take(5)
                .ToListAsync();


            var totalPorPagar = await financas
                .Where(f =>
                    f.Type == "Despesa" &&
                    !f.IsPaid)
                .SumAsync(f => (decimal?)f.Amount) ?? 0;


            // =====================================================
            // DESPESAS POR CATEGORIA
            // =====================================================

            var despesasCategorias = await financasMes
                .Where(f => f.Type == "Despesa")
                .GroupBy(f =>
                    string.IsNullOrWhiteSpace(f.Category)
                        ? "Outros"
                        : f.Category!)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();


            var despesasPorCategoria = despesasCategorias
                .Select(x => new DashboardCategoryViewModel
                {
                    Categoria = x.Categoria,
                    Total = x.Total,

                    Percentagem = totalDespesas > 0
                        ? Math.Round(
                            x.Total / totalDespesas * 100,
                            1)
                        : 0
                })
                .ToList();


            // =====================================================
            // DASHBOARD
            // =====================================================

            var model = new DashboardViewModel
            {
                FirstName = user.FirstName,
                TarefasPendentes = await tarefas
                    .CountAsync(t =>
                        t.Status != "Concluída"),

                TarefasConcluidas = await tarefas
                    .CountAsync(t =>
                        t.Status == "Concluída"),

                ProximosEventos = await eventos
                    .CountAsync(e =>
                        e.StartDate >= hoje),

                ObjetivosEmProgresso = await objetivos
                    .CountAsync(g =>
                        g.Status == "Em progresso"),


                TotalReceitas = totalReceitas,

                TotalDespesas = totalDespesas,

                TotalPorPagar = totalPorPagar,


                TarefasRecentes = await tarefas
                    .Where(t =>
                        t.Status != "Concluída" &&
                        t.DueDate.HasValue)
                    .OrderBy(t => t.DueDate)
                    .Take(5)
                    .ToListAsync(),


                EventosProximos = await eventos
                    .Where(e =>
                        e.StartDate >= hoje)
                    .OrderBy(e => e.StartDate)
                    .Take(5)
                    .ToListAsync(),


                ObjetivosRecentes = await objetivos
                    .Where(g =>
                        g.Status == "Em progresso")
                    .OrderBy(g => g.DueDate)
                    .Take(5)
                    .ToListAsync(),


                PagamentosProximos = pagamentosProximos,

                DespesasPorCategoria = despesasPorCategoria
            };


            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                });
        }
    }
}