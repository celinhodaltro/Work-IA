using MudBlazor;

namespace Work_IA.BlazorDashboard.Theme;

public sealed class AIOfficeTheme
{
    public static MudTheme Create() => new()
    {
        Palette = new Palette()
        {
            Primary = new("#1976D2"),
            Secondary = new("#7C4DFF"),
            Tertiary = new("#00E676"),
            Error = new("#FF5252"),
            Warning = new("#FFD740"),
            Info = new("#40C4FF"),
            Success = new("#69F0AE"),
            Surface = new("#1E1E2E"),
            Background = new("#121220"),
            Dark = new("#0A0A14"),
            AppbarBackground = new("#1976D2"),
            DrawerBackground = new("#1E1E2E"),
            DrawerText = new("#FFFFFF"),
            TextPrimary = new("#FFFFFF"),
            TextSecondary = new("#B0B0C0")
        },
        PaletteDark = new PaletteDark()
        {
            Primary = new("#2196F3"),
            Secondary = new("#9C27B0"),
            Tertiary = new("#69F0AE"),
            Error = new("#FF5252"),
            Warning = new("#FFD740"),
            Info = new("#40C4FF"),
            Success = new("#69F0AE"),
            Surface = new("#1E1E2E"),
            Background = new("#121220"),
            Dark = new("#0A0A14"),
            AppbarBackground = new("#0D47A1"),
            DrawerBackground = new("#1E1E2E"),
            DrawerText = new("#FFFFFF"),
            TextPrimary = new("#FFFFFF"),
            TextSecondary = new("#B0B0C0")
        },
        Typography = new Typography()
        {
            Default = new Default()
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Helvetica Neue", "sans-serif" },
                FontSize = "14px"
            },
            H1 = new H1() { FontSize = "32px", FontWeight = 700 },
            H2 = new H2() { FontSize = "24px", FontWeight = 600 },
            H3 = new H3() { FontSize = "20px", FontWeight = 600 },
            H4 = new H4() { FontSize = "16px", FontWeight = 600 },
            H5 = new H5() { FontSize = "14px", FontWeight = 500 },
            H6 = new H6() { FontSize = "13px", FontWeight = 500 },
            Button = new Button() { TextTransform = "none", FontWeight = 500 }
        },
        Shadows = new Shadow()
        {
            Elevation = new string[]
            {
                "none",                                           // 0
                "0 1px 3px rgba(0,0,0,0.12)",                    // 1
                "0 3px 6px rgba(0,0,0,0.15)",                    // 2
                "0 6px 12px rgba(0,0,0,0.18)",                   // 3
                "0 8px 16px rgba(0,0,0,0.20)",                   // 4
                "0 12px 24px rgba(0,0,0,0.22)",                   // 5
                string.Empty, string.Empty, string.Empty,          // 6,7,8
                string.Empty, string.Empty, string.Empty,          // 9,10,11
                string.Empty, string.Empty, string.Empty,          // 12,13,14
                string.Empty, string.Empty, string.Empty,          // 15,16,17
                string.Empty, string.Empty, string.Empty,          // 18,19,20
                string.Empty, string.Empty, string.Empty, string.Empty // 21,22,23,24
            }
        },
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "240px",
            DrawerWidthRight = "300px"
        }
    };
}
