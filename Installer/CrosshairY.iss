#define MyAppName "CrosshairY"
#define MyAppShortName "CrosshairY"
#define MyAppPublisher "tsgsOFFICIAL"
#define MyAppExeName "CrosshairY.exe"

#ifndef MyAppVersion
#define MyAppVersion GetStringFileInfo("..\publish\self-contained\" + MyAppExeName, "ProductVersion")
#endif

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={userappdata}\{#MyAppShortName}
DefaultGroupName={#MyAppName}
PrivilegesRequired=lowest
CloseApplications=yes
RestartApplications=yes
OutputBaseFilename={#MyAppShortName}-{#MyAppVersion}-Setup
OutputDir=..\publish\installer
SetupIconFile=..\assets\logo.ico
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\publish\self-contained\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
