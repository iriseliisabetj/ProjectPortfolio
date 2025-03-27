using System.Text.Json;
using System.Text.Json.Serialization;
using Domain;
using GameBrain;

namespace DAL;

public class GameRepositoryJson : IGameRepository
{
    
    private const string GamesFileName = "Games.json";
    private readonly string _gamesFilePath;
    private readonly List<Game> _games;

    public GameRepositoryJson()
    {
        _gamesFilePath = Path.Combine(FileHelper.BasePath, GamesFileName);

        if (File.Exists(_gamesFilePath))
        {
            var json = File.ReadAllText(_gamesFilePath);
            _games = JsonSerializer.Deserialize<List<Game>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            }) ?? new List<Game>();
        }
        else
        {
            _games = new List<Game>();
            SaveGames();
        }
    }

    public int SaveGame(string jsonStateString, string gameConfigName, GameType gameType, 
        string playerXPass, string? playerOPass, int configId)
    {
        var newGame = new Game
        {
            Id = _games.Count > 0 ? _games[^1].Id + 1 : 1,
            GameName = gameConfigName,
            ConfigId = configId,
            GameType = gameType,
            GameStateJson = jsonStateString,
            PlayerXPass = playerXPass,
            PlayerOPass = playerOPass,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now
        };
        _games.Add(newGame);
        SaveGames();
        return newGame.Id;
    }

    public (string GameStateJson, string GameConfigName, GameType gameType, string playerXPass, string? playerOPass) LoadGame(string saveName)
    {
        var game = _games.FirstOrDefault(g => g.GameName.Equals(saveName, StringComparison.OrdinalIgnoreCase));
        if (game == null)
            throw new FileNotFoundException($"Game '{saveName}' not found.");

        return (game.GameStateJson, game.GameName, game.GameType, game.PlayerXPass, game.PlayerOPass);
    }

    public (string GameStateJson, string GameConfigName) LoadGameById(int id)
    {
        var game = _games.FirstOrDefault(g => g.Id == id);
        if (game == null)
            throw new FileNotFoundException($"Game with ID '{id}' not found.");

        return (game.GameStateJson, game.GameName);
    }

    public List<string> GetSavedGames()
    {
        return _games.Select(g => g.GameName).ToList();
    }

    public Game? LoadGameEntityById(int gameId)
    {
        return _games.FirstOrDefault(g => g.Id == gameId);
    }

    public List<Game> GetAllGames()
    {
        return _games.ToList();
    }

    public void DeleteGame(int gameId)
    {
        var game = _games.FirstOrDefault(g => g.Id == gameId);
        if (game == null)
            throw new FileNotFoundException($"Game with ID '{gameId}' not found.");

        _games.Remove(game);
        SaveGames();
    }

    public void UpdateGame(Game game)
    {
        var existingGame = _games.FirstOrDefault(g => g.Id == game.Id);
        if (existingGame == null)
            throw new FileNotFoundException($"Game with ID '{game.Id}' not found.");
        
        existingGame.GameName = game.GameName;
        existingGame.GameStateJson = game.GameStateJson;
        existingGame.GameType = game.GameType;
        existingGame.PlayerXPass = game.PlayerXPass;
        existingGame.PlayerOPass = game.PlayerOPass;
        existingGame.CreatedAt = game.CreatedAt;
        existingGame.ModifiedAt = game.ModifiedAt;

        SaveGames();
    }

    private void SaveGames()
    {
        var json = JsonSerializer.Serialize(_games, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });
        File.WriteAllText(_gamesFilePath, json);
    }
}