using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;

namespace WebApp.Pages.Games
{
    public class DeleteModel : PageModel
    {
        private readonly IGameRepository _gameRepository;

        public DeleteModel(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        [BindProperty]
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

        public IActionResult OnPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                _gameRepository.DeleteGame(id.Value);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error deleting game: {ex.Message}");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
