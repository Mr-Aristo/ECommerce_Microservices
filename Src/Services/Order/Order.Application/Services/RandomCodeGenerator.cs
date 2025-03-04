using System.Text;

namespace Order.Application.Services;

public static class RandomCodeGenerator
{
    /// <summary>
    /// Generates Trace Code
    /// </summary>
    /// <param name="lenght"></param>
    /// <returns></returns>
    public static string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new();
        StringBuilder result = new(length);

        for (int i = 0; i < length; i++)
            result.Append(chars[random.Next(chars.Length)]);

        return result.ToString();

    }

}
