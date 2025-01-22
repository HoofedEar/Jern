# Jern
![image](https://github.com/user-attachments/assets/46bb4046-4b5b-4757-b657-b9f2007fd620)


Jern is a terminal-based journaling tool that provides basic encryption for your personal entries. It uses AES-128 encryption with a random initialization vector (IV) to protect your journal entries from casual discovery.

Powered by [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)

## Security Notice

The encryption implementation in Jern is designed for basic privacy and should not be considered cryptographically secure. It provides a deterrent against casual snooping but should not be relied upon for highly sensitive information.

## Features

- Terminal-based interface for distraction-free writing
- Basic encryption using AES-128
- Automatic date-based file naming
- Simple key file management
- Minimal setup required

## Installation

Simply place Jern in a folder or the root of a USB drive, and run the application.
It will create an entries folder automatically, where auto-generated entries (ones based on the current date) will appear.
Your directory structure will look like this after first run:

```
/
├── jern (executable)
├── key file (.k)
└── entries/
```

## Usage

### Creating a New Entry

To create a new entry with today's date:
```bash
./jern
```

To create an entry with a specific name:
```bash
./jern entries/my-entry.se
```

### Opening Existing Entries

To open a previous entry:
```bash
./jern entries/previous-entry.se
```

### Key File Management

- Your encryption key is the filename of a `.k` file
- Use unique keys that are not passwords you use elsewhere
- For additional security, delete, move, or rename the key file after finishing your entry

### Navigation and Editing

- Use mouse for text selction
- Right-click to access the context menu with additional functions
- Navigate between entries using the buttons on the bottom
- Please note: *Common terminal keyboard shortcuts may not work as expected*

### Exiting

Press `Esc` to safely exit Jern. It will prompt you to save any unsaved changes before exiting.

## File Types

- `.se`: Encrypted journal entries (Secure Entry)
- `.k`: Key files used for encryption

## Best Practices

1. Keep backups of your `entries` directory
2. Never share or reuse key file names that you use for other purposes
3. Consider moving completed entries and their key files to secure storage

## Contributing

Feel free to submit issues and pull requests! I'm open to new ideas or features. But please keep in mind, I'd like to keep Jern portable.

## License

[GPL-3.0](https://github.com/HoofedEar/Jern?tab=GPL-3.0-1-ov-file)


