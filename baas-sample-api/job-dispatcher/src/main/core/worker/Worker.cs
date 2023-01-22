using job_dispatcher.src.main.core.job;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace job_dispatcher.src.main.core.worker
{
    internal class Worker : IWorker
    {
        private readonly int id;
        private readonly string name;
        private readonly int workerJobTimeout;
        private Channel<IJob> jobChannel;
        private Channel<IWorker> dispatcherWorkerPoolChannel;
        private Channel<bool> isWorkerClosedChannel;
        public Worker(int id, string name, int workerJobTimeout, Channel<IWorker> dispatcherWorkerPoolChannel)
        {
            this.id = id;
            this.name = name;
            this.workerJobTimeout = workerJobTimeout;
            this.dispatcherWorkerPoolChannel = dispatcherWorkerPoolChannel;
            jobChannel = Channel.CreateUnbounded<IJob>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true });
            isWorkerClosedChannel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { SingleReader = true, SingleWriter = true });
        }

        public void AddJob(IJob job)
        {
            #region add job to worker's jobChannel

            _ = Task.Run(async () =>
            {
                await jobChannel.Writer.WriteAsync(job);
            });
            
            #endregion
        }

        public int GetId()
        {
            return this.id;
        }


        public string GetName()
        {
            return this.name;
        }

        public void Start()
        {
            _ = Task.Run(async () =>
            {
                #region worker register itself to dispatcher worker pool
                await dispatcherWorkerPoolChannel.Writer.WriteAsync(this);
                #endregion

                #region worker reads jobChannel and wait for job from dispatcher

                await foreach (var job in jobChannel.Reader.ReadAllAsync())
                {
                    #region call job's Do method to complete operation
                    var jobTask = job.Do(this);

                    if (await Task.WhenAny(jobTask, Task.Delay(this.workerJobTimeout)) == jobTask)
                    {
                        // task completed within timeout
                        //Console.WriteLine($"worker : {name} data : {job.GetData()}");
                    }
                    else
                    {
                        // timeout logic
                        Console.WriteLine($"worker : {name} data : {job.GetData()} timeout !!!!");
                    }

                    #endregion

                    #region worker register itself to dispatcher worker pool
                    await dispatcherWorkerPoolChannel.Writer.WriteAsync(this);
                    #endregion

                    //await Task.Delay(100);
                }

                await this.isWorkerClosedChannel.Writer.WriteAsync(true);

                #endregion                

            });
        }

        public async Task Stop()
        {
            this.jobChannel.Writer.Complete();
            await isWorkerClosedChannel.Reader.ReadAsync();
        }
    }
}
