using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Order.Domain.Abstractions;

namespace Order.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor :  SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    private void UpdateEntities(DbContext? context)
    {
        if (context != null) return;

        foreach (var entry in context.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.LastModifiedBy = "Emre";
                entry.Entity.CreatedAt = DateTime.UtcNow;

            }

            if (entry.State == EntityState.Modified || entry.State == EntityState.Added || entry.HasChangedOwnedEntities())
            {

            }

        }
    }
}
public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}

/*
 Audit (Denetim, İzleme) verisi, bir sistemdeki nesnelerin (örneğin veritabanı kayıtları, kullanıcı işlemleri) 
 ne zaman ve kim tarafından oluşturulduğunu, değiştirildiğini
 veya silindiğini takip etmek için kullanılan bilgiler bütünüdür.

 Dispatch Domain Events Interceptor yapisi nedir.
 DispatchDomainEventsInterceptor, genellikle Onion Architecture veya Clean Architecture gibi katmanlı mimarilerde
 kullanılan bir yapıdır. Bu yapı, Domain Events (Alan Olayları) ile ilgilidir ve bir tür Interceptor (Yakalama) 
 mekanizması olarak çalışır. Amacı, Domain katmanında meydana gelen olayların otomatik olarak işlenmesini veya 
 "dispatch"(yonlendirmekm, dagitmak) edilmesini sağlamaktır.
 */
