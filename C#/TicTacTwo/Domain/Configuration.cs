using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Configuration : BaseEntity
{
    [MaxLength(128)]
    public string ConfigName { get; set; } = default!;
    public int BoardSizeWidth { get; set; }
    public int BoardSizeHeight { get; set; }
    public int GridSizeWidth { get; set; }
    public int GridSizeHeight { get; set; }
    public int WinCondition { get; set; }
    public int PiecesPerPlayer { get; set; }
    public int MovePieceAfterNMoves { get; set; }
    public ICollection<Game>? Games { get; set; }
}