namespace OrderAgregator.API.Services
{
    public interface ILimitedOrderBackgroundSeviceSignaller
    {
        void Signal();
        Task Wait(CancellationToken cancellationToken);
    }

    internal class LimitedOrderBackgroundSeviceSignaller : ILimitedOrderBackgroundSeviceSignaller
    {
        private readonly SemaphoreSlim _signal = new(1, 1);

        public void Signal()
        {
            if (_signal.CurrentCount == 1)
                return;

            _signal.Release();
        }

        public async Task Wait(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
        }

        public void Dispose() => _signal.Dispose();
    }
}