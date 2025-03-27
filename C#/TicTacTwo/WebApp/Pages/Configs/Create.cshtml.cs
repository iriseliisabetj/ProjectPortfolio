using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;

namespace WebApp.Pages.Configs
{
    public class CreateModel : PageModel
    {
        private readonly IConfigRepository _configRepository;

        public CreateModel(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        [BindProperty]
        public Configuration Configuration { get; set; } = new Configuration();

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _configRepository.SaveConfiguration(Configuration);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error saving configuration: {ex.Message}");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
