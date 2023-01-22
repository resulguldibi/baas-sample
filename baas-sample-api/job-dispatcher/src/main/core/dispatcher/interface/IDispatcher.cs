using job_dispatcher.src.main.core.job;
using System.Threading.Tasks;

namespace job_dispatcher.src.main.core.dispatcher
{
    public interface IDispatcher
    {
        void Start();
        Task Stop();
        Task AddJob(IJob job);
    }
}
