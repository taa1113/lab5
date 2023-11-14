using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.ViewModels;
using WebApplication1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class EnterprisesController : Controller
    {
        private readonly EventsContext _context;
        public EnterprisesController(EventsContext context)
        {
            _context = context;
        }
        public ActionResult Index(int page = 1)
        {
            int pageSize = 25;
            var enterprises = _context.Enterprises.Include(e => e.Manager).Include(e => e.CPE);
            IEnumerable<Enterprise> enterprisesPerPages = enterprises
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToList();
            PageInfo pageInfo = new PageInfo { pageNumber = page, pageSize = pageSize, totalItems = _context.Enterprises.Count() };
            EnterprisesViewModel evm = new EnterprisesViewModel { PageInfo = pageInfo, Enterprises = enterprisesPerPages };
            return View(evm);
        }

        [HttpGet]
        public ActionResult Create()
        {
            SelectList employees = new(_context.Employees, "Id", "Surname");
            ViewBag.Employees = employees;
            return View();
        }

        [HttpPost]
        public ActionResult Create(Enterprise enterprise, Manager manager, CPE cpe)
        {
            _context.Managers.Add(manager);
            _context.CPEs.Add(cpe);
            _context.Enterprises.Add(enterprise);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("HttpNotFound");
            }
            PlannedEvent plannedEvent = _context.PlannedEvents.Find(id);
            if (plannedEvent != null)
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
