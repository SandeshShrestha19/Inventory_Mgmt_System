namespace ECommerce.Domain.Entities;
public class Order
{
  public Guid Id { get; set; }
  public User User { get; set; }
  public Guid UserId { get; set; }
  public decimal TotalPrice { get; set; }
  public DateTime OrderDate { get; set; }
  public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}