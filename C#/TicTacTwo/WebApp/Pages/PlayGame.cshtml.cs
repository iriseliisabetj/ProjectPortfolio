using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class PlayGame : PageModel
{
    private readonly IConfigRepository _configRepository;
    private readonly IGameRepository _gameRepository;

    public PlayGame(IConfigRepository configRepository, IGameRepository gameRepository)
    {
        _configRepository = configRepository;
        _gameRepository = gameRepository;
    }
    
    [BindProperty(SupportsGet = true)]
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public TicTacTwoBrain TicTacTwoBrain { get; set; } = default!;
    public GameConfiguration GameConfiguration { get; set; }
    
    public List<(string Value, string Text)> Directions = new()
    {
        ("up", "Up"),
        ("down", "Down"),
        ("left", "Left"),
        ("right", "Right"),
        ("upleft", "UpLeft"),
        ("upright", "UpRight"),
        ("downleft", "DownLeft"),
        ("downright", "DownRight")
    };
    
    public int RemainingPiecesX { get; set; }
    public int RemainingPiecesO { get; set; }
    
    public int MovePieceAfterNMoves { get; set; }

    public int GridStartX { get; set; }
    public int GridStartY { get; set; }
    public int GridEndX { get; set; }
    public int GridEndY { get; set; }
    
    [TempData]
    public string Message { get; set; } = string.Empty;
    public bool ShowMoveGridOptions { get; set; }
    public bool PlacePieceActive { get; set; }
    public bool IsSpecialMoveAllowed { get; set; }
    public bool IsMovePieceActive { get; set; }

    [BindProperty(SupportsGet = true)] 
    public string? PlayerRole { get; set; } = string.Empty;
    public bool IsVictory { get; set; }

    public IActionResult OnGet(int? x, int? y, string mode, bool gridMoved = false, int? sourceX = null, int? sourceY = null, int? destX = null, int? destY = null)
    {
        if (GameId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Invalid Game ID.");
            return Page();
        }

        var (jsonState, loadedGameName) = _gameRepository.LoadGameById(GameId);
        if (string.IsNullOrEmpty(jsonState))
        {
            ModelState.AddModelError(string.Empty, "Game not found or has no state.");
            return Page();
        }

        var gameEntity = _gameRepository.LoadGameEntityById(GameId);
        if (gameEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Game entity not found.");
            return Page();
        }

        var configEntity = _configRepository.GetConfigurationById(gameEntity.ConfigId);
        if (configEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Configuration not found.");
            return Page();
        }

        GameConfiguration = new GameConfiguration
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

        var brain = new TicTacTwoBrain(GameConfiguration, EGamePiece.X);
        brain.SetGameStateJson(jsonState);

        TicTacTwoBrain = brain;
        GameName = loadedGameName;

        RemainingPiecesX = brain.GetRemainingPieces(EGamePiece.X);
        RemainingPiecesO = brain.GetRemainingPieces(EGamePiece.O);

        MovePieceAfterNMoves = GameConfiguration.MovePieceAfterNMoves;

        IsVictory = brain.CheckVictory();

        var gridPos = brain.GetGridPosition();
        GridStartX = gridPos.col;
        GridStartY = gridPos.row;
        GridEndX = GridStartX + GameConfiguration.GridSizeWidth - 1;
        GridEndY = GridStartY + GameConfiguration.GridSizeHeight - 1;

        var needed = brain.PiecesNeededBeforeAdditionalActions();
        var xPlaced = GameConfiguration.PiecesPerPlayer - RemainingPiecesX;
        var oPlaced = GameConfiguration.PiecesPerPlayer - RemainingPiecesO;

        IsSpecialMoveAllowed = xPlaced >= needed && oPlaced >= needed;
        ShowMoveGridOptions = mode == "MoveGrid";
        PlacePieceActive = mode == "PlacePiece";
        IsMovePieceActive = mode == "MovePiece";

        if (string.IsNullOrEmpty(PlayerRole))
        {
            var storedRole = HttpContext.Session.GetString("PlayerRole") ?? string.Empty;
            if (!string.IsNullOrEmpty(storedRole))
            {
                return RedirectToPage("./PlayGame", new { GameId, PlayerRole = storedRole, mode });
            }

            ModelState.AddModelError(string.Empty, "The PlayerRole field is required.");
            return Page();
        }

        HttpContext.Session.SetString("PlayerRole", PlayerRole);

        if (IsNewGame(brain))
        {
            CenterGrid(brain, loadedGameName);
            PlacePieceActive = false;
            IsMovePieceActive = false;
            ShowMoveGridOptions = false;
        }

        if (IsVictory)
        {
            var currentPlayerRole = HttpContext.Session.GetString("PlayerRole") ?? string.Empty;
            Message = currentPlayerRole + " won! This game is finished. Please reset or create a new game.";
            PlacePieceActive = false;
            IsMovePieceActive = false;
            ShowMoveGridOptions = false;
            return Page();
        }

        if (IsMovePieceActive)
        {
            if (sourceX.HasValue && sourceY.HasValue && !destX.HasValue && !destY.HasValue)
            {
                TempData["SourceX"] = sourceX;
                TempData["SourceY"] = sourceY;
            }
            
            if (sourceX.HasValue && sourceY.HasValue && destX.HasValue && destY.HasValue)
            {
                return OnPostMovePiece(sourceX.Value, sourceY.Value, destX.Value, destY.Value, mode);
            }
        }
        
        if (x.HasValue && y.HasValue && mode != "MovePiece")
        {
            var currentPlayer = brain.GetCurrentTurn();
            
            if ((currentPlayer == EGamePiece.X && PlayerRole != "X") ||
                (currentPlayer == EGamePiece.O && PlayerRole != "O"))
            {
                ModelState.AddModelError("", "You are not the current player.");
                return Page();
            }

            if (!MultiplayerTurnCheck(currentPlayer)) return Page();

            var placed = brain.MakeAMove(x.Value, y.Value);
            if (!placed)
            {
                ModelState.AddModelError("", "Failed to place piece.");
                return Page();
            }

            var victory = brain.CheckVictory();
            UpdateRepositoryAndSetMessage(loadedGameName, brain, victory, currentPlayer, gameEntity);

            return RedirectToPage("./PlayGame", new { GameId, PlayerRole });
        }

        return Page();
    }
    
    public IActionResult OnPostPlacePiece(int x, int y)
    {
        return RedirectToPage("./PlayGame", new { GameId, PlayerRole, mode = "PlacePiece" });
    }
    
    public IActionResult OnPostStartMovePiece()
    {
        var (jsonState, gameName) = _gameRepository.LoadGameById(GameId);
        if (string.IsNullOrEmpty(jsonState))
        {
            ModelState.AddModelError(string.Empty, "Game not found.");
            return Page();
        }
        var gameEntity = _gameRepository.LoadGameEntityById(GameId);
        if (gameEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Game entity not found.");
            return Page();
        }
        var configEntity = _configRepository.GetConfigurationById(gameEntity.ConfigId);
        if (configEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Configuration not found.");
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
        var brain = new TicTacTwoBrain(gameConfig, EGamePiece.X);
        brain.SetGameStateJson(jsonState);
        var piecesNeeded = brain.PiecesNeededBeforeAdditionalActions();
        var xPlaced = gameConfig.PiecesPerPlayer - brain.GetRemainingPieces(EGamePiece.X);
        var oPlaced = gameConfig.PiecesPerPlayer - brain.GetRemainingPieces(EGamePiece.O);
        if (xPlaced < piecesNeeded || oPlaced < piecesNeeded)
        {
            ModelState.AddModelError(string.Empty, "Special moves are not available yet.");
            return Page();
        }
        return RedirectToPage("./PlayGame", new { GameId, PlayerRole, mode = "MovePiece" });
    }

    public IActionResult OnPostStartMoveGrid()
    {
        var (jsonState, gameName) = _gameRepository.LoadGameById(GameId);
        if (string.IsNullOrEmpty(jsonState))
        {
            ModelState.AddModelError(string.Empty, "Game not found.");
            return Page();
        }

        var gameEntity = _gameRepository.LoadGameEntityById(GameId);
        if (gameEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Game entity not found.");
            return Page();
        }

        var configEntity = _configRepository.GetConfigurationById(gameEntity.ConfigId);
        if (configEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Configuration not found.");
            return Page();
        }

        GameConfiguration = new GameConfiguration
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
            
        var brain = new TicTacTwoBrain(GameConfiguration, EGamePiece.X);
        brain.SetGameStateJson(jsonState);
        if (brain.CheckVictory())
        {
            var currentPlayerRole = HttpContext.Session.GetString("PlayerRole") ?? string.Empty;
            Message = currentPlayerRole + " won! This game is finished. Please reset or create a new game.";
            IsVictory = true;
            return Page();
        }

        var piecesNeeded = brain.PiecesNeededBeforeAdditionalActions();
        var xPlaced = GameConfiguration.PiecesPerPlayer - brain.GetRemainingPieces(EGamePiece.X);
        var oPlaced = GameConfiguration.PiecesPerPlayer - brain.GetRemainingPieces(EGamePiece.O);
        if (xPlaced < piecesNeeded || oPlaced < piecesNeeded)
        {
            ModelState.AddModelError(string.Empty, "Special moves are not available yet.");
            return Page();
        }

        return RedirectToPage("./PlayGame", new { GameId, PlayerRole, mode = "MoveGrid" });
    }

    public IActionResult OnPostMoveGrid(string direction)
    {
        var (jsonState, gameName) = _gameRepository.LoadGameById(GameId);
        if (string.IsNullOrEmpty(jsonState))
        {
            ModelState.AddModelError(string.Empty, "Game not found.");
            return Page();
        }

        var gameEntity = _gameRepository.LoadGameEntityById(GameId);
        if (gameEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Game entity not found.");
            return Page();
        }

        var configEntity = _configRepository.GetConfigurationById(gameEntity.ConfigId);
        if (configEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Configuration not found.");
            return Page();
        }

        GameConfiguration = new GameConfiguration
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
            
        var brain = new TicTacTwoBrain(GameConfiguration, EGamePiece.X);
        brain.SetGameStateJson(jsonState);
        if (brain.CheckVictory())
        {
            var currentPlayerRole = HttpContext.Session.GetString("PlayerRole") ?? string.Empty;
            Message = currentPlayerRole + " won! This game is finished. Please reset or create a new game.";
            IsVictory = true;
            return Page();
        }
        
        var currentPlayer = brain.GetCurrentTurn();
        if (!MultiplayerTurnCheck(currentPlayer))
        {
            return Page();
        }

        var piecesNeeded = brain.PiecesNeededBeforeAdditionalActions();
        var xPlaced = GameConfiguration.PiecesPerPlayer - brain.GetRemainingPieces(EGamePiece.X);
        var oPlaced = GameConfiguration.PiecesPerPlayer - brain.GetRemainingPieces(EGamePiece.O);
        if (xPlaced < piecesNeeded || oPlaced < piecesNeeded)
        {
            ModelState.AddModelError(string.Empty, "Special moves are not available yet.");
            return Page();
        }

        var moved = brain.MoveGrid(direction);
        if (!moved)
        {
            ModelState.AddModelError(string.Empty, "Invalid grid movement direction.");
            return Page();
        }

        var victory = brain.CheckVictory();
        UpdateRepositoryAndSetMessage(gameName, brain, victory, brain.GetCurrentTurn(), gameEntity);

        return RedirectToPage("./PlayGame", new { GameId, PlayerRole, gridMoved = true });
    }

    public IActionResult OnPostMovePiece(int sourceX, int sourceY, int destX, int destY, string mode)
    {
        if (mode != "MovePiece" || GameId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Invalid request.");
            return Page();
        }

        var (jsonState, loadedGameName) = _gameRepository.LoadGameById(GameId);
        if (string.IsNullOrEmpty(jsonState))
        {
            ModelState.AddModelError(string.Empty, "Game not found.");
            return Page();
        }

        var gameEntity = _gameRepository.LoadGameEntityById(GameId);
        if (gameEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Game entity not found.");
            return Page();
        }

        var configEntity = _configRepository.GetConfigurationById(gameEntity.ConfigId);
        if (configEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Configuration not found.");
            return Page();
        }

        GameConfiguration = new GameConfiguration
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

        var brain = new TicTacTwoBrain(GameConfiguration, EGamePiece.X);
        brain.SetGameStateJson(jsonState);
        
        if (!IsCellInsideBoard(sourceX, sourceY, brain) || !IsCellInsideBoard(destX, destY, brain))
        {
            ModelState.AddModelError(string.Empty, "Invalid move: coordinates out of bounds.");
            return Page();
        }

        var currentPlayer = brain.GetCurrentTurn();
        
        if (brain.GameBoard[sourceX][sourceY] != currentPlayer)
        {
            ModelState.AddModelError(string.Empty, "Source cell does not contain your piece.");
            return Page();
        }
        
        if (brain.GameBoard[destX][destY] != EGamePiece.Empty)
        {
            ModelState.AddModelError(string.Empty, "Destination cell is not empty.");
            return Page();
        }
        
        brain.MovePiece(sourceX, sourceY, destX, destY);
        brain.GameBoard[sourceX][sourceY] = EGamePiece.Empty;
        
        var victory = brain.CheckVictory();
        UpdateRepositoryAndSetMessage(loadedGameName, brain, victory, currentPlayer, gameEntity);

        return RedirectToPage("./PlayGame", new { GameId, PlayerRole, mode = "MovePiece" });
    }

    public IActionResult OnPostResetGame()
    {
        var game = _gameRepository.LoadGameEntityById(GameId);
        if (game == null)
        {
            ModelState.AddModelError(string.Empty, "Game not found.");
            return Page();
        }
        var configEntity = _configRepository.GetConfigurationById(game.ConfigId);
        if (configEntity == null)
        {
            ModelState.AddModelError(string.Empty, "Configuration not found.");
            return Page();
        }
        GameConfiguration = new GameConfiguration
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
        var brain = new TicTacTwoBrain(GameConfiguration, EGamePiece.X);
        brain.SetGameStateJson(game.GameStateJson);
        brain.ResetGame();
        var updatedJson = brain.GetGameStateAsJson();
        if (updatedJson.Length > 20000)
        {
            ModelState.AddModelError(string.Empty, "Game state is too large to save.");
            return Page();
        }
        game.GameStateJson = updatedJson;
        game.ModifiedAt = DateTime.Now;
        _gameRepository.UpdateGame(game);
        Message = "Game has been reset successfully.";
        PlacePieceActive = false;
        IsMovePieceActive = false;
        ShowMoveGridOptions = false;
        IsSpecialMoveAllowed = false;
        IsVictory = false;

        return RedirectToPage("./PlayGame", new { GameId, PlayerRole });
    }

    public IActionResult OnPostAiMove()
    {
        var gameEntity = _gameRepository.LoadGameEntityById(GameId);
        if (gameEntity == null)
        {
            ModelState.AddModelError("", "Game not found.");
            return Page();
        }

        var configEntity = _configRepository.GetConfigurationById(gameEntity.ConfigId);
        if (configEntity == null)
        {
            ModelState.AddModelError("", "Configuration not found.");
            return Page();
        }

        GameConfiguration = new GameConfiguration
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
        
        var brain = new TicTacTwoBrain(GameConfiguration, EGamePiece.X);
        brain.SetGameStateJson(gameEntity.GameStateJson);
        
        if (brain.CheckVictory())
        {
            Message = "This game is already finished. Please reset or create a new game.";
            return RedirectToPage("./PlayGame", new { GameId, PlayerRole });
        }
        
        if (brain.GameType == GameType.PlayerVsAi)
        {
            if (brain.GetCurrentTurn() == EGamePiece.O)
            {
                var gridMgr = new GridManager(
                    GameConfiguration.GridSizeWidth,
                    GameConfiguration.GridSizeHeight,
                    GameConfiguration.BoardSizeWidth,
                    GameConfiguration.BoardSizeHeight);

                var ai = new TicTacTwoAi(gridMgr);
                ai.MakeAiDecision(brain);

                var isVictory = brain.CheckVictory();
                UpdateRepositoryAndSetMessage(gameEntity.GameName, brain, isVictory, EGamePiece.O, gameEntity);
            }
            else
            {
                Message = "It's not the AI's turn. No move performed.";
            }
        }
        else if (brain.GameType == GameType.AiVsAi)
        {
            var gridMgr = new GridManager(
                GameConfiguration.GridSizeWidth,
                GameConfiguration.GridSizeHeight,
                GameConfiguration.BoardSizeWidth,
                GameConfiguration.BoardSizeHeight);

            var ai = new TicTacTwoAi(gridMgr);
            
            var currentPlayer = brain.GetCurrentTurn();
            ai.MakeAiDecision(brain);

            var isVictory = brain.CheckVictory();
            UpdateRepositoryAndSetMessage(gameEntity.GameName, brain, isVictory, currentPlayer, gameEntity);
        }
        else
        {
            Message = "AI move is not applicable for this game type.";
        }

        return RedirectToPage("./PlayGame", new { GameId, PlayerRole });
    }

    private bool CheckVictoryOrRole(TicTacTwoBrain brain)
    {
        if (brain.CheckVictory())
        {
            var winner = brain.GetCurrentTurn() == EGamePiece.X ? "O" : "X";
            Message = $"{winner} wins! This game is finished. Please reset or create a new game.";
            return false;
        }
        return true;
    }

    private bool IsNewGame(TicTacTwoBrain brain)
    {
        return brain.GetRemainingPieces(EGamePiece.X) == brain.GetGameConfiguration().PiecesPerPlayer
            && brain.GetRemainingPieces(EGamePiece.O) == brain.GetGameConfiguration().PiecesPerPlayer;
    }

    private void CenterGrid(TicTacTwoBrain brain, string gameName)
    {
        var config = brain.GetGameConfiguration();
        var row = (config.BoardSizeHeight - config.GridSizeHeight) / 2;
        var col = (config.BoardSizeWidth - config.GridSizeWidth) / 2;
        brain.SetGridPosition(row, col);

        var updated = brain.GetGameStateAsJson();
        if (updated.Length > 20000)
        {
            ModelState.AddModelError(string.Empty, "Game state is too large to save.");
            return;
        }

        var game = _gameRepository.LoadGameEntityById(GameId);
        if (game != null)
        {
            game.GameStateJson = updated;
            game.ModifiedAt = DateTime.Now;
            _gameRepository.UpdateGame(game);
        }
    }

    private bool IsCellInsideBoard(int x, int y, TicTacTwoBrain brain)
    {
        if (x < 0 || x >= brain.DimX) return false;
        if (y < 0 || y >= brain.DimY) return false;
        return true;
    }

    private bool MultiplayerTurnCheck(EGamePiece currentPlayer)
    {
        if (string.IsNullOrEmpty(PlayerRole) || (PlayerRole != "X" && PlayerRole != "O"))
        {
            ModelState.AddModelError("", "You must choose a valid player role (X or O).");
            return false;
        }
        if ((PlayerRole == "X" && currentPlayer != EGamePiece.X) ||
            (PlayerRole == "O" && currentPlayer != EGamePiece.O))
        {
            ModelState.AddModelError("", "It's not your turn.");
            return false;
        }
        return true;
    }

    private void UpdateRepositoryAndSetMessage(string gameName, TicTacTwoBrain brain, bool isVictory, EGamePiece currentPlayer, Game gameEntity)
    {
        var updatedJson = brain.GetGameStateAsJson();
        if (updatedJson.Length > 20000)
        {
            ModelState.AddModelError(string.Empty, "Game state is too large to save.");
            return;
        }
        
        gameEntity.GameStateJson = updatedJson;
        gameEntity.ModifiedAt = DateTime.Now;
        _gameRepository.UpdateGame(gameEntity);

        if (isVictory)
        {
            var winner = currentPlayer == EGamePiece.X ? "Player X" : "Player O";
            Message = $"{winner} wins! This game is finished. Please reset or create a new game.";
        }
        else
        {
            brain.NextMoveBy = currentPlayer == EGamePiece.X ? EGamePiece.O : EGamePiece.X;

            var updatedJsonWithTurnSwitch = brain.GetGameStateAsJson();
            gameEntity.GameStateJson = updatedJsonWithTurnSwitch;
            gameEntity.ModifiedAt = DateTime.Now;
            _gameRepository.UpdateGame(gameEntity);

            Message = "Move made. Turn switched.";
        }
    }
}
