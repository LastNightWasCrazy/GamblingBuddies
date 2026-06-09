using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamblingBuddies.Controllers
{
    [Authorize]
    public class ArchiveController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ArchiveController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            string archivePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "reports");
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            var files = Directory.GetFiles(archivePath)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new ArchivedFileViewModel
                {
                    FileName = f.Name,
                    FileSize = f.Length,
                    UploadDate = f.LastWriteTime,
                    FilePath = $"/uploads/reports/{f.Name}"
                })
                .ToList();

            return View(files);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Proszę wybrać plik.";
                return RedirectToAction(nameof(Index));
            }

            if (file.ContentType != "application/pdf")
            {
                TempData["Error"] = "Dozwolone są tylko pliki PDF.";
                return RedirectToAction(nameof(Index));
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "reports");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{file.FileName}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            TempData["Success"] = $"Plik {file.FileName} został przesłany pomyślnie.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Delete(string fileName)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "reports", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["Success"] = $"Plik {fileName} został usunięty.";
            }
            else
            {
                TempData["Error"] = "Plik nie istnieje.";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(string fileName)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "reports", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
    }

    public class ArchivedFileViewModel
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string FilePath { get; set; }
    }
}