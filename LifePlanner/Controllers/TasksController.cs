using LifePlanner.Data;
using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using LifePlanner.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly IGenericRepository<TaskItem> _repository;
        private readonly IGenericRepository<Goal> _goalRepository;
        private readonly IGenericRepository<Tag> _tagRepository;
        private readonly IGenericRepository<TaskTag> _taskTagRepository;
        private readonly IUserHelper _userHelper;

        public TasksController(
            IGenericRepository<TaskItem> repository,
            IGenericRepository<Goal> goalRepository,
            IGenericRepository<Tag> tagRepository,
            IGenericRepository<TaskTag> taskTagRepository,
            IUserHelper userHelper)
        {
            _repository = repository;
            _goalRepository = goalRepository;
            _tagRepository = tagRepository;
            _taskTagRepository = taskTagRepository;
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
        // LISTA DE TAREFAS
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var tasks = await _repository
                .GetAll()
                .Where(t => t.UserId == user.Id)
                .Include(t => t.Goal)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tasks);
        }


        // =========================================================
        // CRIAR TAREFA - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new TaskFormViewModel();

            await LoadFormDataAsync(model);

            return View(model);
        }


        // =========================================================
        // CRIAR TAREFA - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync(model);

                return View(model);
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Se foi escolhido um objetivo,
            // verificar se pertence ao utilizador atual
            if (model.GoalId.HasValue)
            {
                var goalExists = await _goalRepository
                    .GetAll()
                    .AnyAsync(g =>
                        g.Id == model.GoalId.Value &&
                        g.UserId == user.Id);

                if (!goalExists)
                {
                    ModelState.AddModelError(
                        nameof(model.GoalId),
                        "O objetivo selecionado não é válido.");

                    await LoadFormDataAsync(model);

                    return View(model);
                }
            }


            var taskItem = new TaskItem
            {
                Title = model.Title,
                Description = model.Description,
                CreatedDate = DateTime.Now,
                DueDate = model.DueDate,
                Priority = model.Priority,
                Status = model.Status,
                GoalId = model.GoalId,
                UserId = user.Id
            };

            await _repository.CreateAsync(taskItem);


            // Criar relações entre tarefa e tags
            foreach (var tagId in model.SelectedTagIds.Distinct())
            {
                var taskTag = new TaskTag
                {
                    TaskItemId = taskItem.Id,
                    TagId = tagId
                };

                await _taskTagRepository.CreateAsync(taskTag);
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDITAR TAREFA - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var taskItem = await _repository
                .GetAll()
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);

            if (taskItem == null)
            {
                return NotFound();
            }


            // Mostrar apenas os objetivos
            // pertencentes ao utilizador atual
            var goals = await _goalRepository
                .GetAll()
                .Where(g => g.UserId == user.Id)
                .OrderBy(g => g.Title)
                .ToListAsync();


            var tags = await _tagRepository
                .GetAll()
                .OrderBy(t => t.Name)
                .ToListAsync();


            var model = new TaskFormViewModel
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                DueDate = taskItem.DueDate,
                Priority = taskItem.Priority,
                Status = taskItem.Status,
                GoalId = taskItem.GoalId,

                Goals = goals.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Title
                }),

                Tags = tags,

                SelectedTagIds = taskItem.TaskTags
                    .Select(tt => tt.TagId)
                    .ToList()
            };

            return View(model);
        }


        // =========================================================
        // EDITAR TAREFA - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            TaskFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync(model);

                return View(model);
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }


            // Procurar apenas uma tarefa
            // pertencente ao utilizador atual
            var taskItem = await _repository
                .GetAll()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);

            if (taskItem == null)
            {
                return NotFound();
            }


            // Se foi escolhido um objetivo,
            // verificar se pertence ao utilizador atual
            if (model.GoalId.HasValue)
            {
                var goalExists = await _goalRepository
                    .GetAll()
                    .AnyAsync(g =>
                        g.Id == model.GoalId.Value &&
                        g.UserId == user.Id);

                if (!goalExists)
                {
                    ModelState.AddModelError(
                        nameof(model.GoalId),
                        "O objetivo selecionado não é válido.");

                    await LoadFormDataAsync(model);

                    return View(model);
                }
            }


            // Atualizar dados da tarefa
            taskItem.Title = model.Title;
            taskItem.Description = model.Description;
            taskItem.DueDate = model.DueDate;
            taskItem.Priority = model.Priority;
            taskItem.Status = model.Status;
            taskItem.GoalId = model.GoalId;

            // CreatedDate e UserId não são alterados
            await _repository.UpdateAsync(taskItem);


            // Obter tags atualmente associadas
            var existingTaskTags = await _taskTagRepository
                .GetAll()
                .Where(tt => tt.TaskItemId == id)
                .ToListAsync();


            var selectedTagIds = model.SelectedTagIds
                .Distinct()
                .ToList();


            // Eliminar associações que foram desmarcadas
            foreach (var taskTag in existingTaskTags)
            {
                if (!selectedTagIds.Contains(taskTag.TagId))
                {
                    await _taskTagRepository
                        .DeleteAsync(taskTag);
                }
            }


            // IDs das tags que já estavam associadas
            var existingTagIds = existingTaskTags
                .Select(tt => tt.TagId)
                .ToList();


            // Adicionar novas associações
            foreach (var tagId in selectedTagIds)
            {
                if (!existingTagIds.Contains(tagId))
                {
                    var taskTag = new TaskTag
                    {
                        TaskItemId = taskItem.Id,
                        TagId = tagId
                    };

                    await _taskTagRepository
                        .CreateAsync(taskTag);
                }
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DETALHES DA TAREFA
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var taskItem = await _repository
                .GetAll()
                .Include(t => t.Goal)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }


        // =========================================================
        // ELIMINAR TAREFA - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var taskItem = await _repository
                .GetAll()
                .Include(t => t.Goal)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }


        // =========================================================
        // ELIMINAR TAREFA - POST
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

            var taskItem = await _repository
                .GetAll()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == user.Id);

            if (taskItem == null)
            {
                return NotFound();
            }


            // Eliminar primeiro as relações TaskTag
            var taskTags = await _taskTagRepository
                .GetAll()
                .Where(tt => tt.TaskItemId == id)
                .ToListAsync();

            foreach (var taskTag in taskTags)
            {
                await _taskTagRepository
                    .DeleteAsync(taskTag);
            }


            // Eliminar tarefa
            await _repository.DeleteAsync(taskItem);

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // CARREGAR OBJETIVOS E TAGS
        // =========================================================

        private async Task LoadFormDataAsync(
            TaskFormViewModel model)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                model.Goals = new List<SelectListItem>();
                model.Tags = new List<Tag>();

                return;
            }


            // Apenas objetivos do utilizador atual
            var goals = await _goalRepository
                .GetAll()
                .Where(g => g.UserId == user.Id)
                .OrderBy(g => g.Title)
                .ToListAsync();


            model.Goals = goals.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Title
            });


            model.Tags = await _tagRepository
                .GetAll()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
    }
}