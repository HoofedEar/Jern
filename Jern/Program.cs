using Jern;
using Terminal.Gui;

if (args.Length > 0)
{
    if (!args[0].EndsWith(".se"))
    {
        Console.WriteLine(@"Invalid File Type - only .se files are supported.");
        return;
    }
    Application.Init();
    FileHelpers.BasePath = AppContext.BaseDirectory;
    FileHelpers.CurrentFile = args[0];
    Console.Title = $@"Jern - {Path.GetFileName(FileHelpers.CurrentFile)}";
    Application.Run<MainWindow>();
}
else
{
    Application.Init();
    FileHelpers.BasePath = AppContext.BaseDirectory;
    FileHelpers.CurrentFile = FileHelpers.BasePath + "entries/" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".se";
    Console.Title = $@"Jern - {Path.GetFileName(FileHelpers.CurrentFile)}";
    Application.Run<MainWindow>();
}
Application.Shutdown();