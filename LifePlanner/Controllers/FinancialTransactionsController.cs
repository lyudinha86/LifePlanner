using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    public class FinancialTransactionsController : Controller
    {
        private readonly IGenericRepository<FinancialTransaction> _repository;

        public FinancialTransactionsController(
            IGenericRepository<FinancialTransaction> repository)
        {
            _repository = repository;
        }

        // LISTA DE MOVIMENTOS
        public async Task<IActionResult> Index()
        {
            var transactions = await _repository
                .GetAll()
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.TotalReceitas = transactions
                .Where(t => t.Type == "Receita")
                .Sum(t => t.Amount);

            ViewBag.TotalDespesas = transactions
                .Where(t => t.Type == "Despesa")
                .Sum(t => t.Amount);

            ViewBag.Saldo = ViewBag.TotalReceitas - ViewBag.TotalDespesas;

            return View(transactions);
        }

        // CRIAR - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CRIAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            FinancialTransaction transaction)
        {
            if (transaction.Amount <= 0)
            {
                ModelState.AddModelError(
                    "Amount",
                    "O valor deve ser superior a zero.");
            }

            if (transaction.Type != "Receita" &&
                transaction.Type != "Despesa")
            {
                ModelState.AddModelError(
                    "Type",
                    "Selecione um tipo válido.");
            }

            if (!ModelState.IsValid)
            {
                return View(transaction);
            }

            await _repository.CreateAsync(transaction);

            return RedirectToAction(nameof(Index));
        }

        // EDITAR - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _repository.GetByIdAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // EDITAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FinancialTransaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (transaction.Amount <= 0)
            {
                ModelState.AddModelError(
                    "Amount",
                    "O valor deve ser superior a zero.");
            }

            if (transaction.Type != "Receita" &&
                transaction.Type != "Despesa")
            {
                ModelState.AddModelError(
                    "Type",
                    "Selecione um tipo válido.");
            }

            if (!ModelState.IsValid)
            {
                return View(transaction);
            }

            try
            {
                await _repository.UpdateAsync(transaction);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _repository.ExistsAsync(transaction.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // DETALHES
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _repository.GetByIdAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // ELIMINAR - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _repository.GetByIdAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // ELIMINAR - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _repository.GetByIdAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(transaction);

            return RedirectToAction(nameof(Index));
        }
    }
}
