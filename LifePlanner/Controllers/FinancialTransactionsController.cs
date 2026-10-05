using LifePlanner.Data;
using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class FinancialTransactionsController : Controller
    {
        private readonly IGenericRepository<FinancialTransaction> _repository;
        private readonly IUserHelper _userHelper;

        public FinancialTransactionsController(
            IGenericRepository<FinancialTransaction> repository,
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
        // LISTA DE MOVIMENTOS
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Mostrar apenas movimentos do utilizador atual
            var transactions = await _repository
                .GetAll()
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();


            // Calcular totais apenas do utilizador atual
            ViewBag.TotalReceitas = transactions
                .Where(t => t.Type == "Receita")
                .Sum(t => t.Amount);

            ViewBag.TotalDespesas = transactions
                .Where(t => t.Type == "Despesa")
                .Sum(t => t.Amount);

            ViewBag.Saldo =
                ViewBag.TotalReceitas -
                ViewBag.TotalDespesas;


            return View(transactions);
        }


        // =========================================================
        // CRIAR MOVIMENTO - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CRIAR MOVIMENTO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            FinancialTransaction transaction)
        {
            // UserId é definido pelo sistema
            ModelState.Remove(nameof(FinancialTransaction.UserId));


            // Validar valor
            if (transaction.Amount <= 0)
            {
                ModelState.AddModelError(
                    nameof(FinancialTransaction.Amount),
                    "O valor deve ser superior a zero.");
            }


            // Validar tipo
            if (transaction.Type != "Receita" &&
                transaction.Type != "Despesa")
            {
                ModelState.AddModelError(
                    nameof(FinancialTransaction.Type),
                    "Selecione um tipo válido.");
            }


            if (!ModelState.IsValid)
            {
                return View(transaction);
            }

            // Validar pagamento pendente
            if (!transaction.IsPaid && !transaction.DueDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(FinancialTransaction.DueDate),
                    "Indique a data de vencimento.");
            }

            // Um movimento pago não necessita de data de vencimento
            if (transaction.IsPaid)
            {
                transaction.DueDate = null;
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Associar movimento ao utilizador autenticado
            transaction.UserId = user.Id;

            await _repository.CreateAsync(transaction);

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDITAR MOVIMENTO - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            var transaction = await _repository
                .GetAll()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);


            if (transaction == null)
            {
                return NotFound();
            }


            return View(transaction);
        }


        // =========================================================
        // EDITAR MOVIMENTO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FinancialTransaction model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


            // UserId não vem do formulário
            ModelState.Remove(nameof(FinancialTransaction.UserId));


            // Validar valor
            if (model.Amount <= 0)
            {
                ModelState.AddModelError(
                    nameof(FinancialTransaction.Amount),
                    "O valor deve ser superior a zero.");
            }


            // Validar tipo
            if (model.Type != "Receita" &&
                model.Type != "Despesa")
            {
                ModelState.AddModelError(
                    nameof(FinancialTransaction.Type),
                    "Selecione um tipo válido.");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validar pagamento pendente
            if (!model.IsPaid && !model.DueDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(FinancialTransaction.DueDate),
                    "Indique a data de vencimento.");
            }

            if (model.IsPaid)
            {
                model.DueDate = null;
            }


            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Procurar apenas um movimento
            // pertencente ao utilizador atual
            var transaction = await _repository
                .GetAll()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);


            if (transaction == null)
            {
                return NotFound();
            }


            // Atualizar apenas os campos permitidos
            transaction.Description = model.Description;
            transaction.Type = model.Type;
            transaction.Amount = model.Amount;
            transaction.TransactionDate = model.TransactionDate;
            transaction.Category = model.Category;
            transaction.DueDate = model.DueDate;
            transaction.IsPaid = model.IsPaid;

            // UserId não é alterado


            try
            {
                await _repository.UpdateAsync(transaction);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _repository
                    .GetAll()
                    .AnyAsync(t =>
                        t.Id == id &&
                        t.UserId == user.Id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DETALHES DO MOVIMENTO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            var transaction = await _repository
                .GetAll()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);


            if (transaction == null)
            {
                return NotFound();
            }


            return View(transaction);
        }


        // =========================================================
        // ELIMINAR MOVIMENTO - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            var transaction = await _repository
                .GetAll()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);


            if (transaction == null)
            {
                return NotFound();
            }


            return View(transaction);
        }


        // =========================================================
        // ELIMINAR MOVIMENTO - POST
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


            var transaction = await _repository
                .GetAll()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);


            if (transaction == null)
            {
                return NotFound();
            }


            try
            {
                await _repository.DeleteAsync(transaction);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não foi possível eliminar este movimento.");

                return View("Delete", transaction);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}