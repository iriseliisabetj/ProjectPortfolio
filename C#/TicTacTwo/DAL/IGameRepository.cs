using Domain;
using GameBrain;

namespace DAL;

public interface IGameRepository
{
    public int SaveGame(string jsonStateString, string gameConfigName, GameType gameType, 
        string playerXPass, string? playerOPass, int configId);
    (string GameStateJson, string GameConfigName, GameType gameType, string playerXPass, string? playerOPass) LoadGame(string saveName);
    (string GameStateJson, string GameConfigName) LoadGameById(int id);
    List<string> GetSavedGames();
    Game? LoadGameEntityById(int gameId);
    List<Game> GetAllGames();
    void DeleteGame(int gameId);
    void UpdateGame(Game game);
}