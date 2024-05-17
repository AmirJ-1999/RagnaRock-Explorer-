using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Racnarock_explorer.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Racnarock_explorer.Pages
{
    public class EditTourModel : PageModel
    {
        [BindProperty]
        public Tour Tour { get; set; }

        public IActionResult OnGet(int id)
        {
            var tours = LoadToursFromFile();
            Tour = tours.FirstOrDefault(t => t.Id == id);

            if (Tour == null)
            {
                return RedirectToPage("/Tours");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                var tours = LoadToursFromFile();
                var existingTour = tours.FirstOrDefault(t => t.Id == Tour.Id);

                if (existingTour != null)
                {
                    existingTour.Title = Tour.Title;
                    existingTour.Description = Tour.Description;
                    SaveToursToFile(tours);
                    TempData["SuccessMessage"] = "Tour updated successfully.";
                    return RedirectToPage("/Tours");
                }
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
