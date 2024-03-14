using FluentValidation;
using MediatR;
using MediatR.Pipeline;
using OrderAgregator.API.Handlers.Commands;
using OrderAgregator.API.Handlers.ExceptionHandlers;
using OrderAgregator.API.Models;
using OrderAgregator.API.Services;
using OrderAgregator.API.Validation;
using Serilog;

namespace OrderAgregator.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<RateLimiterConfiguration>(builder.Configuration.GetSection("RateLimit"));           

            builder.Services.AddControllers();

            /* Add logging */
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            /* Add validators */
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            /* Add Mediatr */
            builder.Services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblyContaining<Program>()
                    .AddBehavior<IPipelineBehavior<CreateOrderCommand, Result<Unit, DomainError>>, ValidationBehavior<CreateOrderCommand, Unit>>();
            });
            builder.Services.AddTransient<IRequestExceptionHandler<CreateOrderCommand, Result<Unit, DomainError>, Exception>, CreateOrderExceptionHandler>();
            builder.Services.AddTransient<IRequestExceptionHandler<SendAggregatedOrdersCommand, Result<Unit, DomainError>, Exception>, SendAggregatedOrdersExceptionHandler>();

            /* 
             * Note:
             * We can use sqLite or Redis to cache orders
             */

            /* Add sqlLite order cache */
            builder.Services.AddScoped<Cache.SqLiteCache.ISqLiteDatabase, Cache.SqLiteCache.SqLiteDatabase>();
            builder.Services.AddScoped<Cache.IOrderCache, Cache.SqLiteCache.SqLiteOrderCache>();

            /* Add redis order cache */
            //builder.Services.AddScoped<Cache.RedisCache.IRedisDatabase, Cache.RedisCache.RedisDatabase>();
            //builder.Services.AddScoped<Cache.IOrderCache, Cache.RedisCache.RedisOrderCache>();

            /* Add Service background */
            builder.Services.AddSingleton<ILimitedOrderBackgroundSeviceSignaller, LimitedOrderBackgroundSeviceSignaller>();
            builder.Services.AddHostedService<LimitedOrderBackgroundService>();

            /* Add external service */
            builder.Services.AddSingleton<Services.ExternalApiServices.IExternalApi, Services.ExternalApiServices.FakeExternalService>();
            /*
             Note:
             If we were to implement IExternalApi to call an external service insted of printing to console, we should implement RETRY policy.
             One possible way would be with POLLY nuget package
             */

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
