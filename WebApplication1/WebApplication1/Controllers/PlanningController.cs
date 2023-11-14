using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using WebApplication1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PlanningController : Controller
    {
        private readonly EventsContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PlanningController(EventsContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            // Проверка, существует ли пользователь
            var adminUser = await _userManager.FindByNameAsync("admin");

            if (adminUser == null)
            {
                // Проверка, существует ли роль администратора
                var adminRoleExists = await _roleManager.RoleExistsAsync("admin");

                if (!adminRoleExists)
                {
                    // Создание роли администратора
                    var role = new IdentityRole("admin");
                    await _roleManager.CreateAsync(role);
                }

                // Создание пользователя с ролью администратора
                var user = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com"
                };

                var result = await _userManager.CreateAsync(user, "_Aaa123");

                if (result.Succeeded)
                {
                    // Присвоение роли администратора пользователю
                    await _userManager.AddToRoleAsync(user, "admin");
                }
            }

            int pageSize = 25;
            var plannedEvents = _context.PlannedEvents.Include(e => e.Enterprise).Include(e => e.Event).Include(e => e.Responsible).Include(e => e.Finance);
            IEnumerable<PlannedEvent> eventsPerPages = plannedEvents
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToList();
            PageInfo pageInfo = new PageInfo { pageNumber = page, pageSize = pageSize, totalItems = _context.PlannedEvents.Count() };
            PlanningViewModel pvm = new PlanningViewModel { PageInfo = pageInfo, PlannedEvents = eventsPerPages };
            return View(pvm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public ActionResult Create()
        {
            SelectList events = new(_context.Events, "Id", "Name");
            SelectList enterprises = new(_context.Enterprises, "Id", "Name");
            SelectList employees = new(_context.Employees, "Id", "Surname");
            SelectList financings = new(_context.SourcesOfFinancing, "Id", "Id");
            ViewBag.Events = events;
            ViewBag.Enterprises = enterprises;
            ViewBag.Employees = employees;
            ViewBag.SourcesOfFinancing = financings;
            return View();
        }

        [HttpPost]
        public ActionResult Create(PlannedEvent plannedEvent)
        {
            _context.PlannedEvents.Add(plannedEvent);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if(id == null)
            {
                return RedirectToAction("HttpNotFound");
            }
            PlannedEvent plannedEvent = _context.PlannedEvents.Find(id);
            if(plannedEvent != null) 
            {
                SelectList events = new(_context.Events, "Id", "Name");
                SelectList enterprises = new(_context.Enterprises, "Id", "Name");
                SelectList employees = new(_context.Employees, "Id", "Surname");
                SelectList financings = new(_context.SourcesOfFinancing, "Id", "Id");
                ViewBag.Events = events;
                ViewBag.Enterprises = enterprises;
                ViewBag.Employees = employees;
                ViewBag.SourcesOfFinancing = financings;
                return View(plannedEvent);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(PlannedEvent plannedEvent)
        {
            _context.Entry(plannedEvent).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Remove(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("HttpNotFound");
            }
            PlannedEvent plannedEvent = _context.PlannedEvents.Find(id);
            if (plannedEvent != null)
            {
                _context.PlannedEvents.Remove(plannedEvent);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}