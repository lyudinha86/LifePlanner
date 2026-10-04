using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    public class PlannerEventsController : Controller
    {
        private readonly IGenericRepository<PlannerEvent> _repository;

        public PlannerEventsController(
            IGenericRepository<PlannerEvent> repository)
        {
            _repository = repository;
        }

        // LISTA DE EVENTOS
        public async Task<IActionResult> Index()
        {
            var events = await _repository
                .GetAll()
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }

        // CRIAR EVENTO - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CRIAR EVENTO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlannerEvent plannerEvent)
        {
            if (!ModelState.IsValid)
            {
                return View(plannerEvent);
            }

            if (plannerEvent.EndDate.HasValue &&
                plannerEvent.EndDate < plannerEvent.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "A data de fim não pode ser anterior à data de início.");

                return View(plannerEvent);
            }

            await _repository.CreateAsync(plannerEvent);

            return RedirectToAction(nameof(Index));
        }

        // EDITAR EVENTO - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var plannerEvent = await _repository.GetByIdAsync(id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            return View(plannerEvent);
        }

        // EDITAR EVENTO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PlannerEvent plannerEvent)
        {
            if (id != plannerEvent.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(plannerEvent);
            }

            if (plannerEvent.EndDate.HasValue &&
                plannerEvent.EndDate < plannerEvent.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "A data de fim não pode ser anterior à data de início.");

                return View(plannerEvent);
            }

            try
            {
                await _repository.UpdateAsync(plannerEvent);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _repository.ExistsAsync(plannerEvent.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // DETALHES DO EVENTO
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var plannerEvent = await _repository.GetByIdAsync(id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            return View(plannerEvent);
        }

        // ELIMINAR EVENTO - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var plannerEvent = await _repository.GetByIdAsync(id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            return View(plannerEvent);
        }

        // ELIMINAR EVENTO - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plannerEvent = await _repository.GetByIdAsync(id);

            if (plannerEvent == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(plannerEvent);

            return RedirectToAction(nameof(Index));
        }
    }
}
