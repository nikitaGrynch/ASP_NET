namespace ASP_NET.Services.Kdf;

/// <summary>
/// Key Derivation Function Service (RFC 8018)
/// </summary>
public interface IKdfService
{
    /// <summary>
    /// Make Derived key from password and salt
    /// </summary>
    /// <param name="password">Password string</param>
    /// <param name="salt">Salt string</param>
    /// <returns>Derived Key string</returns>
    String GetDerivedKey(String password, String salt);
    
}