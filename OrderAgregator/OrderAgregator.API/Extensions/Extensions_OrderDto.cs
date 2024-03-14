namespace OrderAgregator.API
{
    public static partial class Extensions
    {
        public static Models.Order MapToOrder(this Models.OrderDto order)
        {
            return new Models.Order
            {
                ProductId = order.ProductId,
                Quantity = order.Quantity
            };
        }

        public static IEnumerable<Models.Order> MapToOrders(this IEnumerable<Models.OrderDto> orders)
        {
            if (orders is null)
                yield break;

            foreach (var order in orders)
            {
                yield return order.MapToOrder();
            }
        }
    }
}
