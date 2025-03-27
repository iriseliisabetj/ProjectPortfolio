using System.ComponentModel.DataAnnotations;
using GameBrain;

namespace Domain;

public class Game : BaseEntity
{
    [MaxLength(128)]
    public string GameName { get; set; } = default!;
    [MaxLength(20000)]
    public string GameStateJson { get; set; } = default!;
    
    public int ConfigId { get; set; }
    public Configuration? Config { get; set; }
    
    [Required]
    public GameType GameType { get; set; }
    
    [MaxLength(100)]
    public string PlayerXPass { get; set; } = default!;
    [MaxLength(100)]
    public string? PlayerOPass { get; set; }

}