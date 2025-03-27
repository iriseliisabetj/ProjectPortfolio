using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;

namespace WebApp.Pages.Configs
{
    public class DeleteModel : PageModel
    {
        private readonly IConfigRepository _configRepository;

        public DeleteModel(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        [BindProperty]
        public Configuration Configuration { get; set; } = new Configuration();

        public IActionResult OnGet(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return NotFound();
            }

            try
            {
                Configuration = _configRepository.GetConfigurationByName(name);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidDataException)
            {
                ModelState.AddModelError(string.Empty, $"Configuration '{name}' is corrupted.");
                return Page();
            }

            return Page();
        }

        public IActionResult OnPostAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return NotFound();
            }

            try
            {
                _configRepository.DeleteConfiguration(name);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error deleting configuration: {ex.Message}");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
