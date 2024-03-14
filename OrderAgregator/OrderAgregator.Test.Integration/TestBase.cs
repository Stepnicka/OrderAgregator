using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderAgregator.API;
using System.Text.Json;

namespace OrderAgregator.Test.Integration
{
    public abstract class TestBase : IDisposable
    {
        private WebApplicationFactory<Program> _waf = null!;

        protected HttpClient _client = null!;

        protected IServiceScopeFactory _scopeFactory = null!;

        protected API.Services.ExternalApiServices.IExternalApi _externalApi = NSubstitute.Substitute.For<API.Services.ExternalApiServices.IExternalApi>();

        protected JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public TestBase()
        {
            _waf = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                var testConfiguration = new ConfigurationBuilder()
                    .AddJsonFile(path: "appsettings.test.json", optional: false, reloadOnChange: false)
                    .Build();

                builder.UseConfiguration(testConfiguration);

                builder.ConfigureTestServices(services =>
                {
                    /* Remove all cache options */
                    services.RemoveAll<API.Cache.IOrderCache>();
                    services.RemoveAll<API.Cache.RedisCache.IRedisDatabase>();
                    services.RemoveAll<API.Cache.SqLiteCache.ISqLiteDatabase>();

                    /* Add SqlLite */
                    services.AddScoped<API.Cache.SqLiteCache.ISqLiteDatabase, API.Cache.SqLiteCache.SqLiteDatabase>();
                    services.AddScoped<API.Cache.IOrderCache, API.Cache.SqLiteCache.SqLiteOrderCache>();

                    /* Remove external service & add substitute */
                    services.RemoveAll<API.Services.ExternalApiServices.IExternalApi>();
                    services.AddSingleton(_externalApi);
                });
            });

            _scopeFactory = _waf.Services.GetRequiredService<IServiceScopeFactory>();
            _client = _waf.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _waf.Dispose();
        }
    }
}
