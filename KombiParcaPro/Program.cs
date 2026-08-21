using System.Globalization;

namespace KombiParcaPro;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("tr-TR");ApplicationConfiguration.Initialize();Database.Initialize();Application.Run(new MainForm());
        }
        catch(Exception ex)
        {
            var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"KombiParcaPro");Directory.CreateDirectory(folder);var log=Path.Combine(folder,"startup-error.txt");File.WriteAllText(log,DateTime.Now+Environment.NewLine+ex);MessageBox.Show("Program başlatılamadı. Hata kaydı:\n"+log+"\n\n"+ex.Message,"KombiParcaPro",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
    }
}
