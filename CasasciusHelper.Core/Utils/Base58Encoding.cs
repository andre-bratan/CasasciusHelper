namespace CasasciusHelper.Core.Utils;

public static class Base58Encoding
{
    private static readonly string AlphabetString = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public static readonly char[] AlphabetChars = AlphabetString.ToArray();
}
