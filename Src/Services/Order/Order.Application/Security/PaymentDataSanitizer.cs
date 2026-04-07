namespace Order.Application.Security;

public static class PaymentDataSanitizer
{
    public static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return "****";
        }

        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 0)
        {
            return "****";
        }

        var containsMaskChars = cardNumber.Contains('*');
        if (digitsOnly.Length <= 4)
        {
            return containsMaskChars
                ? $"**** **** **** {digitsOnly}"
                : "****";
        }

        var visiblePart = digitsOnly[^4..];
        return $"**** **** **** {visiblePart}";
    }

    public static string RedactCvv() => "***";
}
