public class Enable2FAResponseModel
{
  public string SecretKey {get; set;} = default!;
  public string QrCodeUri {get; set;} = default!;
  public string QrCodeImage {get; set;} = default!;
}
public class Verfiy2FAModel
{
  public string Code {get; set;} = default!;
}