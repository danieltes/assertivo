using Assertivo;

namespace QuickstartSmoke;

public class UnitTest1
{
    [Fact]
    public void My_first_assertion()
    {
        int result = 2 + 2;
        result.Should().Be(4);
    }
}
