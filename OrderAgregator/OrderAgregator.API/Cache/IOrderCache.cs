using OrderAgregator.API.Models;

namespace OrderAgregator.API.Cache
{
    /// <summary>
    ///     Temporary orders cache
    /// </summary>
    public interface IOrderCache
    {
        /// <summary>
        ///     Retrieve orders from cache
        /// </summary>
        Task<IReadOnlyList<Order>> GetOrders();

        /// <summary>
        ///     Insert orders into cache
        /// </summary>
        Task SaveOrders(IEnumerable<Order> orders);
    }
}
