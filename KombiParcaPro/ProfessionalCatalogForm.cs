using System.Data;
using System.Diagnostics;

namespace KombiParcaPro;

internal sealed class ProfessionalCatalogForm : Form
{
    readonly TreeView tree=new(){Dock=DockStyle.Fill,Font=new("Segoe UI Semibold",10),HideSelection=false};
    readonly DataGridView grid=new(){Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    readonly Label title=new(){Dock=DockStyle.Top,Height=48,Font=new("Segoe UI Semibold",15),Padding=new(12)};
    readonly Resource[] data=Seed();
    public ProfessionalCatalogForm()
    {
        Text="Profesyonel Teknik Kütüphane";Width=1280;Height=780;MinimumSize=new(900,600);StartPosition=FormStartPosition.CenterParent;
        var header=new Panel{Dock=DockStyle.Top,Height=72,BackColor=Color.FromArgb(24,28,34)};header.Controls.Add(new Label{Text="PROFESYONEL TEKNİK KÜTÜPHANE",Dock=DockStyle.Fill,ForeColor=Color.White,Font=new("Segoe UI Semibold",18),TextAlign=ContentAlignment.MiddleLeft,Padding=new(22,0,0,0)});header.Controls.Add(new Panel{Dock=DockStyle.Bottom,Height=5,BackColor=Color.FromArgb(241,112,20)});
        var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=300,FixedPanel=FixedPanel.Panel1};split.Panel1.Padding=new(10);split.Panel2.Padding=new(10);split.Panel1.Controls.Add(tree);split.Panel2.Controls.Add(grid);split.Panel2.Controls.Add(title);Controls.Add(split);Controls.Add(header);
        foreach(var category in data.Select(x=>x.Category).Distinct())tree.Nodes.Add(category);tree.AfterSelect+=(_,_)=>LoadCategory();grid.CellDoubleClick+=(_,e)=>{if(e.RowIndex<0)return;using var f=new ProfessionalResourceDetailForm(tree.SelectedNode?.Text??"",Convert.ToString(grid.Rows[e.RowIndex].Cells["Katalog/Parça Grubu"].Value)??"",Convert.ToString(grid.Rows[e.RowIndex].Cells["Üretici"].Value)??"",Convert.ToString(grid.Rows[e.RowIndex].Cells["İçerik"].Value)??"",Convert.ToString(grid.Rows[e.RowIndex].Cells["Doğrulama"].Value)??"",Convert.ToString(grid.Rows[e.RowIndex].Cells["Bağlantı"].Value)??"");f.ShowDialog(this);};if(tree.Nodes.Count>0)tree.SelectedNode=tree.Nodes[0];
    }
    void LoadCategory(){var category=tree.SelectedNode?.Text??"";title.Text=category;var t=new DataTable();t.Columns.Add("Katalog/Parça Grubu");t.Columns.Add("Üretici");t.Columns.Add("İçerik");t.Columns.Add("Doğrulama");t.Columns.Add("Bağlantı");foreach(var x in data.Where(x=>x.Category==category))t.Rows.Add(x.Name,x.Manufacturer,x.Detail,x.Verification,x.Url);grid.DataSource=t;}
    static Resource[] Seed()=>[
      new("Kombi Yedek Parçaları","KombiParcaPro","Yerel stok ve parça kataloğu","1.862 kayıt","Program içi doğrulanmış/veri kaynağı belirtilmiş",""),
      new("Kombi Yedek Parçaları","E.C.A. Montaj Kılavuzları","E.C.A.","Kombi ve ısıtma ürünleri dokümanları","Resmî üretici","https://eca.com.tr/profesyoneller/montaj-kilavuzlari"),
      new("Teknik Servis Aletleri","Ölçüm Cihazları","Testo","Gaz, sıcaklık, basınç ve HVAC ölçüm cihazları","Resmî üretici","https://www.testo.com/tr-TR/"),
      new("Teknik Servis Kimyasalları","Isıtma Sistemi Kimyasalları","Fernox","Temizleyici, inhibitör ve sızdırmazlık ürün grupları","Resmî üretici","https://fernox.com/"),
      new("Ana Kartlar ve Elektronik Komponentler","Kart ve Komponent Sınıfları","Çoklu üretici","Röle, sigorta, varistör, optokuplör, MOSFET, TRIAC, sensör ve konektör","Eşleştirmeler doğrulama aşamasında",""),
      new("Petek ve Radyatörler","Panel Radyatörler","E.C.A. SEREL","Panel radyatör ürün ailesi","Resmî üretici","https://eca.com.tr/isitma-sogutma/radyatorler/panel"),
      new("Petek ve Radyatörler","Dijital Katalog ve Fiyat Listeleri","E.C.A. SEREL","Radyatör ve teknik ürün dokümanları","Resmî üretici","https://eca.com.tr/profesyoneller/fiyat-listeleri"),
      new("Tesisat Parçaları","Sıhhi Tesisat Kılavuzları","E.C.A.","Vana, armatür ve tesisat ürünleri","Resmî üretici","https://eca.com.tr/profesyoneller/montaj-kilavuzlari"),
      new("Yüksek Kapasiteli Kaskad Sistemleri","ecoCRAFT Kaskad Parçaları","Vaillant","Kaskad setleri, baca elemanları ve ürün numaraları","Resmî üretici PDF","https://www.vaillant.com.tr/downloads/profesyoneller/0020150368-01-ecocraft-kaskad-2329048.pdf"),
      new("Yüksek Kapasiteli Kaskad Sistemleri","Kurumsal Ürün Föyü","Vaillant","80–120 kW ecoCRAFT ve kaskad ürünleri","Resmî üretici PDF","https://www.vaillant.com.tr/downloads/7-0-5-vaillant-kurumsal-urun-krlml-foyu-lr-kasim2025-3087159.pdf"),
      new("Merkezi Doğalgaz Kazan Sistemleri","MaxiCondense 48–65 kW","DemirDöküm","Yüksek kapasiteli kazan ve kontrol özellikleri","Resmî üretici PDF","https://www.demirdokum.com.tr/downloads/7-2-1-maxicondense-48-65-kw-brosur-rev-baski-kasim2025-3087440.pdf"),
      new("Hidrolik Şemalar ve Otomasyon","Hidrolik Devre Çizimleri","Vaillant","Kazan, kaskad, pompa ve karışım devresi şemaları","Resmî üretici PDF","https://www.vaillant.com.tr/downloads/profesyoneller/sartnameler/hidrolik-devre-izimleri-aklamalar-2631859.pdf"),
      new("Hidrolik Şemalar ve Otomasyon","calorMATIC 630","Vaillant","Kaskad otomasyonu, modüller ve bağlantı şemaları","Resmî üretici PDF","https://www.vaillant.com.tr/pdf/calormatic-630-821636.pdf")
    ];
    sealed record Resource(string Category,string Name,string Manufacturer,string Detail,string Verification,string Url);
}

internal sealed class ProfessionalResourceDetailForm : Form
{
    readonly string url;
    public ProfessionalResourceDetailForm(string category,string name,string manufacturer,string detail,string verification,string source)
    {
        url=source;Text="Teknik Katalog Detayı";Width=950;Height=680;StartPosition=FormStartPosition.CenterParent;
        var header=new Label{Text=name,Dock=DockStyle.Top,Height=72,BackColor=Color.FromArgb(24,28,34),ForeColor=Color.White,Font=new("Segoe UI Semibold",18),TextAlign=ContentAlignment.MiddleLeft,Padding=new(20,0,0,0)};
        var tabs=new TabControl{Dock=DockStyle.Fill,Font=new("Segoe UI",10)};tabs.TabPages.Add(Page("Genel",$"Kategori: {category}\r\nÜretici: {manufacturer}\r\nKatalog/Parça Grubu: {name}\r\n\r\nAçıklama:\r\n{detail}\r\n\r\nDoğrulama: {verification}"));tabs.TabPages.Add(Page("Teknik Alanlar",Fields(category)));tabs.TabPages.Add(Page("Uyumluluk ve Muadil",$"Bu bölümde üretici kodu, stok kodu, muadil parça numarası, kapasite/değer, bağlantı ölçüsü ve uyumlu sistemler doğrulandıkça gösterilecektir.\r\n\r\nKategori: {category}\r\nÜretici: {manufacturer}"));tabs.TabPages.Add(Page("Fotoğraf / PDF / Şema",string.IsNullOrWhiteSpace(source)?"Doğrulanmış doküman veya görsel bağlantısı henüz eklenmedi.":$"Doğrulanmış kaynak:\r\n{source}\r\n\r\nKaynağı açmak için alttaki düğmeyi kullanın."));
        var open=new Button{Text="RESMÎ KAYNAĞI / PDF'Yİ AÇ",Dock=DockStyle.Bottom,Height=46,BackColor=Color.FromArgb(241,112,20),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Enabled=!string.IsNullOrWhiteSpace(source)};open.Click+=(_,_)=>Process.Start(new ProcessStartInfo(url){UseShellExecute=true});Controls.Add(tabs);Controls.Add(open);Controls.Add(header);
    }
    static TabPage Page(string title,string text){var p=new TabPage(title){Padding=new(12)};p.Controls.Add(new TextBox{Text=text,Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BorderStyle=BorderStyle.None,Font=new("Segoe UI",10)});return p;}
    static string Fields(string category)=>category switch{
      "Teknik Servis Aletleri"=>"Ölçüm türü • ölçüm aralığı • hassasiyet • prob/sensör tipi • bağlantı • kalibrasyon • üretici/stok kodu • aksesuarlar",
      "Teknik Servis Kimyasalları"=>"Kimyasal türü • kullanım amacı • sistem hacmi/dozaj • malzeme uyumluluğu • uygulama yöntemi • güvenlik bilgi formu • üretici kodu",
      "Ana Kartlar ve Elektronik Komponentler"=>"Komponent türü • üretici kodu • elektriksel değer • tolerans • güç/gerilim • kılıf ölçüsü • pin dizilimi • kart/model uyumluluğu • muadil",
      "Petek ve Radyatörler"=>"Tip • yükseklik/uzunluk • ısıl güç • bağlantı ölçüsü • çalışma basıncı • renk • vana/konsol uyumluluğu • stok kodu",
      "Tesisat Parçaları"=>"Parça tipi • çap/DN • diş standardı • malzeme • basınç sınıfı • sıcaklık aralığı • bağlantı tipi • üretici kodu",
      "Yüksek Kapasiteli Kaskad Sistemleri"=>"Kapasite (kW) • cihaz adedi • baca çapı • kolektör/set kodu • kontrol modülü • pompa/hidrolik denge • sistem şeması • uyumlu cihazlar",
      "Merkezi Doğalgaz Kazan Sistemleri"=>"Kazan tipi • kapasite • brülör/gaz valfi • fan • eşanjör • kontrol kartı • sensör • pompa • baca • emniyet elemanı • üretici kodu",
      _=>"Parça adı • stok kodu • üretici kodu • OEM/muadil kodu • teknik özellik • uyumlu marka/model • fotoğraf • PDF • şema • doğrulama kaynağı"};
}
