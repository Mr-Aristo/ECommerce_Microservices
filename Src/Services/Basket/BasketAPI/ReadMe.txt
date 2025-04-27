MediatR'da <,> Kullanımı

Nedir?

<,> C#'ta open generics kullanımıdır ve iki generic parametre bekleyen bir yapıyı ifade eder.

Koddan Ornek:

config.AddOpenBehavior(typeof(ValidationBehavior<,>));
config.AddOpenBehavior(typeof(LoggingBehavior<,>));

Neden Kullanılır?

ValidationBehavior<TRequest, TResponse> gibi iki generic parametre alan bir sınıfın tür parametreleri belirtilmeden kaydedilmesini sağlar.

MediatR, runtime sırasında bu tür parametrelerini otomatik olarak doldurur.

Nasıl Çalışır?

ValidationBehavior<TRequest, TResponse> gibi bir generic sınıfı şablon olarak MediatR pipeline'ına ekler.

İster ProductQuery ister OrderCommand gibi farklı türler işlensin, MediatR bu generic davranışı tüm türler için kullanabilir.

Kaç Tür Parametresi Bekleniyor?

<,> → 2 generic parametre

<,,> → 3 generic parametre

Eğer <,> Kullanılmazsa?

Tür parametrelerini belirlemek zorunda kalırsın.

Open generic yapısı olmaz, esneklik kaybolur.

Avantajları:

Esneklik: Farklı türler için tek bir davranış.

Yeniden Kullanılabilirlik: Aynı generic davranış birden fazla türle çalışır.

Basit Kayıt: Türler belirtilmeden pipeline'a eklenir.


Decorate satırında:

      builder.Services.AddScoped<IBasketRepository ,BasketRepository>();
      builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

    Diyor ki: "IBasketRepository istenirse, doğrudan BasketRepository vermek yerine, önce BasketRepository'yi oluştur, sonra onu kullanarak CachedBasketRepository oluştur ve onu ver!"

    Yani artık IBasketRepository = CachedBasketRepository örneği olacak.

    Ancak CachedBasketRepository içinde gerçek BasketRepository de olacak! (dependency injection ile içeride)

