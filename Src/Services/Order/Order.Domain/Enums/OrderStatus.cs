namespace Order.Domain.Enums;

public enum OrderStatus
{
    // Existing values kept as-is (Status is persisted as string) for backward compatibility.
    Draft = 1,
    Pending = 2,
    Completed = 3,
    Cancelled = 4,
    // Order lifecycle (FEAT-001)
    Confirmed = 5,
    Processing = 6,
    Shipped = 7,
    Delivered = 8,
    Failed = 9,
    ReturnRequested = 10,
    Returned = 11
}

/*
Enum’lar genellikle veritabanında tamsayı (int) olarak saklanır.
String yerine int kullanmak, depolama açısından daha az yer kaplar ve sorguların daha hızlı çalışmasını sağlar.
Örneğin, veritabanında şu şekilde saklamak daha pratiktir:

Id	OrderStatus
1	2 (Pending)
2	3 (Completed)
3	1 (Draft)

Eğer string olarak saklasaydık:

Id	OrderStatus
1	"Pending"
2	"Completed"
3	"Draft"
Bu durumda:

String karşılaştırmaları daha fazla bellek ve işlem gücü gerektirir.
Index'ler int üzerinde daha verimli çalışır.
String'de typo (yazım hatası) riski vardır (örn: "Pendng" gibi).
 */