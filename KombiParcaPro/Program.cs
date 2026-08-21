using System.Globalization;

namespace KombiParcaPro;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("tr-TR");
        ApplicationConfiguration.Initialize();
        Database.Initialize();
        Application.Run(new MainForm());
    }
}
