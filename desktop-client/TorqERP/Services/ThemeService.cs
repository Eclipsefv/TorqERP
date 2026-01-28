using MudBlazor;
namespace TorqERP.Services;

public class TorqThemeService
{
    public bool IsDarkMode => true;

    public MudTheme Theme { get; } = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#3eaf7c",
            Secondary = "#82b1ff",
            Background = "#1a1a27",
            Surface = "#1e1e2d",
            AppbarBackground = "#1a1a27",
            DrawerBackground = "#1a1a27",
            TextPrimary = "#e1e1e1",
            DrawerText = "#e1e1e1",
            ActionDefault = "#3eaf7c"
        }
    };
}