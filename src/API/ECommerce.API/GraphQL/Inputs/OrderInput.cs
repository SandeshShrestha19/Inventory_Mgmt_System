public record OrderInput(
  Guid UserId,
  List<OrderItemInput> Items
);