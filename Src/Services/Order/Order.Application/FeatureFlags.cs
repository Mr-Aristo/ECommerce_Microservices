namespace Order.Application;

/// <summary>
/// Centralizes feature flag keys so config, code, and deployment files reference the same name.
/// </summary>
public static class FeatureFlags
{
    public const string OrderFulfillment = "OrderFulfillment";
}
