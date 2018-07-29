namespace Tinja.Abstractions.Injection.Dependency
{
    /// <summary>
    /// an interface for create <see cref="ICallDependencyElementBuilder"/>
    /// </summary>
    public interface ICallDependencyElementBuilderFactory
    {
        ICallDependencyElementBuilder CreateBuilder();
    }
}
