namespace GameBrain;

public class TicTacTwoAi
{
    private readonly Random _random = new Random();
    private readonly GridManager _gridManager;

    public TicTacTwoAi(GridManager gridManager)
    {
        _gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
    }
        
    public void MakeAiDecision(TicTacTwoBrain brain)
    {
        var moveType = DecideMove(brain);

        switch (moveType)
        {
            case "PlacePiece":
                var placeMove = GetAiMove(brain.GameBoard, EGamePiece.O, EGamePiece.X, brain);
                if (placeMove.HasValue)
                {
                    brain.MakeAMove(placeMove.Value.x, placeMove.Value.y);
                }
                break;
            case "MovePiece":
                var pieceMove = GetMovePieceCoordinates(brain.GameBoard, EGamePiece.O);
                if (pieceMove.HasValue)
                {
                    brain.MovePiece(pieceMove.Value.oldX, pieceMove.Value.oldY, pieceMove.Value.newX, pieceMove.Value.newY);
                }
                break;
            case "MoveGrid":
                var gridDirection = GetValidGridDirection(brain);
                if (gridDirection != null)
                {
                    brain.MoveGrid(gridDirection);
                }
                break;
            case "None":
                Console.WriteLine("AI has no valid moves.");
                break;
        }
    }
    
    public string DecideMove(TicTacTwoBrain brain)
    {
        var options = new List<string> { "PlacePiece" };

        if (brain.GetRemainingPieces(EGamePiece.O) <= 0)
        {
            options.Remove("PlacePiece");
        }

        var isSpecialAllowed = brain.GetRemainingPieces(EGamePiece.O) <= brain.PiecesNeededBeforeAdditionalActions();
        if (isSpecialAllowed)
        {
            options.Add("MovePiece");
            options.Add("MoveGrid");
        }

        return options.Count > 0 ? options[_random.Next(options.Count)] : "None";
    }
    
    public (int x, int y)? GetAiMove(EGamePiece[][] board, EGamePiece aiPiece, EGamePiece opponentPiece, TicTacTwoBrain brain)
    {
        var move = GetWinningOrBlockingMove(board, aiPiece, opponentPiece, brain);
        if (move.HasValue) return move;

        return GetRandomMove(board);
    }
    
    private string? GetValidGridDirection(TicTacTwoBrain brain)
    {
        var directions = new[] { "up", "down", "left", "right", "upleft", "upright", "downleft", "downright" };
        var validDirections = directions
            .Where(direction => _gridManager != null && _gridManager.CanMoveGrid(direction))
            .ToList();

        return validDirections.Count > 0 ? validDirections[_random.Next(validDirections.Count)] : null;
    }
    
    private (int oldX, int oldY, int newX, int newY)? GetMovePieceCoordinates(EGamePiece[][] board, EGamePiece aiPiece)
    {
        var ownPieces = new List<(int x, int y)>();
        var emptySpaces = new List<(int x, int y)>();

        for (int x = 0; x < board.Length; x++)
        {
            for (int y = 0; y < board[x].Length; y++)
            {
                if (board[x][y] == aiPiece) ownPieces.Add((x, y));
                else if (board[x][y] == EGamePiece.Empty) emptySpaces.Add((x, y));
            }
        }

        if (ownPieces.Count == 0 || emptySpaces.Count == 0) return null;

        var pieceToMove = ownPieces[_random.Next(ownPieces.Count)];
        var targetSpace = emptySpaces[_random.Next(emptySpaces.Count)];

        return (pieceToMove.x, pieceToMove.y, targetSpace.x, targetSpace.y);
    }
    
    private (int x, int y)? GetRandomMove(EGamePiece[][] board)
    {
        var validMoves = new List<(int x, int y)>();
        for (int x = 0; x < board.Length; x++)
        {
            for (int y = 0; y < board[x].Length; y++)
            {
                if (board[x][y] == EGamePiece.Empty) validMoves.Add((x, y));
            }
        }

        return validMoves.Count > 0 ? validMoves[_random.Next(validMoves.Count)] : null;
    }
    
    private (int x, int y)? GetWinningOrBlockingMove(EGamePiece[][] board, EGamePiece aiPiece, EGamePiece opponentPiece, TicTacTwoBrain brain)
    {
        var winningMove = FindMoveThatWins(board, aiPiece, brain);
        if (winningMove.HasValue) return winningMove;

        var blockingMove = FindMoveThatWins(board, opponentPiece, brain);
        if (blockingMove.HasValue) return blockingMove;

        return null;
    }

    private (int x, int y)? FindMoveThatWins(EGamePiece[][] board, EGamePiece piece, TicTacTwoBrain brain)
    {

        for (int x = 0; x < board.Length; x++)
        {
            for (int y = 0; y < board[x].Length; y++)
            {
                if (board[x][y] != EGamePiece.Empty) continue;

                board[x][y] = piece;
                var isWin = brain.CheckVictoryForPlayer(piece);
                board[x][y] = EGamePiece.Empty;

                if (isWin) return (x, y);
            }
        }

        return null;
    }
}