[Setup]
AppName=Foreman
AppVersion=<%= version %>
DefaultDirName={pf}\Foreman
DefaultGroupName=Foreman
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=<%= File.basename(t.name, ".exe") %>
OutputDir=<%= File.dirname(t.name) %>

[Icons]
Name: "{group}\Foreman"; Filename: "{app}\Foreman.exe"

[Files]
Source: "foreman\Foreman.exe"; DestDir: "{app}"; Flags: replacesameversion
