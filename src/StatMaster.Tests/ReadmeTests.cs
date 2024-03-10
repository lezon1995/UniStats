namespace StatMaster.Tests
{
    public class ReadmeTests
    {
        [Fact]
        public void Health_Modification_Test()
        {
            // Arrange
            var health = new ModValue<float>(100f);

            // Assert initial health value
            Assert.Equal(100f, health.Value);

            // Act - Apply modifiers
            health.Add(Mod.Mul(1.10f));
            health.Add(Mod.Add(5f, "+5 health"));

            // Assert modified health value
            Assert.Equal(115f, health.Value);
        }

        [Fact]
        public void Damage_Modification_Test()
        {
            // Arrange
            var damage = new ModValue<float>(10f);
            int notificationCount = 0;
            damage.OnChanged += (_, _) => notificationCount++;

            // Assert initial damage value and notification count
            Assert.Equal(10f, damage.Value);
            Assert.Equal(0, notificationCount);

            // Act - Apply modifiers
            damage.Add(Mod.Mul(1.50f));
            damage.Add(Mod.Add(3f, "+3 damage"));

            // Assert modified damage value and notification count
            Assert.Equal(18f, damage.Value);
            Assert.Equal(2, notificationCount);
        }

        [Fact]
        public void Health_Bounds_Test()
        {
            // Arrange
            var maxHealth = new ModValue<float>(100f);
            var health = new RangeValue<float>(maxHealth.Value, 0f, maxHealth);
            int notificationCount = 0;
            health.OnChanged += (_, _) => notificationCount++;

            // Assert initial health and notification count
            Assert.Equal(0, notificationCount);
            Assert.Equal(100f, health.Value);
            Assert.Equal(100f, maxHealth.Value);

            // Act - Modify health
            health.Value -= 10f;
            maxHealth.Add(Mod.Add(20f, "+20 level gain"));

            // Assert modified health and maxHealth
            Assert.Equal(90f, health.Value);
            Assert.Equal(120f, maxHealth.Value);
        }

        [Fact]
        public void Health_Calculation_Test()
        {
            // Arrange
            var constitution = new ModValue<int>(10);
            int level = 10;
            var hpAdjustment = constitution.Select(
                con => (float)Math.Round((con - 10f) / 3f) * level
            );
            var maxHealth = new ModValue<float>(100f);
            int notificationCount = 0;
            maxHealth.OnChanged += (_, _) => notificationCount++;

            // Act - Apply modifiers
            maxHealth.Add(Mod.Add(hpAdjustment));

            // Assert initial max health and notification count
            Assert.Equal(100f, maxHealth.Value);
            Assert.Equal(1, notificationCount);

            // Act - Modify constitution
            constitution.Initial.Value = 15;

            // Assert modified max health and notification count
            Assert.Equal(120f, maxHealth.Value);
            Assert.Equal(2, notificationCount);
        }

        [Fact]
        public void Constitution_WithZip_Test()
        {
            // Arrange
            var constitution = new ModValue<int>(10);
            var level = new Property<int>(10);
            var hpAdjustment = constitution.Zip(level, (_constitution, _level) => (float)Math.Round((_constitution - 10f) / 3f) * _level);
            var maxHealth = new ModValue<float>(100f);
            int notificationCount = 0;
            maxHealth.OnChanged += (_, _) => notificationCount++;

            // Act - Apply modifiers
            maxHealth.Add(Mod.Add(hpAdjustment));

            // Assert initial max health and notification count
            Assert.Equal(100f, maxHealth.Value);
            Assert.Equal(1, notificationCount);

            // Act - Modify constitution
            constitution.Initial.Value = 15;

            // Assert modified max health and notification count
            Assert.Equal(120f, maxHealth.Value);
            Assert.Equal(2, notificationCount);

            // Act - Modify level
            level.Value = 15;

            // Assert modified max health and notification count
            Assert.Equal(130f, maxHealth.Value);
            Assert.Equal(3, notificationCount);
        }
    }
}