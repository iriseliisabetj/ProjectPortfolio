using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;

namespace WebApp.Pages.Games
{
    public class DetailsModel : PageModel
    {
        private readonly IGameRepository _gameRepository;

        public DetailsModel(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public Game Game { get; set; } = new Game();

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = _gameRepository.LoadGameEntityById(id.Value);
            if (game == null)
            {
                return NotFound();
            }

            Game = game;
            return Page();
        }
    }
}
