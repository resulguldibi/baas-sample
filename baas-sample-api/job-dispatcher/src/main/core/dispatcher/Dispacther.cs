using job_dispatcher.src.main.core.job;
using job_dispatcher.src.main.core.worker;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace job_dispatcher.src.main.core.dispatcher
{
    internal class Dispacther : IDispatcher
    {
        private readonly string name;
        private readonly int workerCount;
        private readonly int workerJobTimeout;
        private List<IWorker> workers;
        private Channel<IJob> jobChannel;
        private Channel<IWorker> workerPoolChannel;
        private Channel<bool> isJobCompletedChannel;
        public Dispacther(string name, int workerCount, int workerJobTimeout)
        {
            this.name = name;
            this.workerCount = workerCount;
            this.workerJobTimeout = workerJobTimeout;
            workers = new List<IWorker>();
            jobChannel = Channel.CreateUnbounded<IJob>(new UnboundedChannelOptions() { SingleWriter = true, SingleReader = true });
            workerPoolChannel = Channel.CreateBounded<IWorker>(new BoundedChannelOptions(this.workerCount) { SingleReader = true, SingleWriter = false });
            isJobCompletedChannel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { SingleReader = true, SingleWriter = true });
        }

        public async Task AddJob(IJob job)
        {
            await jobChannel.Writer.WriteAsync(job);
        }

        public void Start()
        {
            #region init workers
            for (int i = 0; i < this.workerCount; i++)
            {
                var worker = new Worker(i, $"worker-{i}", this.workerJobTimeout, this.workerPoolChannel);
                workers.Add(worker);
                worker.Start();
                Console.WriteLine($"dispatcher {name} : worker-{i} started");
            }

            #endregion

            #region listen jobChannel, listen workerPoolChannel and send job to available worker
            _ = Task.Run(async () =>
            {
                await foreach (var job in jobChannel.Reader.ReadAllAsync())
                {
                    var worker = await workerPoolChannel.Reader.ReadAsync();
                    worker.AddJob(job);
                }

                #region close isJobCompletedChannel channel when jobChannel is closed
                await isJobCompletedChannel.Writer.WriteAsync(true);
                #endregion

            });

            Console.WriteLine($"dispatcher {name} started");
            #endregion
        }

        public async Task Stop()
        {
            #region close jobChannel
            jobChannel.Writer.Complete();
            Console.WriteLine($"dispatcher {name} :  jobChannel completed");
            #endregion

            #region wait for jobChannel to be closed
            await isJobCompletedChannel.Reader.ReadAsync();
            Console.WriteLine($"dispatcher {name} :  jobChannel closed");
            #endregion

            #region stop workers
            foreach (var worker in workers)
            {
                await worker.Stop();
                Console.WriteLine($"dispatcher {name} :  worker: {worker.GetName()} stopped");
            }
            #endregion

            #region close workerPoolChannel

            workerPoolChannel.Writer.Complete();
            Console.WriteLine($"dispatcher {name} : workerPoolChannel completed");
            #endregion

            Console.WriteLine($"dispatcher {name} stopped");
        }
    }
}
