using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class JoinPlayer : PageModel
{
    private readonly IGameRepository _gameRepository;

    public JoinPlayer(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    [BindProperty(SupportsGet = true)]
    public int GameId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string PlayerRole { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public Game Game { get; set; } = default!;

    public IActionResult OnGet()
    {
        if (!Enum.TryParse<PlayerRoleEnum>(PlayerRole, true, out var role))
        {
            return BadRequest("Invalid player role.");
        }

        Game = _gameRepository.LoadGameEntityById(GameId);
        if (Game == null)
        {
            return NotFound();
        }
        
        if (Game.GameType == GameType.AiVsAi)
        {
            if (role != PlayerRoleEnum.Spectator)
            {
                return BadRequest("This is an AI vs AI game. Please join as Spectator.");
            }
            return Page();
        }

        if (Game.GameType != GameType.Multiplayer && Game.GameType != GameType.PlayerVsAi)
        {
            return Page();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!Enum.TryParse<PlayerRoleEnum>(PlayerRole, true, out var role))
        {
            ModelState.AddModelError(string.Empty, "Invalid player role.");
            return Page();
        }
    
        Game = _gameRepository.LoadGameEntityById(GameId);
        if (Game == null)
        {
            ModelState.AddModelError(string.Empty, "Game not found.");
            return Page();
        }
        
        if (Game.GameType == GameType.AiVsAi)
        {
            if (role != PlayerRoleEnum.Spectator)
            {
                ModelState.AddModelError(string.Empty, "This is an AI vs AI game. Please join as Spectator.");
                return Page();
            }

            HttpContext.Session.SetString("GameId", Game.Id.ToString());
            HttpContext.Session.SetString("PlayerRole", "Spectator");
            
            return RedirectToPage("./PlayGame", new { GameId = Game.Id, PlayerRole = "Spectator" });
        }
        
        if (Game.GameType == GameType.PlayerVsAi)
        {
            if (role == PlayerRoleEnum.X)
            {
                if (!string.IsNullOrEmpty(Game.PlayerXPass) && Game.PlayerXPass != Password)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password for Player X.");
                    return Page();
                }

                HttpContext.Session.SetString("GameId", Game.Id.ToString());
                HttpContext.Session.SetString("PlayerRole", "X");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "In PlayerVsAi mode, only Player X can join.");
                return Page();
            }
        }

        else if (role == PlayerRoleEnum.X || role == PlayerRoleEnum.O)
        {
            if (role == PlayerRoleEnum.X && !string.IsNullOrEmpty(Game.PlayerXPass) && Game.PlayerXPass != Password)
            {
                ModelState.AddModelError(string.Empty, "Incorrect password for Player X.");
                return Page();
            }

            if (role == PlayerRoleEnum.O && !string.IsNullOrEmpty(Game.PlayerOPass) && Game.PlayerOPass != Password)
            {
                ModelState.AddModelError(string.Empty, "Incorrect password for Player O.");
                return Page();
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid player role.");
            return Page();
        }
        
        HttpContext.Session.SetString("GameId", Game.Id.ToString());
        HttpContext.Session.SetString("PlayerRole", role.ToString());
        return RedirectToPage("./PlayGame", new { GameId = Game.Id, PlayerRole });
    }

    private enum PlayerRoleEnum
    {
        X,
        O,
        Spectator
    }
}