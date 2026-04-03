; SortDemo Inno Setup Script
; Download Inno Setup from: https://jrsoftware.org/isinfo.php

[Setup]
AppName=SortDemo
AppVersion=1.0.0
AppPublisher=SortDemo
AppPublisherURL=https://github.com/cbluethman/sortdemo
DefaultDirName={autopf}\SortDemo
DefaultGroupName=SortDemo
OutputDir=Output
OutputBaseFilename=SortDemoSetup
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\SortDemo.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "publish\SortDemo.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\SortDemo"; Filename: "{app}\SortDemo.exe"
Name: "{autodesktop}\SortDemo"; Filename: "{app}\SortDemo.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Run]
Filename: "{app}\SortDemo.exe"; Description: "Launch SortDemo"; Flags: nowait postinstall skipifsilent
