using OtpNet;
using QRCoder;
using static QRCoder.QRCodeGenerator;

public class TwoFactorService
{

  public string GenerateSecretKey()
  {
    var key = KeyGeneration.GenerateRandomKey(20);
    return Base32Encoding.ToString(key);
  }

  public string GenerateQrCodeUri(string email, string secretKey)
  {
    return $"otpauth://totp/ECommerce={email}?secret={secretKey}&issuer=ECommerceApp";
  }

  public string GenerateQrCodeImage(string qrCodeUri)
  {
    using var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, ECCLevel.Q);
    using var qrCode = new PngByteQRCode(qrCodeData);
    var qrCodeBytes = qrCode.GetGraphic(6);
    return Convert.ToBase64String(qrCodeBytes);
  }

  public bool VerifyCode(string secretKey, string code)
  {
    var key = Base32Encoding.ToBytes(secretKey);
    var totp = new Totp(key);
    return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
  }

}