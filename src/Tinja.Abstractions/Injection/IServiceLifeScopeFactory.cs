namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScopeFactory
    {
        IServiceLifeScope Create(IServiceResolver resolver);

        IServiceLifeScope Create(IServiceResolver resolver, IServiceLifeScope scope);
    }
}
