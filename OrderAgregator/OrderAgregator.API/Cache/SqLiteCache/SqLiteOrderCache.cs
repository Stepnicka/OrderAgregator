using OrderAgregator.API.Models;
using System.Data;

namespace OrderAgregator.API.Cache.SqLiteCache
{
    /// <inheritdoc cref="IOrderCache"/>
    public class SqLiteOrderCache : IOrderCache
    {
        private readonly ISqLiteDatabase _sql;

        public SqLiteOrderCache(ISqLiteDatabase sql)
        {
            this._sql = sql;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Order>> GetOrders()
        {
            var result = await _sql.LoadData<Order, dynamic>(sql: @"
                    UPDATE [Order]
                    SET [State] = 1
                    WHERE [State] = 0
                    RETURNING *;

                    DELETE FROM [Order] WHERE [State] = 1;
            ", commandType: CommandType.Text, parameters: new { });

            return result;
        }

        /// <inheritdoc/>
        public async Task SaveOrders(IEnumerable<Order> orders)
        {
            await _sql.SaveData<dynamic>(sql: @"
                    INSERT INTO [Order]([ProductId],[Quantity],[State]) 
                    VALUES (@productId,@quantity, 0);
            ", commandType: CommandType.Text, parameters: orders.Select(order => new { productId = order.ProductId, quantity = order.Quantity }).ToList());
        }
    }
}
