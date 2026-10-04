using LifePlanner.Data;
using LifePlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LifePlanner.Models.ViewModels;

namespace LifePlanner.Controllers
{
    public class TasksController : Controller
    {
        private readonly IGenericRepository<TaskItem> _repository;
        private readonly IGenericRepository<Goal> _goalRepository;
        private readonly IGenericRepository<Tag> _tagRepository;
        private readonly IGenericRepository<TaskTag> _taskTagRepository;

        public TasksController(
     IGenericRepository<TaskItem> repository,
     IGenericRepository<Goal> goalRepository,
     IGenericRepository<Tag> tagRepository,
     IGenericRepository<TaskTag> taskTagRepository)
        {
            _repository = repository;
            _goalRepository = goalRepository;
            _tagRepository = tagRepository;
            _taskTagRepository = taskTagRepository;
        }

        // LISTA DE TAREFAS
        public async Task<IActionResult> Index()
        {
            var tasks = await _repository
                .GetAll()
                .Include(t => t.Goal)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tasks);
        }

        // CRIAR TAREFA - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var goals = await _goalRepository
                .GetAll()
                .OrderBy(g => g.Title)
                .ToListAsync();

            var tags = await _tagRepository
                .GetAll()
                .OrderBy(t => t.Name)
                .ToListAsync();

            var model = new TaskFormViewModel
            {
                Goals = goals.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Title
                }),

                Tags = tags
            };

            return View(model);
        }

        // CRIAR TAREFA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var goals = await _goalRepository
                    .GetAll()
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

                return View(model);
            }

            var taskItem = new TaskItem
            {
                Title = model.Title,
                Description = model.Description,
                CreatedDate = DateTime.Now,
                DueDate = model.DueDate,
                Priority = model.Priority,
                Status = model.Status,
                GoalId = model.GoalId
            };

            await _repository.CreateAsync(taskItem);

            foreach (var tagId in model.SelectedTagIds)
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

        // EDITAR TAREFA - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var taskItem = await _repository
                .GetAll()
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            var goals = await _goalRepository
                .GetAll()
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


        // EDITAR TAREFA - POST
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
                var goals = await _goalRepository
                    .GetAll()
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

                return View(model);
            }

            var taskItem = await _repository.GetByIdAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            // Atualizar os dados da tarefa
            taskItem.Title = model.Title;
            taskItem.Description = model.Description;
            taskItem.DueDate = model.DueDate;
            taskItem.Priority = model.Priority;
            taskItem.Status = model.Status;
            taskItem.GoalId = model.GoalId;

            await _repository.UpdateAsync(taskItem);


            // Obter as tags atualmente associadas à tarefa
            var existingTaskTags = await _taskTagRepository
                .GetAll()
                .Where(tt => tt.TaskItemId == id)
                .ToListAsync();


            // Eliminar associações que foram desmarcadas
            foreach (var taskTag in existingTaskTags)
            {
                if (!model.SelectedTagIds.Contains(taskTag.TagId))
                {
                    await _taskTagRepository.DeleteAsync(taskTag);
                }
            }


            // Obter os IDs das tags que já estavam associadas
            var existingTagIds = existingTaskTags
                .Select(tt => tt.TagId)
                .ToList();


            // Adicionar novas associações
            foreach (var tagId in model.SelectedTagIds)
            {
                if (!existingTagIds.Contains(tagId))
                {
                    var taskTag = new TaskTag
                    {
                        TaskItemId = taskItem.Id,
                        TagId = tagId
                    };

                    await _taskTagRepository.CreateAsync(taskTag);
                }
            }

            return RedirectToAction(nameof(Index));
        }
        // DETALHES DA TAREFA

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var taskItem = await _repository
                .GetAll()
                .Include(t => t.Goal)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // ELIMINAR TAREFA - GET
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

        // ELIMINAR TAREFA - POST
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