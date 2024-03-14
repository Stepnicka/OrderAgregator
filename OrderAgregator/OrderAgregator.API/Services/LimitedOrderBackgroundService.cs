
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
        private readonly RateLimiter _limiter;

        public LimitedOrderBackgroundService(IOptions<RateLimiterConfiguration> limiterConfiguration , IServiceScopeFactory scopeFactory, ILimitedOrderBackgroundSeviceSignaller signaller)
        {
            _scopeFactory = scopeFactory;
            _signaller = signaller;

            var limiterOptions = limiterConfiguration.Value;

            _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                AutoReplenishment = true,
                QueueLimit = 1,
                ReplenishmentPeriod = TimeSpan.FromSeconds(limiterOptions.Seconds),
                TokenLimit = 1,
                TokensPerPeriod = 1,
            }) ;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _signaller.Wait(cancellationToken: stoppingToken);

                await _limiter.AcquireAsync(permitCount: 1, cancellationToken: stoppingToken);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediatr.Send(new Handlers.Commands.SendAggregatedOrdersCommand()); 
                }
            }
        }
    }
}
