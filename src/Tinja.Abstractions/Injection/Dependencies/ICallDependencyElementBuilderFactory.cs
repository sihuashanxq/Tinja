namespace Tinja.Abstractions.Injection.Dependencies
{
    /// <summary>
    /// an interface for create <see cref="ICallDependencyElementBuilder"/>
    /// </summary>
    public interface ICallDependencyElementBuilderFactory
    {
        ICallDependencyElementBuilder CreateBuilder();
    }
}
