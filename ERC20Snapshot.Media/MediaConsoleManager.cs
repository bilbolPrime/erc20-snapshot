namespace BilbolStack.ERC20Snapshot.Media
{
    public class MediaConsoleManager : IMediaManager
    {
        void IMediaManager.Send(string message)
        {
            Console.WriteLine(message);
        }
    }
}
