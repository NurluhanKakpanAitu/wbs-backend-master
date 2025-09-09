using System.Security.Cryptography;

namespace BonusSystem.Core.Common.IDGenerator; 
public class IDGenerator : IIDGenerator
{
    private static readonly char[] Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public string NewId()
    {
        using var rng = RandomNumberGenerator.Create();

        char first = GetRandomLetter(rng);
        string digits = GetRandomDigits(rng, 4);
        char last = GetRandomLetter(rng);

        return $"{first}{digits}{last}";
    }

    private static char GetRandomLetter(RandomNumberGenerator rng)
    {
        var buffer = new byte[1];
        rng.GetBytes(buffer);
        return Letters[buffer[0] % Letters.Length];
    }

    private static string GetRandomDigits(RandomNumberGenerator rng, int length)
    {
        var buffer = new byte[2];
        rng.GetBytes(buffer);
        int number = BitConverter.ToUInt16(buffer, 0) % (int)Math.Pow(10, length);
        return number.ToString($"D{length}");
    }
}
