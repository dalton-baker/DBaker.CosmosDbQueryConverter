using Bogus;
using DBaker.CosmosDbQueryConverter.Tests.Helpers;

namespace DBaker.CosmosDbQueryConverter.Tests;

[TestClass]
public class ParameterizeQueryTests : TestBase
{
    [TestMethod]
    public void HandlesScalarValues()
    {
        // Arrange
        var id = Faker.Random.Guid().ToString();
        var status = Faker.PickRandom("active", "pending", "archived");

        // Act
        var result = CosmosDbQuery.Convert(
            (FormattableString)$"SELECT * FROM c WHERE c.id = {id} AND c.status = {status}");
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
        var categories = Faker.Make(3, () => Faker.Commerce.Categories(1).First()).ToArray();

        // Act
        var result = CosmosDbQuery.Convert(
            (FormattableString)$"SELECT * FROM c WHERE c.category IN {categories}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        for (int i = 0; i < categories.Length; i++)
        {
            Assert.AreEqual(categories[i], parameters[$"@p{i}"]);
        }
    }

    [TestMethod]
    public void ReusesSameValueTwice()
    {
        // Arrange
        var keyword = Faker.Hacker.Verb();

        // Act
        var result = CosmosDbQuery.Convert(
            (FormattableString)$"SELECT * FROM c WHERE c.term1 = {keyword} OR c.term2 = {keyword}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.HasCount(2, parameters);
        Assert.AreEqual(keyword, parameters["@p0"]);
        Assert.AreEqual(keyword, parameters["@p1"]);
    }

    [TestMethod]
    public void AllowsUnusedValues()
    {
        // Arrange
        var used = Faker.Random.Word();
        FormattableString formattable = $"SELECT * FROM c WHERE c.col = {used}";

        // Act
        var result = CosmosDbQuery.Convert(formattable);
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.HasCount(1, parameters);
        Assert.AreEqual(used, parameters["@p0"]);
    }

    [TestMethod]
    public void AllowsEmptyCollection()
    {
        // Arrange
        var empty = Array.Empty<string>();

        // Act
        var result = CosmosDbQuery.Convert(
            (FormattableString)$"SELECT * FROM c WHERE c.tags IN {empty}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.HasCount(0, parameters);
        Assert.Contains("()", result.QueryText);
    }
}
