using DBaker.CosmosDbQueryConverter;
using DBaker.CosmosDbQueryConverter.Tests.Helpers;

namespace DBaker.CosmosDbQueryConverter.Tests;

[TestClass]
public class QueryBuilderHelperBuildQueryTests : TestBase
{
    [TestMethod]
    public void ReplacesScalarPlaceholders()
    {
        // Arrange
        var value1 = Faker.Random.Word();
        var value2 = Faker.Random.Int();
        var query = "SELECT * FROM c WHERE c.name = {0} AND c.age = {1}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, value1, value2);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.AreEqual("SELECT * FROM c WHERE c.name = @p0 AND c.age = @p1", resultQuery);
        Assert.AreEqual(value1, parameters["@p0"]);
        Assert.AreEqual(value2, parameters["@p1"]);
    }

    [TestMethod]
    public void ExpandsEnumerableIntoMultipleParameters()
    {
        // Arrange
        var values = Faker.Make(Faker.Random.Int(2, 5), () => Faker.Random.Word()).ToList();
        var query = "SELECT * FROM c WHERE c.id IN {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, values);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.Contains("IN (@p0", resultQuery);
        for (int i = 0; i < values.Count; i++)
        {
            Assert.AreEqual(values[i], parameters[$"@p{i}"]);
        }
    }

    [TestMethod]
    public void HandlesEmptyEnumerable()
    {
        // Arrange
        var emptyList = new List<string>();
        var query = "SELECT * FROM c WHERE c.tags IN {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, emptyList);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.AreEqual("SELECT * FROM c WHERE c.tags IN ()", resultQuery);
        Assert.IsEmpty(parameters);
    }

    [TestMethod]
    public void ReusesPlaceholderWithSameIndex()
    {
        // Arrange
        var value = Faker.Random.Word();
        var query = "SELECT * FROM c WHERE c.field1 = {0} OR c.field2 = {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, value);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.AreEqual("SELECT * FROM c WHERE c.field1 = @p0 OR c.field2 = @p0", resultQuery);
        Assert.HasCount(1, parameters);
        Assert.AreEqual(value, parameters["@p0"]);
    }

    [TestMethod]
    public void HandlesMixedScalarsAndEnumerables()
    {
        // Arrange
        var scalar1 = Faker.Random.Word();
        var list = Faker.Make(3, () => Faker.Random.Int()).ToArray();
        var scalar2 = Faker.Random.Guid().ToString();
        var query = "SELECT * FROM c WHERE c.name = {0} AND c.age IN {1} AND c.id = {2}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, scalar1, list, scalar2);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.Contains("c.name = @p0", resultQuery);
        Assert.Contains("c.age IN (@p1, @p2, @p3)", resultQuery);
        Assert.Contains("c.id = @p4", resultQuery);
        Assert.AreEqual(scalar1, parameters["@p0"]);
        Assert.AreEqual(list[0], parameters["@p1"]);
        Assert.AreEqual(list[1], parameters["@p2"]);
        Assert.AreEqual(list[2], parameters["@p3"]);
        Assert.AreEqual(scalar2, parameters["@p4"]);
    }

    [TestMethod]
    public void ThrowsOnMissingParameter()
    {
        // Arrange
        var value = Faker.Random.Word();
        var query = "SELECT * FROM c WHERE c.field1 = {0} AND c.field2 = {1}";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            QueryBuilderHelper.BuildQuery(query, value));

        TestContext.WriteLine(ex.Message);
        Assert.Contains("No parameter provided for index {1}", ex.Message);
    }

    [TestMethod]
    public void AllowsUnusedParameters()
    {
        // Arrange
        var value1 = Faker.Random.Word();
        var value2 = Faker.Random.Word();
        var value3 = Faker.Random.Word();
        var query = "SELECT * FROM c WHERE c.name = {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, value1, value2, value3);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.HasCount(1, parameters);
        Assert.AreEqual(value1, parameters["@p0"]);
    }

    [TestMethod]
    public void HandlesNullValue()
    {
        // Arrange
        var query = "SELECT * FROM c WHERE c.field = {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, (object?)null);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.AreEqual("SELECT * FROM c WHERE c.field = @p0", resultQuery);
        Assert.IsNull(parameters["@p0"]);
    }

    [TestMethod]
    public void DoesNotExpandStringAsEnumerable()
    {
        // Arrange
        var stringValue = Faker.Random.Words();
        var query = "SELECT * FROM c WHERE c.text = {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, stringValue);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.AreEqual("SELECT * FROM c WHERE c.text = @p0", resultQuery);
        Assert.HasCount(1, parameters);
        Assert.AreEqual(stringValue, parameters["@p0"]);
    }

    [TestMethod]
    public void DoesNotExpandByteArrayAsEnumerable()
    {
        // Arrange
        var bytes = Faker.Random.Bytes(Faker.Random.Int(5, 20));
        var query = "SELECT * FROM c WHERE c.data = {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, bytes);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.AreEqual("SELECT * FROM c WHERE c.data = @p0", resultQuery);
        Assert.HasCount(1, parameters);
        Assert.AreEqual(bytes, parameters["@p0"]);
    }

    [TestMethod]
    public void HandlesMultipleEnumerablesInQuery()
    {
        // Arrange
        var list1 = Faker.Make(2, () => Faker.Random.Word()).ToList();
        var list2 = Faker.Make(3, () => Faker.Random.Int()).ToArray();
        var query = "SELECT * FROM c WHERE c.names IN {0} AND c.ages IN {1}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, list1, list2);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.Contains("c.names IN (@p0, @p1)", resultQuery);
        Assert.Contains("c.ages IN (@p2, @p3, @p4)", resultQuery);
        Assert.HasCount(5, parameters);
    }

    [TestMethod]
    public void ReusesSameEnumerableParameter()
    {
        // Arrange
        var list = Faker.Make(2, () => Faker.Random.Word()).ToList();
        var query = "SELECT * FROM c WHERE c.field1 IN {0} OR c.field2 IN {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, list);

        // Assert
        TestContext.WriteLine(resultQuery);
        var expectedExpansion = "(@p0, @p1)";
        Assert.Contains($"c.field1 IN {expectedExpansion}", resultQuery);
        Assert.Contains($"c.field2 IN {expectedExpansion}", resultQuery);
        Assert.HasCount(2, parameters);
    }

    [TestMethod]
    public void HandlesOutOfOrderPlaceholders()
    {
        // Arrange
        var value1 = Faker.Random.Word();
        var value2 = Faker.Random.Int();
        var query = "SELECT * FROM c WHERE c.age = {1} AND c.name = {0}";

        // Act
        var (resultQuery, parameters) = QueryBuilderHelper.BuildQuery(query, value1, value2);

        // Assert
        TestContext.WriteLine(resultQuery);
        Assert.Contains("c.age = @p0", resultQuery);
        Assert.Contains("c.name = @p1", resultQuery);
        Assert.AreEqual(value2, parameters["@p0"]);
        Assert.AreEqual(value1, parameters["@p1"]);
    }
}
