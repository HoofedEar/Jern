using System.Text;
using Terminal.Gui;
using DiffMatchPatch;
using Jern.Dialogs;

namespace Jern;

public sealed class MainWindow : Window
{
    private const string EmptyItemTitle = "----";
    private const string VersionString = "Version 1.2";
    private bool _isSaving;

    private static readonly ColorScheme MainScheme = new()
    {
        Normal = Application.Driver!.MakeColor(Color.White, Color.Black)
    };

    private readonly StatusBar _statusBar;

    private readonly TextView _textArea = new()
    {
        Width = Dim.Fill()! - 1,
        Height = Dim.Fill()! - 1,
        WordWrap = true,
        X = 1,
    };

    private readonly diff_match_patch _dmp = new();
    private string _savedText = string.Empty;

    public MainWindow()
    {
        ColorScheme = MainScheme;


        _statusBar = new StatusBar(new Shortcut[]
        {
            new(Key.S.WithCtrl, "Save", SaveFile),
            new(Key.Esc, "Quit", SaveBeforeQuit),
            new(null, "About", ShowAbout),
            new(null, "Prev", () => { LoadEntry(true); }),
            new(null, "Next", () => { LoadEntry(false); })
        });


        Add(_statusBar);
        Add(_textArea);

        return;

        async void SaveFile() => await SaveFileAsync();
    }

    /// <summary>
    /// Performs a check to see if there are any changes in the text area
    /// </summary>
    private bool CheckForChanges()
    {
        var saved = _savedText;
        var current = _textArea.Text;
        var diffs = _dmp.diff_main(saved, current);

        // Clean up the diff
        _dmp.diff_cleanupSemantic(diffs);

        // If there are any diffs, there are unsaved changes
        return diffs.Exists(diff => diff.operation != Operation.EQUAL);
    }

    /// <summary>
    /// Prompts to save the current file before quitting
    /// </summary>
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

    /// <summary>
    /// Resets the text area to a blank state
    /// </summary>
    private void ResetTextArea()
    {
        _textArea.Enabled = true;
        _textArea.Text = "";
        _textArea.SetFocus();
    }

    /// <summary>
    /// Loads an entry from the entries list
    /// </summary>
    /// <param name="loadNext">True - Next recent; False - Previous recent</param>
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

        var currentItemTitle = loadNext ? "Next" : "Prev";
        var nextItemTitle = loadNext ? "Prev" : "Next";

        var currentItem = _statusBar.Subviews.FirstOrDefault(i => i.Title == EmptyItemTitle);
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
            var nextItem = _statusBar.Subviews.FirstOrDefault(i => i.Title == nextItemTitle);
            if (nextItem != null)
                nextItem.Title = EmptyItemTitle;
        }

        if (!EncryptionHelper.Error) return;
        _textArea.Enabled = false;
    }

    public override void OnLoaded()
    {
        if (File.Exists( FileHelpers.BasePath + "RENAME_ME.k"))
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
        else
        {
            SilentSave();
        }

        // Check if previous/next buttons are active
        EntriesCheck();

        base.OnLoaded();
    }

    /// <summary>
    /// Checks to see if any entries are available before or after the current entry
    /// </summary>
    private void EntriesCheck()
    {
        var ahead = FileHelpers.EntryIndex + 1;
        var aheadEntry = FileHelpers.GetEntry(ahead);
        if (aheadEntry == "")
        {
            var nextItem = _statusBar.Subviews.First(i => i.Title == "Next");
            nextItem.Title = EmptyItemTitle;
        }

        var before = FileHelpers.EntryIndex - 1;
        var beforeEntry = FileHelpers.GetEntry(before);
        if (beforeEntry == "")
        {
            var prevItem = _statusBar.Subviews.First(i => i.Title == "Prev");
            prevItem.Title = EmptyItemTitle;
        }
    }

    /// <summary>
    /// Shows the "About" dialog
    /// </summary>
    private static void ShowAbout()
    {
        _ = MessageBox.Query(50, 8,
            "About", $"{VersionString}\nCreated by HoofedEar\nhttps://hoofedear.itch.io/jern\nPowered by Terminal.Gui",
            "Cool");
    }

    /// <summary>
    /// Saves the current file to disk
    /// </summary>
    private async Task SaveFileAsync()
    {
        if (EncryptionHelper.Error) return;
        if (_isSaving) return;
        _isSaving = true;
        var actual = new StringBuilder(_textArea.Text);
        EncryptionHelper.EncryptFile(actual.ToString(), FileHelpers.CurrentFile, FileHelpers.GetKey());
        var saveItem = _statusBar.Subviews.First(i => i.Title == "Save");
        saveItem.Title = "File saved!";
        _statusBar.Enabled = false;
        _savedText = _textArea.Text;
        await Task.Delay(1000);
        saveItem.Title = "Save";
        _statusBar.Enabled = true;
        _isSaving = false;
    }

    /// <summary>
    /// When loading Jern without a file, it will create one based on the date. This saves that file and reloads the
    /// entries list to reflect the new file.
    /// </summary>
    private void SilentSave()
    {
        if (EncryptionHelper.Error) return;
        if (_isSaving) return;
        _isSaving = true;
        var actual = new StringBuilder(_textArea.Text);
        EncryptionHelper.EncryptFile(actual.ToString(), FileHelpers.CurrentFile, FileHelpers.GetKey());
        _savedText = _textArea.Text;
        _isSaving = false;
        FileHelpers.PopulateEntries();
    }
}