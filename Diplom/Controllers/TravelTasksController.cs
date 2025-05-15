using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Diplom.Data;
using Diplom.Models;

namespace Diplom.Controllers
{
    public class TravelTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TravelTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TravelTasks
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TravelTasks.Include(t => t.AssignedTo).Include(t => t.Trip);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TravelTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelTask = await _context.TravelTasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelTask == null)
            {
                return NotFound();
            }

            return View(travelTask);
        }

        // GET: TravelTasks/Create
        public IActionResult Create()
        {
            ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name");
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
            return View();
        }

        // POST: TravelTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,IsCompleted,CompletionDate,AssignedToId,TripId")] TravelTask travelTask)
        {
            // Exclude navigation properties from validation
            ModelState.Remove("AssignedTo");
            ModelState.Remove("Trip");

            // Set foreign keys to null if not needed
            travelTask.AssignedToId = null;
            travelTask.TripId = null;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Validation errors: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Validation errors: " + string.Join(", ", errors));
                ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name", travelTask.AssignedToId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", travelTask.TripId);
                return View(travelTask);
            }

            try
            {
                Console.WriteLine($"Creating TravelTask: Description={travelTask.Description}, IsCompleted={travelTask.IsCompleted}, CompletionDate={travelTask.CompletionDate}");
                _context.Add(travelTask);
                await _context.SaveChangesAsync();
                Console.WriteLine("TravelTask successfully created");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Database error: {errorMessage}");
                ModelState.AddModelError("", $"Database error: {errorMessage}");
                ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name", travelTask.AssignedToId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", travelTask.TripId);
                return View(travelTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                ModelState.AddModelError("", $"General error: {ex.Message}");
                ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name", travelTask.AssignedToId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", travelTask.TripId);
                return View(travelTask);
            }
        }

        // GET: TravelTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelTask = await _context.TravelTasks.FindAsync(id);
            if (travelTask == null)
            {
                return NotFound();
            }
            ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name", travelTask.AssignedToId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", travelTask.TripId);
            return View(travelTask);
        }

        // POST: TravelTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,IsCompleted,CompletionDate,AssignedToId,TripId")] TravelTask travelTask)
        {
            if (id != travelTask.Id)
            {
                return NotFound();
            }

            // Exclude navigation properties from validation
            ModelState.Remove("AssignedTo");
            ModelState.Remove("Trip");

            // Set foreign keys to null
            travelTask.AssignedToId = null;
            travelTask.TripId = null;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Validation errors: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Validation errors: " + string.Join(", ", errors));
                ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name", travelTask.AssignedToId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", travelTask.TripId);
                return View(travelTask);
            }

            try
            {
                _context.Attach(travelTask).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TravelTaskExists(travelTask.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Database error: {errorMessage}");
                ModelState.AddModelError("", $"Database error: {errorMessage}");
                ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name", travelTask.AssignedToId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", travelTask.TripId);
                return View(travelTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                ModelState.AddModelError("", $"General error: {ex.Message}");
                ViewData["AssignedToId"] = new SelectList(_context.TripMembers, "Id", "Name", travelTask.AssignedToId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", travelTask.TripId);
                return View(travelTask);
            }
        }

        // GET: TravelTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelTask = await _context.TravelTasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelTask == null)
            {
                return NotFound();
            }

            return View(travelTask);
        }

        // POST: TravelTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var travelTask = await _context.TravelTasks.FindAsync(id);
            if (travelTask != null)
            {
                _context.TravelTasks.Remove(travelTask);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TravelTaskExists(int id)
        {
            return _context.TravelTasks.Any(e => e.Id == id);
        }
    }
}