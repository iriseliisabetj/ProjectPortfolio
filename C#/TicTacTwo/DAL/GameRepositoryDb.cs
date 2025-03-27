using Domain;
using GameBrain;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class GameRepositoryDb : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepositoryDb(AppDbContext context)
    {
        _context = context;
    }

    public List<string> GetSavedGames()
    {
        return _context.Games
            .Select(g => g.GameName)
            .ToList();
    }

    public Game? LoadGameEntityById(int gameId)
    {
        return _context.Games
            .Include(g => g.Config)
            .FirstOrDefault(g => g.Id == gameId);
    }

    public List<Game> GetAllGames()
    {
        return _context.Games
            .Include(g => g.Config)
            .OrderByDescending(g => g.ModifiedAt)
            .ToList();
    }

    public void DeleteGame(int gameId)
    {
        var game = _context.Games.FirstOrDefault(g => g.Id == gameId);
        if (game == null)
        {
            throw new FileNotFoundException($"Game with ID '{gameId}' not found in the database.");
        }

        _context.Games.Remove(game);
        _context.SaveChanges();
    }

    public void UpdateGame(Game game)
    {
        var existingGame = _context.Games.FirstOrDefault(g => g.Id == game.Id);
        if (existingGame == null)
        {
            throw new FileNotFoundException($"Game with ID '{game.Id}' not found in the database.");
        }
        
        existingGame.GameName = game.GameName;
        existingGame.GameStateJson = game.GameStateJson;
        existingGame.GameType = game.GameType;
        existingGame.PlayerXPass = game.PlayerXPass;
        existingGame.PlayerOPass = game.PlayerOPass;
        existingGame.CreatedAt = game.CreatedAt;
        existingGame.ModifiedAt = game.ModifiedAt;

        _context.Games.Update(existingGame);
        _context.SaveChanges();
    }

    public (string GameStateJson, string GameConfigName, GameType gameType, string playerXPass, string? playerOPass) LoadGame(string name)
    {
        var gameEntity = _context.Games
            .FirstOrDefault(g => EF.Functions.Collate(g.GameName, "NOCASE") == name);

        if (gameEntity == null)
        {
            throw new FileNotFoundException($"Game '{name}' not found in the database.");
        }

        return (gameEntity.GameStateJson, gameEntity.GameName, gameEntity.GameType, gameEntity.PlayerXPass, gameEntity.PlayerOPass);
    }

    public int SaveGame(string gameStateJson, string name, GameType gameType, string playerXPass, string? playerOPass, int configId)
    {
        if (!_context.Configurations.Any(c => c.Id == configId))
        {
            throw new InvalidOperationException($"Configuration with ID '{configId}' does not exist.");
        }

        var normalizedName = name.ToUpper();

        var existingGame = _context.Games
            .FirstOrDefault(g => g.GameName.ToUpper() == normalizedName);

        if (existingGame != null)
        {
            existingGame.ConfigId = configId;
            existingGame.GameStateJson = gameStateJson;
            existingGame.ModifiedAt = DateTime.Now;
            existingGame.PlayerXPass = playerXPass;
            existingGame.PlayerOPass = playerOPass;
            existingGame.GameType = gameType;

            _context.Games.Update(existingGame);
        }
        else
        {
            var gameEntity = new Game
            {
                GameName = name,
                ConfigId = configId,
                GameStateJson = gameStateJson,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                PlayerXPass = playerXPass,
                PlayerOPass = playerOPass,
                GameType = gameType
            };

            _context.Games.Add(gameEntity);
        }

        _context.SaveChanges();
        return existingGame?.Id ?? _context.Games.OrderByDescending(g => g.Id).First().Id;
    }
    
    public (string GameStateJson, string GameConfigName) LoadGameById(int id)
    {
        var gameEntity = _context.Games
            .FirstOrDefault(g => g.Id == id);

        if (gameEntity == null)
        {
            throw new FileNotFoundException($"Game with ID '{id}' not found in the database.");
        }

        return (gameEntity.GameStateJson, gameEntity.GameName);
    }
}