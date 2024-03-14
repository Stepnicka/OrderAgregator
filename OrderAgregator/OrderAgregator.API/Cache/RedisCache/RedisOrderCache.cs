using OrderAgregator.API.Models;

namespace OrderAgregator.API.Cache.RedisCache
{
    public class RedisOrderCache : IOrderCache
    {
        private readonly IRedisDatabase redisDatabase;

        public RedisOrderCache(IRedisDatabase redisDatabase)
        {
            this.redisDatabase = redisDatabase;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Order>> GetOrders()
        {
            var result = new List<Order>();
            var keys = await redisDatabase.GetSetKeys("orders");

            foreach(var key in keys)
            {
                var orders = await redisDatabase.GetData<Order[]>(key, "orders");

                await redisDatabase.RemoveData(key, "orders");

                if (orders is null || orders.Length == 0)
                    continue;

                result.AddRange(orders);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task SaveOrders(IEnumerable<Order> orders)
        {
            await redisDatabase.SetData(Guid.NewGuid().ToString(), "orders", orders, TimeSpan.FromMinutes(10));
        }
    }
}
