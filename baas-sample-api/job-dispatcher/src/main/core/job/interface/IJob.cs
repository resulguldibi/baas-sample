using job_dispatcher.src.main.core.worker;
using System.Threading.Tasks;

namespace job_dispatcher.src.main.core.job
{
    public interface IJob
    {
        Task Do(IWorker worker);
        object GetData();
    }
}
