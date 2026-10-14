![CrosshairY](https://socialify.git.ci/tsgsOFFICIAL/CrosshairY/image?description=1&font=Source+Code+Pro&forks=1&issues=1&language=1&name=1&owner=1&pattern=Transparent&pulls=1&stargazers=1&theme=Dark)

# CrosshairY

**A free and open-source crosshair overlay for any game, no BS required.**

> The #1 CrosshairX alternative since June, 2025. Design it, save it, share it, and keep it on top of any game.

[![Issues](https://img.shields.io/github/issues/tsgsOFFICIAL/CrosshairY)](https://github.com/tsgsOFFICIAL/CrosshairY/issues)
[![Downloads](https://img.shields.io/github/downloads/tsgsOFFICIAL/CrosshairY/total)](https://github.com/tsgsOFFICIAL/CrosshairY/releases)
[![Last Build](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FtsgsOFFICIAL%2FCrosshairY%2Fbadges%2Flatest-build.json)](https://github.com/tsgsOFFICIAL/CrosshairY/releases)
[![Nightly Build](https://github.com/tsgsOFFICIAL/CrosshairY/actions/workflows/nightly.yml/badge.svg)](https://github.com/tsgsOFFICIAL/CrosshairY/actions/workflows/nightly.yml)
[![Release Build](https://github.com/tsgsOFFICIAL/CrosshairY/actions/workflows/release.yml/badge.svg)](https://github.com/tsgsOFFICIAL/CrosshairY/actions/workflows/release.yml)
[![GitHub license](https://img.shields.io/github/license/tsgsOFFICIAL/CrosshairY)](https://github.com/tsgsOFFICIAL/CrosshairY/blob/master/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)
[![WPF](https://img.shields.io/badge/WPF-Modern_UI-teal)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview)
[![GitHub stars](https://img.shields.io/github/stars/tsgsOFFICIAL/CrosshairY?style=social)](https://github.com/tsgsOFFICIAL/CrosshairY)
[![Discord](https://img.shields.io/discord/227048721710317569?logo=discord&logoColor=FFF&label=Discord&labelColor=5865F2)](https://discord.gg/Cddu5aJ)
[![Support on Ko-fi](https://img.shields.io/badge/Support%20me%20on%20Ko--fi-F16061?logo=ko-fi&logoColor=white)](https://ko-fi.com/tsgsOFFICIAL)

## Table of Contents

- [Features](#features)
- [Demo](#demo)
- [Privacy & Safety](#privacy--safety)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
- [Building from Source](#building-from-source)
- [FAQ](#faq)
- [Support the Project](#support-the-project--the-creator)
- [How to Contribute](#how-to-contribute)
- [Star History](#star-history)
- [License](#license)

## Features

- **Customizable overlay**: tweak length, thickness, gap, colors, outline and style until it's exactly yours
- **Works everywhere**: any resolution, fullscreen or windowed
- **Designer, Library and Share Codes**: build crosshairs, keep your favorites and share them with a single code
- **Global hotkeys**: toggle the crosshair or swap to a saved one without leaving the game
- **Click-through**: the overlay never steals focus or mouse input
- **No game modifications**: completely external, nothing touches game files or memory
- **Lightweight and open-source**: tiny footprint, MIT licensed

## Demo

![Demo](https://github.com/tsgsOFFICIAL/CrosshairY/blob/master/full_demo_new.gif?raw=true)

_A customizable crosshair overlay that stays on top of any game._

## Privacy & Safety

- External overlay only: no game files are modified and no game memory is read
- No analytics, telemetry or data collection
- Network access is limited to checking for app updates and downloading community crosshairs from GitHub
- Settings and your library are stored locally next to the app

## Requirements

- Windows 10/11 (64-bit)
- Any game running in fullscreen or windowed mode
- The .NET 10 Desktop Runtime, only if you use the framework-dependent zip (the installer and self-contained zip bundle it)

## Quick Start

1. **Download** the latest release from [Releases](https://github.com/tsgsOFFICIAL/CrosshairY/releases/latest) (recommended)
2. Run `Setup.exe` for a guided install, or extract one of the zips for a portable copy
3. If you extracted a zip, right-click `CrosshairY.exe` > `Send to` > `Desktop (create shortcut)`
4. Launch CrosshairY, pick or design a crosshair and set your toggle hotkey

> **Nightly Builds:** Want the latest code before it gets a tagged release? Every push to `master` automatically rebuilds and republishes the [nightly release](https://github.com/tsgsOFFICIAL/CrosshairY/releases/tag/nightly), both self-contained and framework-dependent, plus the installer. The version number includes the short commit hash it was built from, so you always know exactly what you're running. It's untested beyond CI passing, so expect the occasional rough edge, grab a regular [Release](https://github.com/tsgsOFFICIAL/CrosshairY/releases/latest) instead if you want something stable.

## Building from Source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) installed.

```bash
git clone https://github.com/tsgsOFFICIAL/CrosshairY.git
cd CrosshairY
dotnet restore CrosshairY.csproj
dotnet publish CrosshairY.csproj -c Release -r win-x64 -p:SelfContained=true
```

To build the installer as well, install [Inno Setup 6](https://jrsoftware.org/isinfo.php) and compile `Installer/CrosshairY.iss` after publishing to `publish/self-contained/`.

## FAQ

### How does it work?

CrosshairY renders a transparent, always-on-top, click-through window with a customizable crosshair graphic. It never interacts with game files or memory.

### Is it safe to use?

Yes. CrosshairY is an external overlay that does **not** modify game files or access game memory. That said, anti-cheat policies differ per game, so use it at your own discretion.

### Can this be abused?

CrosshairY only provides a visual overlay and no gameplay advantage beyond a custom crosshair. It's designed for accessibility and convenience.

### The crosshair disappeared, what do I do?

Toggle it with your hotkey. If it keeps happening, open an issue with the details below.

### How do I report an issue?

Open a [GitHub Issue](https://github.com/tsgsOFFICIAL/CrosshairY/issues) or contact me on [Discord](https://discord.gg/Cddu5aJ). Include:

- OS version
- Game and resolution/window mode
- Steps to reproduce the bug

## Support the Project & the creator

This tool is (and always will be) **100% free**.
If it's helping you land shots and you want to fuel more late-night coding sessions, then hit that button and become a legend.

[![Support on Ko-fi](https://img.shields.io/badge/Support%20me%20on%20Ko--fi-F16061?logo=ko-fi&logoColor=white)](https://ko-fi.com/tsgsOFFICIAL)

## How to Contribute

Bugs, ideas, docs, code, all of it helps. A few ground rules so we don't end up untangling a mess later:

**Don't:**

- Rebrand or republish this as your own, keep attribution to me (tsgsOFFICIAL) and this repo intact
- Strip license headers, credits, or "made by" notices when sharing builds or forks
- Drop a giant PR that rewrites half the codebase with no issue or discussion first
- Bundle unrelated changes (formatting sweeps, dependency bumps, drive-by refactors) into a feature fix

**Do:**

- Open an issue first for anything bigger than a small fix, so we agree on the approach before you sink hours into a PR
- Throw a "what if we..." into [GitHub Issues](https://github.com/tsgsOFFICIAL/CrosshairY/issues) or [Discord](https://discord.gg/Cddu5aJ) even if you're not sure it's worth a full PR
- Keep PRs focused, one logical change per PR with a clear "what and why"
- Test on Windows before submitting (WPF, no Linux/macOS builds)
- Match the existing naming, structure, and code style

### (Suggested) Workflow

1. Fork [tsgsOFFICIAL/CrosshairY](https://github.com/tsgsOFFICIAL/CrosshairY)
2. Branch off (`fix/overlay-topmost`, `feature/new-style`, or whatever fits)
3. Make your changes and confirm it still builds: `dotnet build CrosshairY.csproj -c Release`
4. Open a PR against `master`. Short summary, link the issue if there is one

Want to share a crosshair with everyone? Add it to [CommunityCrosshairs.json](CommunityCrosshairs.json) with a PR.

## Star History

<a href="https://www.star-history.com/#tsgsOFFICIAL/CrosshairY&type=date&legend=top-left">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=tsgsOFFICIAL/CrosshairY&type=date&theme=dark&legend=top-left" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=tsgsOFFICIAL/CrosshairY&type=date&legend=top-left" />
   <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=tsgsOFFICIAL/CrosshairY&type=date&legend=top-left" />
 </picture>
</a>

## License

[MIT License](LICENSE), see the file for details.

**Disclaimer**: Use at your own risk. Not affiliated with any game developer or CrosshairX.

---

**Made with rage, caffeine, and zero sleep by tsgsOFFICIAL**
Happy aiming! 🎯
