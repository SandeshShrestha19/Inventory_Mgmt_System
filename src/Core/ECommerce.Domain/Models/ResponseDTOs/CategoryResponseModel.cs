using ECommerce.Domain.Entities;

public class CategoryResponseModel
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public ICollection<Product> Products { get; set; } = new List<Product>();
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset ModifiedAt { get; set; }
}