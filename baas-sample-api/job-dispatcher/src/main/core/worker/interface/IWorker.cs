using job_dispatcher.src.main.core.job;
using System.Threading.Tasks;

namespace job_dispatcher.src.main.core.worker
{
    public interface IWorker
    {
        void Start();
        Task Stop();
        void AddJob(IJob job);
        string GetName();
        int GetId();
    }
}
