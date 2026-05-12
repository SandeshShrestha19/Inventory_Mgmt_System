using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public class PasswordHashHandler
{
  private static int _iterationCount = 100000;
  private static RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

  public static string HashPassword(string password)
  {
    int saltSize = 128 / 8;
    var salt = new byte[saltSize];
    _randomNumberGenerator.GetBytes(salt);
    var subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, _iterationCount, 256 / 8);

    var outputBytes = new byte[13 + salt.Length + subkey.Length];
    outputBytes[0] = 0x01;
    WriteNetworkByteOrder(outputBytes, 1, (uint)KeyDerivationPrf.HMACSHA512);
    WriteNetworkByteOrder(outputBytes, 5, (uint)_iterationCount);
    WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
    Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
    Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);

    return Convert.ToBase64String(outputBytes);
  }
  public static bool VerifyPassword(string password, string hashedPassword)
  {
    var outputBytes = Convert.FromBase64String(hashedPassword);

    byte formatMarker = outputBytes[0];
    var prf = (KeyDerivationPrf)ReadNetworkByteOrder(outputBytes, 1);
    int iterationCount = (int)ReadNetworkByteOrder(outputBytes, 5);
    int saltSize = (int)ReadNetworkByteOrder(outputBytes, 9);

    // Read salt
    var salt = new byte[saltSize];
    Buffer.BlockCopy(outputBytes, 13, salt, 0, saltSize);

    // Read stored subkey
    int subkeyLength = outputBytes.Length - 13 - saltSize;
    var storedSubkey = new byte[subkeyLength];
    Buffer.BlockCopy(outputBytes, 13 + saltSize, storedSubkey, 0, subkeyLength);

    // Hash the input password with the same salt
    var actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterationCount, subkeyLength);

    // Compare
    return CryptographicOperations.FixedTimeEquals(actualSubkey, storedSubkey);
  }

  private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
  {
    return (uint)(buffer[offset] << 24)
        | (uint)(buffer[offset + 1] << 16)
        | (uint)(buffer[offset + 2] << 8)
        | buffer[offset + 3];
  }
  private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
  {
    buffer[offset] = (byte)(value >> 24);
    buffer[offset + 1] = (byte)(value >> 16);
    buffer[offset + 2] = (byte)(value >> 8);
    buffer[offset + 3] = (byte)(value);
  }
}