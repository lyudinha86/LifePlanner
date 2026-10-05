using LifePlanner.Data;
using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class PlannerEventsController : Controller
    {
        private readonly IGenericRepository<PlannerEvent> _repository;
        private readonly IUserHelper _userHelper;

        public PlannerEventsController(
            IGenericRepository<PlannerEvent> repository,
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
        // LISTA DE EVENTOS
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var events = await _repository
                .GetAll()
                .Where(e => e.UserId == user.Id)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }


        // =========================================================
        // CRIAR EVENTO - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CRIAR EVENTO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PlannerEvent plannerEvent)
        {
            // UserId é definido pelo sistema
            ModelState.Remove(nameof(PlannerEvent.UserId));

            if (!ModelState.IsValid)
            {
                return View(plannerEvent);
            }


            // Validar datas
            if (plannerEvent.EndDate.HasValue &&
                plannerEvent.EndDate < plannerEvent.StartDate)
            {
                ModelState.AddModelError(
                    nameof(PlannerEvent.EndDate),
                    "A data de fim não pode ser anterior à data de início.");

                return View(plannerEvent);
            }


            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Associar evento ao utilizador autenticado
            plannerEvent.UserId = user.Id;

            await _repository.CreateAsync(plannerEvent);

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDITAR EVENTO - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Apenas eventos do utilizador atual
            var plannerEvent = await _repository
                .GetAll()
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.UserId == user.Id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            return View(plannerEvent);
        }


        // =========================================================
        // EDITAR EVENTO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PlannerEvent model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


            // UserId não vem do formulário
            ModelState.Remove(nameof(PlannerEvent.UserId));

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Validar datas
            if (model.EndDate.HasValue &&
                model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(
                    nameof(PlannerEvent.EndDate),
                    "A data de fim não pode ser anterior à data de início.");

                return View(model);
            }


            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Procurar o evento diretamente na BD
            // e confirmar que pertence ao utilizador atual
            var plannerEvent = await _repository
                .GetAll()
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.UserId == user.Id);

            if (plannerEvent == null)
            {
                return NotFound();
            }


            // Atualizar apenas os campos permitidos
            plannerEvent.Title = model.Title;
            plannerEvent.Description = model.Description;
            plannerEvent.StartDate = model.StartDate;
            plannerEvent.EndDate = model.EndDate;
            plannerEvent.Location = model.Location;

            // UserId não é alterado

            try
            {
                await _repository.UpdateAsync(plannerEvent);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _repository
                    .GetAll()
                    .AnyAsync(e =>
                        e.Id == id &&
                        e.UserId == user.Id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DETALHES DO EVENTO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var plannerEvent = await _repository
                .GetAll()
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.UserId == user.Id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            return View(plannerEvent);
        }


        // =========================================================
        // ELIMINAR EVENTO - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var plannerEvent = await _repository
                .GetAll()
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.UserId == user.Id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            return View(plannerEvent);
        }


        // =========================================================
        // ELIMINAR EVENTO - POST
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

            var plannerEvent = await _repository
                .GetAll()
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.UserId == user.Id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            try
            {
                await _repository.DeleteAsync(plannerEvent);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não foi possível eliminar este evento.");

                return View("Delete", plannerEvent);
            }


            return RedirectToAction(nameof(Index));
        }
    }
}