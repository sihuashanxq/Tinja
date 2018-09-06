namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScopeFactory
    {
        IServiceLifeScope CreateScope();
    }
}
