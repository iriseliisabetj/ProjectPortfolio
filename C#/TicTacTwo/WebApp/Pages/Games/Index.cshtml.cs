using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;
using GameBrain;

namespace WebApp.Pages.Games
{
    public class IndexModel : PageModel
    {
        private readonly IGameRepository _gameRepository;

        public IndexModel(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public List<Game> MultiplayerGames { get; set; } = new();
        public List<Game> PlayerVsAiGames { get; set; } = new();
        public List<Game> AiVsAiGames { get; set; } = new();

        public void OnGet()
        {
            var allGames = _gameRepository.GetAllGames();

            MultiplayerGames = allGames
                .Where(g => g.GameType == GameType.Multiplayer)
                .OrderByDescending(g => g.ModifiedAt)
                .ToList();

            PlayerVsAiGames = allGames
                .Where(g => g.GameType == GameType.PlayerVsAi)
                .OrderByDescending(g => g.ModifiedAt)
                .ToList();
            
            AiVsAiGames = allGames
                .Where(g => g.GameType == GameType.AiVsAi)
                .OrderByDescending(g => g.ModifiedAt)
                .ToList();
        }
    }
}
