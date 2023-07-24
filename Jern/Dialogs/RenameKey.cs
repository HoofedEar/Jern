using System.Text;
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
            Normal = Application.Driver.MakeAttribute(Color.White, Color.DarkGray)
        };


        var keyName = new TextField
        {
            X = Pos.Center(),
            Y = Pos.Percent(70),
            Width = 14
        };
        Add(keyName);
        Add(new Label(".k")
        {
            Y = keyName.Y,
            X = keyName.X + 6
        });
        
        var button = new Button($"Save")
        {
            X = Pos.Center(), 
            Y = Pos.Percent(80)
        };
        
        button.Clicked += async () =>
        {
            var nameValue = Encoding.UTF8.GetString(keyName.Text.ToByteArray());
            if (string.IsNullOrEmpty(nameValue))
            {
                var error = new Attribute(Color.White, Color.Red);
                keyName.ColorScheme = new ColorScheme()
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
                await using (var stream = new FileStream("RENAME_ME.k", FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }

                File.Move("RENAME_ME.k", $"{nameValue}.k");
            }
            catch (IOException)
            {
                // Handle the case where the destination file already exists
                Add(new Label("EXCEPTION: FILE ALREADY EXISTS"));
            }
            catch (UnauthorizedAccessException)
            {
                // Handle the case where you don't have the required permissions
                Add(new Label("EXCEPTION: INVALID PERMISSIONS"));
            }
            
            Application.RequestStop();
        };
        Add(button);
        
        Add(new Label(
            "Please provide a new name for your key file." +
            "\nThis file is used to securely encrypt your entries." +
            "\n\nOnce you finish your writing session, it is" +
            "\nrecommended to rename this file to something" +
            "\nelse, or move the file out of the Jern folder.")
        {
            X = Pos.Center(),
            Y = Pos.Percent(25)
        });
    }
}