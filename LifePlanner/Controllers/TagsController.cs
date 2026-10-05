using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LifePlanner.Helpers;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class TagsController : Controller
    {
        private readonly IGenericRepository<Tag> _repository;
        private readonly IUserHelper _userHelper;

        public TagsController(IGenericRepository<Tag> repository, IUserHelper userHelper)
        {
            _repository = repository;
            _userHelper = userHelper;
        }

        // =========================================================
        // UTILIZADOR ATUAL
        // =========================================================

        private async Task<User?> GetCurrentUserAsync()
        {
            if (string.IsNullOrEmpty(User.Identity?.Name))
            {
                return null;
            }

            return await _userHelper.GetUserByEmailAsync(User.Identity.Name);
        }

        public async Task<IActionResult> Index()
        {

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var tags = await _repository
                .GetAll()
                .Where(t => t.UserId == user.Id)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(tags);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tag tag)
        {
            ModelState.Remove(nameof(Tag.UserId));

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            if (!string.IsNullOrWhiteSpace(tag.Name))
            {
                var exists = await _repository
                    .GetAll()
                    .AnyAsync(t =>
                        t.UserId == user.Id &&
                        t.Name == tag.Name);

                if (exists)
                {
                    ModelState.AddModelError(
                         nameof(Tag.Name),
                        "Já existe uma tag com este nome.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(tag);
            }
            tag.UserId = user.Id;
            await _repository.CreateAsync(tag);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var tag = await _repository.GetAll().
                FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);

            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tag model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Tag.UserId));

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var exists = await _repository
                    .GetAll()
                    .AnyAsync(t =>
                        t.UserId == user.Id &&
                        t.Name == model.Name &&
                        t.Id != model.Id);

                if (exists)

                {
                    ModelState.AddModelError(
                    nameof(Tag.Name),
                    "Já existe uma tag com este nome.");
                }
            }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var tag = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(t =>
                        t.Id == id &&
                        t.UserId == user.Id);

                if (tag == null)

                {
                    return NotFound();
                }

                tag.Name = model.Name;

            await _repository.UpdateAsync(tag);

                return RedirectToAction(nameof(Index));
            }

            [HttpGet]
            public async Task<IActionResult> Details(int id)
            {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            var tag = await _repository
                    .GetAll()
                    .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.TaskItem)
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

                if (tag == null)
                {
                    return NotFound();
                }

            tag.TaskTags = tag.TaskTags
            .Where(tt =>
                tt.TaskItem != null &&
                tt.TaskItem.UserId == user.Id)
            .ToList();


            return View(tag);
            }

            [HttpGet]
            public async Task<IActionResult> Delete(int id)
            {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            var tag = await _repository
                    .GetAll()
                    .Include(t => t.TaskTags)
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

                if (tag == null)
                {
                    return NotFound();
                }



                return View(tag);
            }

            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var tag = await _repository.GetAll()
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);


            if (tag == null)
                {
                    return NotFound();
                }

            if (tag.TaskTags.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não é possível eliminar esta tag porque está associada a uma ou mais tarefas.");

                return View("Delete", tag);
            }

            try
            {
                await _repository.DeleteAsync(tag);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não foi possível eliminar esta tag.");

                return View("Delete", tag);
            }


            return RedirectToAction(nameof(Index));
            }
        
    }
}
