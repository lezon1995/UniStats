namespace UniStats.Tests
{
    public class ModValueTest
    {
        ModValue<float> health;
        ModValue<float> curHealth;
        IMod<float> boost;
        IMod<float> boost20;
        IMod<IValue<float>, float> damage;

        int countHealth;
        int countCurrentHealth;
        int countDamage;
        int countBoost;

        public ModValueTest()
        {
            InitializeValues();
            InitializeModifiers();
            AttachEventHandlers();
        }

        void InitializeValues()
        {
            health = new ModValue<float>(100F);
            curHealth = new ModValue<float>(health);
        }

        void InitializeModifiers()
        {
            boost = Mod.Mul(1.10F, "10% boost");
            boost20 = Mod.Mul(1.20F, "20% boost");
            damage = Mod.Add(new Property<float>(), "damage");

            health.Add(boost);
            curHealth.Add(damage);
        }

        void AttachEventHandlers()
        {
            health.OnChanged += (_, _) => countHealth++;
            curHealth.OnChanged += (_, _) => countCurrentHealth++;
            damage.OnChanged += () => countDamage++;
            boost.OnChanged += () => countBoost++;
        }

        [Fact]
        public void PropertyValue_Test()
        {
            // Arrange
            var v = new Property<int> { Value = 1 };
            var iv = (IValue<int>)v;

            // Assert initial and current values
            Assert.Equal(1, v.Value);
            Assert.Equal(1, iv.Value);

            // Act
            v.Value = 2;

            // Assert new values
            Assert.Equal(2, v.Value);
            Assert.Equal(2, iv.Value);
        }

        [Fact]
        public void Unmodified_Test()
        {
            // Clear all modifiers from health
            health.Clear();

            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(100f, health.Value);

            // Assert notification counts
            Assert.Equal(1, countHealth);
            Assert.Equal(1, countCurrentHealth);
            Assert.Equal(0, countDamage);
            Assert.Equal(0, countBoost);
        }

        [Fact]
        public void Modified_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Assert notification counts
            Assert.Equal(0, countHealth);
            Assert.Equal(0, countCurrentHealth);
            Assert.Equal(0, countDamage);
            Assert.Equal(0, countBoost);
        }

        [Fact]
        public void Disabled_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Act
            boost.Enabled = false;

            // Assert current health value
            Assert.Equal(100f, health.Value);

            // Assert notification counts
            Assert.Equal(1, countHealth);
            Assert.Equal(1, countCurrentHealth);
            Assert.Equal(0, countDamage);
            Assert.Equal(1, countBoost);
        }

        [Fact]
        public void Notification_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Act
            damage.Context.Value = 10f;

            // Assert notification counts
            Assert.Equal(0, countHealth);
            Assert.Equal(1, countCurrentHealth);
            Assert.Equal(1, countDamage);
            Assert.Equal(0, countBoost);
        }

        [Fact]
        public void NotificationOnAdd_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Assert notification counts
            Assert.Equal(0, countHealth);
            Assert.Equal(0, countCurrentHealth);
            Assert.Equal(0, countDamage);
            Assert.Equal(0, countBoost);

            // Act
            health.Add(boost20);

            // Assert current health value
            Assert.Equal(132f, health.Value);

            // Assert notification counts
            Assert.Equal(1, countHealth);
            Assert.Equal(1, countCurrentHealth);
            Assert.Equal(0, countDamage);
            Assert.Equal(0, countBoost);
        }

        [Fact]
        public void Stat_ToString_Test()
        {
            // Assert initial and current health values
            Assert.Equal("110.0", health.ToString());
            Assert.Equal("110.0", curHealth.ToString());
        }

        [Fact]
        public void Modifier_ToString_Test()
        {
            var m = Mod.Add(1);
            Assert.Equal("+1", m.ToString());

            m = Mod.Add(1, "+1 sword");
            Assert.Equal("\"+1 sword\" +1", m.ToString());

            m = Mod.Sub(1, "-1 poison");
            Assert.Equal("\"-1 poison\" -1", m.ToString());

            m = Mod.Mul(2, "blah");
            Assert.Equal("\"blah\" *2", m.ToString());

            m = Mod.Div(2, "die");
            Assert.Equal("\"die\" /2", m.ToString());

            m = Mod.Set(2, "equal");
            Assert.Equal("\"equal\" =2", m.ToString());
        }

        [Fact]
        public void Different_AccumulationStyle_Test()
        {
            // Arrange
            var strength = new ModValue<float>(10f);
            var strengthPercentageGain = new ModValue<float>(1f);

            // Act
            strengthPercentageGain.Add(Mod.Add(0.10f));
            strength.Add(Mod.Mul(strengthPercentageGain));

            // Assert
            Assert.Equal(11f, strength.Value);
        }

        [Fact]
        public void Different_AccumulationStyle_MixedTypes_Test()
        {
            // Arrange
            var strength = new ModValue<int>(10);
            var strengthPercentageGain = new ModValue<float>(1f);

            // Act
            strengthPercentageGain.Add(Mod.Add(0.1f));
            strength.Add(Mod.Mul(strengthPercentageGain).Cast<float, int>());

            // Assert
            Assert.Equal(11, strength.Value);
        }

        [Fact]
        public void Character_AccumulationStyle_Test()
        {
            // Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.

            // Arrange
            var stat = new ModValue<float>(10f);
            
            var initialPlus = new ModValue<float>();
            var baseTimes = new ModValue<float>(1f);
            var basePlus = new ModValue<float>();
            var totalTimes = new ModValue<float>(1f);
            var totalPlus = new ModValue<float>();

            Assert.True(stat is IValue<float>);
            Assert.True(initialPlus is IValue<float>);
            Assert.True(baseTimes is IValue<float>);
            Assert.True(basePlus is IValue<float>);
            Assert.True(totalTimes is IValue<float>);
            Assert.True(totalPlus is IValue<float>);

            stat.Add(Mod.Add(initialPlus));
            stat.Add(Mod.Mul(baseTimes));
            stat.Add(Mod.Add(basePlus));
            stat.Add(Mod.Mul(totalTimes));
            stat.Add(Mod.Add(totalPlus));

            Assert.Equal(10f, stat.Value);

            // Act
            initialPlus.Add(Mod.Add(1f));
            Assert.Equal(11f, stat.Value);

            baseTimes.Add(Mod.Add(1f));
            Assert.Equal(22f, stat.Value);

            basePlus.Add(Mod.Add(3f));
            Assert.Equal(25f, stat.Value);

            totalTimes.Add(Mod.Add(1f));
            Assert.Equal(50f, stat.Value);

            totalPlus.Add(Mod.Add(5f));
            Assert.Equal(55f, stat.Value);
        }

        [Fact]
        public void Ways_ToAdd_Test()
        {
            // Arrange
            var stat = new ModValue<float>(10f);
            var baseFlatPlus = new ModValue<float>(2f);

            Assert.True(stat is IValue<float>);
            Assert.True(baseFlatPlus is IValue<float>);

            var m = Mod.Add(baseFlatPlus);

            // Act
            stat.Add(m);
            stat.Add(Mod.Add(baseFlatPlus));
            stat.Add(Mod.Add(baseFlatPlus));

            // Assert
            Assert.Equal(16f, stat.Value);
        }

        [Fact]
        public void Ways_ToAdd_Literals_Test()
        {
            // Arrange
            var stat = new ModValue<float>(10f);
            var m = Mod.Add(2f);

            // Act
            stat.Add(m);
            stat.Add(Mod.Add(2f));
            stat.Add(Mod.Add(2f));

            // Assert
            Assert.Equal(16f, stat.Value);
        }

        [Fact]
        public void CharacterStyle_Test()
        {
            // Arrange
            int notifications = 0;
            int notifications2 = 0;
            var stat = new Stat<float>(10F);

            stat.OnChanged += (_, _) => notifications++;
            stat.InitialPlus.OnChanged += (_, _) => notifications2++;

            Assert.Equal(0, notifications);
            Assert.Equal(0, notifications2);
            Assert.Equal(10f, stat.Value);

            // Act
            stat.InitialPlus.Add(Mod.Add(1f));

            Assert.Equal(1, notifications);
            Assert.Equal(1, notifications2);
            Assert.Equal(11f, stat.Value);

            stat.InitialPlus.Add(Mod.Add(1f));

            // Assert
            Assert.Equal(2, notifications);
            Assert.Equal(2, notifications2);
            Assert.Equal(12f, stat.Value);
        }

        [Fact]
        public void ModifierPriority_Test()
        {
            health.Clear();

            health.Add(boost);
            health.Add(-10, damage);

            Assert.Equal(damage, health.Mods.First());
            Assert.Equal(boost, health.Mods.Skip(1).First());

            health.Add(boost20);

            Assert.Equal(boost20, health.Mods.Skip(2).First());
        }
    }
}