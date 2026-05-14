public record UpdateOrderInput(
  Guid UserId,
  List<OrderItemInput> Items
);