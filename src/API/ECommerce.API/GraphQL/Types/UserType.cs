using ECommerce.Domain.Entities;
using HotChocolate.Types;

namespace ECommerce.API.GraphQL.Types;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Field(u => u.Id);
        descriptor.Field(u => u.Name);
        descriptor.Field(u => u.Email);
        descriptor.Field(u => u.CreatedAt);
        descriptor.Field(u => u.Orders);  
    }
}