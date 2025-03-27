using GameBrain;

namespace DAL;

public class ConfigRepositoryHardcoded
{
    private readonly Dictionary<int, GameConfiguration> _predefinedConfigurations;
    private readonly Dictionary<string, int> _nameToIdMap;
    
    public ConfigRepositoryHardcoded()
    {
        _predefinedConfigurations = new Dictionary<int, GameConfiguration>
        {
            {
                1, new GameConfiguration
                {
                    Name = "Classic Tic-Tac-Two",
                    BoardSizeWidth = 5,
                    BoardSizeHeight = 5,
                    GridSizeWidth = 3,
                    GridSizeHeight = 3,
                    PiecesPerPlayer = 4,
                    WinCondition = 3,
                    MovePieceAfterNMoves = 2
                }
            },
            {
                2, new GameConfiguration
                {
                    Name = "Tic Tac Toe",
                    BoardSizeWidth = 3,
                    BoardSizeHeight = 3,
                    GridSizeWidth = 3,
                    GridSizeHeight = 3,
                    PiecesPerPlayer = 3,
                    WinCondition = 3,
                    MovePieceAfterNMoves = 2
                }
            }
        };
        
        _nameToIdMap = _predefinedConfigurations.ToDictionary(
            kvp => kvp.Value.Name,
            kvp => kvp.Key
        );
    }

    public List<string> GetConfigurationNames()
    {
        return _predefinedConfigurations.Values.Select(config => config.Name).ToList();
    }

    public GameConfiguration GetConfigurationByName(string name)
    {
        if (_nameToIdMap.TryGetValue(name, out var id) &&
            _predefinedConfigurations.TryGetValue(id, out var config))
        {
            return config;
        }

        throw new KeyNotFoundException($"Predefined configuration '{name}' not found.");
    }

    public int GetConfigurationIdByName(string name)
    {
        if (_nameToIdMap.TryGetValue(name, out var id))
        {
            return id;
        }

        throw new KeyNotFoundException($"Predefined configuration '{name}' not found.");
    }
}