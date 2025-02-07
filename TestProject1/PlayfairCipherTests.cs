using pleifer.Services;

namespace TestProject1;

public class PlayfairCipherTests
{
    private readonly PlayfairCipher _cipher = new PlayfairCipher();

    [Theory]
    [InlineData("Hello World", "passkey", "MARKHRVQXRLK")]
    [InlineData("Раз Два Три", "ключ", "СЧИЕГБУСЁБ")]
    [InlineData("Брррррррра", "ряря", "ВЯДЦДЦДЦДЦДЦДЦЯБ")]
    public void Encrypt_EnglishText_ReturnsEncryptedString(string plaintext, string key, string expectedCiphertext)
    {
        string encryptedText = _cipher.Encrypt(plaintext, key);
        Assert.Equal(expectedCiphertext, encryptedText);
    }

    [Theory]
    [InlineData("MARKHRVQXRLK", "passkey", "HELLOWORLD")]
    [InlineData("СЧИЕГБУСЁБ", "ключ", "РАЗДВАТРИ")]
    [InlineData("ВЯДЦДЦДЦДЦДЦДЦЯБ", "ряря", "БРРРРРРРРА")]
    public void Decrypt_EnglishCiphertext_ReturnsDecryptedString(string ciphertext, string key, string expectedPlaintext)
    {
        string decryptedText = _cipher.Decrypt(ciphertext, key);
        Assert.Equal(expectedPlaintext, decryptedText);
    }

    [Fact]
    public void Encrypt_TextWithOddLength_AddsReplacementChar()
    {
        string encrypted = _cipher.Encrypt("CAT", "MYA");
        Assert.Equal(4, encrypted.Length); // Длина должна стать четной
    }
}