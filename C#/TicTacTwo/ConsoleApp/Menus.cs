using DAL;
using GameBrain;
using MenuSystem;

namespace ConsoleApp;

public static class Menus
{
    private static readonly GameRepositoryJson GameRepository = new GameRepositoryJson();
    
    private static readonly Menu OptionsMenu =
        new Menu(
            EMenuLevel.Secondary,
            "TIC-TAC-TWO Options", [
                new MenuItem()
                {
                    Shortcut = "X",
                    Title = "X Starts",
                    MenuItemAction = () => SetStartingPlayerAndContinue(EGamePiece.X)
                },
                new MenuItem()
                {
                    Shortcut = "O",
                    Title = "O Starts",
                    MenuItemAction = () => SetStartingPlayerAndContinue(EGamePiece.O)
                },
            ]);

    public static Menu MainMenu = new Menu(
        EMenuLevel.Main,
        "TIC-TAC-TWO", [
            new MenuItem()
            {
                Shortcut = "O",
                Title = "Options",
                MenuItemAction = OptionsMenu.Run
            },
            new MenuItem()
            {
                Shortcut = "N",
                Title = "New game",
                MenuItemAction = StartNewGame
            },
            new MenuItem()
            {
                Shortcut = "L",
                Title = "Load Game",
                MenuItemAction = LoadSavedGame
            }
        ]);
    
    private static string SetStartingPlayerAndContinue(EGamePiece player)
    {
        OptionsController.SetStartingPlayer(player);
        Console.Clear();
        Console.WriteLine($"{player} will start the game.");
        Console.WriteLine("Press any key to return to the options...");
        Console.ReadKey();
        
        return "Options";
    }
    
    private static string LoadSavedGame()
    {
        GameController.LoadGame();
    
        if (GameController.GameBrain == null)
        {
            Console.WriteLine("No game loaded. Returning to main menu.");
            Console.ReadKey();
            return "MainMenu";
        }
        
        GameController.MainLoop();

        return "MainMenu";
    }
    
    private static string StartNewGame()
    {
        GameController.GameBrain = null;
        
        return GameController.MainLoop();
    }
}
