namespace BGR.Console.Tests.Unit;

public class TypeRegistrarTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldNotThrowShould()
  {
    var mockBuilder = new Mock<IHostBuilder>();

    Should.NotThrow(() => new TypeRegistrar(mockBuilder.Object));
  }

  [Fact]
  public void Build_WhenCalled_ItShouldReturnResolverAndBuildHost()
  {
    var mockHost = new Mock<IHost>();
    var mockBuilder = new Mock<IHostBuilder>();

    mockBuilder
      .Setup(static b => b.Build())
      .Returns(mockHost.Object);

    var registrar = new TypeRegistrar(mockBuilder.Object);

    var resolver = registrar.Build();

    resolver.ShouldBeOfType<TypeResolver>();
    mockBuilder.Verify(static b => b.Build(), Times.Once);
  }

  [Fact]
  public void Register_WhenCalledWithType_ItShouldAddToContainer()
  {
    var builder = Host.CreateDefaultBuilder();
    var registrar = new TypeRegistrar(builder);
    registrar.Register(typeof(IService), typeof(ServiceImplementation));

    using var host = builder.Build();
    var service = host.Services.GetService<IService>();

    service.ShouldNotBeNull();
    service.ShouldBeOfType<ServiceImplementation>();
  }

  [Fact]
  public void RegisterInstance_WhenCalledWithInstance_ItShouldAddToContainer()
  {
    var builder = Host.CreateDefaultBuilder();
    var registrar = new TypeRegistrar(builder);
    var instance = new ServiceImplementation();
    registrar.RegisterInstance(typeof(IService), instance);

    using var host = builder.Build();
    var service = host.Services.GetService<IService>();

    service.ShouldBeSameAs(instance);
  }

  [Fact]
  public void RegisterLazy_WhenCalledWithFunc_ItShouldAddToContainer()
  {
    var builder = Host.CreateDefaultBuilder();
    var registrar = new TypeRegistrar(builder);
    registrar.RegisterLazy(typeof(IService), static () => new ServiceImplementation());

    using var host = builder.Build();
    var service = host.Services.GetService<IService>();

    service.ShouldNotBeNull();
    service.ShouldBeOfType<ServiceImplementation>();
  }

  [Fact]
  public void RegisterLazy_WhenFuncIsNull_ItShouldThrow()
  {
    var builder = Host.CreateDefaultBuilder();
    var registrar = new TypeRegistrar(builder);

    Should.Throw<ArgumentNullException>(() => registrar.RegisterLazy(typeof(IService), null!));
  }

  private interface IService { }
  private sealed class ServiceImplementation : IService { }
}