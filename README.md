# KombiParcaPro

Kombi ve ısıtma sistemleri servisleri için çevrimdışı yedek parça katalog ve stok kontrol programı.

## Özellikler

- Parça adı, kategori, marka, model, OEM ve muadil kodla anlık arama
- Stok giriş–çıkış hareketleri ve eksi stok engeli
- Kritik stokların renkli uyarısı ve tek tıkla filtrelenmesi
- Alış/satış fiyatı, raf konumu, tedarikçi ve not kayıtları
- Kullanıcı verilerini bilgisayarda SQLite veritabanında saklama
- Windows 10/11 x64 için kendi içinde .NET bulunan kurulum paketi

## Kurulum

GitHub sayfasındaki **Releases** bölümünden `KombiParcaPro_Setup_v1.0.0_x64.exe` dosyasını indirin ve çalıştırın.

Veritabanı `%LOCALAPPDATA%\KombiParcaPro\kombiparcapro.db` konumunda tutulur ve program kaldırıldığında kullanıcı verileri korunur.
