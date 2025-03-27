using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameBrain;

public class TicTacTwoBrain
{
    private EGamePiece[][] _gameBoard;
    public EGamePiece NextMoveBy { get; set; }
    private GameConfiguration _gameConfiguration;
    private GridManager _gridManager;
    private Dictionary<EGamePiece, int> _remainingPieces;
    public GameType GameType { get; set; }
    private TicTacTwoAi? _ai;
    
    public string PasswordX { get; set; }
    public string? PasswordO { get; set; }
    
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    public TicTacTwoBrain(GameConfiguration gameConfiguration, EGamePiece startingPlayer)
    {
        _gameConfiguration = gameConfiguration;
        _gameBoard = InitializeEmptyBoard();
        
        _gridManager = new GridManager(_gameConfiguration.GridSizeWidth, _gameConfiguration.GridSizeHeight, 
            _gameConfiguration.BoardSizeWidth, _gameConfiguration.BoardSizeHeight);
        
        NextMoveBy = startingPlayer;

        _remainingPieces = new Dictionary<EGamePiece, int>
        {
            { EGamePiece.X, _gameConfiguration.PiecesPerPlayer },
            { EGamePiece.O, _gameConfiguration.PiecesPerPlayer }
        };
    }
    
    private EGamePiece[][] InitializeEmptyBoard()
    {
        var board = new EGamePiece[_gameConfiguration.BoardSizeWidth][];
        for (int x = 0; x < _gameConfiguration.BoardSizeWidth; x++)
        {
            board[x] = new EGamePiece[_gameConfiguration.BoardSizeHeight];
            for (int y = 0; y < _gameConfiguration.BoardSizeHeight; y++)
            {
                board[x][y] = EGamePiece.Empty;
            }
        }
        return board;
    }
    
    public string GetGameStateAsJson()
    {
        var serializableState = new GameState
        {
            GameConfiguration = _gameConfiguration,
            GameBoard = _gameBoard,
            NextMoveBy = NextMoveBy,
            RemainingPiecesX = _remainingPieces[EGamePiece.X],
            RemainingPiecesO = _remainingPieces[EGamePiece.O],
            GridRow = _gridManager.GridPosition.row,
            GridCol = _gridManager.GridPosition.col,
            GameType = GameType
        };

        return JsonSerializer.Serialize(serializableState, JsonOptions);
    }
    
    public void SetGameStateJson(string dbGameGameState)
    {
        var gameState = JsonSerializer.Deserialize<GameState>(dbGameGameState, JsonOptions);
        if (gameState == null)
        {
            throw new JsonException("Failed to deserialize game state.");
        }
        _gameConfiguration = gameState.GameConfiguration;
        _gameBoard = gameState.GameBoard;
        NextMoveBy = gameState.NextMoveBy;
        _remainingPieces[EGamePiece.X] = gameState.RemainingPiecesX;
        _remainingPieces[EGamePiece.O] = gameState.RemainingPiecesO;
        _gridManager.SetGridPosition(gameState.GridRow, gameState.GridCol);
        GameType = gameState.GameType;
        
        if (GameType == GameType.PlayerVsAi && _ai == null)
        {
            _ai = new TicTacTwoAi(_gridManager);
        }
    }
    
    public void SetRemainingPieces(EGamePiece player, int count)
    {
        if (_remainingPieces.ContainsKey(player))
        {
            _remainingPieces[player] = count;
        }
    }

    public GameConfiguration GetGameConfiguration()
    {
        return _gameConfiguration;
    }

    public void SetGridPosition(int row, int col)
    {
        _gridManager.SetGridPosition(row, col);
    }
    
    public (int row, int col) GetGridPosition()
    {
        return _gridManager.GridPosition;
    }
    
    public (int width, int height) GetGridSize()
    {
        return (_gridManager.GridSizeWidth, _gridManager.GridSizeHeight);
    }
    
    public EGamePiece[][] GameBoard
    {
        get => _gameBoard;
        set => _gameBoard = value;
    }

    public int DimX => _gameBoard.Length;
    public int DimY => _gameBoard.Length > 0 ? _gameBoard[0].Length : 0;
    public EGamePiece GetCurrentTurn() => NextMoveBy;
    public int GetRemainingPieces(EGamePiece player) => _remainingPieces[player];
    public int GetTotalPiecesPerPlayer() => _gameConfiguration.PiecesPerPlayer;
    public int PiecesNeededBeforeAdditionalActions() => _gameConfiguration.GridSizeWidth - 1;

    public bool MakeAMove(int x, int y)
    {
        if (_gameBoard[x][y] != EGamePiece.Empty)
        {
            return false;
        }

        if (_remainingPieces[NextMoveBy] == 0)
        {
            return false;
        }

        _gameBoard[x][y] = NextMoveBy;
        _remainingPieces[NextMoveBy]--;

        return true;
    }
    
    public bool MoveGrid(string direction)
    {
        var moved = _gridManager.MoveGrid(direction);
        
        return moved;
    }

    public bool CheckVictory()
    {
        var winCondition = Math.Min(_gridManager.GridSizeWidth, _gridManager.GridSizeHeight);

        return _gridManager.CheckGridVictory(_gameBoard, winCondition, NextMoveBy);
    }
    
    public bool CheckVictoryForPlayer(EGamePiece player)
    {
        return _gridManager.CheckGridVictory(_gameBoard, _gameConfiguration.WinCondition, player);
    }
    
    public bool MovePiece(int oldX, int oldY, int newX, int newY)
    {
        if (_gameBoard[oldX][oldY] != NextMoveBy)
        {
            Console.WriteLine("You can only move your own pieces.");
            return false;
        }

        if (newX < 0 || newX >= _gameConfiguration.BoardSizeWidth || 
            newY < 0 || newY >= _gameConfiguration.BoardSizeHeight ||
            _gameBoard[newX][newY] != EGamePiece.Empty)
        {
            Console.WriteLine("Invalid move! The new position must be within the board and empty.");
            return false;
        }
    
        _gameBoard[oldX][oldY] = EGamePiece.Empty;
        _gameBoard[newX][newY] = NextMoveBy;
        
        return true;
    }

    public void ResetGame()
    {
        for (var x = 0; x < _gameConfiguration.BoardSizeWidth; x++)
        {
            for (var y = 0; y < _gameConfiguration.BoardSizeHeight; y++)
            {
                _gameBoard[x][y] = EGamePiece.Empty;
            }
        }
        NextMoveBy = EGamePiece.X;
        _remainingPieces[EGamePiece.X] = _gameConfiguration.PiecesPerPlayer;
        _remainingPieces[EGamePiece.O] = _gameConfiguration.PiecesPerPlayer;
    }
    
}
