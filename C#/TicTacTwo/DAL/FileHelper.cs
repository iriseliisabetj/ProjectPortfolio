namespace DAL;

public static class FileHelper
{
    public static string BasePath = Environment
                                        .GetFolderPath(Environment.SpecialFolder.UserProfile)
                                    + Path.DirectorySeparatorChar + "tic-tac-two" + Path.DirectorySeparatorChar;
}