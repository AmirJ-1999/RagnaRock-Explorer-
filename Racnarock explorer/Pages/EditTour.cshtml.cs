using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Racnarock_explorer.Models;
using Racnarock_explorer.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Racnarock_explorer.Pages
{
    public class EditTourModel : PageModel
    {
        private readonly ILogger<EditTourModel> _logger;
        private readonly FileUploadService _fileUploadService;

        public EditTourModel(ILogger<EditTourModel> logger, FileUploadService fileUploadService)
        {
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        [BindProperty]
        public Tour Tour { get; set; }

        [BindProperty]
        public IFormFile AudioFile { get; set; }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("OnGet called with id: {Id}", id);

            var tours = LoadToursFromFile();
            Tour = tours.FirstOrDefault(t => t.Id == id);

            if (Tour == null)
            {
                _logger.LogWarning("Tour with id: {Id} not found", id);
                return RedirectToPage("/Tours");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPost called");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                return Page();
            }

            var tours = LoadToursFromFile();
            var existingTour = tours.FirstOrDefault(t => t.Id == Tour.Id);

            if (existingTour != null)
            {
                existingTour.Title = Tour.Title;
                existingTour.Description = Tour.Description;

                if (AudioFile != null)
                {
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Music");
                    var audioUrl = await _fileUploadService.UploadFileAsync(AudioFile, uploadFolder);
                    existingTour.AudioUrl = audioUrl;
                }

                SaveToursToFile(tours);

                _logger.LogInformation("Tour updated successfully");
                TempData["SuccessMessage"] = "Tour updated successfully.";
                return RedirectToPage(new { id = Tour.Id });
            }
            else
            {
                _logger.LogWarning("Tour with id: {Id} not found for updating", Tour.Id);
            }

            return Page();
        }

        private List<Tour> LoadToursFromFile()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "tours.json");
            var tours = new List<Tour>();

            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                tours = JsonSerializer.Deserialize<List<Tour>>(json);
            }

            return tours;
        }

        private void SaveToursToFile(List<Tour> tours)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "tours.json");
            var json = JsonSerializer.Serialize(tours, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }
    }
}
