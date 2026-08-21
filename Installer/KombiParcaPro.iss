#define MyAppName "KombiParcaPro"
#define MyAppVersion "2.1.2"
#define MyAppPublisher "KombiParcaPro"
#define MyAppExeName "KombiParcaPro.exe"

[Setup]
AppId={{9A24A30C-BC2F-4E7B-9A84-7FE04C31C854}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\KombiParcaPro
DefaultGroupName=KombiParcaPro
OutputDir=..\SetupOutput
OutputBaseFilename=KombiParcaPro_Setup_v2.1.2_x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\KombiParcaPro\Assets\KombiParcaPro.ico

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Ek görevler:"

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\KombiParcaPro"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\KombiParcaPro"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "KombiParcaPro'yu çalıştır"; Flags: nowait postinstall skipifsilent
