namespace ASP_NET.Services.Random;
/// <summary>
/// Random String Function Service
/// </summary>
public interface IRandomService
{
    /// <summary>
    /// Random string function of desired length, including only safe chars
    /// </summary>
    /// <param name="length">Desired string length</param>
    /// <returns></returns>
    String RandomString(int length);
    /// <summary>
    /// Random string function of desired length for email verification code
    /// </summary>
    /// <param name="length">Desired string length</param>
    /// <returns></returns>
    String ConfirmCode(int length);
    /// <summary>
    /// Random string function of desired length for filenames
    /// </summary>
    /// <param name="length">Desired string length</param>
    /// <returns></returns>
    String RandomFileName(int length);
}