using ClosedXML.Excel;
using Diplom.Models;
using Diplom.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Diplom.Controllers
{
    public class ImportExcelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int MAX_FILESIZE = 5000 * 1024; // 5 MB

        public ImportExcelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ImportExcel
        public IActionResult Index()
        {
            ViewData["IsLoading"] = false;
            ViewData["ErrorMessage"] = "";
            ViewData["StartTime"] = null;
            ViewData["EndTime"] = null;
            ViewData["TripList"] = new List<Trip>();
            ViewData["TripMemberList"] = new List<TripMember>();
            ViewData["ExpenseList"] = new List<Expense>();
            ViewData["NotificationList"] = new List<Notification>();
            ViewData["TravelTaskList"] = new List<TravelTask>();
            return View("~/Views/Excel/Import.cshtml");
        }

        // POST: /ImportExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IFormFile file)
        {
            bool isLoading = true;
            string errorMessage = "";
            DateTime? startTime = null;
            DateTime? endTime = null;
            var tripList = new List<Trip>();
            var tripMemberList = new List<TripMember>();
            var expenseList = new List<Expense>();
            var notificationList = new List<Notification>();
            var travelTaskList = new List<TravelTask>();

            if (file == null || file.Length == 0)
            {
                errorMessage = "Пожалуйста, выберите файл Excel.";
                isLoading = false;
                SetViewData(isLoading, errorMessage, startTime, endTime, tripList, tripMemberList, expenseList, notificationList, travelTaskList);
                return View("~/Views/Excel/Import.cshtml");
            }

            if (file.Length > MAX_FILESIZE)
            {
                errorMessage = $"Размер файла превышает допустимый лимит ({MAX_FILESIZE / 1024} КБ).";
                isLoading = false;
                SetViewData(isLoading, errorMessage, startTime, endTime, tripList, tripMemberList, expenseList, notificationList, travelTaskList);
                return View("~/Views/Excel/Import.cshtml");
            }

            try
            {
                using (var fileStream = file.OpenReadStream())
                {
                    var randomFile = Path.GetTempFileName();
                    var extension = Path.GetExtension(file.FileName);
                    var targetFilePath = Path.ChangeExtension(randomFile, extension);
                    using (var destinationStream = new FileStream(targetFilePath, FileMode.Create))
                    {
                        await fileStream.CopyToAsync(destinationStream);
                        startTime = DateTime.Now;
                        errorMessage = await GetDataTableFromExcelAsync(destinationStream, tripList, tripMemberList, expenseList, notificationList, travelTaskList);
                        endTime = DateTime.Now;
                    }
                }
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }

            isLoading = false;
            SetViewData(isLoading, errorMessage, startTime, endTime, tripList, tripMemberList, expenseList, notificationList, travelTaskList);
            return View("~/Views/Excel/Import.cshtml"); 
        }

        private void SetViewData(bool isLoading, string errorMessage, DateTime? startTime, DateTime? endTime,
            List<Trip> tripList, List<TripMember> tripMemberList, List<Expense> expenseList,
            List<Notification> notificationList, List<TravelTask> travelTaskList)
        {
            ViewData["IsLoading"] = isLoading;
            ViewData["ErrorMessage"] = errorMessage;
            ViewData["StartTime"] = startTime;
            ViewData["EndTime"] = endTime;
            ViewData["TripList"] = tripList;
            ViewData["TripMemberList"] = tripMemberList;
            ViewData["ExpenseList"] = expenseList;
            ViewData["NotificationList"] = notificationList;
            ViewData["TravelTaskList"] = travelTaskList;
        }

        private async Task<string> GetDataTableFromExcelAsync(FileStream file, List<Trip> tripList, List<TripMember> tripMemberList,
            List<Expense> expenseList, List<Notification> notificationList, List<TravelTask> travelTaskList)
        {
            string errorMessage = "";
            using (var context = _context)
            {
                using (var workbook = new XLWorkbook(file))
                {
                    string errWorksheet = "";
                    int? errIndexRow = 0;

                    try
                    {
                        foreach (IXLWorksheet worksheet in workbook.Worksheets)
                        {
                            errIndexRow = 1;
                            errWorksheet = worksheet.Name;

                            if (worksheet.Name == "Trips")
                            {
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    errIndexRow++;
                                    var trip = new Trip();
                                    var range = worksheet.RangeUsed();
                                    if (range == null)
                                        throw new Exception("Лист Trips пуст или не содержит данных.");

                                    var table = range.AsTable();
                                    if (table == null)
                                        throw new Exception("Не удалось преобразовать диапазон в таблицу в листе Trips.");

                                    var idColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Id");
                                    var nameColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Name");
                                    var startDateColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "StartDate");
                                    var endDateColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "EndDate");
                                    var budgetColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Budget");

                                    if (idColumn == null || nameColumn == null || startDateColumn == null || endDateColumn == null || budgetColumn == null)
                                        throw new Exception("Один или несколько заголовков столбцов (Id, Name, StartDate, EndDate, Budget) отсутствуют в листе Trips.");

                                    var idCell = row.Cell(idColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var nameCell = row.Cell(nameColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var startDateCell = row.Cell(startDateColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var endDateCell = row.Cell(endDateColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var budgetCell = row.Cell(budgetColumn.RangeAddress.FirstAddress.ColumnNumber).Value;

                                    if (string.IsNullOrEmpty(idCell.ToString()))
                                        throw new Exception("Поле Id пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(nameCell.ToString()))
                                        throw new Exception("Поле Name пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(startDateCell.ToString()))
                                        throw new Exception("Поле StartDate пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(endDateCell.ToString()))
                                        throw new Exception("Поле EndDate пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(budgetCell.ToString()))
                                        throw new Exception("Поле Budget пустое или отсутствует.");

                                    if (!int.TryParse(idCell.ToString(), out int id))
                                        throw new Exception("Поле Id должно быть целым числом.");
                                    if (!DateTime.TryParse(startDateCell.ToString(), out DateTime startDate))
                                        throw new Exception("Поле StartDate должно быть датой.");
                                    if (!DateTime.TryParse(endDateCell.ToString(), out DateTime endDate))
                                        throw new Exception("Поле EndDate должно быть датой.");
                                    if (!decimal.TryParse(budgetCell.ToString(), out decimal budget))
                                        throw new Exception("Поле Budget должно быть числом.");

                                    trip.Id = id;
                                    trip.Name = nameCell.ToString();
                                    trip.StartDate = startDate;
                                    trip.EndDate = endDate;
                                    trip.Budget = budget;

                                    tripList.Add(trip);
                                }
                            }

                            errIndexRow = 1;
                            if (worksheet.Name == "TripMembers")
                            {
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    errIndexRow++;
                                    var tripMember = new TripMember();
                                    var range = worksheet.RangeUsed();
                                    if (range == null)
                                        throw new Exception("Лист TripMembers пуст или не содержит данных.");

                                    var table = range.AsTable();
                                    if (table == null)
                                        throw new Exception("Не удалось преобразовать диапазон в таблицу в листе TripMembers.");

                                    var idColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Id");
                                    var nameColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Name");
                                    var contributionColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Contribution");

                                    if (idColumn == null || nameColumn == null || contributionColumn == null)
                                        throw new Exception("Один или несколько заголовков столбцов (Id, Name, Contribution) отсутствуют в листе TripMembers.");

                                    var idCell = row.Cell(idColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var nameCell = row.Cell(nameColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var contributionCell = row.Cell(contributionColumn.RangeAddress.FirstAddress.ColumnNumber).Value;

                                    if (string.IsNullOrEmpty(idCell.ToString()))
                                        throw new Exception("Поле Id пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(nameCell.ToString()))
                                        throw new Exception("Поле Name пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(contributionCell.ToString()))
                                        throw new Exception("Поле Contribution пустое или отсутствует.");

                                    if (!int.TryParse(idCell.ToString(), out int id))
                                        throw new Exception("Поле Id должно быть целым числом.");
                                    if (!decimal.TryParse(contributionCell.ToString(), out decimal contribution))
                                        throw new Exception("Поле Contribution должно быть числом.");

                                    tripMember.Id = id;
                                    tripMember.Name = nameCell.ToString();
                                    tripMember.Contribution = contribution;

                                    tripMemberList.Add(tripMember);
                                }
                            }

                            // Аналогичная логика для остальных листов (Expenses, Notifications, TravelTasks)
                            errIndexRow = 1;
                            if (worksheet.Name == "Expenses")
                            {
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    errIndexRow++;
                                    var expense = new Expense();
                                    var range = worksheet.RangeUsed();
                                    if (range == null)
                                        throw new Exception("Лист Expenses пуст или не содержит данных.");

                                    var table = range.AsTable();
                                    if (table == null)
                                        throw new Exception("Не удалось преобразовать диапазон в таблицу в листе Expenses.");

                                    var idColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Id");
                                    var descriptionColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Description");
                                    var amountColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Amount");

                                    if (idColumn == null || descriptionColumn == null || amountColumn == null)
                                        throw new Exception("Один или несколько заголовков столбцов (Id, Description, Amount) отсутствуют в листе Expenses.");

                                    var idCell = row.Cell(idColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var descriptionCell = row.Cell(descriptionColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var amountCell = row.Cell(amountColumn.RangeAddress.FirstAddress.ColumnNumber).Value;

                                    if (string.IsNullOrEmpty(idCell.ToString()))
                                        throw new Exception("Поле Id пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(descriptionCell.ToString()))
                                        throw new Exception("Поле Description пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(amountCell.ToString()))
                                        throw new Exception("Поле Amount пустое или отсутствует.");

                                    if (!int.TryParse(idCell.ToString(), out int id))
                                        throw new Exception("Поле Id должно быть целым числом.");
                                    if (!decimal.TryParse(amountCell.ToString(), out decimal amount))
                                        throw new Exception("Поле Amount должно быть числом.");

                                    expense.Id = id;
                                    expense.Description = descriptionCell.ToString();
                                    expense.Amount = amount;
                                    expense.PaidById = null;

                                    expenseList.Add(expense);
                                }
                            }

                            errIndexRow = 1;
                            if (worksheet.Name == "Notifications")
                            {
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    errIndexRow++;
                                    var notification = new Notification();
                                    var range = worksheet.RangeUsed();
                                    if (range == null)
                                        throw new Exception("Лист Notifications пуст или не содержит данных.");

                                    var table = range.AsTable();
                                    if (table == null)
                                        throw new Exception("Не удалось преобразовать диапазон в таблицу в листе Notifications.");

                                    var idColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Id");
                                    var messageColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Message");
                                    var sendDateColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "SendDate");
                                    var isReadColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "IsRead");

                                    if (idColumn == null || messageColumn == null || sendDateColumn == null || isReadColumn == null)
                                        throw new Exception("Один или несколько заголовков столбцов (Id, Message, SendDate, IsRead) отсутствуют в листе Notifications.");

                                    var idCell = row.Cell(idColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var messageCell = row.Cell(messageColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var sendDateCell = row.Cell(sendDateColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var isReadCell = row.Cell(isReadColumn.RangeAddress.FirstAddress.ColumnNumber).Value;

                                    if (string.IsNullOrEmpty(idCell.ToString()))
                                        throw new Exception("Поле Id пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(messageCell.ToString()))
                                        throw new Exception("Поле Message пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(sendDateCell.ToString()))
                                        throw new Exception("Поле SendDate пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(isReadCell.ToString()))
                                        throw new Exception("Поле IsRead пустое или отсутствует.");

                                    if (!int.TryParse(idCell.ToString(), out int id))
                                        throw new Exception("Поле Id должно быть целым числом.");
                                    if (!DateTime.TryParse(sendDateCell.ToString(), out DateTime sendDate))
                                        throw new Exception("Поле SendDate должно быть датой.");
                                    if (!bool.TryParse(isReadCell.ToString(), out bool isRead))
                                        throw new Exception("Поле IsRead должно быть булевым значением.");

                                    notification.Id = id;
                                    notification.Message = messageCell.ToString();
                                    notification.SendDate = sendDate;
                                    notification.IsRead = isRead;
                                    notification.ReceiverId = null;

                                    notificationList.Add(notification);
                                }
                            }

                            errIndexRow = 1;
                            if (worksheet.Name == "TravelTasks")
                            {
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    errIndexRow++;
                                    var travelTask = new TravelTask();
                                    var range = worksheet.RangeUsed();
                                    if (range == null)
                                        throw new Exception("Лист TravelTasks пуст или не содержит данных.");

                                    var table = range.AsTable();
                                    if (table == null)
                                        throw new Exception("Не удалось преобразовать диапазон в таблицу в листе TravelTasks.");

                                    var idColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Id");
                                    var descriptionColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "Description");
                                    var isCompletedColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "IsCompleted");
                                    var completionDateColumn = table.FindColumn(c => c.FirstCell().Value.ToString() == "CompletionDate");

                                    if (idColumn == null || descriptionColumn == null || isCompletedColumn == null || completionDateColumn == null)
                                        throw new Exception("Один или несколько заголовков столбцов (Id, Description, IsCompleted, CompletionDate) отсутствуют в листе TravelTasks.");

                                    var idCell = row.Cell(idColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var descriptionCell = row.Cell(descriptionColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var isCompletedCell = row.Cell(isCompletedColumn.RangeAddress.FirstAddress.ColumnNumber).Value;
                                    var completionDateCell = row.Cell(completionDateColumn.RangeAddress.FirstAddress.ColumnNumber).Value;

                                    if (string.IsNullOrEmpty(idCell.ToString()))
                                        throw new Exception("Поле Id пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(descriptionCell.ToString()))
                                        throw new Exception("Поле Description пустое или отсутствует.");
                                    if (string.IsNullOrEmpty(isCompletedCell.ToString()))
                                        throw new Exception("Поле IsCompleted пустое или отсутствует.");

                                    if (!int.TryParse(idCell.ToString(), out int id))
                                        throw new Exception("Поле Id должно быть целым числом.");
                                    if (!bool.TryParse(isCompletedCell.ToString(), out bool isCompleted))
                                        throw new Exception("Поле IsCompleted должно быть булевым значением.");
                                    DateTime? completionDate = string.IsNullOrEmpty(completionDateCell.ToString()) ? null : DateTime.TryParse(completionDateCell.ToString(), out DateTime date) ? date : throw new Exception("Поле CompletionDate должно быть датой или пустым.");

                                    travelTask.Id = id;
                                    travelTask.Description = descriptionCell.ToString();
                                    travelTask.IsCompleted = isCompleted;
                                    travelTask.CompletionDate = completionDate;
                                    travelTask.AssignedToId = null;

                                    travelTaskList.Add(travelTask);
                                }
                            }
                        }

                        //// Создаем новую поездку, если TripList пуст
                        //if (!tripList.Any())
                        //{
                        //    var newTrip = new Trip
                        //    {
                        //        Id = 0,
                        //        Name = "Imported Trip",
                        //        StartDate = DateTime.Now,
                        //        EndDate = DateTime.Now.AddDays(7),
                        //        Budget = 0
                        //    };
                        //    tripList.Add(newTrip);
                        //}

                        // Назначаем TripId для всех записей
                        int tripId = tripList.First().Id;
                        foreach (var member in tripMemberList)
                            member.TripId = tripId;
                        foreach (var expense in expenseList)
                            expense.TripId = tripId;
                        foreach (var notification in notificationList)
                            notification.TripId = tripId;
                        foreach (var task in travelTaskList)
                            task.TripId = tripId;

                        // Сброс значений первичного ключа
                        foreach (var el in tripList) el.Id = 0;
                        foreach (var el in tripMemberList) el.Id = 0;
                        foreach (var el in expenseList) el.Id = 0;
                        foreach (var el in notificationList) el.Id = 0;
                        foreach (var el in travelTaskList) el.Id = 0;

                        using (var tr = context.Database.BeginTransaction())
                        {
                            context.AddRange(tripList);
                            await context.SaveChangesAsync();

                            context.AddRange(tripMemberList);
                            await context.SaveChangesAsync();

                            context.AddRange(expenseList);
                            await context.SaveChangesAsync();

                            context.AddRange(notificationList);
                            await context.SaveChangesAsync();

                            context.AddRange(travelTaskList);
                            await context.SaveChangesAsync();

                            tr.Commit();
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!string.IsNullOrEmpty(errWorksheet))
                            errorMessage = $"Ошибка в импорте в таблице {errWorksheet}, строка - {errIndexRow}";
                        errorMessage += "\n" + exception.Message;
                        if (exception.InnerException != null)
                            errorMessage += "\nInner Exception: " + exception.InnerException.Message;
                    }
                }
            }
            return errorMessage;
        }
    }
}