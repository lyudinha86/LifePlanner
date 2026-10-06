using LifePlanner.Models.ViewModels;
using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IGenericRepository<TaskItem> _taskRepository;
        private readonly IGenericRepository<PlannerEvent> _eventRepository;
        private readonly IGenericRepository<Goal> _goalRepository;
        private readonly IGenericRepository<Tag> _tagRepository;

        public AdminController(
            UserManager<User> userManager,
            IGenericRepository<TaskItem> taskRepository,
            IGenericRepository<PlannerEvent> eventRepository,
            IGenericRepository<Goal> goalRepository,
            IGenericRepository<Tag> tagRepository)
        {
            _userManager = userManager;
            _taskRepository = taskRepository;
            _eventRepository = eventRepository;
            _goalRepository = goalRepository;
            _tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index()
        {
            var tarefas = _taskRepository.GetAll();

            var model = new AdminDashboardViewModel
            {
                TotalUtilizadores =
                    await _userManager.Users.CountAsync(),

                TotalTarefas =
                    await tarefas.CountAsync(),

                TotalEventos =
                    await _eventRepository.GetAll().CountAsync(),

                TotalObjetivos =
                    await _goalRepository.GetAll().CountAsync(),

                TotalTags =
                    await _tagRepository.GetAll().CountAsync(),

                TarefasPendentes =
                    await tarefas.CountAsync(t =>
                        t.Status == "Pendente"),

                TarefasEmProgresso =
                    await tarefas.CountAsync(t =>
                        t.Status == "Em progresso"),

                TarefasConcluidas =
                    await tarefas.CountAsync(t =>
                        t.Status == "Concluída"),

                Utilizadores =
                    await _userManager.Users
                        .OrderBy(u => u.FirstName)
                        .Take(5)
                        .ToListAsync()
            };

            return View(model);
        }
    }
}