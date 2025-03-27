namespace GameBrain;

public record struct GameConfiguration()
{
    public string Name { get; set; } = default!;
    public int BoardSizeWidth { get; set; } = 3;
    public int BoardSizeHeight { get; set; } = 3;
    public int GridSizeWidth { get; set; } = 3;
    public int GridSizeHeight { get; set; } = 3;
    public int WinCondition { get; set; } = 3;
    public int PiecesPerPlayer { get; set; } = 4;
    public int MovePieceAfterNMoves { get; set; } = 0; // 0 - disabled

    public override string ToString() =>
        $"Board {BoardSizeWidth}x{BoardSizeHeight}, " +
        $"Grid {GridSizeWidth}x{GridSizeHeight}, " +
        $"pieces in a line needed to win: {WinCondition}, " +
        $"can move piece after {MovePieceAfterNMoves} moves";
}
