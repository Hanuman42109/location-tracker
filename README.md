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
| `Platforms/iOS/Info.plist` | Location usage description |

## Before you run
1. Android only: put your Google Maps API key in `Platforms/Android/AndroidManifest.xml`
   (`YOUR_GOOGLE_MAPS_API_KEY`). iOS uses Apple Maps and needs no key.
2. If you don't have the .NET 10 SDK, change `net10.0-*` to `net9.0-*` / `net8.0-*` in the `.csproj`.

## Testing without walking around
* Android emulator: "..." (Extended controls) > Location > load a GPX/KML route and press play.
* iOS simulator: Features > Location > Freeway Drive / City Run.
Tap "Start tracking" first, then play the route.

## Next steps
* Background tracking: Android foreground service (`FOREGROUND_SERVICE_LOCATION`,
  `ACCESS_BACKGROUND_LOCATION`) and iOS `UIBackgroundModes = location`.
* Replace the circle heat map with a native tile overlay for very large datasets.
