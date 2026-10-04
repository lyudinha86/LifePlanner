using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    public class GoalsController : Controller
    {
        private readonly IGenericRepository<Goal> _repository;

        public GoalsController(IGenericRepository<Goal> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var goals = await _repository
                .GetAll()
                .OrderBy(g => g.DueDate)
                .ToListAsync();

            return View(goals);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Goal goal)
        {
            if (!ModelState.IsValid)
            {
                return View(goal);
            }

            goal.StartDate = DateTime.Now;

            await _repository.CreateAsync(goal);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var goal = await _repository.GetByIdAsync(id);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Goal goal)
        {
            if (id != goal.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(goal);
            }

            await _repository.UpdateAsync(goal);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var goal = await _repository
                .GetAll()
                .Include(g => g.TaskItems)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var goal = await _repository.GetByIdAsync(id);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var goal = await _repository.GetByIdAsync(id);

            if (goal == null)
            {
                return NotFound();
            }

            try
            {
                await _repository.DeleteAsync(goal);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Não é possível eliminar este objetivo porque existem tarefas associadas.");

                return View("Delete", goal);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
