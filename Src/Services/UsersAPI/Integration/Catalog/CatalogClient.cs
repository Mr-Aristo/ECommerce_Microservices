using System.Net.Http.Json;

namespace UsersAPI.Integration.Catalog;

// Product summary as exposed by Catalog's /products/by-ids (extra fields are ignored on deserialization).
public record CatalogProductSummary(Guid Id, string Name, decimal Price, string? ImageFile);

public interface ICatalogClient
{
    Task<IReadOnlyDictionary<Guid, CatalogProductSummary>> GetProductsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}

public class CatalogClient(HttpClient http, ILogger<CatalogClient> logger) : ICatalogClient
{
    public async Task<IReadOnlyDictionary<Guid, CatalogProductSummary>> GetProductsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, CatalogProductSummary>();

        var query = string.Join("&", ids.Select(id => $"ids={id}"));

        try
        {
            var response = await http.GetFromJsonAsync<ProductsEnvelope>(
                $"/products/by-ids?{query}", cancellationToken);

            return response?.Products?.ToDictionary(p => p.Id)
                   ?? new Dictionary<Guid, CatalogProductSummary>();
        }
        catch (Exception ex)
        {
            // Enrichment is best-effort: degrade to ids-only favorites if Catalog is unavailable.
            logger.LogWarning(ex, "Catalog enrichment failed; returning favorites without product details");
            return new Dictionary<Guid, CatalogProductSummary>();
        }
    }

    private record ProductsEnvelope(List<CatalogProductSummary> Products);
}
