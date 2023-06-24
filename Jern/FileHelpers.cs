namespace Jern;

public static class FileHelpers
{
    public static string CurrentFile { get; set; } = "";
    public static string BasePath { get; set; } = "";
    private static List<string> Entries { get; set; } = new();
    public static int EntryIndex { get; set; }

    /// <summary>
    /// Finds a file with .k extension to use as the key for all files
    /// </summary>
    public static string GetKey()
    {
        var keyFile = Directory.GetFiles(BasePath, "*.k").FirstOrDefault();
        return keyFile == null
            ? "" // We already handle an invalid key for already-made entries, so this is fine.
            : Path.GetFileNameWithoutExtension(keyFile);
    }

    /// <summary>
    /// Gets all of the current entries in the "entries" directory
    /// </summary>
    public static void PopulateEntries()
    {
        var entriesDir = BasePath + "entries/";
        var dirInfo = new DirectoryInfo(entriesDir);
        var files = dirInfo.GetFiles("*.se");
        Entries = files.OrderBy(f => f.CreationTime).Select(f => f.Name).ToList();
        EntryIndex = Entries.FindIndex(e => e == Path.GetFileName(CurrentFile));
    }

    public static string GetEntry(int index)
    {
        if (index > Entries.Count - 1 || index < 0)
        {
            // Out of index
            return "";
        }

        var entry = "entries/" + Entries[index];
        return entry;
    }
}