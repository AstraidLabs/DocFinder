# DocFinder

This repository contains a WPF application for searching and indexing documents.

## Settings

DocFinder uses a JSON based settings system located in the user's **LocalAppData** folder
(`%LOCALAPPDATA%/DocFinder/settings.json`).  The defaults are defined in code and are
applied when no file exists or when new properties are introduced.  This mimics the
behaviour of .NET application/user settings where application scoped values are read
only and user scoped values can be modified at runtime.

User settings are only written to disk when `SaveAsync` is called on the settings service.
If the application exits without calling `SaveAsync`, changes are discarded – similar to
[`Settings.Save()` in .NET](https://gigi.nullneuron.net/gigilabs/saving-user-preferences-in-wpf/).

New settings added in future versions will automatically fall back to their default
values when loading older configuration files.
