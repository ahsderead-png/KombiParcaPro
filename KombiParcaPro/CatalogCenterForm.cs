using System.Diagnostics;

namespace KombiParcaPro;

internal sealed class CatalogCenterForm : Form
{
    readonly DataGridView photos=new(){Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells};
    readonly ListBox pdfs=new(){Dock=DockStyle.Fill,Font=new("Segoe UI",11)};
    readonly string pdfFolder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"KombiParcaPro","PdfCatalogs");
    public CatalogCenterForm()
    {
        Text="KombiParcaPro — PDF ve Fotoğraf Kataloğu";Width=1150;Height=760;MinimumSize=new(850,560);StartPosition=FormStartPosition.CenterParent;
        var header=new Panel{Dock=DockStyle.Top,Height=70,BackColor=Color.FromArgb(24,28,34)};header.Controls.Add(new Label{Text="PDF VE FOTOĞRAF KATALOĞU",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Padding=new(22,0,0,0),ForeColor=Color.White,Font=new("Segoe UI Semibold",18)});header.Controls.Add(new Panel{Dock=DockStyle.Bottom,Height=5,BackColor=Color.FromArgb(241,112,20)});
        var tabs=new TabControl{Dock=DockStyle.Fill,Font=new("Segoe UI",10)};tabs.TabPages.Add(BuildPhotoTab());tabs.TabPages.Add(BuildPdfTab());Controls.Add(tabs);Controls.Add(header);
        photos.DataSource=Database.PhotoCatalog();if(photos.Columns.Contains("Id"))photos.Columns["Id"]!.Visible=false;photos.CellDoubleClick+=(_,e)=>{if(e.RowIndex<0)return;var id=Convert.ToInt32(photos.Rows[e.RowIndex].Cells["Id"].Value);using var f=new PartDetailForm(id);f.ShowDialog(this);};
        Directory.CreateDirectory(pdfFolder);LoadPdfs();
    }
    TabPage BuildPhotoTab(){var page=new TabPage("Fotoğraf Kataloğu"){Padding=new(10)};var note=new Label{Text="Fotoğraflı parçalar • Bir parçaya çift tıklayarak zengin detay ve galeri ekranını açın.",Dock=DockStyle.Top,Height=42,TextAlign=ContentAlignment.MiddleLeft,Font=new("Segoe UI Semibold",10)};page.Controls.Add(photos);page.Controls.Add(note);return page;}
    TabPage BuildPdfTab()
    {
        var page=new TabPage("PDF Kataloğu"){Padding=new(10)};var actions=new FlowLayoutPanel{Dock=DockStyle.Top,Height=48,Padding=new(0,6,0,0)};var add=Button("PDF katalog ekle");add.Click+=(_,_)=>AddPdf();var open=Button("Seçili PDF'yi aç");open.Click+=(_,_)=>OpenPdf();var folder=Button("Katalog klasörünü aç");folder.Click+=(_,_)=>Process.Start(new ProcessStartInfo(pdfFolder){UseShellExecute=true});var remove=Button("Listeden kaldır");remove.Click+=(_,_)=>RemovePdf();actions.Controls.AddRange(new Control[]{add,open,folder,remove});var note=new Label{Dock=DockStyle.Bottom,Height=58,Text="PDF kataloglar bilgisayarınızdaki güvenli katalog klasöründe saklanır. Üretici veya tedarikçi PDF'lerini 'PDF katalog ekle' ile programa dahil edebilirsiniz.",ForeColor=Color.DimGray,Padding=new(4)};page.Controls.Add(pdfs);page.Controls.Add(actions);page.Controls.Add(note);pdfs.DoubleClick+=(_,_)=>OpenPdf();return page;
    }
    static Button Button(string text)=>new(){Text=text,AutoSize=true,Height=34,BackColor=Color.FromArgb(241,112,20),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
    void LoadPdfs(){pdfs.Items.Clear();foreach(var file in Directory.EnumerateFiles(pdfFolder,"*.pdf").OrderBy(Path.GetFileName))pdfs.Items.Add(new PdfItem(file));}
    void AddPdf(){using var dialog=new OpenFileDialog{Filter="PDF katalogları (*.pdf)|*.pdf",Multiselect=true,Title="PDF katalog seçin"};if(dialog.ShowDialog(this)!=DialogResult.OK)return;foreach(var source in dialog.FileNames){var dest=Path.Combine(pdfFolder,Path.GetFileName(source));if(!Path.GetFullPath(source).Equals(Path.GetFullPath(dest),StringComparison.OrdinalIgnoreCase))File.Copy(source,dest,true);}LoadPdfs();}
    void OpenPdf(){if(pdfs.SelectedItem is PdfItem item)Process.Start(new ProcessStartInfo(item.Path){UseShellExecute=true});else MessageBox.Show("Önce bir PDF katalog seçin.");}
    void RemovePdf(){if(pdfs.SelectedItem is not PdfItem item)return;if(MessageBox.Show($"{item} katalog listesinden kaldırılsın mı?","PDF Kataloğu",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;File.Delete(item.Path);LoadPdfs();}
    sealed record PdfItem(string Path){public override string ToString()=>System.IO.Path.GetFileNameWithoutExtension(Path);}
}
