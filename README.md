# YouPresence

Shows what you're watching on YouTube in your Discord status.

Consists of two parts: a browser extension that reads the YouTube player, and
a native host that talks to the Discord client over IPC.

## Requirements

- Windows
- Discord desktop client (the web version won't work, Rich Presence needs the local IPC socket)
- A Chromium browser: Chrome, Opera, Edge, Brave

## Installation

**1. Install the host**

Download `YouPresence-Setup.exe` from [Releases][releases] and run it.

Windows SmartScreen will warn you because the installer isn't code signed.
Click "More info" then "Run anyway" if you're okay with that. I honestly just don't want to pay Microsoft to 'sign' the installer.

**2. Install the extension**

Download `YouPresence-Extension.zip` from the same release and extract it
somewhere permanent. Not your Downloads folder, since the extension breaks if
you delete it later.

Then:

- Go to `chrome://extensions` (or `opera://extensions`)
- Turn on Developer mode
- Click "Load unpacked" and select the extracted folder

**3. Restart your browser**

The host registration is read at startup, so this step is not optional.

Start a video and your Discord status should update within a few seconds.

## How it works

The extension can't talk to Discord directly, so there's a small native
program in between:

```
content script -> service worker -> native messaging -> YouPresence host -> Discord
```

The content script polls the YouTube player once a second. The host decides
what's worth sending: Discord rate limits presence updates to one every 15
seconds, so only real changes get pushed (track switches, pausing, seeking).

## Troubleshooting

The host writes to `%LOCALAPPDATA%\YouPresence\host.log`. Start there.

**Nothing in the log at all** — the browser isn't starting the host. Check
that the registry key exists and points at an existing file:

```powershell
Get-ItemProperty "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.youpresence.waylo.tech"
```

**Log says "host started" but nothing else** — the host is running but the
extension isn't allowed to talk to it. The extension ID in
`%LOCALAPPDATA%\Programs\YouPresence\native-host-manifest.json` has to match
the one shown on your extensions page.

**Messages arrive but no status appears** — check that Discord is running,
and that Settings > Activity Privacy > "Share your detected activities with
others" is on. Also note you can't see your own Rich Presence buttons; only
other people can.

## Building from source

```powershell
dotnet publish EnhancedRpc.Host -r win-x64 -c Release --self-contained -p:PublishSingleFile=true -o publish
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

Output lands in `installer-output/`.

For development you can skip the installer and register your debug build
directly:

```powershell
.\EnhancedRpc.Host\bin\Debug\net10.0\EnhancedRpc.Host.exe --register
```

Only one registration can be active at a time, so run `--register` on
whichever build you want the browser to start.

## License

[License](LICENSE)

[releases]: https://github.com/[CHECK-YOUR-REPO]/releases