using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;
using GameBrain;

namespace WebApp.Pages.Games
{
    public class EditModel : PageModel
    {
        private readonly IGameRepository _gameRepository;
        private readonly IConfigRepository _configRepository;

        public EditModel(IGameRepository gameRepository, IConfigRepository configRepository)
        {
            _gameRepository = gameRepository;
            _configRepository = configRepository;
        }

        [BindProperty]
        public Game Game { get; set; } = new Game();

        public SelectList ConfigSelectList { get; set; } = default!;
        public SelectList GameTypeSelectList { get; set; } = default!;

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

            var configNames = _configRepository.GetConfigurationNames();
            ConfigSelectList = new SelectList(configNames, Game.GameName);
            GameTypeSelectList = new SelectList(Enum.GetValues(typeof(GameType)).Cast<GameType>(), Game.GameType);

            return Page();
        }

        public IActionResult OnPost()
        {
            ConfigSelectList = new SelectList(_configRepository.GetConfigurationNames(), Game.GameName);
            GameTypeSelectList = new SelectList(Enum.GetValues(typeof(GameType)).Cast<GameType>(), Game.GameType);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!_configRepository.GetConfigurationNames().Contains(Game.GameName, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Selected configuration does not exist.");
                return Page();
            }

            try
            {
                var config = _configRepository.GetConfigurationByName(Game.GameName);
                if (config == null)
                {
                    ModelState.AddModelError(string.Empty, "Selected configuration does not exist.");
                    return Page();
                }

                var gameConfig = new GameConfiguration()
                {
                    Name = config.ConfigName,
                    BoardSizeHeight = config.BoardSizeHeight,
                    BoardSizeWidth = config.BoardSizeWidth,
                    GridSizeHeight = config.GridSizeHeight,
                    GridSizeWidth = config.GridSizeWidth,
                    MovePieceAfterNMoves = config.MovePieceAfterNMoves,
                    PiecesPerPlayer = config.PiecesPerPlayer,
                    WinCondition = config.WinCondition
                };
                
                var gameState = new GameState
                {
                    GameConfiguration = gameConfig,
                    GameBoard = InitializeGameBoard(config.BoardSizeWidth, config.BoardSizeHeight),
                    NextMoveBy = EGamePiece.X,
                    RemainingPiecesX = config.PiecesPerPlayer,
                    RemainingPiecesO = config.PiecesPerPlayer,
                    GridRow = 0,
                    GridCol = 0,
                    GameType = Game.GameType
                };

                Game.GameStateJson = JsonSerializer.Serialize(gameState, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                });

                Game.ModifiedAt = DateTime.Now;

                _gameRepository.UpdateGame(Game);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating game: {ex.Message}");
                return Page();
            }
        }

        private EGamePiece[][] InitializeGameBoard(int width, int height)
        {
            var board = new EGamePiece[width][];
            for (int x = 0; x < width; x++)
            {
                board[x] = new EGamePiece[height];
                for (int y = 0; y < height; y++)
                {
                    board[x][y] = EGamePiece.Empty;
                }
            }
            return board;
        }
    }
}
