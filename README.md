# Soru-Cevap Portalı — Vize

Bu sürüm, proje yönergesindeki **Ara Sınav** iş paketleriyle sınırlıdır:

- SQL Server, Code First ve EF Core migration
- Soru, cevap ve kategori modelleri
- Repository deseni ve yönetici paneli
- Cookie tabanlı yönetici oturumu ve yetkilendirme
- Soru/kategori yönetimi ve AJAX ile soru arama

Genel kullanıcı arayüzü, ASP.NET Identity üyeliği, rol yönetimi ve SignalR final kapsamı olduğu için bu sürümde bulunmaz.

## Çalıştırma

Gereksinimler: Visual Studio 2022, .NET 8 SDK ve SQL Server LocalDB.

1. `SoruCevapPortal.slnx` dosyasını açın.
2. Proje klasöründe yönetici hesabını User Secrets ile tanımlayın:

   ```powershell
   dotnet user-secrets set "Admin:Username" "admin" --project .\SoruCevapPortal.csproj
   dotnet user-secrets set "Admin:Password" "kendi-belirledigin-sifre" --project .\SoruCevapPortal.csproj
   ```

   İkinci komuttaki örnek şifreyi kendi belirlediğiniz bir şifreyle değiştirin.
3. Projeyi çalıştırın. İlk açılışta migration'lar uygulanır ve örnek kategoriler eklenir.
4. `/Account/Login` adresinden ayarladığınız yönetici bilgileriyle giriş yapın.

Veritabanı bağlantısı `appsettings.json` içindeki `DefaultConnection` ayarındadır. Gerçek parolaları kaynak koduna veya `appsettings.json` dosyasına yazmayın.
