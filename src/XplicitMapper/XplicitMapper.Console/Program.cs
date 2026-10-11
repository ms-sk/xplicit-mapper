

using Microsoft.Extensions.DependencyInjection;
using XplicitMapper.Abstractions;
using XplicitMapper.Console;
using XplicitMapper.Core;
using XplicitMapper.DependencyInjection;
using XplicitMapper.Extensions;

var services = new ServiceCollection();
services.AddXplicitMapper(opt =>
{
    opt.Configure(x => x.EnableJsonConsoleDumps());
    opt.AddMapper<UserMapper>();
});

var provider = services.BuildServiceProvider();

var mapper = provider.GetService<IMapperProvider>()!;

var list = new List<User>() { new User { Login = "John Doe", Email = "jOhN@examPLe.com" }

};

var userDtos = list.MapToList<User, UserDto>(mapper);
Console.ReadLine();