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
        private readonly IGenericRepository<FinancialTransaction> _financialRepository;

        public CalendarController(
            IGenericRepository<PlannerEvent> eventRepository,
            IGenericRepository<TaskItem> taskRepository,
            IUserHelper userHelper,
            IGenericRepository<FinancialTransaction> financialRepository)
        {
            _eventRepository = eventRepository;
            _taskRepository = taskRepository;
            _userHelper = userHelper;
            _financialRepository = financialRepository;
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

            var payments = await _financialRepository
    .GetAll()
    .Where(f =>
        f.UserId == user.Id &&
        f.Type == "Despesa" &&
        !f.IsPaid &&
        f.DueDate.HasValue)
    .Select(f => new
    {
        id = f.Id,
        title = f.Description + " - " + f.Amount.ToString("N2") + " €",
        start = f.DueDate!.Value,
        end = (DateTime?)null,
        type = "payment"
    })
    .ToListAsync();

            ViewBag.Events = events;
            ViewBag.Tasks = tasks;
            ViewBag.Payments = payments;

            


            return View();
        }
    }
}
