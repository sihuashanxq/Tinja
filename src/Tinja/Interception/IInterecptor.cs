namespace Tinja.Interception
{
    public interface IInterecptor
    {
        void OnExecuting();

        void OnExecuted();
    }
}
