using job_dispatcher.src.main.core.worker;
using System.Threading.Tasks;

namespace job_dispatcher.src.main.core.job
{
    public abstract class Job : IJob
    {
        public readonly object data;
        public Job(object data)
        {
            this.data = data;
        }

        public abstract Task Do(IWorker worker);

        public object GetData()
        {
            return this.data;
        }
    }
}
