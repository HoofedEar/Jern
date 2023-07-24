using System.Text;
using Terminal.Gui;
using DiffMatchPatch;
using Jern.Dialogs;
using NStack;

namespace Jern;

public sealed class MainWindow : Window
{
    private static readonly ColorScheme MainScheme = new()
    {
        Normal = Application.Driver.MakeAttribute(Color.White, Color.Black)
    };

    private readonly StatusBar _statusBar;

    private readonly TextView _textArea = new()
    {
        Width = Dim.Fill(),
        Height = Dim.Fill() - 1,
        WordWrap = true
    };

    private readonly diff_match_patch _dmp = new();
    private ustring _savedText = string.Empty;

    public MainWindow()
    {
        ColorScheme = MainScheme;

        async void SaveFile() => await SaveFileAsync();

        _statusBar = new StatusBar(new StatusItem[]
        {
            new(Key.AltMask | Key.S, "Save (Alt+S)", SaveFile),
            new(Key.AltMask | Key.Q, "Quit (Alt+Q)", SaveBeforeQuit),
            new(Key.AltMask | Key.I, "About (Alt+I)", ShowAbout),
            new(Key.AltMask | Key.PageUp, "Previous (Alt+PgUp)", () => {LoadEntry(false);}),
            new(Key.AltMask | Key.PageDown, "Next (Alt+PgDn)", () => {LoadEntry(true);})
        });


        Add(_statusBar);
        Add(_textArea);
    }

    private bool CheckForChanges()
    {
        var saved = _savedText.ToString() ?? throw new Exception("Shit's broken bruv");
        var current = _textArea.Text.ToString() ?? throw new Exception("Shit's broken bruv");
        var diffs = _dmp.diff_main(saved, current);
    
        // Clean up the diff
        _dmp.diff_cleanupSemantic(diffs);

        // If there are any diffs, there are unsaved changes
        return diffs.Exists(diff => diff.operation != Operation.EQUAL);
    }

    private async void SaveBeforeQuit()
    {
        if (EncryptionHelper.Error)
        {
            Application.RequestStop();
            return;
        }

        // If there are any diffs, there are unsaved changes
        if (CheckForChanges())
        {
            var prompt = MessageBox.Query(50, 8,
                "Save?", "Would you like to save this file before quitting?",
                "Yea", "Nah", "Cancel");

            switch (prompt)
            {
                case 0:
                    await SaveFileAsync();
                    Application.RequestStop();
                    break;
                case 1:
                    Application.RequestStop();
                    break;
                case 2:
                    return;
            }
        }
        else
        {
            Application.RequestStop();
        }
    }

    private void ResetTextArea()
    {
        _textArea.Enabled = true;
        _textArea.Text = "";
        _textArea.SetFocus();
    }
    
    private async void LoadEntry(bool loadNext)
    {
        var direction = loadNext ? -1 : 1; // -1 for next, 1 for previous
        var index = FileHelpers.EntryIndex + direction;
        var entry = FileHelpers.GetEntry(index);
        if (entry == "") return;
        
        if (CheckForChanges())
        {
            var prompt = MessageBox.Query(50, 8,
                "Save?", "Would you like to save your changes before changing entries?",
                "Yea", "Nah", "Cancel");

            switch (prompt)
            {
                case 0:
                    await SaveFileAsync();
                    break;
                case 1:
                    break;
                case 2:
                    return;
            }
        }
        
        var currentItemTitle = loadNext ? "Previous (Alt+PgUp)" : "Next (Alt+PgDn)";
        var nextItemTitle = loadNext ? "Next (Alt+PgDn)" : "Previous (Alt+PgUp)";
        const string defaultItemTitle = "- -";
    
        var currentItem = _statusBar.Items.FirstOrDefault(i => i.Title == defaultItemTitle);
        if (currentItem != null)
        {
            currentItem.Title = currentItemTitle;
        }

        FileHelpers.EntryIndex = index;
        FileHelpers.CurrentFile = entry;
        Title = Path.GetFileName(FileHelpers.CurrentFile);
        var result = EncryptionHelper.DecryptFile(FileHelpers.CurrentFile, FileHelpers.GetKey());
        ResetTextArea();
        _textArea.Text = result;
        _savedText = _textArea.Text;
        _textArea.MoveEnd();

        // Check ahead
        var ahead = FileHelpers.EntryIndex + direction;
        var aheadEntry = FileHelpers.GetEntry(ahead);
        if (aheadEntry == "")
        {
            var nextItem = _statusBar.Items.FirstOrDefault(i => i.Title == nextItemTitle);
            if (nextItem != null)
                nextItem.Title = defaultItemTitle;
        }

        if (!EncryptionHelper.Error) return;
        _textArea.Enabled = false;
    }

    public override void OnLoaded()
    {
        if (File.Exists("RENAME_ME.k"))
        {
            Application.Run<RenameKey>();
        }
        
        Title = Path.GetFileName(FileHelpers.CurrentFile);
        if (File.Exists(FileHelpers.CurrentFile))
        {
            var result = EncryptionHelper.DecryptFile(FileHelpers.CurrentFile, FileHelpers.GetKey());
            ResetTextArea();
            _textArea.Text = result;
            _savedText = _textArea.Text;
            _textArea.MoveEnd();

            if (EncryptionHelper.Error)
            {
                _textArea.Enabled = false;
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

    private void ShowAbout()
    {
        var _ = MessageBox.Query(50, 8,
            "About", "Version 1.1\nCreated by HoofedEar\nhttps://hoofedear.itch.io/jern\nPowered by Terminal.Gui",
            "Cool");
    }

    private async Task SaveFileAsync()
    {
        if (EncryptionHelper.Error) return;
        var actual = new StringBuilder(Encoding.UTF8.GetString(_textArea.Text.ToByteArray()));
        EncryptionHelper.EncryptFile(actual.ToString(), FileHelpers.CurrentFile, FileHelpers.GetKey());
        var saveItem = _statusBar.Items.First(i => i.Title == "Save (Alt+S)");
        saveItem.Title = "File saved!";
        _statusBar.Enabled = false;
        _savedText = _textArea.Text;
        await Task.Delay(1000);
        saveItem.Title = "Save (Alt+S)";
        _statusBar.Enabled = true;
    }
}