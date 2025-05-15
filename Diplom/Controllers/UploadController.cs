using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Diplom.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Diplom.Models;

namespace Diplom.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public UploadController(
            IWebHostEnvironment environment,
            ILogger<UploadController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _environment = environment;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> UploadPhoto()
        {
            var avatarPath = HttpContext.Session.GetString("AvatarPath") ?? "/media/default_avatar.png";
            ViewBag.AvatarPath = avatarPath;

            await SetUserInfoAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile photo)
        {
            string avatarPath = "/media/default_avatar.png";

            if (photo != null && photo.Length > 0)
            {
                try
                {
                    // Генерация хэша от содержимого фото для предотвращения дубликатов
                    var hash = GenerateFileHash(photo);

                    // Путь для сохранения фото, основанный на хэше
                    var savePath = Path.Combine(_environment.WebRootPath, "media", $"{hash}{Path.GetExtension(photo.FileName)}");

                    // Если файл с таким хэшем уже существует, используем его
                    if (!System.IO.File.Exists(savePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                        // Сохраняем файл
                        await using var stream = new FileStream(savePath, FileMode.Create);
                        await photo.CopyToAsync(stream);

                        _logger.LogInformation("Avatar uploaded and saved at: {Path}", savePath);
                    }

                    avatarPath = $"/media/{hash}{Path.GetExtension(photo.FileName)}";
                    HttpContext.Session.SetString("AvatarPath", avatarPath);

                    // Удаляем старые дубликаты
                    DeleteOldDuplicates(hash);

                }
                catch (Exception ex)
                {
                    _logger.LogError("Error uploading file: {Message}", ex.Message);
                }
            }

            ViewBag.AvatarPath = avatarPath;
            await SetUserInfoAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ClearPhoto()
        {
            var defaultPath = "/media/default_avatar.png";
            HttpContext.Session.SetString("AvatarPath", defaultPath);
            ViewBag.AvatarPath = defaultPath;

            await SetUserInfoAsync();
            return View("UploadPhoto");
        }

        private async Task SetUserInfoAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.Email = user?.Email;
                ViewBag.PhoneNumber = string.IsNullOrEmpty(user?.PhoneNumber) ? "Не указан" : user.PhoneNumber;
                ViewBag.Gender = user?.GetLocalizedGender() ?? "Не указан";
            }
            else
            {
                ViewBag.Email = "Неизвестно";
                ViewBag.PhoneNumber = "не указан";
                ViewBag.Gender = "Неизвестно";
            }
        }

        private string GenerateFileHash(IFormFile file)
        {
            using var md5 = MD5.Create();
            using var stream = file.OpenReadStream();
            var hashBytes = md5.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private void DeleteOldDuplicates(string hash)
        {
            var directoryPath = Path.Combine(_environment.WebRootPath, "media");
            var files = Directory.GetFiles(directoryPath, $"{hash}*");

            // Оставляем только один файл, остальные удаляем
            foreach (var file in files.Skip(1)) // Пропускаем первый файл, который уже используется
            {
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                    _logger.LogInformation("Duplicate file deleted: {File}", file);
                }
            }
        }
    }
}
