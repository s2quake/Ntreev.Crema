using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.ServiceModel
{
    class CremaDispatcherScheduler : TaskScheduler
    {
        private static readonly object lockobj = new object();
        private readonly CancellationToken cancellation;
        private readonly BlockingCollection<Task> taskQueue = new BlockingCollection<Task>();
        private bool isExecuting;

        public CremaDispatcherScheduler(CancellationToken cancellation)
        {
            this.cancellation = cancellation;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        protected override void QueueTask(Task task)
        {
            this.taskQueue.Add(task, this.cancellation);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued) return false;

            return isExecuting && TryExecuteTask(task);
        }

        internal void Run()
        {
            
            while (this.cancellation.IsCancellationRequested == false)
            {
                isExecuting = true;

                if (taskQueue.TryTake(out var task) == true)
                {
                    //var task = taskQueue.Take(this.cancellation);
                    TryExecuteTask(task);
                }
                
                //try
                //{
                //foreach (var task in taskQueue.GetConsumingEnumerable(this.cancellation))
                //{

                //}
                //}
                //catch (OperationCanceledException)
                //{ }
                //finally
                //{
                //    isExecuting = false;
                //}
                Thread.Sleep(1);
            }

            //Task[] GetTask()
            //{
            //    lock (lockobj) return this.taskList.ToArray();
            //}
        }

        //protected sealed override bool TryDequeue(Task task)
        //{
        //    lock (lockobj) return this.taskQueue...Remove(task);
        //}
    }
}
