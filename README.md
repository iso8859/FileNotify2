# FileNotify2
Perform an action on file modification


With FileNotify2 execute an action when a file is modified.

FileNotify2 replaces FileNotify. We have learned from users since 1999 and completely redesigned this tool.

Very powerful functionnality : 
* write your own script in C#, unlimited actions.
* Very intuitive,  with plenty of script templates
* Trigger on new, deleted, changed and unchanged files
* Very secure to install on a production server, unzip and run install.bat
* Easily handle open file (put by FTP for example)

Use fnAdmin to create your fileNotify2.xml settings file.
fnservice2.exe is the new main exe
Run it to start in console mode.
To install/uninstall the service :
fnService2.exe install
fnService2.exe uninstall

Version history
Version 1.0, first version created 15DEC07
Version 2.0, 11JUN19 - Add fmService2 that uses https://github.com/Topshelf/Topshelf for a better service management.
