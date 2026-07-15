using ECommerce.Domain.Constants;

public record UpdateOrderInput(
  Guid UserId,
  List<OrderItemInput> Items,
  OrderStatus OrderStatus
);