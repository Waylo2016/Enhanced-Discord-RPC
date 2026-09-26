[Setup]
AppName=YouPresence
AppVersion=0.1
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
Filename: "{app}\EnhancedRpc.Host.exe"; Parameters: "--register"; Flags: runhidden

[UninstallRun]
Filename: "{app}\EnhancedRpc.Host.exe"; Parameters: "--unregister"; Flags: runhidden waituntilterminated