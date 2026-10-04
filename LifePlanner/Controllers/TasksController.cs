using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    public class TasksController : Controller
    {
        private readonly IGenericRepository<TaskItem> _repository;
        private readonly IGenericRepository<Goal> _goalRepository;

        public TasksController(
            IGenericRepository<TaskItem> repository,
            IGenericRepository<Goal> goalRepository)
        {
            _repository = repository;
            _goalRepository = goalRepository;
        }

        // LISTA DE TAREFAS
        public async Task<IActionResult> Index()
        {
            var tasks = await _repository
                .GetAll()
                .Include(t => t.Goal)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tasks);
        }

        // CRIAR TAREFA - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var goals = await _goalRepository
                .GetAll()
                .OrderBy(g => g.Title)
                .ToListAsync();

            ViewBag.Goals = new SelectList(
                goals,
                "Id",
                "Title");

            return View();
        }

        // CRIAR TAREFA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem taskItem)
        {
            if (!ModelState.IsValid)
            {
                var goals = await _goalRepository
                    .GetAll()
                    .OrderBy(g => g.Title)
                    .ToListAsync();

                ViewBag.Goals = new SelectList(
                    goals,
                    "Id",
                    "Title",
                    taskItem.GoalId);

                return View(taskItem);
            }

            taskItem.CreatedDate = DateTime.Now;

            await _repository.CreateAsync(taskItem);

            return RedirectToAction(nameof(Index));
        }

        // EDITAR TAREFA - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var taskItem = await _repository.GetByIdAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            var goals = await _goalRepository
                .GetAll()
                .OrderBy(g => g.Title)
                .ToListAsync();

            ViewBag.Goals = new SelectList(
                goals,
                "Id",
                "Title",
                taskItem.GoalId);

            return View(taskItem);
        }

        // EDITAR TAREFA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var goals = await _goalRepository
                    .GetAll()
                    .OrderBy(g => g.Title)
                    .ToListAsync();

                ViewBag.Goals = new SelectList(
                    goals,
                    "Id",
                    "Title",
                    taskItem.GoalId);

                return View(taskItem);
            }

            try
            {
                await _repository.UpdateAsync(taskItem);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _repository.ExistsAsync(taskItem.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // DETALHES DA TAREFA
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var taskItem = await _repository
                .GetAll()
                .Include(t => t.Goal)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // ELIMINAR TAREFA - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var taskItem = await _repository.GetByIdAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // ELIMINAR TAREFA - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _repository.GetByIdAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(taskItem);

            return RedirectToAction(nameof(Index));
        }
    }
}