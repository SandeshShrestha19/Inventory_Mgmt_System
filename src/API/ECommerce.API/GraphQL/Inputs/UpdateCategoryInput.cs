using ECommerce.Domain.Entities;

public record UpdateCategoryInput
(
   string? Name,
   ICollection<Guid>? ProductIds
);