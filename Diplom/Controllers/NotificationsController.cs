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
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Notifications.Include(n => n.Receiver).Include(n => n.Trip);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .Include(n => n.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name");
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
            return View();
        }

        // POST: Notifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Message,SendDate,IsRead,ReceiverId,TripId")] Notification notification)
        {
            // Исключаем валидацию для навигационных свойств
            ModelState.Remove("Receiver");
            ModelState.Remove("Trip");

            // Устанавливаем внешние ключи в null
            notification.ReceiverId = null;
            notification.TripId = null;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Ошибки валидации: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Ошибки валидации: " + string.Join(", ", errors));
                ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name", notification.ReceiverId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", notification.TripId);
                return View(notification);
            }

            try
            {
                Console.WriteLine($"Создание Notification: Message={notification.Message}, SendDate={notification.SendDate}, IsRead={notification.IsRead}");
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                Console.WriteLine("Notification успешно создан");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Ошибка базы данных: {errorMessage}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {errorMessage}");
                ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name", notification.ReceiverId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", notification.TripId);
                return View(notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Общая ошибка: {ex.Message}");
                ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name", notification.ReceiverId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", notification.TripId);
                return View(notification);
            }
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name", notification.ReceiverId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", notification.TripId);
            return View(notification);
        }

        // POST: Notifications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Message,SendDate,IsRead,ReceiverId,TripId")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            // Исключаем валидацию для навигационных свойств
            ModelState.Remove("Receiver");
            ModelState.Remove("Trip");

            // Устанавливаем внешние ключи в null
            notification.ReceiverId = null;
            notification.TripId = null;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Ошибки валидации: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Ошибки валидации: " + string.Join(", ", errors));
                ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name", notification.ReceiverId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", notification.TripId);
                return View(notification);
            }

            try
            {
                _context.Attach(notification).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(notification.Id))
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
                ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name", notification.ReceiverId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", notification.TripId);
                return View(notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Общая ошибка: {ex.Message}");
                ViewData["ReceiverId"] = new SelectList(_context.TripMembers, "Id", "Name", notification.ReceiverId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", notification.TripId);
                return View(notification);
            }
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .Include(n => n.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}