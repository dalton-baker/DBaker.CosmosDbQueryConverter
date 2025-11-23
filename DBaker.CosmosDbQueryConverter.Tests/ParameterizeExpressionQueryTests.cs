using DBaker.CosmosDbQueryConverter.Tests.Helpers;

namespace DBaker.CosmosDbQueryConverter.Tests;

[TestClass]
public class ParameterizeExpressionQueryTests : TestBase
{
    [TestMethod]
    public void ExtractsFieldPath()
    {
        // Arrange
        var value = Faker.Random.Word();

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.Prop} = {value}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.prop ", result.QueryText);
        Assert.AreEqual(value, parameters["@p0"]);
    }

    [TestMethod]
    public void ExtractsSubdocumentFieldPath()
    {
        // Arrange
        var value = Faker.Random.Word();

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.SubDoc.Prop} = {value}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.subDoc.prop ", result.QueryText);
        Assert.AreEqual(value, parameters["@p0"]);
    }

    [TestMethod]
    public void ExtractsJsonNameFieldPath()
    {
        // Arrange
        var value = Faker.Random.Word();

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.NewtonsoftProp} = {value} AND {c.SystemTextProp} = {value}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.newtonsoft_prop ", result.QueryText);
        Assert.Contains(" c.system_text ", result.QueryText);
        Assert.AreEqual(value, parameters["@p0"]);
        Assert.AreEqual(value, parameters["@p1"]);
    }

    [TestMethod]
    public void ExtractsSubdocumentJsonNameFieldPath()
    {
        // Arrange
        var value = Faker.Random.Word();

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.SubDoc.NewtonsoftProp} = {value} AND {c.SubDoc.SystemTextProp} = {value}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.subDoc.newtonsoft_prop ", result.QueryText);
        Assert.Contains(" c.subDoc.system_text ", result.QueryText);
        Assert.AreEqual(value, parameters["@p0"]);
        Assert.AreEqual(value, parameters["@p1"]);
    }

    [TestMethod]
    public void ExpandsListParameter()
    {
        // Arrange
        var values = Faker.Make(3, () => Faker.Random.Word());

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.Prop} IN {values}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.prop IN (", result.QueryText);
        for (int i = 0; i < values.Count; i++)
        {
            Assert.AreEqual(values[i], parameters[$"@p{i}"]);
        }
    }

    [TestMethod]
    public void ExpandsListInSubdocument()
    {
        // Arrange
        var values = Faker.Make(3, () => Faker.Random.Word());

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.SubDoc.Prop} IN {values}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.subDoc.prop IN (", result.QueryText);
        for (int i = 0; i < values.Count; i++)
        {
            Assert.AreEqual(values[i], parameters[$"@p{i}"]);
        }
    }

    [TestMethod]
    public void ExpandsListWithJsonNameFields()
    {
        // Arrange
        var values = Faker.Make(2, () => Faker.Random.Word());

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.NewtonsoftProp} IN {values} AND {c.SystemTextProp} IN {values}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.newtonsoft_prop IN (", result.QueryText);
        Assert.Contains(" c.system_text IN (", result.QueryText);
        Assert.AreEqual(values[0], parameters["@p0"]);
        Assert.AreEqual(values[1], parameters["@p1"]);
        Assert.AreEqual(values[0], parameters["@p2"]);
        Assert.AreEqual(values[1], parameters["@p3"]);
    }

    [TestMethod]
    public void ExpandsListWithJsonNameFieldsInSubdocument()
    {
        // Arrange
        var values = Faker.Make(2, () => Faker.Random.Word());

        // Act
        var result = CosmosDbQuery.Convert((TestDocument c) =>
            $"SELECT * FROM {c} WHERE {c.SubDoc.NewtonsoftProp} IN {values} AND {c.SubDoc.SystemTextProp} IN {values}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" c.subDoc.newtonsoft_prop IN (", result.QueryText);
        Assert.Contains(" c.subDoc.system_text IN (", result.QueryText);
        Assert.AreEqual(values[0], parameters["@p0"]);
        Assert.AreEqual(values[1], parameters["@p1"]);
        Assert.AreEqual(values[0], parameters["@p2"]);
        Assert.AreEqual(values[1], parameters["@p3"]);
    }

    [TestMethod]
    public void ExtractsFieldPathsAcrossTwoDocuments()
    {
        // Arrange
        var value = Faker.Random.Word();

        // Act
        var result = CosmosDbQuery.Convert((TestDocument doc, TestSubDocument sub) =>
            $"""SELECT * FROM {doc} JOIN sub IN {sub} WHERE {doc.Prop} = {value} AND {sub.Prop} = {value}""");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" doc.prop = @p0", result.QueryText);
        Assert.Contains(" sub.prop = @p1", result.QueryText);
        Assert.AreEqual(value, parameters["@p0"]);
        Assert.AreEqual(value, parameters["@p1"]);
    }

    [TestMethod]
    public void ExtractsJsonNamePathsAcrossTwoDocuments()
    {
        // Arrange
        var value = Faker.Random.Word();

        // Act
        var result = CosmosDbQuery.Convert((TestDocument doc, TestSubDocument sub) =>
            $"SELECT * FROM {doc} JOIN sub IN {sub} WHERE {doc.NewtonsoftProp} = {value} AND {sub.SystemTextProp} = {value}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" doc.newtonsoft_prop = @p0", result.QueryText);
        Assert.Contains(" sub.system_text = @p1", result.QueryText);
        Assert.AreEqual(value, parameters["@p0"]);
        Assert.AreEqual(value, parameters["@p1"]);
    }

    [TestMethod]
    public void ExpandsListsAcrossTwoDocuments()
    {
        // Arrange
        var values1 = Faker.Make(2, () => Faker.Random.Word());
        var values2 = Faker.Make(3, () => Faker.Random.Word());

        // Act
        var result = CosmosDbQuery.Convert((TestDocument a, TestSubDocument b) =>
            $"SELECT * FROM {a} JOIN sub IN {b} WHERE {a.Prop} IN {values1} AND {b.Prop} IN {values2}");
        var parameters = result.GetDictionaryQueryParameters();

        // Assert
        TestContext.WriteLine(result.QueryText);
        Assert.Contains(" a.prop IN (", result.QueryText);
        Assert.Contains(" b.prop IN (", result.QueryText);

        // Validate expanded parameter values
        for (int i = 0; i < values1.Count; i++)
            Assert.AreEqual(values1[i], parameters[$"@p{i}"]);

        for (int i = 0; i < values2.Count; i++)
            Assert.AreEqual(values2[i], parameters[$"@p{i + values1.Count}"]);
    }
}