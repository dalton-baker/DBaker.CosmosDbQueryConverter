using AutoFixture;
using Bogus;

namespace DBaker.CosmosQueryDefinitionBuilder.Tests.Helpers;

public abstract class TestBase
{
    public TestContext TestContext { get; set; } = null!;
    protected Faker Faker = new();
    protected Fixture Fixture = new();
}