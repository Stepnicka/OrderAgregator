using System.Collections.Immutable;

namespace OrderAgregator.API.Services.ExternalApiServices
{
    public interface IExternalApi
    {
        /// <summary>
        ///     Send orderds to external service
        /// </summary>
        Task SendOrders(ImmutableArray<Models.Order> orders);
    }
}
