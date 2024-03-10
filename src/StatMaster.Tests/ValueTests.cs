namespace StatMaster.Tests
{
    public class ValueTests
    {
        Property<int> a;
        IValue<int> b;

        public ValueTests()
        {
            a = new Property<int>();
            b = a.Select(x => x + 1, (v, x) => v.Value = x - 1);
        }

        [Fact]
        public void Value_Select_Test()
        {
            Assert.Equal(0, a.Value);
            Assert.Equal(1, b.Value);

            a.Value = 2;
            Assert.Equal(2, a.Value);
            Assert.Equal(3, b.Value);

            b.Value = 3;
            Assert.Equal(2, a.Value);
            Assert.Equal(3, b.Value);

            b.Value = 4;
            Assert.Equal(3, a.Value);
            Assert.Equal(4, b.Value);
        }

        [Fact]
        public void Bounded_InputValue_Test()
        {
            var boundedValue = new RangeValue<float>(100f, 0f, 100f);
            var v = new ModValue<float>(boundedValue);

            v.Add(Mod.Add(10f));
            Assert.Equal(110f, v.Value);

            boundedValue.Value = 200f;
            Assert.Equal(110f, v.Value);
        }

        [Fact]
        public void Bounded_OutputValue_Test()
        {
            var v = new RangeModValue<IValue<float>, float>(new Property<float>(100f), 0f, 100f);

            v.Add(Mod.Add(10f));
            Assert.Equal(100f, v.Value);

            v.Initial.Value = 200f;
            Assert.Equal(100f, v.Value);
        }

        [Fact]
        public void Bounded_InputOutputValue_Test()
        {
            var v = new RangeModValue<IValue<float>, float>(
                new RangeValue<float>(100f, 0f, 100f),
                0f,
                100f
            );

            v.Add(Mod.Add(10f));
            Assert.Equal(100f, v.Value);

            v.Initial.Value = 200f;
            Assert.Equal(100f, v.Value);
        }

        [Fact]
        public void INumber_Test()
        {
        }
    }
}