using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace UDP_Relay_Core
{
    public static class AsyncExtensions
    {
        /// <summary>
        /// Adds cancellation support to a task.
        /// </summary>
        /// <typeparam name="T">Any generic type.</typeparam>
        /// <param name="task">Task to add cancellation to.</param>
        /// <param name="cancellationToken">Cancellation token to watch for cancellation.</param>
        /// <returns>Task</returns>
        /// <exception cref="OperationCanceledException">If canceled.</exception>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(obj => ((TaskCompletionSource<bool>)obj).TrySetResult(true), taskCompletionSource))
            {
                if (task != await Task.WhenAny(task, taskCompletionSource.Task))
                { // If task is not the first to complete, it is canceled if cancellation is requested.
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return task.Result;
        }
    }
}
