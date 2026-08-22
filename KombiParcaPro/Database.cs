using Microsoft.Data.Sqlite;
using System.Data;
using System.Reflection;
using System.Text.Json;

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
        EnsureColumn(c,"CatalogKey","TEXT NOT NULL DEFAULT ''"); EnsureColumn(c,"Technical","TEXT NOT NULL DEFAULT ''");
        EnsureColumn(c,"Compatibility","TEXT NOT NULL DEFAULT ''"); EnsureColumn(c,"Manufacturer","TEXT NOT NULL DEFAULT ''");
        EnsureColumn(c,"ManufacturerCode","TEXT NOT NULL DEFAULT ''"); EnsureColumn(c,"SupplierCode","TEXT NOT NULL DEFAULT ''");
        EnsureColumn(c,"SourceUrl","TEXT NOT NULL DEFAULT ''"); EnsureColumn(c,"ImageUrl","TEXT NOT NULL DEFAULT ''");
        EnsureColumn(c,"Verification","TEXT NOT NULL DEFAULT ''"); EnsureColumn(c,"BrandPriority","INTEGER NOT NULL DEFAULT 99");
        using(var ix=c.CreateCommand()){ix.CommandText="CREATE UNIQUE INDEX IF NOT EXISTS IX_Parts_CatalogKey ON Parts(CatalogKey) WHERE CatalogKey<>''; CREATE TABLE IF NOT EXISTS AppMeta(Key TEXT PRIMARY KEY,Value TEXT NOT NULL);";ix.ExecuteNonQuery();}
        Seed(c);
        ImportCatalog(c);
    }

    static void EnsureColumn(SqliteConnection c,string name,string definition){using var q=c.CreateCommand();q.CommandText="PRAGMA table_info(Parts)";using var r=q.ExecuteReader();while(r.Read())if(r.GetString(1).Equals(name,StringComparison.OrdinalIgnoreCase))return;r.Close();using var a=c.CreateCommand();a.CommandText=$"ALTER TABLE Parts ADD COLUMN {name} {definition}";a.ExecuteNonQuery();}
    static void ImportCatalog(SqliteConnection c){using(var v=c.CreateCommand()){v.CommandText="SELECT Value FROM AppMeta WHERE Key='CatalogVersion'";if(Convert.ToString(v.ExecuteScalar())=="3.0.0")return;}var assembly=Assembly.GetExecutingAssembly();var resources=assembly.GetManifestResourceNames().Where(n=>n.StartsWith("KombiParcaPro.catalog-",StringComparison.Ordinal)&&n.EndsWith(".json",StringComparison.Ordinal)).OrderBy(n=>n).ToArray();if(resources.Length==0)return;using var tx=c.BeginTransaction();foreach(var resource in resources){using var s=assembly.GetManifestResourceStream(resource);if(s is null)continue;var rows=JsonSerializer.Deserialize<List<CatalogRow>>(s)??[];foreach(var x in rows){using var q=c.CreateCommand();q.Transaction=tx;q.CommandText="""INSERT OR IGNORE INTO Parts(CatalogKey,Name,Category,Brand,Model,OemCode,EquivalentCode,Supplier,Unit,Stock,MinStock,Notes,Technical,Compatibility,Manufacturer,ManufacturerCode,SupplierCode,SourceUrl,ImageUrl,Verification,BrandPriority,UpdatedAt) VALUES($k,$n,$c,$b,$m,$o,$e,$s,'Adet',0,0,$no,$t,$co,$ma,$mc,$sc,$u,$i,$v,$p,$d)""";q.Parameters.AddWithValue("$k",x.CatalogKey);q.Parameters.AddWithValue("$n",x.Name);q.Parameters.AddWithValue("$c",x.Category);q.Parameters.AddWithValue("$b",x.BrandDisplay);q.Parameters.AddWithValue("$m",x.Model);q.Parameters.AddWithValue("$o",x.OemCode);q.Parameters.AddWithValue("$e",x.ManufacturerCode);q.Parameters.AddWithValue("$s",x.Supplier);q.Parameters.AddWithValue("$no",x.Verification);q.Parameters.AddWithValue("$t",x.Technical);q.Parameters.AddWithValue("$co",x.Compatibility);q.Parameters.AddWithValue("$ma",x.Manufacturer);q.Parameters.AddWithValue("$mc",x.ManufacturerCode);q.Parameters.AddWithValue("$sc",x.SupplierCode);q.Parameters.AddWithValue("$u",x.SourceUrl);q.Parameters.AddWithValue("$i",x.ImageUrl);q.Parameters.AddWithValue("$v",x.Verification);q.Parameters.AddWithValue("$p",x.Priority);q.Parameters.AddWithValue("$d",DateTime.Now.ToString("s"));q.ExecuteNonQuery();}}using(var meta=c.CreateCommand()){meta.Transaction=tx;meta.CommandText="INSERT OR REPLACE INTO AppMeta(Key,Value) VALUES('CatalogVersion','3.0.0')";meta.ExecuteNonQuery();}tx.Commit();}

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

    public static DataTable Search(string term, bool lowOnly, string brand="Tüm Markalar")
    {
        using var c = new SqliteConnection(ConnectionString); c.Open(); using var cmd = c.CreateCommand();
        cmd.CommandText = """
        SELECT Id, Name AS 'Parça Adı', Category AS 'Kategori', Brand AS 'Marka', Model AS 'Model/Uyumluluk',
        OemCode AS 'OEM Kodu', EquivalentCode AS 'Muadil Kodu', Stock AS 'Stok', Unit AS 'Birim',
        MinStock AS 'Asgari Stok', SalePrice AS 'Satış Fiyatı', Shelf AS 'Raf', Supplier AS 'Tedarikçi'
        FROM Parts WHERE ($q='' OR Name LIKE $like OR Category LIKE $like OR Brand LIKE $like OR Model LIKE $like OR OemCode LIKE $like OR EquivalentCode LIKE $like OR Technical LIKE $like OR Compatibility LIKE $like OR SupplierCode LIKE $like)
        AND ($brand='Tüm Markalar' OR Brand=$brand)
        AND ($low=0 OR (Stock<=MinStock AND MinStock>0)) ORDER BY BrandPriority,Brand,Model,Name
        """;
        cmd.Parameters.AddWithValue("$q", term); cmd.Parameters.AddWithValue("$like", $"%{term}%"); cmd.Parameters.AddWithValue("$low", lowOnly ? 1 : 0);cmd.Parameters.AddWithValue("$brand",brand);
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

    public static PhotoInfo? GetPhoto(int id)
    {
        using var c=new SqliteConnection(ConnectionString);c.Open();using var q=c.CreateCommand();
        q.CommandText="SELECT Name,Brand,Model,OemCode,SupplierCode,ImageUrl,SourceUrl,Verification FROM Parts WHERE Id=$id";q.Parameters.AddWithValue("$id",id);using var r=q.ExecuteReader();
        return r.Read()?new PhotoInfo(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6),r.GetString(7)):null;
    }

    public static CatalogDetail? GetCatalogDetail(int id)
    {
        using var c=new SqliteConnection(ConnectionString);c.Open();using var q=c.CreateCommand();q.CommandText="SELECT Id,Name,Category,Brand,Model,OemCode,EquivalentCode,Supplier,Shelf,Unit,Stock,MinStock,PurchasePrice,SalePrice,Notes,Technical,Compatibility,Manufacturer,ManufacturerCode,SupplierCode,SourceUrl,ImageUrl,Verification FROM Parts WHERE Id=$id";q.Parameters.AddWithValue("$id",id);using var r=q.ExecuteReader();if(!r.Read())return null;
        return new CatalogDetail(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6),r.GetString(7),r.GetString(8),r.GetString(9),r.GetDouble(10),r.GetDouble(11),r.GetDouble(12),r.GetDouble(13),r.GetString(14),r.GetString(15),r.GetString(16),r.GetString(17),r.GetString(18),r.GetString(19),r.GetString(20),r.GetString(21),r.GetString(22));
    }
    public static DataTable FindAlternatives(int id)
    {
        using var c=new SqliteConnection(ConnectionString);c.Open();using var q=c.CreateCommand();q.CommandText="""
        SELECT Name AS 'Parça Adı',Brand AS 'Üretici/Marka',ManufacturerCode AS 'Üretici Kodu',OemCode AS 'OEM Kodu',SupplierCode AS 'Stok Kodu',Model AS 'Uyumlu Model',Verification AS 'Doğrulama'
        FROM Parts WHERE Id<>$id AND (Category=(SELECT Category FROM Parts WHERE Id=$id) OR (OemCode<>'' AND OemCode=(SELECT OemCode FROM Parts WHERE Id=$id)) OR (ManufacturerCode<>'' AND ManufacturerCode=(SELECT ManufacturerCode FROM Parts WHERE Id=$id))) ORDER BY BrandPriority,Brand,Name LIMIT 100
        """;q.Parameters.AddWithValue("$id",id);using var r=q.ExecuteReader();var t=new DataTable();t.Load(r);return t;
    }
    public static DataTable PhotoCatalog()
    {
        using var c=new SqliteConnection(ConnectionString);c.Open();using var q=c.CreateCommand();q.CommandText="SELECT Id,Name AS 'Parça Adı',Brand AS 'Marka',Model AS 'Model',OemCode AS 'OEM Kodu',SupplierCode AS 'Stok Kodu' FROM Parts WHERE ImageUrl<>'' ORDER BY BrandPriority,Brand,Name";using var r=q.ExecuteReader();var t=new DataTable();t.Load(r);return t;
    }
    public static DataTable CategoryCatalog(string category)
    {
        using var c=new SqliteConnection(ConnectionString);c.Open();using var q=c.CreateCommand();q.CommandText="SELECT Id,Name AS 'Parça Adı',Brand AS 'Üretici',ManufacturerCode AS 'Üretici Kodu',SupplierCode AS 'Stok Kodu',Model AS 'Uyumlu Sistem',Verification AS 'Doğrulama' FROM Parts WHERE Category=$category ORDER BY BrandPriority,Brand,Name";q.Parameters.AddWithValue("$category",category);using var r=q.ExecuteReader();var t=new DataTable();t.Load(r);return t;
    }

    public static void MoveStock(int id, double qty, string type, string note)
    {
        var delta = type == "Giriş" ? qty : -qty; using var c = new SqliteConnection(ConnectionString); c.Open(); using var tx = c.BeginTransaction();
        using var u = c.CreateCommand(); u.Transaction=tx; u.CommandText="UPDATE Parts SET Stock=Stock+$d,UpdatedAt=$u WHERE Id=$id AND Stock+$d>=0"; u.Parameters.AddWithValue("$d",delta); u.Parameters.AddWithValue("$u",DateTime.Now.ToString("s")); u.Parameters.AddWithValue("$id",id); if(u.ExecuteNonQuery()==0) throw new InvalidOperationException("Stok eksi değere düşemez.");
        using var m=c.CreateCommand(); m.Transaction=tx; m.CommandText="INSERT INTO Movements(PartId,MovementType,Quantity,Description,CreatedAt) VALUES($id,$t,$q,$n,$d)"; m.Parameters.AddWithValue("$id",id);m.Parameters.AddWithValue("$t",type);m.Parameters.AddWithValue("$q",qty);m.Parameters.AddWithValue("$n",note);m.Parameters.AddWithValue("$d",DateTime.Now.ToString("s"));m.ExecuteNonQuery();tx.Commit();
    }
}

internal sealed record PhotoInfo(string Name,string Brand,string Model,string OemCode,string SupplierCode,string ImageUrl,string SourceUrl,string Verification);
internal sealed record CatalogDetail(int Id,string Name,string Category,string Brand,string Model,string OemCode,string EquivalentCode,string Supplier,string Shelf,string Unit,double Stock,double MinStock,double PurchasePrice,double SalePrice,string Notes,string Technical,string Compatibility,string Manufacturer,string ManufacturerCode,string SupplierCode,string SourceUrl,string ImageUrl,string Verification);

internal sealed class CatalogRow { public string CatalogKey{get;set;}="";public string BrandDisplay{get;set;}="";public string Model{get;set;}="";public string Category{get;set;}="";public string Name{get;set;}="";public string Manufacturer{get;set;}="";public string ManufacturerCode{get;set;}="";public string OemCode{get;set;}="";public string SupplierCode{get;set;}="";public string Technical{get;set;}="";public string Compatibility{get;set;}="";public string Supplier{get;set;}="";public string SourceUrl{get;set;}="";public string ImageUrl{get;set;}="";public string Verification{get;set;}="";public int Priority{get;set;}=99; }

internal sealed class Part
{
    public int Id { get; set; } public string Name { get; set; }=""; public string Category { get; set; }=""; public string Brand { get; set; }=""; public string Model { get; set; }=""; public string OemCode { get; set; }=""; public string EquivalentCode { get; set; }=""; public string Supplier { get; set; }=""; public string Shelf { get; set; }=""; public string Unit { get; set; }="Adet"; public double Stock { get; set; } public double MinStock { get; set; } public double PurchasePrice { get; set; } public double SalePrice { get; set; } public string Notes { get; set; }="";
}
