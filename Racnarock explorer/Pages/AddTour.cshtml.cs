using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Racnarock_explorer.Models;
using Racnarock_explorer.Services;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Racnarock_explorer.Pages
{
    public class AddTourModel : PageModel
    {
        private readonly ILogger<AddTourModel> _logger;
        private readonly FileUploadService _fileUploadService;

        public AddTourModel(ILogger<AddTourModel> logger, FileUploadService fileUploadService)
        {
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        [BindProperty]
        public Tour Tour { get; set; }

        [BindProperty]
        public IFormFile AudioFile { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("LoggedInUser") != "admin")
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (AudioFile == null)
            {
                ModelState.AddModelError("AudioFile", "Please select a file to upload.");
                return Page();
            }

            // Upload the file and set the AudioUrl
            var filePath = await _fileUploadService.UploadFileAsync(AudioFile, Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Music"));
            if (filePath == null)
            {
                ModelState.AddModelError("", "Error uploading file.");
                return Page();
            }

            Tour.AudioUrl = filePath; // Set the AudioUrl here

            // Custom validation for AudioUrl
            if (string.IsNullOrEmpty(Tour.AudioUrl))
            {
                ModelState.AddModelError("Tour.AudioUrl", "AudioUrl is required.");
            }

            // Validate the model again after setting AudioUrl
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid after setting AudioUrl");
                foreach (var modelStateKey in ViewData.ModelState.Keys)
                {
                    var modelStateVal = ViewData.ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        _logger.LogError($"Error in {modelStateKey}: {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            SaveTour(Tour);
            TempData["SuccessMessage"] = "Tour added successfully!";
            return RedirectToPage("/Tours");
        }

        private void SaveTour(Tour tour)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "tours.json");
            List<Tour> tours = new List<Tour>();
            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                tours = JsonSerializer.Deserialize<List<Tour>>(json);
            }

            tour.Id = tours.Count > 0 ? tours[^1].Id + 1 : 1;
            tours.Add(tour);

            var updatedJson = JsonSerializer.Serialize(tours, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, updatedJson);
        }
    }
}
