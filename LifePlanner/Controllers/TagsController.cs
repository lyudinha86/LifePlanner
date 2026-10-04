using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class TagsController : Controller
    {
        private readonly IGenericRepository<Tag> _repository;

        public TagsController(IGenericRepository<Tag> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var tags = await _repository
                .GetAll()
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
            if (await _repository.GetAll()
                .AnyAsync(t => t.Name == tag.Name))
            {
                ModelState.AddModelError(
                    "Name",
                    "Já existe uma tag com este nome.");
            }

            if (!ModelState.IsValid)
            {
                return View(tag);
            }

            await _repository.CreateAsync(tag);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _repository.GetByIdAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.Id)
            {
                return NotFound();
            }

            if (await _repository.GetAll()
                .AnyAsync(t => t.Name == tag.Name && t.Id != tag.Id))
            {
                ModelState.AddModelError(
                    "Name",
                    "Já existe uma tag com este nome.");
            }

            if (!ModelState.IsValid)
            {
                return View(tag);
            }

            await _repository.UpdateAsync(tag);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var tag = await _repository
                .GetAll()
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.TaskItem)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _repository
                .GetAll()
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(t => t.Id == id);

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
            var tag = await _repository.GetByIdAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(tag);

            return RedirectToAction(nameof(Index));
        }
    }
}
