copy ..\fnService\bin\debug\*.dll .
copy ..\fnService\bin\debug\*.exe .
copy ..\fnAdmin\bin\debug\fnAdmin.exe .
del FileNotify2.zip
d:\apps\bin\zip.exe FileNotify2.zip *.dll *.exe uninstall.bat install.bat *.txt Templates\* *.url