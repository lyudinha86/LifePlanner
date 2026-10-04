using LifePlanner.Data;
using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class GoalsController : Controller
    {
        private readonly IGenericRepository<Goal> _repository;
        private readonly IUserHelper _userHelper;

        public GoalsController(
            IGenericRepository<Goal> repository,
            IUserHelper userHelper)
        {
            _repository = repository;
            _userHelper = userHelper;
        }


        // =========================================================
        // OBTER UTILIZADOR ATUAL
        // =========================================================

        private async Task<User?> GetCurrentUserAsync()
        {
            if (string.IsNullOrEmpty(User.Identity?.Name))
            {
                return null;
            }

            return await _userHelper
                .GetUserByEmailAsync(User.Identity.Name);
        }


        // =========================================================
        // LISTA DE OBJETIVOS
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var goals = await _repository
                .GetAll()
                .Where(g => g.UserId == user.Id)
                .OrderBy(g => g.DueDate)
                .ToListAsync();

            return View(goals);
        }


        // =========================================================
        // CRIAR OBJETIVO - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CRIAR OBJETIVO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Goal goal)
        {
            // UserId é definido pelo sistema,
            // não vem do formulário
            ModelState.Remove(nameof(Goal.UserId));

            if (!ModelState.IsValid)
            {
                return View(goal);
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            goal.StartDate = DateTime.Now;
            goal.UserId = user.Id;

            await _repository.CreateAsync(goal);

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDITAR OBJETIVO - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var goal = await _repository
                .GetAll()
                .FirstOrDefaultAsync(
                    g => g.Id == id &&
                         g.UserId == user.Id);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }


        // =========================================================
        // EDITAR OBJETIVO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Goal model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Goal.UserId));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            // Procurar o objetivo diretamente na BD.
            // Assim o UserId nunca vem do formulário.
            var goal = await _repository
                .GetAll()
                .FirstOrDefaultAsync(
                    g => g.Id == id &&
                         g.UserId == user.Id);

            if (goal == null)
            {
                return NotFound();
            }

            goal.Title = model.Title;
            goal.Description = model.Description;
            goal.DueDate = model.DueDate;
            goal.Status = model.Status;

            // Não alteramos StartDate nem UserId.
            await _repository.UpdateAsync(goal);

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DETALHES DO OBJETIVO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var goal = await _repository
                .GetAll()
                .Include(g => g.TaskItems)
                .FirstOrDefaultAsync(
                    g => g.Id == id &&
                         g.UserId == user.Id);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }


        // =========================================================
        // ELIMINAR OBJETIVO - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var goal = await _repository
                .GetAll()
                .Include(g => g.TaskItems)
                .FirstOrDefaultAsync(
                    g => g.Id == id &&
                         g.UserId == user.Id);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }


        // =========================================================
        // ELIMINAR OBJETIVO - POST
        // =========================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var goal = await _repository
                .GetAll()
                .Include(g => g.TaskItems)
                .FirstOrDefaultAsync(
                    g => g.Id == id &&
                         g.UserId == user.Id);

            if (goal == null)
            {
                return NotFound();
            }

            if (goal.TaskItems.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não é possível eliminar este objetivo porque existem tarefas associadas.");

                return View("Delete", goal);
            }

            try
            {
                await _repository.DeleteAsync(goal);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não foi possível eliminar este objetivo.");

                return View("Delete", goal);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}