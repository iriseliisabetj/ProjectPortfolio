using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class Multiplayer : PageModel
{
    private readonly IGameRepository _gameRepository;

    public Multiplayer(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    [BindProperty(SupportsGet = true)]
    public int GameId { get; set; }

    public Game Game { get; set; } = default!;
    
    public GameType GameType { get; set; }

    public IActionResult OnGet()
    {
        Game = _gameRepository.LoadGameEntityById(GameId);
        if (Game == null) 
        {
            return NotFound();
        }
    
        if (Game.GameType != GameType.Multiplayer &&
            Game.GameType != GameType.PlayerVsAi)
        {
            return NotFound();
        }
    
        GameType = Game.GameType;
        
        return Page();
    }
}