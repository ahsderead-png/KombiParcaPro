using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace KombiParcaPro;

internal sealed class PartDetailForm : Form
{
    readonly Part? part;
    readonly PhotoInfo? photo;
    readonly PictureBox hero = new() { Dock=DockStyle.Fill, SizeMode=PictureBoxSizeMode.Zoom, BackColor=Color.White };
    readonly FlowLayoutPanel thumbs = new() { Dock=DockStyle.Fill, AutoScroll=true, WrapContents=false, FlowDirection=FlowDirection.LeftToRight, BackColor=Color.FromArgb(245,246,248), Padding=new(8) };
    readonly Label state = new() { Dock=DockStyle.Bottom, Height=38, TextAlign=ContentAlignment.MiddleCenter, ForeColor=Color.DimGray };
    readonly List<(string Url,Image Image)> images = [];

    public PartDetailForm(Part? value, PhotoInfo? photoInfo)
    {
        part=value;photo=photoInfo;Text="Parça Detayı";Width=1180;Height=780;MinimumSize=new(900,620);StartPosition=FormStartPosition.CenterParent;BackColor=Color.White;
        var header=new Panel{Dock=DockStyle.Top,Height=78,BackColor=Color.FromArgb(24,28,34),Padding=new(22,10,12,8)};
        header.Controls.Add(new Label{Dock=DockStyle.Fill,ForeColor=Color.White,Font=new("Segoe UI Semibold",18),TextAlign=ContentAlignment.MiddleLeft,Text=part is null?"Parça bulunamadı":part.Name});
        var accent=new Panel{Dock=DockStyle.Bottom,Height=5,BackColor=Color.FromArgb(241,112,20)};header.Controls.Add(accent);
        var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=390,FixedPanel=FixedPanel.Panel1,BackColor=Color.Gainsboro};
        split.Panel1.BackColor=Color.FromArgb(248,249,251);split.Panel1.Padding=new(20);split.Panel1.Controls.Add(BuildInfo());
        var gallery=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,Padding=new(12)};gallery.RowStyles.Add(new(SizeType.Percent,100));gallery.RowStyles.Add(new(SizeType.Absolute,122));gallery.RowStyles.Add(new(SizeType.Absolute,48));
        var heroHost=new Panel{Dock=DockStyle.Fill,Padding=new(8),BackColor=Color.White};heroHost.Controls.Add(hero);gallery.Controls.Add(heroHost,0,0);gallery.Controls.Add(thumbs,0,1);gallery.Controls.Add(BuildActions(),0,2);split.Panel2.Controls.Add(gallery);split.Panel2.Controls.Add(state);
        Controls.Add(split);Controls.Add(header);Shown+=async(_,_)=>await LoadGallery();FormClosed+=(_,_)=>{foreach(var x in images)x.Image.Dispose();};
    }

    Control BuildInfo()
    {
        var t=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,Padding=new(4)};t.ColumnStyles.Add(new(SizeType.Absolute,120));t.ColumnStyles.Add(new(SizeType.Percent,100));
        Add(t,"Marka",part?.Brand);Add(t,"Kategori",part?.Category);Add(t,"Model / Uyumluluk",part?.Model);Add(t,"OEM Kodu",part?.OemCode);Add(t,"Muadil Kodu",part?.EquivalentCode);Add(t,"Tedarikçi",part?.Supplier);Add(t,"Raf / Konum",part?.Shelf);Add(t,"Birim",part?.Unit);Add(t,"Mevcut Stok",part?.Stock.ToString("N2"));Add(t,"Asgari Stok",part?.MinStock.ToString("N2"));Add(t,"Alış Fiyatı",part?.PurchasePrice.ToString("N2")+" TL");Add(t,"Satış Fiyatı",part?.SalePrice.ToString("N2")+" TL");Add(t,"Notlar",part?.Notes,true);return t;
    }
    static void Add(TableLayoutPanel t,string label,string? value,bool tall=false){int r=t.RowCount++;t.RowStyles.Add(new(SizeType.AutoSize));t.Controls.Add(new Label{Text=label,AutoSize=true,Font=new("Segoe UI Semibold",10),ForeColor=Color.FromArgb(70,75,82),Padding=new(0,8,0,4)},0,r);t.Controls.Add(new Label{Text=string.IsNullOrWhiteSpace(value)?"—":value,AutoSize=true,MaximumSize=new(230,0),Font=new("Segoe UI",10),ForeColor=Color.FromArgb(25,28,32),Padding=new(0,8,0,tall?18:4)},1,r);}
    Control BuildActions(){var p=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.LeftToRight,Padding=new(8,7,0,0)};var reload=ActionButton("Fotoğrafları yenile");reload.Click+=async(_,_)=>await LoadGallery(true);var source=ActionButton("Kaynak sayfa");source.Click+=(_,_)=>Open(photo?.SourceUrl);var web=ActionButton("Web'de fotoğraf ara");web.Click+=(_,_)=>Open("https://www.google.com/search?tbm=isch&q="+Uri.EscapeDataString($"{part?.Brand} {part?.Name} {part?.Model} {part?.OemCode}"));p.Controls.AddRange(new Control[]{reload,source,web});return p;}
    static Button ActionButton(string text)=>new(){Text=text,AutoSize=true,Height=32,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(241,112,20),ForeColor=Color.White};
    static void Open(string? url){if(!string.IsNullOrWhiteSpace(url))Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}

    async Task LoadGallery(bool force=false)
    {
        state.Text="Fotoğraflar hazırlanıyor...";hero.Image=null;thumbs.Controls.Clear();foreach(var x in images)x.Image.Dispose();images.Clear();
        try
        {
            var urls=await RemoteImageLoader.DiscoverAsync(photo?.ImageUrl,photo?.SourceUrl);
            foreach(var url in urls.Take(8)){try{var image=await RemoteImageLoader.LoadAsync(url,photo?.SourceUrl,force);images.Add((url,image));AddThumbnail(image);}catch{/* Tek bozuk görsel galeriyi durdurmasın. */}}
            if(images.Count==0){state.Text="Bu parça için doğrulanmış fotoğraf bulunamadı. Web'de fotoğraf ara seçeneğini kullanabilirsiniz.";return;}
            hero.Image=images[0].Image;state.Text=$"{images.Count} fotoğraf bulundu • Küçük fotoğrafa tıklayarak büyük pencerede açın.";
        }
        catch(Exception ex){state.Text="Fotoğraflar yüklenemedi: "+ex.Message;}
    }
    void AddThumbnail(Image image)
    {
        var box=new PictureBox{Width=104,Height=94,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.White,BorderStyle=BorderStyle.FixedSingle,Margin=new(5),Image=image,Cursor=Cursors.Hand};
        box.Click+=(_,_)=>{hero.Image=image;using var f=new LargePhotoForm(image,part?.Name??"Parça Fotoğrafı");f.ShowDialog(this);};thumbs.Controls.Add(box);
    }
}

internal sealed class LargePhotoForm : Form
{
    public LargePhotoForm(Image image,string title){Text=title;Width=1200;Height=850;StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(25,25,25);Controls.Add(new PictureBox{Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(25,25,25),Image=image});}
}

internal static class RemoteImageLoader
{
    static readonly HttpClient Http=CreateClient();
    static HttpClient CreateClient(){var h=new HttpClient{Timeout=TimeSpan.FromSeconds(25)};h.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 KombiParcaPro/2.2");return h;}
    public static async Task<IReadOnlyList<string>> DiscoverAsync(string? primary,string? source)
    {
        var urls=new List<string>();if(Uri.TryCreate(primary,UriKind.Absolute,out var first))urls.Add(first.AbsoluteUri);if(!Uri.TryCreate(source,UriKind.Absolute,out var page))return urls;
        try{using var req=new HttpRequestMessage(HttpMethod.Get,page);using var res=await Http.SendAsync(req);res.EnsureSuccessStatusCode();var html=WebUtility.HtmlDecode(await res.Content.ReadAsStringAsync());var pattern="(?:src|data-src|data-zoom-image|href)=[\\\"']([^\\\"']+\\.(?:jpe?g|png|webp)(?:\\?[^\\\"']*)?)[\\\"']";foreach(Match m in Regex.Matches(html,pattern,RegexOptions.IgnoreCase)){if(!Uri.TryCreate(page,m.Groups[1].Value,out var u))continue;if(!u.AbsolutePath.Contains("storage",StringComparison.OrdinalIgnoreCase))continue;urls.Add(u.AbsoluteUri);}}catch{/* Ana görsel yine kullanılabilir. */}
        return urls.Distinct(StringComparer.OrdinalIgnoreCase).Take(12).ToArray();
    }
    public static async Task<Image> LoadAsync(string url,string? source,bool force)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"KombiParcaPro","ImageCache");Directory.CreateDirectory(folder);var file=Path.Combine(folder,Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(url)))+".img");if(force&&File.Exists(file))File.Delete(file);byte[] data;
        if(File.Exists(file))data=await File.ReadAllBytesAsync(file);else{using var req=new HttpRequestMessage(HttpMethod.Get,url);if(Uri.TryCreate(source,UriKind.Absolute,out var referer))req.Headers.Referrer=referer;using var res=await Http.SendAsync(req);res.EnsureSuccessStatusCode();if(res.Content.Headers.ContentType?.MediaType?.StartsWith("image/",StringComparison.OrdinalIgnoreCase)!=true)throw new InvalidDataException("Görsel bağlantısı geçersiz.");data=await res.Content.ReadAsByteArrayAsync();await File.WriteAllBytesAsync(file,data);}
        try{using var decoded=SixLabors.ImageSharp.Image.Load(data);using var ms=new MemoryStream();decoded.Save(ms,new SixLabors.ImageSharp.Formats.Png.PngEncoder());ms.Position=0;using var temp=new Bitmap(ms);return new Bitmap(temp);}catch{if(File.Exists(file))File.Delete(file);throw;}
    }
}
