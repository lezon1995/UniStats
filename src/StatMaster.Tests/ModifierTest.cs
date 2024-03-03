using StatMaster;
using Xunit.Abstractions;

namespace StatMaster.Tests
{
    public class ModifierTest
    {
        readonly ITestOutputHelper _testOutputHelper;

        public ModifierTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Modifier_ToString_Test()
        {
            // This trick works in dotnet core, not in Unity 2021.3 though.
            var m = Modifier.Create((int x) => x + 1);
            // Assert.Equal("(int x) => x + 1", m.ToString());

            _testOutputHelper.WriteLine(m.ToString());
            var n = Modifier.Create((int x) => x + 1, "+1 strength");
            // Assert.Equal("+1 strength", n.ToString());
            _testOutputHelper.WriteLine(n.ToString());
        }

        [Fact]
        public void Covariance_Test()
        {
            IModifier<IValue<int>, int> m = Modifier.Plus(new PropertyValue<int>(1));
            Assert.True(m is IModifier<IValue<int>, int>);
            Assert.True(m is IModifier<IReadOnlyValue<int>, int>);
            IModifier<IReadOnlyValue<int>, int> n = (IModifier<IReadOnlyValue<int>, int>)m;
            Assert.True(n is IModifier<IReadOnlyValue<int>, int>);
        }
    }
}
