using Jern;
using Terminal.Gui;


FileHelpers.BasePath = AppDomain.CurrentDomain.BaseDirectory;
Directory.CreateDirectory(FileHelpers.BasePath + "entries");

if (args.Length > 0)
{
    if (!args[0].EndsWith(".se"))
    {
        Console.WriteLine("Invalid File Type - only .se files are supported.");
        return;
    }

    Application.Init();

    FileHelpers.CurrentFile = args[0];
    Console.Title = $"Jern - {Path.GetFileName(FileHelpers.CurrentFile)}";
}
else
{
    Application.Init();
    FileHelpers.CurrentFile =
        FileHelpers.BasePath + "entries/" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".se";
    Console.Title = $"Jern - {Path.GetFileName(FileHelpers.CurrentFile)}";
}

if (Directory.GetFiles(FileHelpers.BasePath, "*.k").Length == 0)
{
    using var fileStream = File.Create(FileHelpers.BasePath + "RENAME_ME.k");
    fileStream.Close();
}

FileHelpers.PopulateEntries();

Application.QuitKey = Key.F24; // Basically disable the quit key, as we use our own

Dialog.DefaultButtonAlignment = Alignment.Center;
Dialog.DefaultBorderStyle = LineStyle.Single;
MessageBox.DefaultBorderStyle = LineStyle.Single;
Button.DefaultShadow = ShadowStyle.None;
Dialog.DefaultShadow = ShadowStyle.None;

Application.Run<MainWindow>();

Application.Shutdown();