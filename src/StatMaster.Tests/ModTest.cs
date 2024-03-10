using StatMaster;
using Xunit.Abstractions;

namespace StatMaster.Tests
{
    public class ModTest
    {
        readonly ITestOutputHelper _testOutputHelper;

        public ModTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Modifier_ToString_Test()
        {
            // This trick works in dotnet core, not in Unity 2021.3 though.
            var m = Mod.Create((int x) => x + 1);
            // Assert.Equal("(int x) => x + 1", m.ToString());

            _testOutputHelper.WriteLine(m.ToString());
            var n = Mod.Create((int x) => x + 1, "+1 strength");
            // Assert.Equal("+1 strength", n.ToString());
            _testOutputHelper.WriteLine(n.ToString());
        }

        [Fact]
        public void Covariance_Test()
        {
            IMod<IValue<int>, int> m = Mod.Add(new Property<int>(1));
            Assert.True(m is IMod<IValue<int>, int>);
            Assert.True(m is IMod<IValue<int>, int>);
            IMod<IValue<int>, int> n = (IMod<IValue<int>, int>)m;
            Assert.True(n is IMod<IValue<int>, int>);
        }
    }
}
