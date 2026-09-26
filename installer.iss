[Setup]
AppName=YouPresence
AppVersion=0.3
AppPublisher=Waylo
DefaultDirName={autopf}\YouPresence
DisableProgramGroupPage=yes
OutputDir=installer-output
OutputBaseFilename=YouPresence-Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Run]
Filename: "{app}\YouPresence.Host.exe"; Parameters: "--register"; Flags: runhidden

[UninstallRun]
Filename: "{app}\YouPresence.Host.exe"; Parameters: "--unregister"; Flags: runhidden waituntilterminated