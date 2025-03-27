using DAL;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;

namespace WebApp.Pages.Configs
{
    public class IndexModel : PageModel
    {
        private readonly IConfigRepository _configRepository;

        public IndexModel(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public IList<Configuration> Configurations { get; set; } = new List<Configuration>();

        public void OnGet()
        {
            Configurations = _configRepository.GetAllConfigurations();
        }
    }
}
