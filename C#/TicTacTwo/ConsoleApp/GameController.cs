using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleUI;
using DAL;
using Domain;
using GameBrain;
using MenuSystem;

namespace ConsoleApp;

public static class GameController
{
    private static readonly ConfigRepositoryHardcoded PredefinedConfigRepository = new ConfigRepositoryHardcoded();
    private static readonly ConfigRepositoryJson CustomConfigRepository = new ConfigRepositoryJson();
    
    private static IGameRepository _gameRepositoryDb;
    private static IConfigRepository _configRepositoryDb;
    
    private static IGameRepository _gameRepositoryJson;
    private static IConfigRepository _configRepositoryJson;
    
    public static TicTacTwoBrain? GameBrain;

    private static int CurrentConfigId { get; set; }
    
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };
    
    public static void SetRepositories(IConfigRepository configRepositoryDb, IGameRepository gameRepositoryDb, 
        IConfigRepository configRepositoryJson, IGameRepository gameRepositoryJson)
    {
        _configRepositoryDb = configRepositoryDb;
        _gameRepositoryDb = gameRepositoryDb;
        _configRepositoryJson = configRepositoryJson;
        _gameRepositoryJson = gameRepositoryJson;
    }

    public static string MainLoop()
    {
        if (GameBrain == null)
        {
            string chosenConfigShortcut = ChooseConfiguration();
            GameConfiguration chosenConfig;

            try
            {
                string configName;
                if (chosenConfigShortcut.StartsWith("custom:"))
                {
                    configName = chosenConfigShortcut.Substring("custom:".Length);
                    chosenConfig = LoadConfiguration(configName, isPredefined: false);
                }
                else if (chosenConfigShortcut.StartsWith("predefined:"))
                {
                    configName = chosenConfigShortcut.Substring("predefined:".Length);
                    chosenConfig = LoadConfiguration(configName, isPredefined: true);
                }
                else if (chosenConfigShortcut == "customize")
                {
                    Console.WriteLine("Initiating configuration customization.");
                    chosenConfig = CustomizeConfiguration();
                    Console.WriteLine("Configuration customized successfully.");
                }
                else if (chosenConfigShortcut == "MainMenu")
                {
                    Console.WriteLine("Returning to main menu.");
                    return "MainMenu";
                }
                else
                {
                    Console.WriteLine("Invalid configuration selection.");
                    Console.WriteLine("Press any key to return to the main menu...");
                    Console.ReadKey();
                    return "MainMenu";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                return "MainMenu";
            }
            
            Console.Clear();
            Console.WriteLine("Configuration loaded successfully. Initializing game...");
            var startingPlayer = OptionsController.GetStartingPlayer();
            
            PromptForGameTypeAndPasswords(out var chosenGameType, out var passX, out var passO);
            
            GameBrain = new TicTacTwoBrain(chosenConfig, startingPlayer)
            {
                GameType = chosenGameType,
                PasswordX = passX,
                PasswordO = passO
            };
        }

        bool gameOver = false;

        do
        {
            Console.Clear();
            Visualizer.DrawBoard(GameBrain);
            Visualizer.DisplayGameInfo(GameBrain);

            bool continueGame = HandlePlayerMove(GameBrain);

            if (!continueGame)
            {
                Console.WriteLine("Exiting game. Returning to main menu...");
                gameOver = true;
            }

        } while (!gameOver);

        return "Game Over";
    }
    
    private static void PromptForGameTypeAndPasswords(out GameType chosenGameType, out string passX, out string? passO)
    {
        passO = null;

        while (true)
        {
            Console.WriteLine("Please select the game type:");
            Console.WriteLine("1) Multiplayer");
            Console.WriteLine("2) Player vs AI");
            Console.WriteLine("3) AI vs AI");
            Console.Write("Enter your choice: ");
            var input = Console.ReadLine()?.Trim();

            if (input == "1")
            {
                chosenGameType = GameType.Multiplayer;
                passX = PromptForPassword("X");
                passO = PromptForPassword("O");
                break;
            }
            if (input == "2")
            {
                chosenGameType = GameType.PlayerVsAi;
                passX = PromptForPassword("X");
                break;
            }
            if (input == "3")
            {
                chosenGameType = GameType.AiVsAi;
                passX = PromptForPassword("X"); 
                break;
            }
            Console.WriteLine("Invalid choice. Please try again.");
        }
    }

    private static string PromptForPassword(string player)
    {
        while (true)
        {
            Console.Write($"Enter a password for player {player}: ");
            var pass = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(pass))
            {
                return pass;
            }
            Console.WriteLine("Password cannot be empty. Please try again.");
        }
    }

    private static bool HandlePlayerMove(TicTacTwoBrain gameInstance)
    {
        var totalPiecesPerPlayer = gameInstance.GetTotalPiecesPerPlayer();
        var remainingPieces = gameInstance.GetRemainingPieces(gameInstance.GetCurrentTurn());
        var piecesPlacedByCurrentPlayer = totalPiecesPerPlayer - remainingPieces;

        var piecesRequired = gameInstance.PiecesNeededBeforeAdditionalActions();

        if (piecesPlacedByCurrentPlayer < piecesRequired)
        {
            Console.WriteLine($"You need to place {piecesRequired} pieces before getting access to other actions.");
            bool continueGame = PlacePiece(gameInstance);
            return continueGame;
        }

        Console.WriteLine("Choose an action: ");
        Console.WriteLine("1) Place a piece");
        Console.WriteLine("2) Move the grid");
        Console.WriteLine("3) Move a piece");
        Console.WriteLine("S) Save the game");
        Console.WriteLine("E) Exit to main menu");
        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine()?.ToUpper();

        switch (choice)
        {
            case "1":
                return PlacePiece(gameInstance);
            case "2":
                return HandleGridMovement(gameInstance);
            case "3":
                var oldCoords = GetCoordinates("Choose coordinates of the piece you would like to move <x,y>: ");
                if (oldCoords == null)
                {
                    return HandlePlayerMove(gameInstance);
                }
                var newCoords = GetCoordinates("Choose coordinates for the new location <x,y>: ");
                if (newCoords == null)
                {
                    return HandlePlayerMove(gameInstance);
                }
                var currentPlayer = gameInstance.GetCurrentTurn();

                if (gameInstance.MovePiece(oldCoords.Value.x, oldCoords.Value.y, newCoords.Value.x, newCoords.Value.y))
                {
                    if (gameInstance.CheckVictoryForPlayer(currentPlayer))
                    {
                        Console.Clear();
                        Visualizer.DrawBoard(gameInstance);
                        Console.WriteLine($"\n{currentPlayer} wins by moving the piece!");
                        Console.WriteLine("\nPress any key to return to the main menu...");
                        Console.ReadKey();
                        return false;
                    }
                    gameInstance.NextMoveBy = (gameInstance.NextMoveBy == EGamePiece.X) ? EGamePiece.O : EGamePiece.X;
                    return true;
                }

                Console.WriteLine("Invalid move. Try again.");
                return true;
            case "S":
                SaveGame(gameInstance, gameInstance.PasswordX, gameInstance.PasswordO);
                Console.WriteLine("Game saved successfully!");
                Console.ReadKey();
                return true;
            case "E":
                bool shouldExit = HandleExitOption(gameInstance);
                return !shouldExit;
            default:
                Console.Clear();
                Visualizer.DrawBoard(gameInstance);
                Console.WriteLine("Invalid choice. Try again.");
                return HandlePlayerMove(gameInstance);
        }
    }

    private static bool PlacePiece(TicTacTwoBrain gameInstance)
    {
        while (true)
        {
            Console.Write("Give me coordinates <x,y>, save (s) or exit to main menu (e): ");
            string input = Console.ReadLine()!;

            if (input.Equals("s", StringComparison.OrdinalIgnoreCase))
            {
                SaveGame(gameInstance, gameInstance.PasswordX, gameInstance.PasswordO);
                Console.WriteLine("Game saved successfully!");
                continue;
            }

            if (input.Equals("e", StringComparison.OrdinalIgnoreCase))
            {
                var shouldExit = HandleExitOption(gameInstance);
                if (shouldExit) return false;
                continue;
            }

            var inputSplit = input.Split(",");
            if (inputSplit.Length != 2
                || !int.TryParse(inputSplit[0], out var inputX)
                || !int.TryParse(inputSplit[1], out var inputY))
            {
                Console.WriteLine("Invalid input format. Please use x,y.");
                continue;
            }

            if (inputX < 0 || inputX >= gameInstance.DimX
                || inputY < 0 || inputY >= gameInstance.DimY)
            {
                Console.WriteLine($"Coordinates out of bounds. The board is {gameInstance.DimX}x{gameInstance.DimY}. Try again.");
                continue;
            }
            
            var currentPlayer = gameInstance.GetCurrentTurn();
            if (gameInstance.MakeAMove(inputX, inputY))
            {
                if (gameInstance.CheckVictoryForPlayer(currentPlayer))
                {
                    Console.Clear();
                    Visualizer.DrawBoard(gameInstance);
                    Console.WriteLine($"\n{currentPlayer} wins!");
                    Console.WriteLine("\nPress any key to return to the main menu...");
                    Console.ReadKey();
                    return false;
                }

                gameInstance.NextMoveBy = (gameInstance.NextMoveBy == EGamePiece.X) ? EGamePiece.O : EGamePiece.X;
                return true;
            }

            Console.WriteLine("Invalid move. Please try again.");
        }
    }

    private static bool HandleGridMovement(TicTacTwoBrain gameInstance)
    {
        while (true)
        {
            Console.Clear();
            Visualizer.DrawBoard(gameInstance);
            Console.WriteLine("Enter the direction to move the grid or press 's' to save or 'e' to exit:");
            Console.WriteLine("1) up");
            Console.WriteLine("2) down");
            Console.WriteLine("3) left");
            Console.WriteLine("4) right");
            Console.WriteLine("5) upleft");
            Console.WriteLine("6) upright");
            Console.WriteLine("7) downleft");
            Console.WriteLine("8) downright");
            Console.Write("Enter your choice: ");
            var choice = Console.ReadKey().KeyChar;
            Console.WriteLine();

            if (char.ToLower(choice) == 's')
            {
                SaveGame(gameInstance, gameInstance.PasswordX, gameInstance.PasswordO);
                Console.WriteLine("Game saved.");
                continue;
            }

            if (char.ToLower(choice) == 'e')
            {
                var shouldExit = HandleExitOption(gameInstance);
                if (shouldExit)
                {
                    return false;
                }
                continue;
            }

            var direction = choice switch
            {
                '1' => "up",
                '2' => "down",
                '3' => "left",
                '4' => "right",
                '5' => "upleft",
                '6' => "upright",
                '7' => "downleft",
                '8' => "downright",
                _ => null
            };

            if (direction != null && gameInstance.MoveGrid(direction))
            {
                Console.Clear();
                Visualizer.DrawBoard(gameInstance);
                Console.WriteLine($"Grid moved {direction}!");

                var currentPlayer = gameInstance.GetCurrentTurn();
                var opponent = (currentPlayer == EGamePiece.X) ? EGamePiece.O : EGamePiece.X;
                
                if (gameInstance.CheckVictoryForPlayer(currentPlayer))
                {
                    Console.Clear();
                    Visualizer.DrawBoard(gameInstance);
                    Console.WriteLine($"{currentPlayer} wins by moving the grid!");
                    Console.WriteLine("\nPress any key to return to the main menu...");
                    Console.ReadKey();
                    return false;
                }
                
                if (gameInstance.CheckVictoryForPlayer(opponent))
                {
                    Console.Clear();
                    Visualizer.DrawBoard(gameInstance);
                    Console.WriteLine($"{opponent} wins due to grid movement!");
                    Console.WriteLine("\nPress any key to return to the main menu...");
                    Console.ReadKey();
                    return false;
                }
                
                gameInstance.NextMoveBy = (currentPlayer == EGamePiece.X) ? EGamePiece.O : EGamePiece.X;
                return true;
            }
            Console.WriteLine("Invalid move! The grid can't move in that direction. Try again.");
        }
    }

    public static string ChooseConfiguration()
    {
        var predefinedConfigNames = PredefinedConfigRepository.GetConfigurationNames();

        var configMenuItems = new List<MenuItem>();
        
        for (var i = 0; i < predefinedConfigNames.Count; i++)
        {
            var configName = predefinedConfigNames[i];
            configMenuItems.Add(new MenuItem()
            {
                Title = configName,
                Shortcut = (i + 1).ToString(),
                MenuItemAction = () => "predefined:" + configName
            });
        }
        
        configMenuItems.Add(new MenuItem()
        {
            Title = "Customize",
            Shortcut = (predefinedConfigNames.Count + 1).ToString(),
            MenuItemAction = () => "customize"
        });

        var configMenu = new Menu(EMenuLevel.Secondary,
            "TIC-TAC-TWO - choose game config",
            configMenuItems,
            isCustomMenu: true
        );

        var choice = configMenu.Run();

        if (choice == "customize")
        {
            return ChooseCustomConfiguration();
        }
        if (choice.StartsWith("predefined:"))
        {
            return choice;
        }
        return "MainMenu";
    }
    
    public static string ChooseCustomConfiguration()
    {
        var customConfigNames = CustomConfigRepository.GetConfigurationNames();

        var customConfigMenuItems = new List<MenuItem>();
    
        customConfigMenuItems.Add(new MenuItem()
        {
            Title = "New Customization",
            Shortcut = "1",
            MenuItemAction = () => "new"
        });
    
        for (var i = 0; i < customConfigNames.Count; i++)
        {
            var configName = customConfigNames[i];
            customConfigMenuItems.Add(new MenuItem()
            {
                Title = configName,
                Shortcut = (i + 2).ToString(),
                MenuItemAction = () => "custom:" + configName
            });
        }

        var customConfigMenu = new Menu(EMenuLevel.Secondary,
            "TIC-TAC-TWO - choose custom configuration",
            customConfigMenuItems,
            isCustomMenu: true
        );

        var choice = customConfigMenu.Run();

        Console.WriteLine($"User selected: {choice}");

        if (choice == "new")
        {
            try
            {
                var newConfig = CustomizeConfiguration();
                return "custom:" + newConfig.Name;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error customizing configuration: {ex.Message}");
                return "MainMenu";
            }
        }
        if (choice.StartsWith("custom:"))
        {
            return choice;
        }
        return "MainMenu";
    }
    
    private static GameConfiguration CustomizeConfiguration()
    {
        Console.Clear();
        Console.WriteLine("Customize your Tic-Tac-Two game:");

        var boardWidth = GetValidInput("Choose the board width, between 3 to 26: ", 3, 26);
        var boardHeight = GetValidInput("Choose the board height, between 3 to 26: ", 3, 26);

        var gridWidth = GetValidInput($"Choose the grid width, between 3 to {boardWidth}: ", 3, boardWidth);
        var gridHeight = GetValidInput($"Choose the grid height, between 3 to {boardHeight}: ", 3, boardHeight);

        var piecesPerPlayer = GetValidInput($"Choose the number of pieces per player, "
                + $"between {Math.Max(gridWidth, gridHeight)} to {gridWidth * gridHeight / 2}: ",
            Math.Max(gridWidth, gridHeight), gridWidth * gridHeight / 2);

        var newConfig = new GameConfiguration
        {
            BoardSizeWidth = boardWidth,
            BoardSizeHeight = boardHeight,
            GridSizeWidth = gridWidth,
            GridSizeHeight = gridHeight,
            PiecesPerPlayer = piecesPerPlayer,
            WinCondition = Math.Min(gridWidth, gridHeight)
        };
        
        Console.Write("Enter a name for the configuration: ");
        var configName = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(configName))
        {
            newConfig.Name = configName;

            Console.WriteLine("Where would you like to save the configuration?");
            Console.WriteLine("1) Save to Database");
            Console.WriteLine("2) Save to File System");
            Console.Write("Enter your choice: ");
            var storageChoice = Console.ReadLine();

            if (storageChoice == "1")
            {
                _configRepositoryDb.SaveConfiguration(newConfig);
                var dbConfig = _configRepositoryDb.GetConfigurationByName(newConfig.Name);
                CurrentConfigId = dbConfig.Id;
            }
            else if (storageChoice == "2")
            {
                _configRepositoryJson.SaveConfiguration(newConfig);
            }
            else
            {
                Console.WriteLine("Invalid choice. Configuration not saved.");
            }
            Console.Clear();
            Console.WriteLine("Configuration saved successfully!");
        }
        else
        {
            throw new InvalidOperationException("Invalid configuration name. Configuration not saved.");
        }
        
        Console.ReadKey();
        return newConfig;
    }
    
    private static GameConfiguration LoadConfiguration(string configName, bool isPredefined)
    {
        GameConfiguration chosenConfig;

        if (isPredefined)
        {
            chosenConfig = PredefinedConfigRepository.GetConfigurationByName(configName);

            if (chosenConfig == null)
            {
                throw new InvalidOperationException($"Predefined configuration '{configName}' not found.");
            }

            CurrentConfigId = PredefinedConfigRepository.GetConfigurationIdByName(configName);
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Where would you like to load the configuration from?");
            Console.WriteLine("1) Load from Database");
            Console.WriteLine("2) Load from File System");
            Console.Write("Enter your choice: ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                var config = _configRepositoryDb.GetConfigurationByName(configName);

                if (config == null)
                {
                    throw new InvalidOperationException($"Database configuration '{configName}' not found.");
                }

                CurrentConfigId = config.Id;
                chosenConfig = new GameConfiguration
                {
                    Name = config.ConfigName,
                    BoardSizeHeight = config.BoardSizeHeight,
                    BoardSizeWidth = config.BoardSizeWidth,
                    GridSizeHeight = config.GridSizeHeight,
                    GridSizeWidth = config.GridSizeWidth,
                    PiecesPerPlayer = config.PiecesPerPlayer,
                    WinCondition = config.WinCondition,
                    MovePieceAfterNMoves = config.MovePieceAfterNMoves
                };
            }
            else if (choice == "2")
            {
                var config = _configRepositoryJson.GetConfigurationByName(configName);

                if (config == null)
                {
                    throw new InvalidOperationException($"File system configuration '{configName}' not found.");
                }

                CurrentConfigId = 0;
                chosenConfig = new GameConfiguration
                {
                    Name = config.ConfigName,
                    BoardSizeHeight = config.BoardSizeHeight,
                    BoardSizeWidth = config.BoardSizeWidth,
                    GridSizeHeight = config.GridSizeHeight,
                    GridSizeWidth = config.GridSizeWidth,
                    PiecesPerPlayer = config.PiecesPerPlayer,
                    WinCondition = config.WinCondition,
                    MovePieceAfterNMoves = config.MovePieceAfterNMoves
                };
            }
            else
            {
                throw new InvalidOperationException("Invalid choice. Configuration loading failed.");
            }
        }

        return chosenConfig;
    }
    
    private static int GetValidInput(string prompt, int minValue, int maxValue)
    {
        int value;
        do
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (int.TryParse(input, out value) && value >= minValue && value <= maxValue)
            {
                return value;
            }
            Console.WriteLine($"Invalid input! Please enter a number between {minValue} and {maxValue}.");
        } while (true);
    }

    private static (int x, int y)? GetCoordinates(string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine() ?? string.Empty;

        var inputSplit = input.Split(",");
        if (inputSplit.Length != 2 || !int.TryParse(inputSplit[0], out var x) || !int.TryParse(inputSplit[1], out var y))
        {
            Console.WriteLine("Invalid input format. Please provide the coordinates in the format x,y.");
            return null;
        }

        return (x, y);
    }

    private static bool HandleExitOption(TicTacTwoBrain gameInstance)
    {
        Console.WriteLine("Would you like to save before exiting to the main menu? (S - save, E - exit without saving, C - cancel)");
        string exitChoice = Console.ReadLine()!.ToUpper();

        switch (exitChoice)
        {
            case "S":
                SaveGame(gameInstance, gameInstance.PasswordX, gameInstance.PasswordO);
                Console.WriteLine("Game saved successfully!");
                return true;

            case "E":
                Console.WriteLine("Returning to the main menu...");
                return true;

            case "C":
                Console.WriteLine("Cancelled. Returning to the game...");
                return false;

            default:
                Console.WriteLine("Invalid choice. Please try again.");
                return HandleExitOption(gameInstance);
        }
    }
    
    public static void LoadGame()
    {
        Console.WriteLine("Where would you like to load the game from?");
        Console.WriteLine("1) Load from Database");
        Console.WriteLine("2) Load from File System");
        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine();

        List<string> savedGames;
        IGameRepository gameRepository;

        if (choice == "1")
        {
            gameRepository = _gameRepositoryDb;
        }
        else if (choice == "2")
        {
            gameRepository = _gameRepositoryJson;
        }
        else
        {
            Console.WriteLine("Invalid choice. Returning to main menu.");
            Console.ReadKey();
            return;
        }

        savedGames = gameRepository.GetSavedGames();

        if (savedGames.Count == 0)
        {
            Console.WriteLine("No saved games available.");
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
            return;
        }
        
        Console.Clear();
        Console.WriteLine("Select a saved game to load:");
        for (int i = 0; i < savedGames.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {savedGames[i]}");
        }

        Console.Write("Choose a save file: ");
        if (int.TryParse(Console.ReadLine(), out int choiceIndex) && choiceIndex > 0 && choiceIndex <= savedGames.Count)
        {
            var (gameStateJson, gameConfigName, gameType, playerXPass, playerOPass) = gameRepository.LoadGame(savedGames[choiceIndex - 1]);

            var deserializedState = JsonSerializer.Deserialize<GameState>(gameStateJson, JsonOptions);
            if (deserializedState == null)
            {
                Console.WriteLine("Failed to load game state. Returning to main menu...");
                Console.ReadKey();
                return;
            }
            
            deserializedState.PasswordX = playerXPass;
            deserializedState.PasswordO = playerOPass;

            GameBrain = new TicTacTwoBrain(deserializedState.GameConfiguration, deserializedState.NextMoveBy)
            {
                GameType = gameType,
                GameBoard = deserializedState.GameBoard,
                PasswordX = deserializedState.PasswordX,
                PasswordO = deserializedState.PasswordO
            };

            GameBrain.SetRemainingPieces(EGamePiece.X, deserializedState.RemainingPiecesX);
            GameBrain.SetRemainingPieces(EGamePiece.O, deserializedState.RemainingPiecesO);
            GameBrain.SetGridPosition(deserializedState.GridRow, deserializedState.GridCol);
            
            Console.Clear();
            Console.WriteLine("Game loaded successfully!");
            Console.WriteLine("Press any key to start the game...");
            Console.ReadKey();
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Invalid choice. Returning to main menu.");
            Console.ReadKey();
        }
    }
    
    private static void SaveGame(TicTacTwoBrain gameInstance, string passX, string? passO)
    {
        Console.WriteLine("Where would you like to save the game?");
        Console.WriteLine("1) Save to Database");
        Console.WriteLine("2) Save to File System");
        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine();

        Console.Write("Enter a name for your save file: ");
        var saveName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(saveName))
        {
            Console.WriteLine("Save name cannot be empty.");
            return;
        }

        var gameConfig = gameInstance.GetGameConfiguration();
        var gridPos = gameInstance.GetGridPosition();

        if (choice == "1")
        {
            var newConfig = new Configuration
            {
                ConfigName = gameConfig.Name,
                BoardSizeWidth = gameConfig.BoardSizeWidth,
                BoardSizeHeight = gameConfig.BoardSizeHeight,
                GridSizeWidth = gameConfig.GridSizeWidth,
                GridSizeHeight = gameConfig.GridSizeHeight,
                PiecesPerPlayer = gameConfig.PiecesPerPlayer,
                WinCondition = gameConfig.WinCondition,
                MovePieceAfterNMoves = gameConfig.MovePieceAfterNMoves,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
            
            _configRepositoryDb.SaveConfiguration(newConfig);

            var insertedConfig = _configRepositoryDb.GetConfigurationByName(newConfig.ConfigName);
            var configId = insertedConfig.Id;

            var gameState = new GameState
            {
                GameConfiguration = gameConfig,
                GameBoard = gameInstance.GameBoard,
                NextMoveBy = gameInstance.GetCurrentTurn(),
                RemainingPiecesX = gameInstance.GetRemainingPieces(EGamePiece.X),
                RemainingPiecesO = gameInstance.GetRemainingPieces(EGamePiece.O),
                GridRow = gridPos.row,
                GridCol = gridPos.col,
                GameType = gameInstance.GameType,
                PasswordX = passX,
                PasswordO = passO
            };

            var jsonStateString = JsonSerializer.Serialize(gameState, JsonOptions);

            _gameRepositoryDb.SaveGame(
                jsonStateString,
                saveName,
                gameInstance.GameType,
                passX,
                passO,
                configId
            );

            Console.WriteLine("Game saved successfully to the database!");
        }
        else if (choice == "2")
        {
            var gameState = new GameState
            {
                GameConfiguration = gameConfig,
                GameBoard = gameInstance.GameBoard,
                NextMoveBy = gameInstance.GetCurrentTurn(),
                RemainingPiecesX = gameInstance.GetRemainingPieces(EGamePiece.X),
                RemainingPiecesO = gameInstance.GetRemainingPieces(EGamePiece.O),
                GridRow = gridPos.row,
                GridCol = gridPos.col,
                GameType = gameInstance.GameType,
                PasswordX = passX,
                PasswordO = passO
            };

            var jsonStateString = JsonSerializer.Serialize(gameState, JsonOptions);

            _gameRepositoryJson.SaveGame(
                jsonStateString,
                saveName,
                gameInstance.GameType,
                passX,
                passO,
                CurrentConfigId
            );
        }
        else
        {
            Console.WriteLine("Invalid choice. Game not saved.");
            return;
        }
        Console.Clear();
        Visualizer.DrawBoard(gameInstance);
        
        Console.WriteLine("Game saved successfully!");
    }
}