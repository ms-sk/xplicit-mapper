using XplicitMapper.Abstractions;
using XplicitMapper.Core;

namespace XplicitMapper.Console
{
    public sealed class UserMapper : IXMapper<User, UserDto>, IBeforeMap<User,UserDto>, IAfterMap<User,UserDto>
    {
        public void AfterMap(User source, UserDto destination, MappingContext context)
        {
            destination.Email = destination.Email.ToLower();
        }

        public void BeforeMap(User source, UserDto destination, MappingContext context)
        {
            source.Login = source.Login.ToUpper();
        }

        public UserDto Map(User source)
        {
            return new UserDto
            {
                Login = source.Login,
                Email = source.Email
            };
        }
    }
}

