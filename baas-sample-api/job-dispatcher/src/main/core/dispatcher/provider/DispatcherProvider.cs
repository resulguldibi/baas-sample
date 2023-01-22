namespace job_dispatcher.src.main.core.dispatcher.provider
{
    public class DispatcherProvider
    {
        public static IDispatcher GetDispatcher(string name, int workerCount, int workerJobTimeout)
        {
            return new Dispacther(name, workerCount, workerJobTimeout);
        }
    }
}
