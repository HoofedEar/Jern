using System.Text;
using Terminal.Gui;

namespace Jern;

public sealed class MainWindow : Window
{
    private static readonly ColorScheme MainScheme = new()
    {
        Normal = Application.Driver.MakeAttribute(Color.White, Color.Black)
    };

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
        ColorScheme = MainScheme;

        async void SaveFile() => await SaveFileAsync();

        _statusBar = new StatusBar(new StatusItem[]
        {
            new(Key.AltMask | Key.S, "Save (Alt+S)", SaveFile),
            new(Key.AltMask | Key.Q, "Quit (Alt+Q)", () => Application.RequestStop()),
            new(Key.AltMask | Key.I, "About (Alt+I)", ShowAbout),
            new(Key.AltMask | Key.PageUp, "Previous (Alt+PgUp)", LoadPreviousEntry),
            new(Key.AltMask | Key.PageDown, "Next (Alt+PgDn)", LoadNextEntry)
        });

        _invalidBar = new StatusBar(new StatusItem[]
        {
            new(Key.AltMask | Key.Q, "Quit (Alt+Q)", () => Application.RequestStop()),
            new(Key.AltMask | Key.I, "About (Alt+I)", ShowAbout),
            new(Key.AltMask | Key.PageUp, "Previous (Alt+PgUp)", LoadPreviousEntry),
            new(Key.AltMask | Key.PageDown, "Next (Alt+PgDn)", LoadNextEntry)
        });


        Add(_statusBar);
        Add(_textArea);
    }

    private void LoadNextEntry()
    {
        var index = FileHelpers.EntryIndex - 1;
        var entry = FileHelpers.GetEntry(index);
        if (entry == "") return;

        var prevItem = _statusBar.Items.FirstOrDefault(i => i.Title == "- -");
        if (prevItem != null)
        {
            prevItem.Title = "Previous (Alt+PgUp)";
        }

        FileHelpers.EntryIndex = index;
        FileHelpers.CurrentFile = entry;
        Title = Path.GetFileName(FileHelpers.CurrentFile);
        var result = EncryptionHelper.DecryptFile(FileHelpers.CurrentFile, FileHelpers.GetKey());
        _textArea.Text = "";
        _textArea.Text = result;
        _textArea.MoveEnd();

        // Check ahead
        var ahead = FileHelpers.EntryIndex - 1;
        var aheadEntry = FileHelpers.GetEntry(ahead);
        if (aheadEntry == "")
        {
            var nextItem = _statusBar.Items.FirstOrDefault(i => i.Title == "Next (Alt+PgDn)");
            if (nextItem != null)
                nextItem.Title = "- -";
        }

        if (!EncryptionHelper.Error) return;
        _textArea.Enabled = false;
        Remove(_statusBar);
        Add(_invalidBar);
    }

    private void LoadPreviousEntry()
    {
        var index = FileHelpers.EntryIndex + 1;
        var entry = FileHelpers.GetEntry(index);
        if (entry == "") return;

        var nextItem = _statusBar.Items.FirstOrDefault(i => i.Title == "- -");
        if (nextItem != null)
        {
            nextItem.Title = "Next (Alt+PgDn)";
        }


        FileHelpers.EntryIndex = index;
        FileHelpers.CurrentFile = entry;
        Title = Path.GetFileName(FileHelpers.CurrentFile);
        var result = EncryptionHelper.DecryptFile(FileHelpers.CurrentFile, FileHelpers.GetKey());
        _textArea.Text = "";
        _textArea.Text = result;
        _textArea.MoveEnd();

        // Check ahead
        var ahead = FileHelpers.EntryIndex + 1;
        var aheadEntry = FileHelpers.GetEntry(ahead);
        if (aheadEntry == "")
        {
            var prevItem = _statusBar.Items.FirstOrDefault(i => i.Title == "Previous (Alt+PgUp)");
            if (prevItem != null)
                prevItem.Title = "- -";
        }

        if (!EncryptionHelper.Error) return;
        _textArea.Enabled = false;
        Remove(_statusBar);
        Add(_invalidBar);
    }

    public override void OnLoaded()
    {
        Title = Path.GetFileName(FileHelpers.CurrentFile);
        if (File.Exists(FileHelpers.CurrentFile))
        {
            var result = EncryptionHelper.DecryptFile(FileHelpers.CurrentFile, FileHelpers.GetKey());
            _textArea.Text = result;
            _textArea.MoveEnd();

            if (EncryptionHelper.Error)
            {
                _textArea.Enabled = false;
                Remove(_statusBar);
                Add(_invalidBar);
            }
        }

        // Check if previous/next buttons are active
        var ahead = FileHelpers.EntryIndex - 1;
        var aheadEntry = FileHelpers.GetEntry(ahead);
        if (aheadEntry == "")
        {
            var nextItem = _statusBar.Items.First(i => i.Title == "Next (Alt+PgDn)");
            nextItem.Title = "- -";
        }

        var before = FileHelpers.EntryIndex + 1;
        var beforeEntry = FileHelpers.GetEntry(before);
        if (beforeEntry == "")
        {
            var prevItem = _statusBar.Items.First(i => i.Title == "Previous (Alt+PgUp)");
            prevItem.Title = "- -";
        }

        base.OnLoaded();
    }

    private static void ShowAbout()
    {
        var _ = MessageBox.Query(50, 8,
            "About", "Version 1.1\nCreated by HoofedEar\nhttps://hoofedear.itch.io/jern\nPowered by Terminal.Gui",
            "Cool");
    }

    private async Task SaveFileAsync()
    {
        var actual = new StringBuilder(Encoding.UTF8.GetString(_textArea.Text.ToByteArray()));
        EncryptionHelper.EncryptFile(actual.ToString(), FileHelpers.CurrentFile, FileHelpers.GetKey());
        var saveItem = _statusBar.Items.First(i => i.Title == "Save (Alt+S)");
        saveItem.Title = "File saved!";
        _statusBar.Enabled = false;
        await Task.Delay(1000);
        saveItem.Title = "Save (Alt+S)";
        _statusBar.Enabled = true;
    }
}