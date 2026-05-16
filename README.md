# SimTools

> A desktop utility for fans of The Sims and SimCity series. SimTools brings together GPU configuration tools, game tweaks, mod framework setup, and directory management into a single, easy-to-use application — available in 9 languages.

> ⚠️ **This project is in early development.** Features and structure are actively evolving. Contributions and feedback are welcome.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [What Do You Get?](#what-do-you-get)
- [Tools](#tools)
- [Tweaks](#tweaks)
- [Modding Tools](#modding-tools)
- [Bug Fixes](#bug-fixes)
- [Supported Games](#supported-games)
- [Compatibility](#compatibility)
- [Requirements](#requirements)
- [Installation](#installation)
- [Building from Source](#building-from-source)
- [Localisation](#localisation)
  - [Supported Languages](#supported-languages)
  - [Language File Format](#language-file-format)
  - [Adding a New Language](#adding-a-new-language)
- [Project Structure](#project-structure)
- [Configuration File](#configuration-file)
- [Technical Notes](#technical-notes)
- [Contributing](#contributing)
- [Easter Egg](#easter-egg)
- [License](#license)

---

## Overview

SimTools is a Windows desktop application built with WPF (.NET 8) that serves as a one-stop toolkit for players of the Sims and SimCity game series. Many of these games are years or decades old and require manual configuration steps — such as editing graphics rules files or installing compatibility patches — that are not obvious to average users. SimTools simplifies this by providing guided downloads, clear instructions, and direct links to the right tools for each game.

SimTools was previously known as **TS3Tools**, and while its roots are firmly in The Sims 3, the scope has since expanded to cover a wide range of Maxis titles across both The Sims and SimCity franchises. The application is fully localised, supports right-to-left layouts for Arabic, and stores all user preferences in a plain-text INI file that sits alongside the executable.

---

## Features

SimTools offers a comprehensive set of tools, fixes, and guides for getting the most out of your Sims installations:

- **Update GPU Graphics Rules** — Add support for newer graphics cards to the game engine so the game correctly identifies and leverages your hardware.
- **Alder Lake CPU Support** — Install a patch for Intel Alder Lake CPU users who experience immediate crashes to desktop due to the game's incompatibility with hybrid core architectures.
- **Lazy Duchess' Smooth Patch & Enhanced Launcher** — Install engine tweaks for faster CAS loading, reduced lag, and an enhanced EA 1.69 launcher with ASI Loading and CC disable features.
- **nRaas Tweaks** — Install nRaas core mods to mitigate in-game script errors and provide greater debugging control over the game engine.
- **Game Config Guides** — Follow comprehensive guides to manually tweak game configuration files and allow the engine to harness more power from your system.
- **UI Resolution Scaling** — Stretch the in-game UI to fit FHD (1440p) and UHD (2160p) resolutions using the TinyUI Fix PowerShell utility.
- **Loading Optimisation** — Find the best in-game settings to speed up loading into neighbourhoods and reduce lot placement times.
- **Katy Perry Sweet Treats Migration** — Migrate Katy Perry's Sweet Treats from EA to Steam or Retail without losing your content.
- **Expansion & Stuff Pack Deals** — Find various exclusive deals on expansions and stuff packs.
- **Official Patches** — Install patches for the Base Game through to Outdoor Living Stuff (Retail installations only).
- **Simler90's Engine Tweaks** — Install a curated set of engine-level tweaks and fixes authored by Simler90 to reduce lag and resolve long-standing issues.
- **58 Gameplay Fix Mods** — Install a growing collection of individual gameplay fixes for the Base Game, Expansion Packs, and Store items — with more fixes added in future versions.
- **Regul's Save Cleaner** — Install and run Regul's Save Cleaner to debloat saves and keep your game running smoothly over long playthroughs.
- **Curated Mod Browser** — View and download various curated and recommended mods from within the application.
- **Daily Deal Guide** — In-depth guide to obtaining Store items at the best price using the Daily Deal rotation spreadsheet.

---

## What Do You Get?

Installing every tweak and bugfix SimTools has to offer results in a noticeably improved game experience:

- ⚡ **Faster world loading** from the main menu
- ⚡ **Faster lot placement loading**
- ⚡ **Quicker CAS entry** — loading into Create-A-Sim is significantly faster
- ⚡ **Faster CAS clothing browsing** — loading and scrolling through clothing items is smoother
- 🛡️ **Improved stability** — nRaas ErrorTrap and Overwatch will attempt to intercept and stop erroring scripts before they crash your game
- 🔧 **Fewer bugged items** — the included fix mods address a wide range of broken items, missed developer oversights, and broken interactions

---

## Tools

| Tool | Description |
|---|---|
| **GPU Addon** | Allows users to add support for newer graphics cards to the game engine, ensuring the game correctly identifies and uses modern hardware. |
| **Regul Save Cleaner** | Keeps saves in check by scanning for and removing save bloat that accumulates over time, helping maintain game performance in long-running saves. |
| **Mod Framework** | A vetted and prepared version of the TS3 Mod Framework, specifically configured for use with SimTools to ensure maximum compatibility. |

---

## Tweaks

| Tweak | Description |
|---|---|
| **Alder Lake Patcher** | Patches the game executable to run on Intel Alder Lake CPUs, which otherwise cause immediate crashes to desktop due to the game's incompatibility with hybrid core scheduling. |
| **nRaas Core Mods** | A set of nRaas mods included to give users greater control over debugging and engine behaviour, as well as catching and handling script errors before they cause crashes. |
| **Smooth Patch** | Engine-level tweaks by Lazy Duchess that enable faster loading into CAS and smooth out a number of other lag-inducing elements throughout the game. |
| **TinyUI Fix** | A PowerShell utility that stretches and enhances the in-game UI to properly fit FHD (1440p) and UHD (2160p) display resolutions. |
| **LazyDuchess Launcher** | An enhanced replacement launcher for EA 1.69 installations of The Sims 3, featuring ASI Loading support and the ability to disable CC directly from the launcher. |

---

## Modding Tools

| Tool | Description |
|---|---|
| **Create-a-World Tool 1.67 / 1.69** | EA's official Create-A-World tool for The Sims 3, used for creating and editing custom neighbourhoods and worlds. |
| **S3PE** | A package editing utility for reading, editing, and extracting resources from Sims 3 `.package` files. |
| **Sims3Pack Multi Installer** | A utility for extracting `.Sims3Pack` files down to their base elements for editing, staging, or repacking. |
| **S3Dash** | A tool for detecting conflicts and errors between installed mods, helping diagnose issues caused by incompatible packages. |
| **ShowtimeCE** | A conversion utility for converting the Showtime Collector's Edition content. |

---

## Bug Fixes

SimTools ships with two categories of fixes:

### Simler90's Engine Tweaks
A collection of low-level engine tweaks and fixes authored by community member Simler90, targeting a range of known issues, performance regressions, and instabilities in the base game engine.

### 58 Gameplay Fix Mods
A curated set of **58 individual fix mods** (with more coming in future versions) addressing:

- Broken or bugged items in the Base Game, Expansion Packs, and Store content
- Developer oversights that were never officially patched
- Broken interactions between Sims or between Sims and objects
- Compatibility issues between specific content items

Each fix mod is listed individually in the included CC tab within the application, so you can review exactly what each one addresses before installing. Only install fixes for Expansion Packs and Store content you actually have installed.

---

## Supported Games

SimTools covers 14 titles across The Sims and SimCity franchises.

| Series | Title |
|---|---|
| The Sims | The Sims |
| The Sims | The Sims 2 |
| The Sims | The Sims Life Stories |
| The Sims | The Sims Pet Stories |
| The Sims | The Sims Castaway Stories |
| The Sims | The Sims 3 |
| The Sims | The Sims 4 |
| The Sims | The Sims Medieval |
| SimCity | SimCopter |
| SimCity | Streets of SimCity |
| SimCity | SimCity 2000 |
| SimCity | SimCity 3000 Unlimited |
| SimCity | SimCity 4 Deluxe |
| SimCity | SimCity (2013) |

The majority of tools, tweaks, and fixes in the current release are focused on **The Sims 3**. Support for additional titles is planned for future versions.

---

## Compatibility

SimTools is compatible with the following Sims 3 distribution platforms:

| Platform | Supported |
|---|---|
| EA App (1.67 / 1.69) | ✅ |
| Steam | ✅ |
| Retail (disc / retail digital) | ✅ |

SimTools can be installed on top of an existing **1.67 or 1.69** installation. A **clean install of The Sims 3 is strongly recommended** before running SimTools for the best results.

You can run SimTools regardless of which Expansion Packs or Stuff Packs you have installed — just be mindful to only install fixes and patches for packs you actually own.

---

## Requirements

- **Operating System:** Windows 10 or later (64-bit recommended)
- **Runtime:** [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) — required to run the application
- **Internet Connection:** Required for first-time tool and patch downloads; not needed after files are cached locally
- **Visual Studio 2022 or later:** Required only if building from source

---

## Installation

1. Download the latest release from the [Releases](https://github.com/dbrown1986/SimTools/releases) page
2. Extract the ZIP to a folder of your choice (e.g. `C:\Tools\SimTools\`)
3. Run `SimTools_v4.exe`
4. On first launch, select your preferred language
5. Read through the introductory page and click **Continue**
6. Optionally open **Settings** (⚙ button, top-right) to configure your game directories

No installer is required. SimTools is fully portable — all settings are written to `settings.ini` in the same folder as the executable.

---

## Building from Source

### Clone the repository

```bash
git clone https://github.com/dbrown1986/SimTools.git
cd SimTools
```

### Build with the .NET CLI

```bash
dotnet build SimTools_v4.sln --configuration Release
```

Output is placed in `bin/Release/net8.0-windows/`.

### Build with Visual Studio

1. Open `SimTools_v4.sln` in Visual Studio 2022 or later
2. Select the **Release** configuration from the toolbar
3. Press **Ctrl+Shift+B** to build, or **F5** to build and run

### NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `WpfAnimatedGif` | 2.0.2 | Renders the animated plumbob GIF on the splash screen |

---

## Localisation

### Supported Languages

| Code | Language | Script | Direction |
|---|---|---|---|
| `ar` | عربي (Arabic) | Arabic | Right-to-left |
| `zh` | 中国人 (Chinese Simplified) | Han | Left-to-right |
| `de` | Deutsch (German) | Latin | Left-to-right |
| `en` | English | Latin | Left-to-right |
| `es` | Español (Spanish) | Latin | Left-to-right |
| `fr` | Français (French) | Latin | Left-to-right |
| `ja` | 日本語 (Japanese) | CJK | Left-to-right |
| `pt` | Português (Portuguese) | Latin | Left-to-right |
| `ru` | Русский (Russian) | Cyrillic | Left-to-right |

Arabic is the only currently supported RTL language. When Arabic is selected, the entire main window layout flips to a right-to-left flow direction automatically.

### Language File Format

Language files use a simple INI-style format with named sections and key=value pairs. They live in the `Languages/` folder next to the executable and are plain UTF-8 text files — no compilation step is needed.

```ini
[Main]
Btn_GPU=GPU Support >
GPU_Description=Use these tools to identify and install support for your GPU.

[Messages]
Error_Title=Error
Error_OpenLink=Unable to open link: {0}

[Splash]
Loading=v 4.0.1.3868
Retic=Reticulating Splines...
```

Placeholders like `{0}` are replaced at runtime using `string.Format()`. Do not change or remove them when translating.

The following sections must be present in every language file:

| Section | Purpose |
|---|---|
| `[Main]` | Main window button labels and description text |
| `[Messages]` | Error dialogs, warning dialogs |
| `[ContextMenu]` | Right-click context menu item labels |
| `[Language]` | Language selection window text |
| `[Settings]` | Settings window labels and buttons |
| `[Download]` | Download progress window title and status text |
| `[Splash]` | Splash screen version label and typing animation text |

### Adding a New Language

1. Copy `Languages/en.lang` to `Languages/xx.lang`, where `xx` is the [ISO 639-1 language code](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) (e.g. `it` for Italian)
2. Translate every value on the right-hand side of each `=`. **Do not change the keys or section names.**
3. Preserve all `{0}`, `{1}`, etc. placeholders — they are replaced at runtime
4. Add the language to the `Languages` array in `SettingsWindow.cs`:
   ```csharp
   ("it", "Italiano"),
   ```
5. Add a button for it in `LanguageSelectionWindow.xaml` inside the `<UniformGrid>`:
   ```xml
   <Button Content="Italiano" Tag="it" Click="Language_Click"
           Style="{StaticResource LangBtn}" Width="140" Height="35" Foreground="Black">
       <Button.Background>
           <ImageBrush ImageSource="/Images/button_normal.png"/>
       </Button.Background>
   </Button>
   ```
6. If the language reads right-to-left, add its code to the `rtlLanguages` HashSet in `MainWindow.xaml.cs`:
   ```csharp
   var rtlLanguages = new HashSet<string> { "ar", "xx" };
   ```
7. Rebuild the project. The new language will appear in both the language selection screen and the Settings dropdown.

---

## Project Structure

```
SimTools/
│
├── Images/                               # Compiled into the assembly as WPF resources
│   ├── button_normal.png                 # Default button background (grey)
│   ├── button_yellow.png                 # Recommended action button background
│   ├── button_green.png                  # Normal action button background
│   ├── button_red.png                    # Highly recommended / warning button background
│   ├── close-button.png                  # Custom close button (top-right of main window)
│   ├── Menu_Main.png                     # Main window background image
│   ├── plumbob.gif                       # Animated plumbob (splash screen + main window)
│   ├── SimTools_Logo.png                 # Application logo
│   ├── sulsul.png                        # Sul Sul character image (introductory page)
│   ├── TS3_Box2.png                      # Language selection window background
│   └── White_Bkg.png                     # White gradient overlay (introductory page text area)
│
├── Languages/                            # Copied to output directory alongside the .exe
│   ├── en.lang                           # English (base / fallback language)
│   ├── ar.lang                           # Arabic (RTL)
│   ├── zh.lang                           # Chinese Simplified
│   ├── de.lang                           # German
│   ├── es.lang                           # Spanish
│   ├── fr.lang                           # French
│   ├── ja.lang                           # Japanese
│   ├── pt.lang                           # Portuguese
│   └── ru.lang                           # Russian
│
├── Pages/                                # UI windows and supporting classes
│   ├── IntroductoryPage.xaml             # Welcome / introduction page shown before main window
│   ├── IntroductoryPage.xaml.cs          # Continue, What is SimTools? dialog, exit handler
│   ├── LanguageSelectionWindow.xaml      # 3×3 grid language picker layout
│   ├── LanguageSelectionWindow.xaml.cs   # Language click handler, DoNotAskAgain, INI write
│   ├── MainWindow.xaml                   # Main UI layout (840×720, WindowStyle=None)
│   ├── MainWindow.xaml.cs                # Button handlers, context menus, download logic
│   ├── SplashScreenWindow.xaml           # Transparent borderless splash screen layout
│   ├── SplashScreenWindow.xaml.cs        # Fade animations, looping typing animation
│   ├── IniHelper.cs                      # Static INI reader/writer using nested Dictionary
│   └── SettingsWindow.cs                 # Code-only settings window
│
├── App.xaml                              # Application definition (no StartupUri)
├── App.xaml.cs                           # Startup: splash → language → intro → main window
├── AssemblyInfo.cs                       # Assembly-level attributes
├── DownloadProgressWindow.cs             # Code-only download progress window
├── LanguageManager.cs                    # Static .lang file loader; Get() and Format() accessors
│
├── SimTools.ico                          # Application icon
├── SimTools_v4.csproj                    # Project file (.NET 8.0-windows, UseWPF + UseWindowsForms)
└── SimTools_v4.sln                       # Visual Studio solution file
```

---

## Configuration File

SimTools stores all user settings in `settings.ini`, located in the same directory as `SimTools_v4.exe`. The file is created automatically on first run and can be edited manually in any text editor.

```ini
[Language]
SelectedLanguage=en
DoNotAskAgain=false

[Directories]
Sims1_Game=
Sims2_Game=C:\Program Files (x86)\EA Games\The Sims 2
Sims2_Mods=C:\Users\YourName\Documents\EA Games\The Sims 2\Downloads
SimsLifeStories_Game=
SimsLifeStories_Mods=
SimsPetStories_Game=
SimsPetStories_Mods=
SimsCastawayStories_Game=
SimsCastawayStories_Mods=
Sims3_Game=C:\Program Files (x86)\Electronic Arts\The Sims 3
Sims3_Mods=C:\Users\YourName\Documents\Electronic Arts\The Sims 3\Mods
Sims4_Game=
Sims4_Mods=
SimsMedieval_Game=
SimsMedieval_Mods=
SimCopter_Game=
StreetsOfSimCity_Game=
SimCity2000_Game=
SimCity3000_Game=
SimCity4_Game=
SimCity2013_Game=
```

> **Tip:** You do not need to fill in directories for games you do not own. Empty values are simply ignored.

---

## Technical Notes

### WPF + WinForms Interop
SimTools uses both `UseWPF` and `UseWindowsForms` in the project file. This is necessary for the folder browser dialog (`System.Windows.Forms.FolderBrowserDialog`). Because both frameworks define types with the same names (`Button`, `MessageBox`, `Application`, etc.), every affected file uses explicit `using` aliases to resolve ambiguity:

```csharp
using Button     = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
```

### Code-Only Windows
`SettingsWindow` and `DownloadProgressWindow` are built entirely in C# with no XAML files. This sidesteps XAML compilation issues that can arise when WinForms types leak into WPF XAML namespace resolution at build time.

### Context Menu Timing
The GPU and Tweaks context menus are built once in `ApplyLanguage()` rather than in the `ContextMenuOpening` event. This prevents a bug where left-clicking the button fires the event handler and immediately launches the first menu item.

### Download Caching
Downloaded files are stored in the `install/` subfolder relative to the executable. If a file already exists at the target path, the download step is skipped and the file is launched directly. To force a re-download, delete the file from `install/`.

### Transparent Splash Screen
`AllowsTransparency="True"` must be combined with `WindowStyle="None"` for a WPF window to render with a transparent background. Without it, `Background="{x:Null}"` still produces a solid black window.

### Shutdown Lifecycle
During the splash → language → intro → main window transition, `ShutdownMode` is set to `OnExplicitShutdown` to prevent WPF from exiting prematurely when intermediate windows close. After each window, `Dispatcher.HasShutdownStarted` is checked so that clicking the exit button on any window stops the chain cleanly without exceptions.

---

## Contributing

Contributions are welcome. If you would like to add support for a new game, tool, language, or fix mod:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-new-tool`)
3. Make your changes
4. Test on at least one clean Windows 10 or 11 machine
5. Submit a pull request with a clear description of what was added or changed

If you are adding a new language file, ensure all sections and keys from `en.lang` are present, even if some values are temporarily marked `(Translation needed)`.

Bug reports and feature requests can be submitted via the [Issues](https://github.com/dbrown1986/SimTools/issues) page.

---

## Easter Egg

There is a hidden easter egg somewhere within SimTools — tucked away on one specific sub-page. Props to anyone who manages to find it. If you do, feel free to mention it on the Discord.

---

## License

SimTools is released under the terms described in [LICENSE.txt](LICENSE.txt). Please read it before distributing modified versions.
