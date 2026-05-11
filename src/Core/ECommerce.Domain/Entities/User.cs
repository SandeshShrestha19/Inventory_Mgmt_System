namespace ECommerce.Domain.Entities;
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Order> Orders = new List<Order>();

    public void SetPassword(string password)
    {
        this.Password = password;
    }
}