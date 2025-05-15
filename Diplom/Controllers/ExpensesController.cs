using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Diplom.Data;
using Diplom.Models;

namespace Diplom.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var expenses = _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.Trip);
            return View(await expenses.ToListAsync());
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name");
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount")] Expense expense)
        {
            // Исключаем валидацию для навигационных свойств
            ModelState.Remove("PaidBy");
            ModelState.Remove("Trip");
            ModelState.Remove("PaidById");
            ModelState.Remove("TripId");

            // Устанавливаем внешние ключи в null
            expense.PaidById = null;
            expense.TripId = null;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Ошибки валидации: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Ошибки валидации: " + string.Join(", ", errors));
                ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
                return View(expense);
            }

            try
            {
                Console.WriteLine($"Создание Expense: Description={expense.Description}, Amount={expense.Amount}");
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                Console.WriteLine("Expense успешно создан");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Ошибка базы данных: {errorMessage}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {errorMessage}");
                ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
                return View(expense);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Общая ошибка: {ex.Message}");
                ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
                return View(expense);
            }
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name", expense.PaidById);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", expense.TripId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            // Исключаем валидацию для навигационных свойств
            ModelState.Remove("PaidBy");
            ModelState.Remove("Trip");
            ModelState.Remove("PaidById");
            ModelState.Remove("TripId");

            // Устанавливаем внешние ключи в null
            expense.PaidById = null;
            expense.TripId = null;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Ошибки валидации: " + string.Join(", ", errors));
                ModelState.AddModelError("", "Ошибки валидации: " + string.Join(", ", errors));
                ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
                return View(expense);
            }

            try
            {
                _context.Attach(expense).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(expense.Id))
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
                ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
                return View(expense);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Общая ошибка: {ex.Message}");
                ViewData["PaidById"] = new SelectList(_context.TripMembers, "Id", "Name");
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name");
                return View(expense);
            }
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}