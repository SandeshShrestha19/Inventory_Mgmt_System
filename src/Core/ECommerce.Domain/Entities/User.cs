
namespace ECommerce.Domain.Entities;
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive {get; set;} = false;
    public string Role { get; set;} = string.Empty;
    public ICollection<Order> Orders {get; set;} = new List<Order>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Password { get; set; }
    
}