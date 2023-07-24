using Jern;
using Terminal.Gui;


FileHelpers.BasePath = AppContext.BaseDirectory;
Directory.CreateDirectory("entries");

if (args.Length > 0)
{
    if (!args[0].EndsWith(".se"))
    {
        Console.WriteLine(@"Invalid File Type - only .se files are supported.");
        return;
    }

    Application.Init();

    FileHelpers.CurrentFile = args[0];
    Console.Title = $@"Jern - {Path.GetFileName(FileHelpers.CurrentFile)}";
}
else
{
    Application.Init();
    FileHelpers.CurrentFile =
        FileHelpers.BasePath + "entries/" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".se";
    Console.Title = $@"Jern - {Path.GetFileName(FileHelpers.CurrentFile)}";
}

if (!Directory.GetFiles(FileHelpers.BasePath, "*.k").Any())
{
    using var fileStream = File.Create("RENAME_ME.k");
    fileStream.Close();
}

FileHelpers.PopulateEntries();

Application.Run<MainWindow>();

Application.Shutdown();