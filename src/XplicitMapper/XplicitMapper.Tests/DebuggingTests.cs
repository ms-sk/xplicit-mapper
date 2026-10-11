using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;
using XplicitMapper.Debugging;
using XplicitMapper.DependencyInjection;

namespace XplicitMapper.Tests;

public class DebuggingTests
{
    public class DebugSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class DebugDest
    {
        public int Identifier { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class DebugMapper : IXMapper<DebugSource, DebugDest>
    {
        public DebugDest Map(DebugSource source)
        {
            return new DebugDest
            {
                Identifier = source.Id,
                FullName = source.Name,
                Value = source.Amount
            };
        }
    }

    [Fact]
    public void MappingContext_Depth_Tracks_Nesting_Correctly()
    {
        var provider = new MapperProvider();
        provider.Register(new DebugMapper());

        var beforeDepths = new List<int>();
        var afterDepths = new List<int>();

        var options = new XplicitMapperOptions();
        options.BeforeMapDump += (_, _, _, ctx) => beforeDepths.Add(ctx.Depth);
        options.AfterMapDump += (_, _, _, _, ctx) => afterDepths.Add(ctx.Depth);

        var trackingProvider = new MapperProvider(options);
        trackingProvider.Register(new DebugMapper());

        var source = new DebugSource { Id = 1, Name = "Test", Amount = 100.50m };
        trackingProvider.Map<DebugSource, DebugDest>(source);

        beforeDepths.Should().ContainSingle();
        beforeDepths[0].Should().Be(1);
        
        afterDepths.Should().ContainSingle();
        afterDepths[0].Should().Be(1);  // Still 1 because Exit() hasn't been called yet
    }

    [Fact]
    public void EnableJsonConsoleDumps_Can_Be_Configured()
    {
        // Just ensure we can call EnableJsonConsoleDumps without exceptions
        var options = new XplicitMapperOptions();
        var act = () => options.EnableJsonConsoleDumps();
        act.Should().NotThrow();
        
        // And with different combinations
        var act2 = () => options.EnableJsonConsoleDumps(dumpBeforeMap: false, dumpAfterMap: true);
        act2.Should().NotThrow();
    }

    [Fact]
    public void ExtensionMethods_Work_Without_Integrated()
    {
        var obj = new DebugSource { Id = 42, Name = "Extensions Test", Amount = 99.99m };
        
        // ToJsonDump extension
        string json = obj.ToJsonDump();
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("42");
        json.Should().Contain("Extensions Test");
        
        // Just ensure it doesn't throw
        var act = () => obj.DumpToConsole("Test Header");
        act.Should().NotThrow();
    }

    [Fact]
    public void EnableJsonConsoleDumpsFor_T_Can_Be_Configured()
    {
        var options = new XplicitMapperOptions();
        
        // Just ensure it doesn't throw
        var act = () => options.EnableJsonConsoleDumpsFor<DebugSource, DebugDest>();
        act.Should().NotThrow();
    }

    [Fact]
    public void DI_Integration_Supports_Dumps_Configuration()
    {
        var services = new ServiceCollection();
        
        services.AddXplicitMapper(opt =>
        {
            opt.AddMapper<DebugMapper>();
            opt.Configure(cfg =>
            {
                cfg.EnableJsonConsoleDumps();
                cfg.CircularReferenceHandling = CircularReferenceHandling.ReturnNull;
            });
        });

        var sp = services.BuildServiceProvider();
        var provider = sp.GetRequiredService<IMapperProvider>();
        
        provider.Should().NotBeNull();
        
        // Just ensure it works
        var source = new DebugSource { Id = 123, Name = "DI Test", Amount = 1234.56m };
        var dest = provider.Map<DebugSource, DebugDest>(source);
        
        dest.Identifier.Should().Be(123);
        dest.FullName.Should().Be("DI Test");
        dest.Value.Should().Be(1234.56m);
    }
}
