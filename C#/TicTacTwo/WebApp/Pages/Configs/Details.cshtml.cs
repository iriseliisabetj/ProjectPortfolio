using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;

namespace WebApp.Pages.Configs
{
    public class DetailsModel : PageModel
    {
        private readonly IConfigRepository _configRepository;

        public DetailsModel(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

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
    }
}
