using DBaker.CosmosQueryDefinitionBuilder.Tests.Helpers;
using System.Linq.Expressions;

namespace DBaker.CosmosQueryDefinitionBuilder.Tests;

[TestClass]
public class QueryBuilderHelperParameterizeExpressionTests : TestBase
{
    private readonly IFieldNameResolver _resolver = new DefaultFieldNameResolver(useCamelCase: true);

    [TestMethod]
    public void ExtractsScalarValuesFromExpression()
    {
        // Arrange
        var value1 = Faker.Random.Word();
        var value2 = Faker.Random.Int();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE c.name = {value1} AND c.age = {value2}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("@p0", query);
        Assert.Contains("@p1", query);
        Assert.AreEqual(value1, parameters["@p0"]);
        Assert.AreEqual(value2, parameters["@p1"]);
    }

    [TestMethod]
    public void ResolvesFieldPathsInExpression()
    {
        // Arrange
        var value = Faker.Random.Word();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM {c} WHERE {c.Prop} = {value}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains(" c ", query);
        Assert.Contains(" c.prop ", query);
        Assert.AreEqual(value, parameters["@p0"]);
    }

    [TestMethod]
    public void ResolvesNestedFieldPaths()
    {
        // Arrange
        var value = Faker.Random.Word();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE {c.SubDoc.Prop} = {value}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("c.subDoc.prop", query);
        Assert.AreEqual(value, parameters["@p0"]);
    }

    [TestMethod]
    public void ResolvesJsonNameFieldPaths()
    {
        // Arrange
        var value = Faker.Random.Word();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE {c.NewtonsoftProp} = {value}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("c.newtonsoft_prop", query);
        Assert.AreEqual(value, parameters["@p0"]);
    }

    [TestMethod]
    public void ExpandsEnumerableParameters()
    {
        // Arrange
        var values = Faker.Make(3, () => Faker.Random.Word()).ToList();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE c.id IN {values}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("IN (@p0, @p1, @p2)", query);
        for (int i = 0; i < values.Count; i++)
        {
            Assert.AreEqual(values[i], parameters[$"@p{i}"]);
        }
    }

    [TestMethod]
    public void HandlesMixedFieldReferencesAndValues()
    {
        // Arrange
        var name = Faker.Random.Word();
        var ids = Faker.Make(2, () => Faker.Random.Guid().ToString()).ToList();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM {c} WHERE {c.Prop} = {name} AND {c.SubDoc.Prop} IN {ids}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains(" c ", query);
        Assert.Contains("c.prop", query);
        Assert.Contains("c.subDoc.prop", query);
        Assert.AreEqual(name, parameters["@p0"]);
        Assert.AreEqual(ids[0], parameters["@p1"]);
        Assert.AreEqual(ids[1], parameters["@p2"]);
    }

    [TestMethod]
    public void HandlesMultipleDocumentParameters()
    {
        // Arrange
        var value1 = Faker.Random.Word();
        var value2 = Faker.Random.Int();
        Expression<Func<TestDocument, TestSubDocument, FormattableString>> lambda = (doc, sub) =>
            $"SELECT * FROM {doc} JOIN s IN {sub} WHERE {doc.Prop} = {value1} AND {sub.Prop} = {value2}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains(" doc ", query);
        Assert.Contains(" sub ", query);
        Assert.Contains("doc.prop", query);
        Assert.Contains("sub.prop", query);
        Assert.AreEqual(value1, parameters["@p0"]);
        Assert.AreEqual(value2, parameters["@p1"]);
    }

    [TestMethod]
    public void HandlesEmptyEnumerableInExpression()
    {
        // Arrange
        var emptyList = new List<string>();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE c.tags IN {emptyList}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("IN ()", query);
        Assert.IsEmpty(parameters);
    }

    [TestMethod]
    public void HandlesNullValueInExpression()
    {
        // Arrange
        string? nullValue = null;
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE c.field = {nullValue}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("@p0", query);
        Assert.IsNull(parameters["@p0"]);
    }

    [TestMethod]
    public void PreservesStringLiteralsInQuery()
    {
        // Arrange
        var value = Faker.Random.Word();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE c.status = 'active' AND c.name = {value}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("c.status = 'active'", query);
        Assert.AreEqual(value, parameters["@p0"]);
    }

    [TestMethod]
    public void ThrowsOnInvalidExpressionBody()
    {
        // Arrange
        var value = Faker.Random.Word();
        Expression<Func<string>> lambda = () => value;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            QueryBuilderHelper.ParameterizeExpression(lambda, _resolver));

        TestContext.WriteLine(ex.Message);
        Assert.Contains("FormattableStringFactory.Create", ex.Message);
    }

    [TestMethod]
    public void HandlesComplexQueryWithMultipleConditions()
    {
        // Arrange
        var status = Faker.Random.Word();
        var minAge = Faker.Random.Int(18, 100);
        var categories = Faker.Make(3, () => Faker.Random.Word()).ToList();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM {c} WHERE {c.Prop} = {status} AND c.age >= {minAge} AND {c.SubDoc.Prop} IN {categories}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("c.prop", query);
        Assert.Contains("c.subDoc.prop", query);
        Assert.AreEqual(status, parameters["@p0"]);
        Assert.AreEqual(minAge, parameters["@p1"]);
        Assert.AreEqual(categories[0], parameters["@p2"]);
        Assert.AreEqual(categories[1], parameters["@p3"]);
        Assert.AreEqual(categories[2], parameters["@p4"]);
    }

    [TestMethod]
    public void HandlesJsonPropertyAttributesInNestedPath()
    {
        // Arrange
        var value1 = Faker.Random.Word();
        var value2 = Faker.Random.Word();
        Expression<Func<TestDocument, FormattableString>> lambda = c =>
            $"SELECT * FROM c WHERE {c.SubDoc.NewtonsoftProp} = {value1} AND {c.SubDoc.SystemTextProp} = {value2}";

        // Act
        var (query, parameters) = QueryBuilderHelper.ParameterizeExpression(lambda, _resolver);

        // Assert
        TestContext.WriteLine(query);
        Assert.Contains("c.subDoc.newtonsoft_prop", query);
        Assert.Contains("c.subDoc.system_text", query);
        Assert.AreEqual(value1, parameters["@p0"]);
        Assert.AreEqual(value2, parameters["@p1"]);
    }
}
