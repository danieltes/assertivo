using System.Reflection;
using Assertivo;
using Assertivo.Collections;

namespace Assertivo.Tests;

public class AllSatisfyApiSurfaceGuardsTests
{
    [Fact]
    public void GenericCollectionAssertions_OnlyContainPredicateApi_ShouldNotExist()
    {
        var methods = typeof(GenericCollectionAssertions<int>).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        Assert.DoesNotContain(methods, method => method.Name.Equals("OnlyContain", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericCollectionAssertions_AllSatisfyAsyncOverload_ShouldNotExist()
    {
        var methods = typeof(GenericCollectionAssertions<int>)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name.Equals("AllSatisfy", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(2, methods.Count);
        Assert.Contains(methods, method => method.GetParameters()[0].ParameterType == typeof(Action<int>));
        Assert.Contains(methods, method => method.GetParameters()[0].ParameterType == typeof(Action<int, int>));

        Assert.DoesNotContain(methods, method =>
        {
            var firstParameterType = method.GetParameters()[0].ParameterType;
            return firstParameterType.IsGenericType
                   && firstParameterType.GetGenericTypeDefinition() == typeof(Func<,>);
        });

        Assert.DoesNotContain(methods, method =>
        {
            var firstParameterType = method.GetParameters()[0].ParameterType;
            return firstParameterType.IsGenericType
                   && firstParameterType.GetGenericTypeDefinition() == typeof(Func<,,>);
        });
    }

    [Fact]
    public void AssertivoAssembly_NewPublicAllSatisfyExceptionType_ShouldNotExist()
    {
        var publicExceptionTypes = typeof(GenericCollectionAssertions<int>)
            .Assembly
            .GetExportedTypes()
            .Where(type => typeof(Exception).IsAssignableFrom(type));

        Assert.DoesNotContain(
            publicExceptionTypes,
            type => type.Name.Contains("AllSatisfy", StringComparison.Ordinal));
    }

    [Fact]
    public void AllSatisfy_AggregatedFailure_ShouldFallbackToAssertionFailedException()
    {
        IEnumerable<int> subject = [1, 2, 3];

        Assert.Throws<AssertionFailedException>(() =>
            subject.Should().AllSatisfy(value => value.Should().Be(0)));
    }
}
