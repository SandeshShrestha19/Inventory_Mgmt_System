using ECommerce.Domain.Entities;

public class OrderResponseModel
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public decimal TotalPrice { get; set; }
  public DateTime OrderDate { get; set; } = DateTime.UtcNow;
  public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}