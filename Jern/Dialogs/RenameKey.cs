using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Jern.Dialogs;

public sealed class RenameKey : Window
{
    public RenameKey()
    {
        Title = "Rename Key File";
        Width = Dim.Fill();
        Height = Dim.Fill();

        ColorScheme = new ColorScheme
        {
            Normal = Application.Driver!.MakeColor(Color.White, Color.DarkGray)
        };


        var keyName = new TextField
        {
            X = Pos.Center(),
            Y = Pos.Percent(70),
            Width = 14
        };
        Add(keyName);
        Add(new Label
        {
            Text = ".k",
            Y = keyName.Y,
            X = keyName.X + 6
        });

        var button = new Button
        {
            Text = "Save",
            X = Pos.Center(),
            Y = Pos.Percent(80)
        };

        button.MouseClick += (_, _) => { SaveKey(keyName); };
        button.Accept += (_, _) => { SaveKey(keyName); };


        Add(button);
        Add(new Label
        {
            Text = "Please provide a new name for your key file." +
                   "\nThis file is used to securely encrypt your entries." +
                   "\n\nOnce you finish your writing session, it is" +
                   "\nrecommended to rename this file to something" +
                   "\nelse, or move the file out of the Jern folder.",
            X = Pos.Center(),
            Y = Pos.Percent(25)
        });
    }

    private async void SaveKey(TextField keyName)
    {
        var nameValue = keyName.Text;
        if (string.IsNullOrEmpty(nameValue))
        {
            var error = new Attribute(Color.White, Color.Red);
            keyName.ColorScheme = new ColorScheme
            {
                Normal = error,
                HotNormal = error,
                Focus = error,
                HotFocus = error
            };
            await Task.Delay(350);
            keyName.ColorScheme = default;
            return;
        }

        try
        {
            await using (var stream = new FileStream(FileHelpers.BasePath + "RENAME_ME.k", FileMode.Open,
                             FileAccess.ReadWrite, FileShare.None))
            {
                stream.Close();
            }

            File.Move(FileHelpers.BasePath + "RENAME_ME.k", FileHelpers.BasePath + $"{nameValue}.k");
        }
        catch (IOException)
        {
            // Handle the case where the destination file already exists
            Add(new Label { Text = "EXCEPTION: FILE ALREADY EXISTS" });
        }
        catch (UnauthorizedAccessException)
        {
            // Handle the case where you don't have the required permissions
            Add(new Label { Text = "EXCEPTION: INVALID PERMISSIONS" });
        }

        Application.RequestStop();
    }
}