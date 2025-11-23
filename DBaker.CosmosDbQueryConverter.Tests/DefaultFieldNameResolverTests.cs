using DBaker.CosmosDbQueryConverter.Tests.Helpers;
using System.Linq.Expressions;

namespace DBaker.CosmosDbQueryConverter.Tests;

[TestClass]
public class DefaultFieldNameResolverTests : TestBase
{
    [TestMethod]
    public void ResolvesSimplePropertyWithCamelCase()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, string>> expr = c => c.Prop;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.prop", path);
    }

    [TestMethod]
    public void ResolvesSimplePropertyWithoutCamelCase()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: false);
        Expression<Func<TestDocument, string>> expr = c => c.Prop;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.Prop", path);
    }

    [TestMethod]
    public void ResolvesNestedPropertyWithCamelCase()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, string>> expr = c => c.SubDoc.Prop;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.subDoc.prop", path);
    }

    [TestMethod]
    public void ResolvesNewtonsoftJsonPropertyAttribute()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, string>> expr = c => c.NewtonsoftProp;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.newtonsoft_prop", path);
    }

    [TestMethod]
    public void ResolvesSystemTextJsonPropertyNameAttribute()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, string>> expr = c => c.SystemTextProp;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.system_text", path);
    }

    [TestMethod]
    public void ResolvesNestedNewtonsoftJsonProperty()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, string>> expr = c => c.SubDoc.NewtonsoftProp;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.subDoc.newtonsoft_prop", path);
    }

    [TestMethod]
    public void ResolvesNestedSystemTextJsonProperty()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, string>> expr = c => c.SubDoc.SystemTextProp;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.subDoc.system_text", path);
    }

    [TestMethod]
    public void ResolvesParameterExpression()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, TestDocument>> expr = c => c;
        var paramExpr = (ParameterExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(paramExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c", path);
    }

    [TestMethod]
    public void ResolvesUnaryExpressionWithConvert()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        var param = Expression.Parameter(typeof(TestDocument), "c");
        var prop = Expression.Property(param, nameof(TestDocument.Prop));
        var unaryExpr = Expression.Convert(prop, typeof(object));

        // Act
        var success = resolver.TryResolve(unaryExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.prop", path);
    }

    [TestMethod]
    public void ReturnsFailureForNullExpression()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);

        // Act
        var success = resolver.TryResolve(null, out var path);

        // Assert
        Assert.IsFalse(success);
        Assert.IsNull(path);
    }

    [TestMethod]
    public void ReturnsFailureForConstantExpression()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        var value = Faker.Random.Word();
        var constantExpr = Expression.Constant(value);

        // Act
        var success = resolver.TryResolve(constantExpr, out var path);

        // Assert
        Assert.IsFalse(success);
        Assert.IsNull(path);
    }

    [TestMethod]
    public void HandlesCustomParameterName()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        var paramName = Faker.Random.AlphaNumeric(5);
        var param = Expression.Parameter(typeof(TestDocument), paramName);
        Expression<Func<string>> expr = Expression.Lambda<Func<string>>(
            Expression.Property(param, nameof(TestDocument.Prop)));
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual($"{paramName}.prop", path);
    }

    [TestMethod]
    public void HandlesDeeplyNestedProperties()
    {
        // Arrange
        var resolver = new DefaultFieldNameResolver(useCamelCase: true);
        Expression<Func<TestDocument, string>> expr = c => c.SubDoc.Prop;
        var memberExpr = (MemberExpression)expr.Body;

        // Act
        var success = resolver.TryResolve(memberExpr, out var path);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual("c.subDoc.prop", path);
    }
}
