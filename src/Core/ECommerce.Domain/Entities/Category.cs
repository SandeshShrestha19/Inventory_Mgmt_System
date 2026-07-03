using ECommerce.Domain.Entities;

public class Category
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public ICollection<Product> Products { get; set; } = new List<Product>();
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;
}