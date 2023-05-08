using System.Text;
using Terminal.Gui;

namespace Jern;

public sealed class MainWindow : Window
{
    private readonly StatusBar _statusBar;
    private readonly StatusBar _invalidBar;
    private readonly TextView _textArea = new()
    {
        Width = Dim.Fill(),
        Height = Dim.Fill() - 1,
        WordWrap = true
    };

    

    public MainWindow()
    {
        ColorScheme = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black)
        };

        async void SaveFile() => await SaveFileAsync();

        _statusBar = new StatusBar(new StatusItem[]
        {
            new(Key.AltMask | Key.S, "Save (Alt+S)", SaveFile),
            new(Key.AltMask | Key.Q, "Quit (Alt+Q)", () => Application.RequestStop()),
            new(Key.AltMask | Key.I, "About (Alt+I)", ShowAbout)
        });

        _invalidBar = new StatusBar(new StatusItem[]
        {
            new(Key.AltMask | Key.Q, "Quit (Alt+Q)", () => Application.RequestStop())
        });


        Add(_statusBar);
        Add(_textArea);
    }

    public override void OnLoaded()
    {
        Title = Path.GetFileName(FileHelpers.CurrentFile);
        if (File.Exists(FileHelpers.CurrentFile))
        {
            var result = EncryptionHelpers.DecryptFile(FileHelpers.CurrentFile, FileHelpers.GetKey());
            _textArea.Text = result;
            _textArea.MoveEnd();
            
            if (EncryptionHelpers.Error)
            {
                _textArea.Enabled = false;
                Remove(_statusBar);
                Add(_invalidBar);
            }
        }

        base.OnLoaded();
    }

    private static void ShowAbout()
    {
        MessageBox.Query(50, 8,
            "About", "Version 1.0\nCreated by HoofedEar\nhttps://hoofedear.itch.io/jern\nPowered by Terminal.Gui", "Cool");
    }

    private async Task SaveFileAsync()
    {
        var actual = Encoding.UTF8.GetString(_textArea.Text.ToByteArray());
        EncryptionHelpers.EncryptFile(actual, FileHelpers.CurrentFile, FileHelpers.GetKey());
        _statusBar.Items.First(i => i.Title == "Save (Alt+S)").Title = "File saved!";
        _statusBar.Enabled = false;
        await Task.Delay(1000);
        _statusBar.Items.First(i => i.Title == "File saved!").Title = "Save (Alt+S)";
        _statusBar.Enabled = true;
    }
}