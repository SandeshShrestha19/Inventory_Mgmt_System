using ECommerce.Domain.Entities;

namespace ECommerce.API.GraphQL.Types;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Field(u => u.Id);
        descriptor.Field(u => u.Name);
        descriptor.Field(u => u.Email);
        descriptor.Field(u => u.Role);
        descriptor.Field(u => u.IsActive); 
        descriptor.Field(u => u.CreatedAt);
        descriptor.Field(u => u.Orders);  
    }
}