
using MediatR;
using Microsoft.Extensions.Options;
using OrderAgregator.API.Models;
using System.Threading.RateLimiting;

namespace OrderAgregator.API.Services
{
    public class LimitedOrderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILimitedOrderBackgroundSeviceSignaller _signaller;
        private readonly RateLimiterConfiguration _limiterOptions;
        private readonly RateLimiter _limiter;

        public LimitedOrderBackgroundService(IOptions<RateLimiterConfiguration> limiterConfiguration , IServiceScopeFactory scopeFactory, ILimitedOrderBackgroundSeviceSignaller signaller)
        {
            _scopeFactory = scopeFactory;
            _signaller = signaller;
            _limiterOptions = limiterConfiguration.Value!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTime lastRunTime = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                await _signaller.Wait(cancellationToken: stoppingToken);

                if ((DateTime.UtcNow - lastRunTime).TotalSeconds < _limiterOptions.Seconds)
                    await Task.Delay(TimeSpan.FromSeconds(_limiterOptions.Seconds - (DateTime.UtcNow - lastRunTime).TotalSeconds), stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    continue;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediatr.Send(new Handlers.Commands.SendAggregatedOrdersCommand()); 

                    lastRunTime = DateTime.UtcNow;
                }
            }
        }
    }
}
