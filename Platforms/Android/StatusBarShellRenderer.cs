using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace HeatMapTracker;

/// <summary>
/// Shell renderer that extends the title bar colour up behind the system status bar.
/// </summary>
/// <remarks>
/// On Android 15+ apps are drawn edge-to-edge: the Shell header (an <c>AppBarLayout</c>) starts at
/// the very top of the screen and pads its toolbar down by the status bar height. Shell only colours
/// the toolbar, so the padded strip above it keeps the theme's default dark colour. This renderer
/// applies the Shell background colour to the <c>AppBarLayout</c> as well.
/// </remarks>
public class StatusBarShellRenderer : ShellRenderer
{
    protected override IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker()
        => new StatusBarToolbarAppearanceTracker(this);

    private sealed class StatusBarToolbarAppearanceTracker(IShellContext shellContext)
        : ShellToolbarAppearanceTracker(shellContext)
    {
        public override void SetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
        {
            base.SetAppearance(toolbar, toolbarTracker, appearance);

            // The toolbar's parent is the AppBarLayout that sits behind the status bar.
            if (toolbar.Parent is Android.Views.View appBar && appearance.BackgroundColor is not null)
            {
                appBar.SetBackgroundColor(appearance.BackgroundColor.ToPlatform());
            }
        }
    }
}
