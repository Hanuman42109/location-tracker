# HeatMap Tracker (.NET MAUI)

Tracks the user's location, stores every sample in SQLite, and draws the history as a heat map
on top of the .NET MAUI Maps control.

## Project layout
| Path | Purpose |
|---|---|
| `Controls/HeatMap.cs` | `Map` subclass: bins points into a grid and draws blue->red `Circle`s |
| `Services/LocationTracker.cs` | Permission + foreground GPS listening (MAUI Essentials) |
| `Services/LocationRepository.cs` | SQLite persistence (sqlite-net-pcl) |
| `ViewModels/MainViewModel.cs` | MVVM glue (CommunityToolkit.Mvvm) |
| `Views/MainView.xaml` | The single page (map + Start/Stop/Clear) |
| `Platforms/Android/AndroidManifest.xml` | Permissions + **Google Maps API key** |
| `Platforms/Android/StatusBarShellRenderer.cs` | Extends the purple title bar behind the status bar |
| `Platforms/iOS/Info.plist` | Location usage description |

## Setting up your computer

### Windows (Visual Studio)
1. Download **Visual Studio** (the free Community edition is enough) from
   https://visualstudio.microsoft.com/downloads/.
2. In the Visual Studio Installer, tick the **.NET Multi-platform App UI development** workload
   and click **Install**. This installs the .NET SDK, the Android SDK and the Android emulator.
3. Create an emulator: **Tools > Android > Android Device Manager > New**. Choose a phone
   (e.g. Pixel 9) with a recent API level and a system image that includes **Google APIs**
   (needed for Google Maps), then **Create** and **Start**.

### macOS
Visual Studio for Mac was retired by Microsoft in August 2024. Use instead:
1. Install the **.NET SDK** from https://dotnet.microsoft.com/download.
2. Install **Xcode** from the Mac App Store (required for iOS builds and the iOS simulator).
3. Install **Visual Studio Code** and its **.NET MAUI** extension.
4. In a terminal, run `dotnet workload install maui`.

## Before you run
1. **Google Maps API key (Android only).** iOS uses Apple Maps and needs no key.
   1. Go to https://console.cloud.google.com/, create a project and enable **Maps SDK for Android**.
   2. **APIs & Services > Credentials > Create credentials > API key**.
   3. Replace `YOUR_GOOGLE_MAPS_API_KEY` in `Platforms/Android/AndroidManifest.xml` with the key.
   Without a valid key the app still runs and records points, but the map stays blank.
2. If you don't have the .NET 10 SDK, change `net10.0-*` to `net9.0-*` / `net8.0-*` in the `.csproj`.

## Running
Open `HeatMapTracker.slnx` in Visual Studio, pick the emulator in the run-target drop-down and press
**F5**. NuGet restores the packages (sqlite-net-pcl, Microsoft.Maui.Controls.Maps,
CommunityToolkit.Mvvm) automatically. Grant the location permission when asked.

## Testing without walking around
* Android emulator: "..." (Extended controls) > Location > load a GPX/KML route and press play.
* iOS simulator: Features > Location > Freeway Drive / City Run.
Tap "Start tracking" first, then play the route.

## Next steps
* Background tracking: Android foreground service (`FOREGROUND_SERVICE_LOCATION`,
  `ACCESS_BACKGROUND_LOCATION`) and iOS `UIBackgroundModes = location`.
* Replace the circle heat map with a native tile overlay for very large datasets.
