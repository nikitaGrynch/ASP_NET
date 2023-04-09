namespace ASP_NET.Services.Hash;

public interface IHashService
{
    /// <summary>
    /// Вычисление гексодецимального хеш-образа от строчных данных
    /// </summary>
    /// <param name="text">Входные данные</param>
    /// <returns>Строка с гексодецимальным хешэм</returns>
    String Hash(String text);
}