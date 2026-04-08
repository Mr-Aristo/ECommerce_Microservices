namespace Order.Application.Security;

public static class PaymentDataSanitizer
{
    public static string NormalizePaymentToken(string paymentToken)
    {
        if (string.IsNullOrWhiteSpace(paymentToken))
        {
            return "****";
        }

        // Tokenized values can be alphanumeric; preserve them as-is.
        if (paymentToken.Any(ch => !char.IsDigit(ch) && ch != '*' && ch != ' '))
        {
            return paymentToken;
        }

        var digitsOnly = new string(paymentToken.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 0)
        {
            return "****";
        }

        var containsMaskChars = paymentToken.Contains('*');
        if (digitsOnly.Length <= 4)
        {
            return containsMaskChars
                ? $"**** **** **** {digitsOnly}"
                : "****";
        }

        var visiblePart = digitsOnly[^4..];
        return $"**** **** **** {visiblePart}";
    }

    // Backward-compatible name for legacy call sites.
    public static string MaskCardNumber(string cardNumber) => NormalizePaymentToken(cardNumber);

    public static string RedactCvv() => "***";
}
