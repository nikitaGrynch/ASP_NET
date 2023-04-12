using System.Runtime.InteropServices.JavaScript;

namespace ASP_NET.Services.Random;


public class RandomServiceV1 : IRandomService
{
    private readonly System.Random _random = new();
    private readonly string _codeChars = "abcdefghijklmnopqrstuvwxyz0123456789";
    private readonly string _filenameChars = "abcdefghijklmnopqrstuvwxyz0123456789_-=()";
    private readonly string _safeChars = new String(
        Enumerable.Range(20, 107).Select(x => (char)x).ToArray());
    

    public string RandomString(int length)
    {
        return _MakeString(_safeChars, length);
    }

    public string ConfirmCode(int length)
    {
        return _MakeString(_codeChars, length);
    }

    public string RandomFileName(int length)
    {
        return _MakeString(_filenameChars, length);
    }

    private String _MakeString(String sourceString, int length)
    {
        char[] chars = new char[length];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = sourceString[_random.Next((sourceString.Length))];
        }

        return new String(chars);
    }
}