using GameBrain;

namespace ConsoleUI;

public static class Visualizer
{
    public static void DrawBoard(TicTacTwoBrain gameInstance)
    {
        var gridPosition = gameInstance.GetGridPosition();
        var (gridWidth, gridHeight) = gameInstance.GetGridSize();

        Console.Write("   ");
        
        for (var x = 0; x < gameInstance.DimX; x++)
        {
            Console.Write(x < 10 ? $" {x}  " : $" {x} ");
        }
        
        Console.WriteLine();

        for (var y = 0; y < gameInstance.DimY; y++)
        {
            Console.Write($"{y,3}");

            for (var x = 0; x < gameInstance.DimX; x++)
            {
                if (x >= gridPosition.row && x < gridPosition.row + gridWidth &&
                    y >= gridPosition.col && y < gridPosition.col + gridHeight)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ResetColor();
                }

                Console.Write($" {DrawGamePiece(gameInstance.GameBoard[x][y])} ");
                
                Console.ResetColor();

                if (x == gameInstance.DimX - 1) continue;
                Console.Write("|");
            }

            Console.WriteLine();
            if (y == gameInstance.DimY - 1) continue;

        Console.Write("   ");
        for (var x = 0; x < gameInstance.DimX; x++)
        {
            Console.Write("---");
            if (x != gameInstance.DimX - 1)
            {
                Console.Write("+");
            }
        }
        
        Console.WriteLine();
        }

        Console.ResetColor();
    }

    public static void DisplayGameInfo(TicTacTwoBrain gameInstance)
    {
        Console.WriteLine($"\nCurrent Turn: {gameInstance.GetCurrentTurn()}");
        Console.WriteLine($"Remaining pieces for X: {gameInstance.GetRemainingPieces(EGamePiece.X)}");
        Console.WriteLine($"Remaining pieces for O: {gameInstance.GetRemainingPieces(EGamePiece.O)}");
    }

    
    private static string DrawGamePiece(EGamePiece piece) =>
        piece switch
        {
            EGamePiece.O => "O",
            EGamePiece.X => "X",
            _ => " "
        };
}