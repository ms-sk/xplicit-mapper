

using Microsoft.Extensions.DependencyInjection;
using XplicitMapper.Abstractions;
using XplicitMapper.Console;
using XplicitMapper.Core;
using XplicitMapper.DependencyInjection;

var services = new ServiceCollection();
services.AddXplicitMapper(opt =>
{
    opt.Configure(x => x.EnableJsonConsoleDumps());
    opt.AddMapper<UserMapper>();
});

var provider = services.BuildServiceProvider();

var mapper = provider.GetService<IMapperProvider>()!;
var userDto = mapper.Map<User, UserDto>(new User { Login = "John Doe", Email = "jOhN@examPLe.com" });
Console.ReadLine();