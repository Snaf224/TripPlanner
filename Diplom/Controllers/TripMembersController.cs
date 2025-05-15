using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Diplom.Data;
using Diplom.Models;
using Microsoft.AspNetCore.Authorization;

namespace Diplom.Controllers
{
    public class TripMembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripMembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TripMembers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TripMembers.Include(t => t.Trip);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TripMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tripMember = await _context.TripMembers
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tripMember == null)
            {
                return NotFound();
            }

            return View(tripMember);
        }

        [Authorize(Roles = "admin")]
        // GET: TripMembers/Create
        public IActionResult Create()
        {
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
            return View();
        }

        // POST: TripMembers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Contribution,TripId")] TripMember tripMember)
        {
            // Исключаем валидацию для навигационного свойства
            ModelState.Remove("Trip");

            // Устанавливаем внешний ключ в null, если не нужен
            // В данном случае TripId уже передается через Bind, поэтому это может быть необязательно
            // tripMember.TripId = null; // Раскомментируйте, если хотите явно обнулить

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Ошибки валидации: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Ошибки валидации: " + string.Join(", ", errors));
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripMember.TripId);
                return View(tripMember);
            }

            try
            {
                Console.WriteLine($"Создание TripMember: Name={tripMember.Name}, Contribution={tripMember.Contribution}, TripId={tripMember.TripId}");
                _context.TripMembers.Add(tripMember);
                await _context.SaveChangesAsync();
                Console.WriteLine("TripMember успешно создан");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Ошибка базы данных: {errorMessage}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {errorMessage}");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripMember.TripId);
                return View(tripMember);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Общая ошибка: {ex.Message}");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripMember.TripId);
                return View(tripMember);
            }
        }

        [Authorize(Roles = "admin")]
        // GET: TripMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tripMember = await _context.TripMembers.FindAsync(id);
            if (tripMember == null)
            {
                return NotFound();
            }
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripMember.TripId);
            return View(tripMember);
        }

        // POST: TripMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Contribution,TripId")] TripMember tripMember)
        {
            if (id != tripMember.Id)
            {
                return NotFound();
            }

            // Исключаем валидацию для навигационного свойства
            ModelState.Remove("Trip");

            // Устанавливаем внешний ключ в null, если не нужен
            // tripMember.TripId = null; // Раскомментируйте, если хотите явно обнулить

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Ошибки валидации: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Ошибки валидации: " + string.Join(", ", errors));
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripMember.TripId);
                return View(tripMember);
            }

            try
            {
                Console.WriteLine($"Обновление TripMember: Id={tripMember.Id}, Name={tripMember.Name}, Contribution={tripMember.Contribution}, TripId={tripMember.TripId}");
                _context.Attach(tripMember).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                Console.WriteLine("TripMember успешно обновлен");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TripMemberExists(tripMember.Id))
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
                Console.WriteLine($"Ошибка базы данных: {errorMessage}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {errorMessage}");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripMember.TripId);
                return View(tripMember);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Общая ошибка: {ex.Message}");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripMember.TripId);
                return View(tripMember);
            }
        }

        // GET: TripMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tripMember = await _context.TripMembers
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tripMember == null)
            {
                return NotFound();
            }

            return View(tripMember);
        }

        // POST: TripMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tripMember = await _context.TripMembers.FindAsync(id);
            if (tripMember == null)
            {
                return NotFound();
            }

            try
            {
                Console.WriteLine($"Удаление TripMember: Id={id}");
                _context.TripMembers.Remove(tripMember);
                await _context.SaveChangesAsync();
                Console.WriteLine("TripMember успешно удален");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Ошибка базы данных: {errorMessage}");
                ModelState.AddModelError("", $"Ошибка при удалении: {errorMessage}");
                return View(tripMember);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Общая ошибка: {ex.Message}");
                return View(tripMember);
            }
        }

        private bool TripMemberExists(int id)
        {
            return _context.TripMembers.Any(e => e.Id == id);
        }
    }
}