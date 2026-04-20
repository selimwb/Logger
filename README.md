Logger

Modern .NET 10 uygulamaları için geliştirilmiş yüksek performanslı, asenkron ve thread-safe dosya loglama kütüphanesi.

Özellikler

%100 Asenkron Mimari: Arka planda System.Threading.Channels kullanarak ana iş parçacığını (main thread) I/O işlemleriyle yormaz.

Thread-Safe: Singleton tasarım deseni ile çoklu thread ortamlarında çakışma olmadan güvenle çalışır.

Otomatik Dosya Rotasyonu: Belirlenen maksimum dosya boyutuna ulaşıldığında aktif log dosyasını arşivler ve yeni bir dosya üzerinden devam eder.

Dinamik Flush Kontrolü: Logların diske yazılma aralığını saniye cinsinden özelleştirebilme imkanı.

Zengin Format Desteği: ISO 8601 dâhil 7 farklı tarih formatı ve log mesajlarını ayırmak için 8 farklı ayırıcı (Splitter) seçeneği.

Kalıcı Ayarlar: Yapılandırmalar JSON formatında otomatik olarak diske (logSettings.dat) kaydedilir ve uygulamanın sonraki açılışlarında geri yüklenir.

Kurulum

Projeyi klonlayıp derleyerek Logger.dll dosyasını doğrudan kendi projelerinize dâhil edebilirsiniz.

Projeyi indirin veya klonlayın.

Terminalde proje dizinindeyken dotnet build -c Release komutunu çalıştırın.

bin/Release/net10.0/ klasörü içinde oluşan Logger.dll dosyasını kopyalayın.

Kendi projenizde "Add Project Reference" (veya "Add Reference") seçeneği ile bu DLL'i projenize ekleyin.

(Alternatif olarak sağ taraftaki Releases bölümünden derlenmiş en güncel DLL sürümünü indirebilirsiniz.)

Hızlı Başlangıç

Loglama işlemlerine başlamak oldukça basittir:

using Logger;

// Logger örneğini al (Singleton)
var log = Log.GetLogger();

// Standart log ekleme
await log.AddLog("Uygulama başarıyla başlatıldı.");

// Uyarı ve Hata logları
await log.AddWarningLog("Hafıza kullanımı %80'in üzerine çıktı.");
await log.AddErrorLog("Veritabanı bağlantısı zaman aşımına uğradı.");


Özelleştirme ve Ayarlar

Kütüphane çalışırken anlık olarak log formatlarını ve davranışlarını değiştirebilirsiniz:

// Tarih ve mesajı ok işareti ( > ) ile ayır
log.ChangeSplitter(Splitter.Arrow);

// Tarih formatını ISO 8601 olarak ayarla (örn: 2026-04-20 20:45:04)
log.ChangeDateFormat(DateFormat.Iso8601DateTime);

// 10 MB (10485760 bytes) sınırına ulaşıldığında dosyayı arşivle
log.ChangeFileSize(10485760);

// Logları diske her 5 saniyede bir yaz (Flush)
log.ChangeFlushTime(5);

// Uyarı ve Hata öneklerini özelleştir
log.ChangeErrorText("FATAL_ERROR");
log.ChangeWarningText("WARN");


Geliştirici: Selim Aksakallı