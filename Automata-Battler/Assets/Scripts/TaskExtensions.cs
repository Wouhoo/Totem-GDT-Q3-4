using System.Threading.Tasks;

public static class TaskExtensions
{
    /// <summary>
    /// Fire-and-forget an async Task without compiler warning or silent unobserved exceptions.
    /// </summary>
    public static void Forget(this Task task)
    {
        if (!task.IsCompleted || task.IsFaulted)
        {
            _ = ForgetAwaited(task);
        }

        static async Task ForgetAwaited(Task t)
        {
            try
            {
                await t.ConfigureAwait(false);
            }
            catch
            {
                // exceptions swallowed silently or optionally log
            }
        }
    }
}