using ClosedXML.Excel;
using Diplom.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Diplom.Data;
using System.Runtime.InteropServices;

namespace Diplom.Controllers.Excel
{
    public class ExcelExportController : Controller
    {

        private readonly ApplicationDbContext _context;

        public ExcelExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Export()
        {
            return View("/Views/Excel/Export.cshtml"); // Явно указываем путь
        }

        public IActionResult Download()
        {
            using var workbook = new XLWorkbook();

            // Trips worksheet
            var worksheet1 = workbook.Worksheets.Add("Trips");
            worksheet1.Cell(1, 1).Value = "Id";
            worksheet1.Cell(1, 2).Value = "Name";
            worksheet1.Cell(1, 3).Value = "StartDate";
            worksheet1.Cell(1, 4).Value = "EndDate";
            worksheet1.Cell(1, 5).Value = "Budget";
            worksheet1.Row(1).Style.Font.Bold = true;

            int i = 2;
            foreach (var trip in _context.Trips)
            {
                worksheet1.Cell(i, 1).Value = trip.Id;
                worksheet1.Cell(i, 2).Value = trip.Name;
                worksheet1.Cell(i, 3).Value = trip.StartDate;
                worksheet1.Cell(i, 4).Value = trip.EndDate;
                worksheet1.Cell(i, 5).Value = trip.Budget;
                i++;
            }

            // TripMembers worksheet
            var worksheet2 = workbook.Worksheets.Add("TripMembers");
            worksheet2.Cell(1, 1).Value = "Id";
            worksheet2.Cell(1, 2).Value = "Name";
            worksheet2.Cell(1, 3).Value = "Contribution";
            worksheet2.Row(1).Style.Font.Bold = true;

            i = 2;
            foreach (var member in _context.TripMembers)
            {
                worksheet2.Cell(i, 1).Value = member.Id;
                worksheet2.Cell(i, 2).Value = member.Name;
                worksheet2.Cell(i, 3).Value = member.Contribution;
                i++;
            }

            // Expenses worksheet
            var worksheet3 = workbook.Worksheets.Add("Expenses");
            worksheet3.Cell(1, 1).Value = "Id";
            worksheet3.Cell(1, 2).Value = "Description";
            worksheet3.Cell(1, 3).Value = "Amount";
            worksheet3.Row(1).Style.Font.Bold = true;

            i = 2;
            foreach (var expense in _context.Expenses)
            {
                worksheet3.Cell(i, 1).Value = expense.Id;
                worksheet3.Cell(i, 2).Value = expense.Description;
                worksheet3.Cell(i, 3).Value = expense.Amount;
                i++;
            }

            // Notifications worksheet
            var worksheet4 = workbook.Worksheets.Add("Notifications");
            worksheet4.Cell(1, 1).Value = "Id";
            worksheet4.Cell(1, 2).Value = "Message";
            worksheet4.Cell(1, 3).Value = "SendDate";
            worksheet4.Cell(1, 4).Value = "IsRead";
            worksheet4.Row(1).Style.Font.Bold = true;

            i = 2;
            foreach (var notification in _context.Notifications)
            {
                worksheet4.Cell(i, 1).Value = notification.Id;
                worksheet4.Cell(i, 2).Value = notification.Message;
                worksheet4.Cell(i, 3).Value = notification.SendDate;
                worksheet4.Cell(i, 4).Value = notification.IsRead;
                i++;
            }

            // TravelTasks worksheet
            var worksheet5 = workbook.Worksheets.Add("TravelTasks");
            worksheet5.Cell(1, 1).Value = "Id";
            worksheet5.Cell(1, 2).Value = "Description";
            worksheet5.Cell(1, 3).Value = "IsCompleted";
            worksheet5.Cell(1, 4).Value = "CompletionDate";
            worksheet5.Row(1).Style.Font.Bold = true;

            i = 2;
            foreach (var task in _context.TravelTasks)
            {
                worksheet5.Cell(i, 1).Value = task.Id;
                worksheet5.Cell(i, 2).Value = task.Description;
                worksheet5.Cell(i, 3).Value = task.IsCompleted;
                worksheet5.Cell(i, 4).Value = task.CompletionDate;
                i++;
            }

            // Save the workbook to a memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Return the Excel file as a download
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "TravelData.xlsx");
        }
    }
}
