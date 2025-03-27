namespace GameBrain;

public class GameState
{
    public GameConfiguration GameConfiguration { get; set; }
    public EGamePiece[][] GameBoard { get; set; } = default!;
    public EGamePiece NextMoveBy { get; set; }
    public int RemainingPiecesX { get; set; }
    public int RemainingPiecesO { get; set; }
    public int GridRow { get; set; }
    public int GridCol { get; set; }
    public bool IsVictory { get; set; }
    public GameType GameType { get; set; }
    public string PasswordX { get; set; }
    public string? PasswordO { get; set; } 
}