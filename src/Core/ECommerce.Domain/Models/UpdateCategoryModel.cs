using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Models;

public class UpdateCategoryModel
{
  public string? Name { get; set; } = string.Empty;
  public ICollection<Guid>? ProductIds { get; set; }
}