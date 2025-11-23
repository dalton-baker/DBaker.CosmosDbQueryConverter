using DBaker.CosmosQueryDefinitionBuilder.Tests.Helpers;

namespace DBaker.CosmosQueryDefinitionBuilder.Tests;

[TestClass]
public class BuildQueryTests : TestBase
{
    [TestMethod]
    public void ReplacesScalarsCorrectly()
    {
        // Arrange
        var id = Faker.Random.Guid().ToString();
        var status = Faker.PickRandom("active", "inactive", "pending");
        var query = "SELECT * FROM c WHERE c.id = {0} AND c.status = {1}";

        // Act
        var result = CosmosDbQuery.Convert(query, id, status);
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.HasCount(2, parameters);
        Assert.AreEqual(id, parameters["@p0"]);
        Assert.AreEqual(status, parameters["@p1"]);
    }

    [TestMethod]
    public void ExpandsEnumerableParameter()
    {
        // Arrange
        var ids = Enumerable.Range(0, 3).Select(_ => Faker.Random.Guid().ToString()).ToList();
        var query = "SELECT * FROM c WHERE c.id IN {0}";

        // Act
        var result = CosmosDbQuery.Convert(query, ids);
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        for (int i = 0; i < ids.Count; i++)
        {
            Assert.AreEqual(ids[i], parameters[$"@p{i}"]);
        }
    }

    [TestMethod]
    public void ReusesParametersWithSameIndex()
    {
        // Arrange
        var name = Faker.Name.FirstName();
        var query = "SELECT * FROM c WHERE c.name = {0} OR c.alias = {0}";

        // Act
        var result = CosmosDbQuery.Convert(query, name);
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.HasCount(1, parameters);
        Assert.AreEqual(name, parameters["@p0"]);
        Assert.AreEqual(2, result.QueryText.Split("@p0").Length - 1);
    }

    [TestMethod]
    public void AllowsUnusedValuesWithoutException()
    {
        // Arrange
        var used = Faker.Random.Word();
        var unused = Faker.Random.Word();
        var query = "SELECT * FROM c WHERE c.col = {0}";

        // Act
        var result = CosmosDbQuery.Convert(query, used, unused);
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.HasCount(1, parameters);
        Assert.AreEqual(used, parameters["@p0"]);
    }

    [TestMethod]
    public void ThrowsOnMissingParameterIndex()
    {
        // Arrange
        var value = Faker.Random.Guid().ToString();
        var query = "SELECT * FROM c WHERE c.id = {1}";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            CosmosDbQuery.Convert(query, value));

        TestContext.WriteLine(ex.Message);
        Assert.Contains("No parameter provided for index {1}", ex.Message);
    }
}
