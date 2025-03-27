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
    public class CreateModel : PageModel
    {
        private readonly IConfigRepository _configRepository;
        private readonly IGameRepository _gameRepository;

        public CreateModel(IConfigRepository configRepository, IGameRepository gameRepository)
        {
            _configRepository = configRepository;
            _gameRepository = gameRepository;
        }

        public SelectList ConfigSelectList { get; set; } = default!;
        public SelectList GameTypeSelectList { get; set; } = default!;

        [BindProperty]
        public Game Game { get; set; } = new Game();

        public IActionResult OnGet()
        {
            var configurations = _configRepository.GetAllConfigurations();
            ConfigSelectList = new SelectList(configurations, "Id", "ConfigName");
            GameTypeSelectList = new SelectList(Enum.GetValues(typeof(GameType)).Cast<GameType>());
            return Page();
        }

        public IActionResult OnPost()
        {
            var configurations = _configRepository.GetAllConfigurations();
            ConfigSelectList = new SelectList(configurations, "Id", "ConfigName");
            GameTypeSelectList = new SelectList(Enum.GetValues(typeof(GameType)).Cast<GameType>());

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!_configRepository.ConfigurationExists(Game.ConfigId))
            {
                ModelState.AddModelError(string.Empty, "Selected configuration does not exist.");
                return Page();
            }

            var configEntity = _configRepository.GetConfigurationById(Game.ConfigId);
            if (configEntity == null)
            {
                ModelState.AddModelError(string.Empty, "Selected configuration does not exist.");
                return Page();
            }

            var gameConfig = new GameConfiguration
            {
                Name = configEntity.ConfigName,
                BoardSizeWidth = configEntity.BoardSizeWidth,
                BoardSizeHeight = configEntity.BoardSizeHeight,
                GridSizeWidth = configEntity.GridSizeWidth,
                GridSizeHeight = configEntity.GridSizeHeight,
                WinCondition = configEntity.WinCondition,
                PiecesPerPlayer = configEntity.PiecesPerPlayer,
                MovePieceAfterNMoves = configEntity.MovePieceAfterNMoves
            };

            var gameState = new GameState
            {
                GameConfiguration = gameConfig,
                GameBoard = InitializeGameBoard(gameConfig.BoardSizeWidth, gameConfig.BoardSizeHeight),
                NextMoveBy = EGamePiece.X,
                RemainingPiecesX = gameConfig.PiecesPerPlayer,
                RemainingPiecesO = gameConfig.PiecesPerPlayer,
                GridRow = 0,
                GridCol = 0,
                GameType = Game.GameType
            };

            Game.GameStateJson = JsonSerializer.Serialize(gameState, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            });

            Game.CreatedAt = DateTime.Now;
            Game.ModifiedAt = DateTime.Now;

            var newGameId = _gameRepository.SaveGame(
                Game.GameStateJson,
                Game.GameName,
                Game.GameType,
                Game.PlayerXPass,
                Game.PlayerOPass,
                Game.ConfigId
            );

            if (Game.GameType == GameType.PlayerVsAi || Game.GameType == GameType.Multiplayer)
            {
                return RedirectToPage("../Multiplayer", new { GameId = newGameId });
            }

            return RedirectToPage("./Index");
        }
        
        public EGamePiece[][] InitializeGameBoard(int width, int height)
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
        
        public JsonResult OnGetGameStateJson(int configId)
        {
            var configEntity = _configRepository.GetConfigurationById(configId);
            if (configEntity == null)
            {
                return new JsonResult(new { success = false, message = "Configuration not found." });
            }

            var gameConfig = new GameConfiguration
            {
                Name = configEntity.ConfigName,
                BoardSizeWidth = configEntity.BoardSizeWidth,
                BoardSizeHeight = configEntity.BoardSizeHeight,
                GridSizeWidth = configEntity.GridSizeWidth,
                GridSizeHeight = configEntity.GridSizeHeight,
                WinCondition = configEntity.WinCondition,
                PiecesPerPlayer = configEntity.PiecesPerPlayer,
                MovePieceAfterNMoves = configEntity.MovePieceAfterNMoves
            };

            var gameState = new GameState
            {
                GameConfiguration = gameConfig,
                GameBoard = InitializeGameBoard(gameConfig.BoardSizeWidth, gameConfig.BoardSizeHeight),
                NextMoveBy = EGamePiece.X,
                RemainingPiecesX = gameConfig.PiecesPerPlayer,
                RemainingPiecesO = gameConfig.PiecesPerPlayer,
                GridRow = 0,
                GridCol = 0,
                GameType = Game.GameType
            };

            var gameStateJson = JsonSerializer.Serialize(gameState, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            });

            return new JsonResult(new { success = true, gameStateJson });
        }
    }
}
