using Domain;
using GameBrain;

namespace DAL;

public class ConfigRepositoryDb : IConfigRepository
{
    private readonly AppDbContext _context;

    public ConfigRepositoryDb(AppDbContext context)
    {
        _context = context;
    }

    public List<string> GetConfigurationNames()
    {
        return _context.Configurations
                       .Select(c => c.ConfigName)
                       .ToList();
    }

    public Configuration GetConfigurationByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Configuration name cannot be null or empty.", nameof(name));

        var nameLower = name.ToLowerInvariant();
        var configEntity = _context.Configurations
            .FirstOrDefault(c => c.ConfigName.ToLower() == nameLower);

        if (configEntity == null)
            throw new InvalidOperationException($"Configuration '{name}' not found.");

        return new Configuration
        {
            Id = configEntity.Id,
            ConfigName = configEntity.ConfigName,
            BoardSizeWidth = configEntity.BoardSizeWidth,
            BoardSizeHeight = configEntity.BoardSizeHeight,
            GridSizeWidth = configEntity.GridSizeWidth,
            GridSizeHeight = configEntity.GridSizeHeight,
            WinCondition = configEntity.WinCondition,
            PiecesPerPlayer = configEntity.PiecesPerPlayer,
            MovePieceAfterNMoves = configEntity.MovePieceAfterNMoves,
            CreatedAt = configEntity.CreatedAt,
            ModifiedAt = configEntity.ModifiedAt
        };
    }

    public void SaveConfiguration(GameConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
            throw new ArgumentException("Configuration name cannot be null or empty.", nameof(config.Name));

        var nameLower = config.Name.ToLowerInvariant();
        
        var existingConfig = _context.Configurations
            .FirstOrDefault(c => c.ConfigName == nameLower);

        if (existingConfig != null)
            throw new InvalidOperationException($"A configuration with the name '{config.Name}' already exists.");

        var newConfig = new Configuration
        {
            ConfigName = nameLower,
            BoardSizeWidth = config.BoardSizeWidth,
            BoardSizeHeight = config.BoardSizeHeight,
            GridSizeWidth = config.GridSizeWidth,
            GridSizeHeight = config.GridSizeHeight,
            WinCondition = config.WinCondition,
            PiecesPerPlayer = config.PiecesPerPlayer,
            MovePieceAfterNMoves = config.MovePieceAfterNMoves
        };

        _context.Configurations.Add(newConfig);
        _context.SaveChanges();
    }

    public void DeleteConfiguration(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Configuration name cannot be null or empty.", nameof(name));
        }

        var config = _context.Configurations
            .FirstOrDefault(c => c.ConfigName.ToLower() == name.ToLowerInvariant()); // Use ToLower for comparison

        if (config == null)
        {
            throw new FileNotFoundException($"Configuration '{name}' not found in the database.");
        }

        _context.Configurations.Remove(config);
        _context.SaveChanges();
    }

    public List<Configuration> GetAllConfigurations()
    {
        return _context.Configurations.ToList();
    }

    public Configuration? GetConfigurationById(int id)
    {
        return _context.Configurations.FirstOrDefault(c => c.Id == id);
    }

    public bool ConfigurationExists(int? configId)
    {
        if (configId == null)
        {
            return false;
        }
        return _context.Configurations.Any(c => c.Id == configId.Value);
    }

    public void SaveConfiguration(Configuration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Configuration cannot be null.");
        }

        var existingConfig = _context.Configurations.FirstOrDefault(c => c.Id == config.Id);
        if (existingConfig != null)
        {
            existingConfig.ConfigName = config.ConfigName;
            existingConfig.BoardSizeWidth = config.BoardSizeWidth;
            existingConfig.BoardSizeHeight = config.BoardSizeHeight;
            existingConfig.GridSizeWidth = config.GridSizeWidth;
            existingConfig.GridSizeHeight = config.GridSizeHeight;
            existingConfig.WinCondition = config.WinCondition;
            existingConfig.PiecesPerPlayer = config.PiecesPerPlayer;
            existingConfig.MovePieceAfterNMoves = config.MovePieceAfterNMoves;
            existingConfig.ModifiedAt = DateTime.Now;

            _context.Configurations.Update(existingConfig);
        }
        else
        {
            config.CreatedAt = DateTime.Now;
            config.ModifiedAt = DateTime.Now;
            _context.Configurations.Add(config);
        }

        _context.SaveChanges();
    }
}