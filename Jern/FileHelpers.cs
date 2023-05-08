namespace Jern;

public static class FileHelpers
{
    public static string CurrentFile = "";
    public static string BasePath = "";

    /// <summary>
    /// Finds a file with .k extension to use as the key for all files
    /// </summary>
    public static string GetKey()
    {
        var keyFile = Directory.GetFiles(BasePath, "*.k").FirstOrDefault();
        return keyFile == null 
            ? "" // We already handle an invalid key for already-made entries, so this is fine.
            : keyFile.Replace(".k", "");
    }
}