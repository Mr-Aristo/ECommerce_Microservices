namespace Order.Domain.Abstractions;

public interface IEntity<T> : IEntity
{
    public T Id { get; set; }
}
public interface IEntity
{
    /* Audit (Denetim, İzleme)  Sistemde ne olduğunu tarihsel olarak kaydetme */
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set ; }


}
/*
 Bu tasarım, farklı türde kimlikler (örneğin, int, Guid, string) kullanmak isteyen varlıkların ihtiyaçlarını karşılar.
 public class User : IEntity<int> { ... } 
 public class Product : IEntity<Guid> { ... } 

 IEntity<T> Neden IEntity'i Miras Almış?
 Bu tasarım, birleşik bir yapı oluşturmak için kullanılmıştır.
 
 IEntity<T> genel özellikleri (CreatedAt, CreatedBy, vb.) içeren IEntity'i genişletir ve buna Id özelliğini ekler.
 Bu sayede:
 Kimliksiz Varlıklar: IEntity tek başına kullanılabilir. (Örneğin, log veya geçici tablolar için.)
 Kimlikli Varlıklar: IEntity<T> kullanılarak hem kimlik hem de audit bilgileri eklenir.

 Bu yapı, açık/kapalı prensibi (OCP) ve arayüz ayrımı prensibi (ISP) ile uyumludur.
 sÖzellikle IEntity ve IEntity<T>'nin ayrılması, sınıfların yalnızca ihtiyaç duyduğu özellikleri implement etmesini sağlar.
 */