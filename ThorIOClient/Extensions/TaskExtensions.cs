using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ThorIOClient.Extensions
{

    public static class TaskExtensions
    {
        public static void Repeat(this System.Threading.Tasks.Task taskToRepeat, CancellationToken cancellationToken, TimeSpan intervalTimeSpan)
        {
            var action = taskToRepeat
                .GetType()
                .GetField("m_action", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(taskToRepeat) as Action;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (cancellationToken.WaitHandle.WaitOne(intervalTimeSpan))
                        break;
                    if (cancellationToken.IsCancellationRequested)
                        break;
                     System.Threading.Tasks.Task.Factory.StartNew(action, cancellationToken);
                }
            }, cancellationToken);
        }
    }

}