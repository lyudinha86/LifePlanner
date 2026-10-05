using LifePlanner.Data;
using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifePlanner.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly IGenericRepository<PlannerEvent> _eventRepository;
        private readonly IGenericRepository<TaskItem> _taskRepository;
        private readonly IUserHelper _userHelper;

        public CalendarController(
            IGenericRepository<PlannerEvent> eventRepository,
            IGenericRepository<TaskItem> taskRepository,
            IUserHelper userHelper)
        {
            _eventRepository = eventRepository;
            _taskRepository = taskRepository;
            _userHelper = userHelper;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(User.Identity?.Name))
            {
                return Unauthorized();
            }

            var user = await _userHelper
                .GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return Unauthorized();
            }

            var events = await _eventRepository
                .GetAll()
                .Where(e => e.UserId == user.Id)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartDate,
                    end = e.EndDate,
                    type = "event"
                })
                .ToListAsync();

            var tasks = await _taskRepository
                .GetAll()
                .Where(t =>
                    t.UserId == user.Id &&
                    t.DueDate.HasValue)
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    start = t.DueDate!.Value,
                    end = (DateTime?)null,
                    type = "task"
                })
                .ToListAsync();

            ViewBag.Events = events;
            ViewBag.Tasks = tasks;

            return View();
        }
    }
}
