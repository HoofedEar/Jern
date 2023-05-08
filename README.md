# Jern
![image](https://user-images.githubusercontent.com/1261392/236711666-709956ae-1cf5-44e0-8a79-2c20613b8899.png)

Jern is a journaling tool that encrypts entries using AES-128 encryption with a random initialization vector (IV) and a key which is the filename of a .k file. To open an encrypted journal entry, you need Jern and the corresponding key file. However, it's important to note that this encryption method is not perfectly secure and is designed to deter someone who discovers the encrypted files.

The .k filename serves as your encryption key, so it's important not to use a password that you use elsewhere. Additionally, once you finish writing your journal entry, it's recommended to delete/move/rename the key file.

To use Jern, you should create a folder where Jern will be stored, and a subfolder called "entries" where all your journal entries will be saved as .se files. When you open Jern without specifying a filename, it will use the current date and save the entry in an "entries" folder located in the same directory as Jern. Jern will only open .se files, and you can open previous entries through the Terminal by typing `jern entries/previous-entry.se` in the directory where Jern is located.

When it comes to typing and editing, keep in mind that Jern runs in the terminal, so common keyboard shortcuts for cutting and pasting may not work. You can select text using your mouse and use right-click to bring up a menu with various functions, including keyboard shortcuts. To safely exit Jern, press Alt+Q.

Powered by [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)
