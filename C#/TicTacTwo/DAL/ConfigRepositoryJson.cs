using System.Text.Json;
using System.Text.Json.Serialization;
using Domain;
using GameBrain;

namespace DAL;

public class ConfigRepositoryJson : IConfigRepository
{
    private const string ConfigsFileName = "Configurations.json";
    private readonly string _configsFilePath;
    private readonly List<Configuration> _configs;

    public ConfigRepositoryJson()
    {
        _configsFilePath = Path.Combine(FileHelper.BasePath, ConfigsFileName);
        if (File.Exists(_configsFilePath))
        {
            var json = File.ReadAllText(_configsFilePath);
            _configs = JsonSerializer.Deserialize<List<Configuration>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            }) ?? new List<Configuration>();
        }
        else
        {
            _configs = new List<Configuration>();
            SaveConfigs();
        }
    }

    public List<string> GetConfigurationNames()
    {
        return _configs.Select(c => c.ConfigName).ToList();
    }

    public Configuration GetConfigurationByName(string name)
    {
        var config = _configs.FirstOrDefault(c => c.ConfigName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (config == null)
            throw new FileNotFoundException($"Configuration '{name}' not found.");

        return new Configuration
        {
            Id = config.Id,
            ConfigName = config.ConfigName,
            BoardSizeWidth = config.BoardSizeWidth,
            BoardSizeHeight = config.BoardSizeHeight,
            GridSizeWidth = config.GridSizeWidth,
            GridSizeHeight = config.GridSizeHeight,
            WinCondition = config.WinCondition,
            PiecesPerPlayer = config.PiecesPerPlayer,
            MovePieceAfterNMoves = config.MovePieceAfterNMoves,
            CreatedAt = config.CreatedAt,
            ModifiedAt = config.ModifiedAt
        };
    }

    public void SaveConfiguration(GameConfiguration config)
    {
        var existing = _configs.FirstOrDefault(c => c.ConfigName.Equals(config.Name, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.BoardSizeWidth = config.BoardSizeWidth;
            existing.BoardSizeHeight = config.BoardSizeHeight;
            existing.GridSizeWidth = config.GridSizeWidth;
            existing.GridSizeHeight = config.GridSizeHeight;
            existing.WinCondition = config.WinCondition;
            existing.PiecesPerPlayer = config.PiecesPerPlayer;
            existing.MovePieceAfterNMoves = config.MovePieceAfterNMoves;
            existing.ModifiedAt = DateTime.Now;
        }
        else
        {
            var newConfig = new Configuration
            {
                Id = _configs.Any() ? _configs.Max(c => c.Id) + 1 : 1,
                ConfigName = config.Name,
                BoardSizeWidth = config.BoardSizeWidth,
                BoardSizeHeight = config.BoardSizeHeight,
                GridSizeWidth = config.GridSizeWidth,
                GridSizeHeight = config.GridSizeHeight,
                WinCondition = config.WinCondition,
                PiecesPerPlayer = config.PiecesPerPlayer,
                MovePieceAfterNMoves = config.MovePieceAfterNMoves,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
            _configs.Add(newConfig);
        }
        SaveConfigs();
    }

    public void DeleteConfiguration(string name)
    {
        var config = _configs.FirstOrDefault(c => c.ConfigName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (config != null)
        {
            _configs.Remove(config);
            SaveConfigs();
        }
        else
        {
            throw new FileNotFoundException($"Configuration '{name}' not found.");
        }
    }

    public List<Configuration> GetAllConfigurations()
    {
        return _configs.ToList();
    }

    public Configuration? GetConfigurationById(int id)
    {
        return _configs.FirstOrDefault(c => c.Id == id);
    }

    public bool ConfigurationExists(int? configId)
    {
        if (configId == null)
            return false;
        return _configs.Any(c => c.Id == configId.Value);
    }

    public void SaveConfiguration(Configuration config)
    {
        var existing = _configs.FirstOrDefault(c => c.Id == config.Id);
        if (existing != null)
        {
            existing.ConfigName = config.ConfigName;
            existing.BoardSizeWidth = config.BoardSizeWidth;
            existing.BoardSizeHeight = config.BoardSizeHeight;
            existing.GridSizeWidth = config.GridSizeWidth;
            existing.GridSizeHeight = config.GridSizeHeight;
            existing.WinCondition = config.WinCondition;
            existing.PiecesPerPlayer = config.PiecesPerPlayer;
            existing.MovePieceAfterNMoves = config.MovePieceAfterNMoves;
            existing.ModifiedAt = DateTime.Now;
        }
        else
        {
            config.Id = _configs.Any() ? _configs.Max(c => c.Id) + 1 : 1;
            config.CreatedAt = DateTime.Now;
            config.ModifiedAt = DateTime.Now;
            _configs.Add(config);
        }
        SaveConfigs();
    }

    private void SaveConfigs()
    {
        var json = JsonSerializer.Serialize(_configs, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });
        File.WriteAllText(_configsFilePath, json);
    }
}