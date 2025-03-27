using GameBrain;

namespace ConsoleApp;

public static class OptionsController
{
    private static EGamePiece _startingPlayer = EGamePiece.X;
    
    public static void SetStartingPlayer(EGamePiece player)
    {
        _startingPlayer = player;
    }
    
    public static EGamePiece GetStartingPlayer()
    {
        return _startingPlayer;
    }
}