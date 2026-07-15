using ECommerce.Domain.Constants;

public class PlaceOrderModel
{
  public Guid UserId { get; set; }
  public List<OrderItemModel> Items { get; set; } = new List<OrderItemModel>();
  public OrderStatus OrderStatus { get; set; }
}