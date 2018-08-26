namespace Tinja.Abstractions.Injection.Dependencies
{
    /// <summary>
    /// an interface for create <see cref="ICallDependElementBuilder"/>
    /// </summary>
    public interface ICallDependElementBuilderFactory
    {
        ICallDependElementBuilder CreateBuilder();
    }
}
