using System.Diagnostics;

namespace KombiParcaPro;

internal sealed class MainForm : Form
{
    readonly TextBox search = new() { PlaceholderText="Parça adı, OEM kodu, marka veya model ara...", Dock=DockStyle.Fill, Font=new("Segoe UI",12) };
    readonly CheckBox low = new() { Text="Kritik stokları göster", AutoSize=true };
    readonly ComboBox brand = new() { DropDownStyle=ComboBoxStyle.DropDownList, Dock=DockStyle.Fill, Font=new("Segoe UI",10) };
    readonly DataGridView grid = new() { Dock=DockStyle.Fill, ReadOnly=true, AllowUserToAddRows=false, SelectionMode=DataGridViewSelectionMode.FullRowSelect, MultiSelect=false, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells };
    readonly Label status = new() { Dock=DockStyle.Fill, TextAlign=ContentAlignment.MiddleLeft };

    public MainForm()
    {
        Text="KombiParcaPro — Yedek Parça Katalog ve Stok"; Width=1450; Height=820; StartPosition=FormStartPosition.CenterScreen; MinimumSize=new(1050,650); BackColor=Color.FromArgb(245,246,248);
        var title=new Label { Text="KombiParcaPro 2.3", Dock=DockStyle.Left, Width=300, Font=new("Segoe UI Semibold",20), ForeColor=Color.White, TextAlign=ContentAlignment.MiddleLeft, Padding=new(78,0,0,0) };
        var header=new Panel { Dock=DockStyle.Top, Height=72, BackColor=Color.FromArgb(25,25,25) }; header.Controls.Add(title);
        var logoPath=Path.Combine(AppContext.BaseDirectory,"Assets","3D-Teknik-Servis-Logo.png");if(File.Exists(logoPath)){var logo=new PictureBox{Dock=DockStyle.Left,Width=70,SizeMode=PictureBoxSizeMode.Zoom,Padding=new(8)};logo.Image=Image.FromFile(logoPath);header.Controls.Add(logo);logo.BringToFront();}
        var orange=new Panel { Dock=DockStyle.Bottom, Height=5, BackColor=Color.FromArgb(241,112,20) }; header.Controls.Add(orange);
        var buttons=new FlowLayoutPanel { Dock=DockStyle.Right, Width=790, FlowDirection=FlowDirection.LeftToRight, Padding=new(5,15,0,0) };
        buttons.Controls.AddRange(new Control[]{Btn("Parça Detayı",(_,_)=>ShowDetails()),Btn("Kataloglar",(_,_)=>ShowCatalogs()),Btn("Yeni Parça",(_,_)=>Edit(null)),Btn("Düzenle",(_,_)=>EditSelected()),Btn("Stok Giriş",(_,_)=>Move("Giriş")),Btn("Stok Çıkış",(_,_)=>Move("Çıkış")),Btn("Yenile",(_,_)=>LoadData())}); header.Controls.Add(buttons);
        brand.Items.AddRange(new object[]{"Tüm Markalar","Demirdöküm","E.C.A.","Baymak","Ariston","Vaillant","Viessmann","Bosch","Buderus","Baxi","Ferroli","Üniversal/Diğer"});brand.SelectedIndex=0;
        var searchBar=new TableLayoutPanel { Dock=DockStyle.Top, Height=62, Padding=new(16,13,16,8), ColumnCount=3 }; searchBar.ColumnStyles.Add(new(SizeType.Percent,100));searchBar.ColumnStyles.Add(new(SizeType.Absolute,190));searchBar.ColumnStyles.Add(new(SizeType.Absolute,190)); searchBar.Controls.Add(search,0,0);searchBar.Controls.Add(brand,1,0);searchBar.Controls.Add(low,2,0);
        var footer=new TableLayoutPanel { Dock=DockStyle.Bottom, Height=38, Padding=new(12,0,12,0), ColumnCount=2 }; footer.ColumnStyles.Add(new(SizeType.Percent,100));footer.ColumnStyles.Add(new(SizeType.Absolute,330));footer.Controls.Add(status,0,0); var info=new LinkLabel { Text="Veriler bu bilgisayarda çevrimdışı saklanır", Dock=DockStyle.Fill, TextAlign=ContentAlignment.MiddleRight }; footer.Controls.Add(info,1,0);
        var host=new Panel { Dock=DockStyle.Fill, Padding=new(16,0,16,8) }; host.Controls.Add(grid);
        Controls.Add(host);Controls.Add(footer);Controls.Add(searchBar);Controls.Add(header);
        search.TextChanged+=(_,_)=>LoadData();brand.SelectedIndexChanged+=(_,_)=>LoadData();low.CheckedChanged+=(_,_)=>LoadData();grid.CellDoubleClick+=(_,e)=>{if(e.RowIndex>=0)ShowDetails();};LoadData();
    }
    static Button Btn(string text, EventHandler action){var b=new Button{Text=text,AutoSize=true,Height=36,BackColor=Color.FromArgb(241,112,20),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Font=new("Segoe UI Semibold",9)};b.FlatAppearance.BorderSize=0;b.Click+=action;return b;}
    void LoadData(){grid.DataSource=Database.Search(search.Text.Trim(),low.Checked,brand.SelectedItem?.ToString()??"Tüm Markalar");if(grid.Columns.Contains("Id"))grid.Columns["Id"]!.Visible=false;foreach(DataGridViewRow row in grid.Rows){double s=Convert.ToDouble(row.Cells["Stok"].Value),m=Convert.ToDouble(row.Cells["Asgari Stok"].Value);if(m>0&&s<=m)row.DefaultCellStyle.BackColor=Color.MistyRose;}status.Text=$"{grid.Rows.Count:N0} parça listeleniyor • Öncelik: Demirdöküm → E.C.A. → Baymak → Ariston";}
    int? SelectedId()=>grid.CurrentRow is null?null:Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
    void EditSelected(){var id=SelectedId();if(id is null){MessageBox.Show("Önce bir parça seçin.");return;}Edit(Database.Get(id.Value));}
    void ShowDetails(){var id=SelectedId();if(id is null){MessageBox.Show("Önce bir parça seçin.");return;}using var f=new PartDetailForm(id.Value);f.ShowDialog(this);}
    void ShowCatalogs(){using var f=new CatalogCenterForm();f.ShowDialog(this);}
    void Edit(Part? p){using var f=new PartForm(p);if(f.ShowDialog(this)==DialogResult.OK)LoadData();}
    void Move(string type){var id=SelectedId();if(id is null){MessageBox.Show("Önce bir parça seçin.");return;}using var f=new StockForm(type);if(f.ShowDialog(this)!=DialogResult.OK)return;try{Database.MoveStock(id.Value,f.Quantity,type,f.Note);LoadData();}catch(Exception ex){MessageBox.Show(ex.Message,"Stok işlemi",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
}

internal sealed class PartForm : Form
{
    readonly Part p; readonly Dictionary<string,TextBox> fields=new(); readonly NumericUpDown stock=new(){Maximum=1000000,DecimalPlaces=2}; readonly NumericUpDown min=new(){Maximum=1000000,DecimalPlaces=2}; readonly NumericUpDown purchase=new(){Maximum=10000000,DecimalPlaces=2}; readonly NumericUpDown sale=new(){Maximum=10000000,DecimalPlaces=2};
    public PartForm(Part? part){p=part??new();Text=p.Id==0?"Yeni Parça":"Parça Düzenle";Width=670;Height=720;StartPosition=FormStartPosition.CenterParent;var table=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new(16),ColumnCount=2,AutoScroll=true};table.ColumnStyles.Add(new(SizeType.Absolute,150));table.ColumnStyles.Add(new(SizeType.Percent,100));
        Add(table,"Parça Adı","Name",p.Name);Add(table,"Kategori","Category",p.Category);Add(table,"Marka / Tip","Brand",p.Brand);Add(table,"Model / Uyumluluk","Model",p.Model);Add(table,"OEM Kodu","OemCode",p.OemCode);Add(table,"Muadil Kodu","EquivalentCode",p.EquivalentCode);Add(table,"Tedarikçi","Supplier",p.Supplier);Add(table,"Raf / Konum","Shelf",p.Shelf);Add(table,"Birim","Unit",p.Unit);Num(table,"Mevcut Stok",stock,(decimal)p.Stock);Num(table,"Asgari Stok",min,(decimal)p.MinStock);Num(table,"Alış Fiyatı",purchase,(decimal)p.PurchasePrice);Num(table,"Satış Fiyatı",sale,(decimal)p.SalePrice);Add(table,"Notlar","Notes",p.Notes,true);var save=new Button{Text="KAYDET",Dock=DockStyle.Fill,Height=42,BackColor=Color.FromArgb(241,112,20),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};save.Click+=Save;table.Controls.Add(new Label(),0,table.RowCount);table.Controls.Add(save,1,table.RowCount-1);Controls.Add(table);}
    void Add(TableLayoutPanel t,string label,string key,string value,bool multi=false){int r=t.RowCount++;t.RowStyles.Add(new(SizeType.AutoSize));t.Controls.Add(new Label{Text=label,AutoSize=true,Padding=new(0,8,0,0)},0,r);var box=new TextBox{Text=value,Dock=DockStyle.Fill,Multiline=multi,Height=multi?70:30};fields[key]=box;t.Controls.Add(box,1,r);}
    static void Num(TableLayoutPanel t,string label,NumericUpDown n,decimal value){int r=t.RowCount++;t.RowStyles.Add(new(SizeType.AutoSize));t.Controls.Add(new Label{Text=label,AutoSize=true,Padding=new(0,8,0,0)},0,r);n.Value=value;n.Dock=DockStyle.Fill;t.Controls.Add(n,1,r);}
    void Save(object? s,EventArgs e){if(string.IsNullOrWhiteSpace(fields["Name"].Text)){MessageBox.Show("Parça adı zorunludur.");return;}p.Name=fields["Name"].Text.Trim();p.Category=fields["Category"].Text.Trim();p.Brand=fields["Brand"].Text.Trim();p.Model=fields["Model"].Text.Trim();p.OemCode=fields["OemCode"].Text.Trim();p.EquivalentCode=fields["EquivalentCode"].Text.Trim();p.Supplier=fields["Supplier"].Text.Trim();p.Shelf=fields["Shelf"].Text.Trim();p.Unit=fields["Unit"].Text.Trim();p.Stock=(double)stock.Value;p.MinStock=(double)min.Value;p.PurchasePrice=(double)purchase.Value;p.SalePrice=(double)sale.Value;p.Notes=fields["Notes"].Text.Trim();Database.Save(p);DialogResult=DialogResult.OK;}
}

internal sealed class StockForm : Form
{
    readonly NumericUpDown qty=new(){Minimum=.01M,Maximum=1000000,DecimalPlaces=2,Value=1,Dock=DockStyle.Fill};readonly TextBox note=new(){Dock=DockStyle.Fill};public double Quantity=>(double)qty.Value;public string Note=>note.Text.Trim();
    public StockForm(string type){Text=$"Stok {type}";Width=430;Height=210;StartPosition=FormStartPosition.CenterParent;var t=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new(16),ColumnCount=2};t.ColumnStyles.Add(new(SizeType.Absolute,100));t.ColumnStyles.Add(new(SizeType.Percent,100));t.Controls.Add(new Label{Text="Miktar",AutoSize=true},0,0);t.Controls.Add(qty,1,0);t.Controls.Add(new Label{Text="Açıklama",AutoSize=true},0,1);t.Controls.Add(note,1,1);var ok=new Button{Text="ONAYLA",Dock=DockStyle.Fill,BackColor=Color.FromArgb(241,112,20),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};ok.Click+=(_,_)=>DialogResult=DialogResult.OK;t.Controls.Add(ok,1,2);Controls.Add(t);}
}

internal sealed class PhotoForm : Form
{
    readonly PhotoInfo? info; readonly PictureBox picture=new(){Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.White}; readonly Label state=new(){Dock=DockStyle.Bottom,Height=46,TextAlign=ContentAlignment.MiddleCenter};
    public PhotoForm(PhotoInfo? value)
    {
        info=value;Text="Parça Detayı ve Fotoğrafı";Width=900;Height=760;StartPosition=FormStartPosition.CenterParent;
        var title=new Label{Dock=DockStyle.Top,Height=90,Font=new("Segoe UI Semibold",13),Padding=new(12),Text=info is null?"Parça bulunamadı":$"{info.Brand} • {info.Name}\n{info.Model}"};
        var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=42,FlowDirection=FlowDirection.LeftToRight,Padding=new(10,5,0,0)};
        var reload=new Button{Text="Fotoğrafı yeniden yükle",AutoSize=true};reload.Click+=async(_,_)=>await LoadPhoto(true);
        var source=new LinkLabel{AutoSize=true,Padding=new(12,8,0,0),Text="Kaynak ürün sayfasını aç"};source.LinkClicked+=(_,_)=>OpenUrl(info?.SourceUrl);
        var searchImage=new LinkLabel{AutoSize=true,Padding=new(12,8,0,0),Text="Web'de fotoğraf ara"};searchImage.LinkClicked+=(_,_)=>OpenUrl("https://www.google.com/search?tbm=isch&q="+Uri.EscapeDataString($"{info?.Brand} {info?.Name} {info?.Model} {info?.OemCode}"));
        actions.Controls.AddRange(new Control[]{reload,source,searchImage});Controls.Add(picture);Controls.Add(state);Controls.Add(actions);Controls.Add(title);Shown+=async(_,_)=>await LoadPhoto();
    }
    static void OpenUrl(string? url){if(!string.IsNullOrWhiteSpace(url))Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}
    async Task LoadPhoto(bool force=false)
    {
        if(string.IsNullOrWhiteSpace(info?.ImageUrl)){state.Text="Bu kayıt için doğrulanmış fotoğraf henüz eklenmedi. 'Web'de fotoğraf ara' bağlantısını kullanabilirsiniz.";return;}
        try
        {
            state.Text="Fotoğraf yükleniyor...";var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"KombiParcaPro","ImageCache");Directory.CreateDirectory(folder);var file=Path.Combine(folder,Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(info.ImageUrl)))+".img");
            if(force&&File.Exists(file))File.Delete(file);byte[] data;
            if(File.Exists(file))data=await File.ReadAllBytesAsync(file);else{using var http=new HttpClient{Timeout=TimeSpan.FromSeconds(25)};http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 KombiParcaPro/2.1.3");if(Uri.TryCreate(info.SourceUrl,UriKind.Absolute,out var referer))http.DefaultRequestHeaders.Referrer=referer;using var response=await http.GetAsync(info.ImageUrl);response.EnsureSuccessStatusCode();if(response.Content.Headers.ContentType?.MediaType?.StartsWith("image/",StringComparison.OrdinalIgnoreCase)!=true)throw new InvalidDataException("Sunucu fotoğraf yerine farklı bir içerik döndürdü.");data=await response.Content.ReadAsByteArrayAsync();await File.WriteAllBytesAsync(file,data);}
            try{using var decoded=SixLabors.ImageSharp.Image.Load(data);using var ms=new MemoryStream();decoded.Save(ms,new SixLabors.ImageSharp.Formats.Png.PngEncoder());ms.Position=0;using var temp=new Bitmap(ms);var old=picture.Image;picture.Image=new Bitmap(temp);old?.Dispose();state.Text=$"Fotoğraf gösteriliyor • Kaynak: {info.Verification}";}catch{if(File.Exists(file))File.Delete(file);throw;}
        }
        catch(Exception ex){state.Text="Fotoğraf yüklenemedi. 'Yeniden yükle' düğmesini deneyin. Hata: "+ex.Message;}
    }
}
