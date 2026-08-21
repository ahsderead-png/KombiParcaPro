using Microsoft.Data.Sqlite;
using System.Data;

namespace KombiParcaPro;

internal static class Database
{
    private static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KombiParcaPro");
    private static readonly string DbPath = Path.Combine(Folder, "kombiparcapro.db");
    private static string ConnectionString => $"Data Source={DbPath}";

    public static void Initialize()
    {
        Directory.CreateDirectory(Folder);
        using var c = new SqliteConnection(ConnectionString); c.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS Parts(
          Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Category TEXT NOT NULL,
          Brand TEXT NOT NULL DEFAULT '', Model TEXT NOT NULL DEFAULT '', OemCode TEXT NOT NULL DEFAULT '',
          EquivalentCode TEXT NOT NULL DEFAULT '', Supplier TEXT NOT NULL DEFAULT '', Shelf TEXT NOT NULL DEFAULT '',
          Unit TEXT NOT NULL DEFAULT 'Adet', Stock REAL NOT NULL DEFAULT 0, MinStock REAL NOT NULL DEFAULT 0,
          PurchasePrice REAL NOT NULL DEFAULT 0, SalePrice REAL NOT NULL DEFAULT 0, Notes TEXT NOT NULL DEFAULT '', UpdatedAt TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS Movements(
          Id INTEGER PRIMARY KEY AUTOINCREMENT, PartId INTEGER NOT NULL, MovementType TEXT NOT NULL,
          Quantity REAL NOT NULL, Description TEXT NOT NULL DEFAULT '', CreatedAt TEXT NOT NULL,
          FOREIGN KEY(PartId) REFERENCES Parts(Id));
        CREATE TABLE IF NOT EXISTS Suppliers(
          Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Phone TEXT NOT NULL DEFAULT '',
          Website TEXT NOT NULL DEFAULT '', Address TEXT NOT NULL DEFAULT '', Notes TEXT NOT NULL DEFAULT '');
        """;
        cmd.ExecuteNonQuery();
        Seed(c);
    }

    private static void Seed(SqliteConnection c)
    {
        using var count = c.CreateCommand(); count.CommandText = "SELECT COUNT(*) FROM Parts";
        if (Convert.ToInt32(count.ExecuteScalar()) > 0) return;
        var rows = new[] {
          ("Sirkülasyon Pompası", "Pompa", "Üniversal", "Yoğuşmalı kombiler", "", "", 2d, 1d),
          ("Üç Yollu Vana Tamir Takımı", "Hidrolik Grup", "Muadil", "Çeşitli modeller", "", "", 5d, 2d),
          ("NTC Sıcaklık Sensörü", "Sensör", "Üniversal", "10 kΩ", "", "", 10d, 4d),
          ("Hava Basınç Prosestatı", "Sensör", "Üniversal", "Yoğuşmasız kombi", "", "", 3d, 1d),
          ("Emniyet Ventili 3 bar", "Hidrolik Grup", "Üniversal", "1/2 inç", "", "", 6d, 2d),
          ("Ateşleme Elektrodu", "Yanma Grubu", "Muadil", "Çeşitli modeller", "", "", 4d, 2d),
          ("İyonizasyon Elektrodu", "Yanma Grubu", "Muadil", "Çeşitli modeller", "", "", 4d, 2d),
          ("Otomatik Hava Purjörü", "Hidrolik Grup", "Üniversal", "", "", "", 5d, 2d)
        };
        foreach (var r in rows) {
            using var q = c.CreateCommand();
            q.CommandText = "INSERT INTO Parts(Name,Category,Brand,Model,OemCode,EquivalentCode,Stock,MinStock,UpdatedAt) VALUES($n,$c,$b,$m,$o,$e,$s,$min,$u)";
            q.Parameters.AddWithValue("$n", r.Item1); q.Parameters.AddWithValue("$c", r.Item2); q.Parameters.AddWithValue("$b", r.Item3); q.Parameters.AddWithValue("$m", r.Item4); q.Parameters.AddWithValue("$o", r.Item5); q.Parameters.AddWithValue("$e", r.Item6); q.Parameters.AddWithValue("$s", r.Item7); q.Parameters.AddWithValue("$min", r.Item8); q.Parameters.AddWithValue("$u", DateTime.Now.ToString("s")); q.ExecuteNonQuery();
        }
    }

    public static DataTable Search(string term, bool lowOnly)
    {
        using var c = new SqliteConnection(ConnectionString); c.Open(); using var cmd = c.CreateCommand();
        cmd.CommandText = """
        SELECT Id, Name AS 'Parça Adı', Category AS 'Kategori', Brand AS 'Marka', Model AS 'Model/Uyumluluk',
        OemCode AS 'OEM Kodu', EquivalentCode AS 'Muadil Kodu', Stock AS 'Stok', Unit AS 'Birim',
        MinStock AS 'Asgari Stok', SalePrice AS 'Satış Fiyatı', Shelf AS 'Raf', Supplier AS 'Tedarikçi'
        FROM Parts WHERE ($q='' OR Name LIKE $like OR Category LIKE $like OR Brand LIKE $like OR Model LIKE $like OR OemCode LIKE $like OR EquivalentCode LIKE $like)
        AND ($low=0 OR Stock<=MinStock) ORDER BY Name
        """;
        cmd.Parameters.AddWithValue("$q", term); cmd.Parameters.AddWithValue("$like", $"%{term}%"); cmd.Parameters.AddWithValue("$low", lowOnly ? 1 : 0);
        using var reader = cmd.ExecuteReader(); var table = new DataTable(); table.Load(reader); return table;
    }

    public static void Save(Part p)
    {
        using var c = new SqliteConnection(ConnectionString); c.Open(); using var q = c.CreateCommand();
        q.CommandText = p.Id == 0 ?
          "INSERT INTO Parts(Name,Category,Brand,Model,OemCode,EquivalentCode,Supplier,Shelf,Unit,Stock,MinStock,PurchasePrice,SalePrice,Notes,UpdatedAt) VALUES($n,$c,$b,$m,$o,$e,$sp,$sh,$u,$st,$mi,$pp,$sa,$no,$up)" :
          "UPDATE Parts SET Name=$n,Category=$c,Brand=$b,Model=$m,OemCode=$o,EquivalentCode=$e,Supplier=$sp,Shelf=$sh,Unit=$u,Stock=$st,MinStock=$mi,PurchasePrice=$pp,SalePrice=$sa,Notes=$no,UpdatedAt=$up WHERE Id=$id";
        q.Parameters.AddWithValue("$id", p.Id); q.Parameters.AddWithValue("$n", p.Name); q.Parameters.AddWithValue("$c", p.Category); q.Parameters.AddWithValue("$b", p.Brand); q.Parameters.AddWithValue("$m", p.Model); q.Parameters.AddWithValue("$o", p.OemCode); q.Parameters.AddWithValue("$e", p.EquivalentCode); q.Parameters.AddWithValue("$sp", p.Supplier); q.Parameters.AddWithValue("$sh", p.Shelf); q.Parameters.AddWithValue("$u", p.Unit); q.Parameters.AddWithValue("$st", p.Stock); q.Parameters.AddWithValue("$mi", p.MinStock); q.Parameters.AddWithValue("$pp", p.PurchasePrice); q.Parameters.AddWithValue("$sa", p.SalePrice); q.Parameters.AddWithValue("$no", p.Notes); q.Parameters.AddWithValue("$up", DateTime.Now.ToString("s")); q.ExecuteNonQuery();
    }

    public static Part? Get(int id)
    {
        using var c = new SqliteConnection(ConnectionString); c.Open(); using var q = c.CreateCommand(); q.CommandText = "SELECT * FROM Parts WHERE Id=$id"; q.Parameters.AddWithValue("$id", id); using var r = q.ExecuteReader();
        if (!r.Read()) return null;
        return new Part { Id=id, Name=r.GetString(r.GetOrdinal("Name")), Category=r.GetString(r.GetOrdinal("Category")), Brand=r.GetString(r.GetOrdinal("Brand")), Model=r.GetString(r.GetOrdinal("Model")), OemCode=r.GetString(r.GetOrdinal("OemCode")), EquivalentCode=r.GetString(r.GetOrdinal("EquivalentCode")), Supplier=r.GetString(r.GetOrdinal("Supplier")), Shelf=r.GetString(r.GetOrdinal("Shelf")), Unit=r.GetString(r.GetOrdinal("Unit")), Stock=r.GetDouble(r.GetOrdinal("Stock")), MinStock=r.GetDouble(r.GetOrdinal("MinStock")), PurchasePrice=r.GetDouble(r.GetOrdinal("PurchasePrice")), SalePrice=r.GetDouble(r.GetOrdinal("SalePrice")), Notes=r.GetString(r.GetOrdinal("Notes")) };
    }

    public static void MoveStock(int id, double qty, string type, string note)
    {
        var delta = type == "Giriş" ? qty : -qty; using var c = new SqliteConnection(ConnectionString); c.Open(); using var tx = c.BeginTransaction();
        using var u = c.CreateCommand(); u.Transaction=tx; u.CommandText="UPDATE Parts SET Stock=Stock+$d,UpdatedAt=$u WHERE Id=$id AND Stock+$d>=0"; u.Parameters.AddWithValue("$d",delta); u.Parameters.AddWithValue("$u",DateTime.Now.ToString("s")); u.Parameters.AddWithValue("$id",id); if(u.ExecuteNonQuery()==0) throw new InvalidOperationException("Stok eksi değere düşemez.");
        using var m=c.CreateCommand(); m.Transaction=tx; m.CommandText="INSERT INTO Movements(PartId,MovementType,Quantity,Description,CreatedAt) VALUES($id,$t,$q,$n,$d)"; m.Parameters.AddWithValue("$id",id);m.Parameters.AddWithValue("$t",type);m.Parameters.AddWithValue("$q",qty);m.Parameters.AddWithValue("$n",note);m.Parameters.AddWithValue("$d",DateTime.Now.ToString("s"));m.ExecuteNonQuery();tx.Commit();
    }
}

internal sealed class Part
{
    public int Id { get; set; } public string Name { get; set; }=""; public string Category { get; set; }=""; public string Brand { get; set; }=""; public string Model { get; set; }=""; public string OemCode { get; set; }=""; public string EquivalentCode { get; set; }=""; public string Supplier { get; set; }=""; public string Shelf { get; set; }=""; public string Unit { get; set; }="Adet"; public double Stock { get; set; } public double MinStock { get; set; } public double PurchasePrice { get; set; } public double SalePrice { get; set; } public string Notes { get; set; }="";
}
