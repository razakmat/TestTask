
namespace TestTask
{
    /// <summary>
    /// Class <c>PeriodicSynchronizationStarter</c> periodically starts synchronization process of class <c>Synchronization</c>
    /// </summary>
    /// <param name="sync"> Instance of class <c>Synchronization</c></param>
    /// <param name="interval">How often Synchronization is started (in seconds)</param>
    public class PeriodicSynchronizationStarter(Synchronization sync, int interval)
    {
        /// <summary>
        /// Instance of class <c>Synchronization</c>
        /// </summary>
        protected Synchronization _sync = sync;
        /// <summary>
        /// Interval between synchronization in seconds
        /// </summary>
        protected int _interval = interval;
        /// <summary>
        /// Cancelation token for PeriodicTimer
        /// </summary>
        protected CancellationTokenSource _cancellationToken = new();
        /// <summary>
        /// Will hold method <c>PeriodicSyncTimerAsync</c> for starting PeriodicTimer
        /// </summary>
        protected Task? _task;

        /// <summary>
        /// Method <c>Start</c> starts the PeriodicTimer for starting Synchronization process
        /// </summary>
        public void Start()
        {
            _task = PeriodicSyncTimerAsync();
        }

        /// <summary>
        /// Method <c>Start</c> stops the PeriodicTimer, that starts synchronization process
        /// </summary>
        /// <returns>returns type Task necessary for async methods (for await command)</returns>
        public async Task Stop()
        {
            if (_task != null)
            {
                _cancellationToken.Cancel();
                await _task;
                _cancellationToken.Dispose();
            }
        }

        /// <summary>
        /// Method <c>PeriodicSyncTimerAsync</c> starts synchronization process in intervals given by
        /// attribut <c>_interval</c>.
        /// When method <c>Stop</c> is called timer terminates.
        /// </summary>
        /// <returns>returns type Task necessary for async methods (for await command)</returns>
        private async Task PeriodicSyncTimerAsync()
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_interval));
            try
            {
                do
                {
                    try
                    {
                        _sync.Start();
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine(ex.Message.ToString());
                    }
                } while (await timer.WaitForNextTickAsync(_cancellationToken.Token));
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}