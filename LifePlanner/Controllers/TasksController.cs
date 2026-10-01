using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    public class TasksController : Controller
    {
        private readonly IGenericRepository<TaskItem> _repository;

        public TasksController(IGenericRepository<TaskItem> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await _repository
                .GetAll()
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tasks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem taskItem)
        {
            if (!ModelState.IsValid)
            {
                return View(taskItem);
            }

            taskItem.CreatedDate = DateTime.Now;

            await _repository.CreateAsync(taskItem);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var taskItem = await _repository.GetByIdAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var taskItem = await _repository.GetByIdAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

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
