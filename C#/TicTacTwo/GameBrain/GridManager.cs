namespace GameBrain;

public class GridManager
{
    public int GridSizeWidth { get; set; }
    public int GridSizeHeight { get; set; }
    public (int row, int col) GridPosition { get; private set; }

    public int BoardSizeWidth { get; set; }
    public int BoardSizeHeight { get; set; }

    public GridManager(int gridSizeWidth, int gridSizeHeight, 
        int boardSizeWidth, int boardSizeHeight, (int row, int col)? initialPosition = null)
    {
        GridSizeWidth = gridSizeWidth;
        GridSizeHeight = gridSizeHeight;
        BoardSizeWidth = boardSizeWidth;
        BoardSizeHeight = boardSizeHeight;
        
        GridPosition = initialPosition.HasValue ? initialPosition.Value 
            : (boardSizeWidth / 2 - gridSizeWidth / 2, boardSizeHeight / 2 - gridSizeHeight / 2);
    }

    public bool CanMoveGrid(string direction)
    {
        var moves = new Dictionary<string, (int rowDelta, int colDelta)>
        {
            { "up", (-1, 0) }, { "down", (1, 0) }, { "left", (0, -1) }, { "right", (0, 1) },
            { "upleft", (-1, -1) }, { "upright", (-1, 1) }, { "downleft", (1, -1) }, { "downright", (1, 1) }
        };
        if (!moves.ContainsKey(direction.ToLower()))
        {
            return false;
        }

        var (rowDelta, colDelta) = moves[direction];
        var newRow = GridPosition.row + rowDelta;
        var newCol = GridPosition.col + colDelta;
        
        if (newRow >= 0 && newRow + GridSizeHeight <= BoardSizeHeight &&
            newCol >= 0 && newCol + GridSizeWidth <= BoardSizeWidth)
        {
            return true;
        }
        return false;
    }

    public bool MoveGrid(string direction)
    {
        if (!CanMoveGrid(direction))
        {
            return false;
        }

        var moves = new Dictionary<string, (int rowDelta, int colDelta)>
        {
            { "up", (-1, 0) }, { "down", (1, 0) }, { "left", (0, -1) }, { "right", (0, 1) },
            { "upleft", (-1, -1) }, { "upright", (-1, 1) }, { "downleft", (1, -1) }, { "downright", (1, 1) }
        };
        var (rowDelta, colDelta) = moves[direction];
        GridPosition = (GridPosition.row + rowDelta, GridPosition.col + colDelta);
        return true;
    }
    
    public bool CheckGridVictory(EGamePiece[][] gameBoard, int winCondition, EGamePiece player)
    {
        var (gridRow, gridCol) = GridPosition;

        for (var y = gridRow; y < gridRow + GridSizeHeight; y++)
        {
            for (var x = gridCol; x < gridCol + GridSizeWidth; x++)
            {
                if (gameBoard[x][y] == player)
                {
                    if (CheckLine(gameBoard, x, y, 1, 0, winCondition, player, gridRow, gridCol) ||
                        CheckLine(gameBoard, x, y, 0, 1, winCondition, player, gridRow, gridCol) ||
                        CheckLine(gameBoard, x, y, 1, 1, winCondition, player, gridRow, gridCol) ||
                        CheckLine(gameBoard, x, y, 1, -1, winCondition, player, gridRow, gridCol))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CheckLine(EGamePiece[][] gameBoard, int startX, int startY,
        int xDelta, int yDelta,
        int winCondition,
        EGamePiece player,
        int gridRow, int gridCol)
    {
        var count = 0;
        var currentX = startX;
        var currentY = startY;

        while (currentX >= gridCol && currentX < gridCol + GridSizeWidth &&
               currentY >= gridRow && currentY < gridRow + GridSizeHeight)
        {
            if (gameBoard[currentX][currentY] == player)
            {
                count++;
                if (count == winCondition) return true;
            }
            else
            {
                break;
            }

            currentX += xDelta;
            currentY += yDelta;
        }
        return false;
    }
    
    public void SetGridPosition(int row, int col)
    {
        GridPosition = (row, col);
    }
}