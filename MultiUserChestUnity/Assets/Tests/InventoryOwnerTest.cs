using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryOwnerTest {
        [Test]
        public void NullInventoryGetOwner() {
            InventoryOwner owner = InventoryOwner.GetOwner((Inventory)null);
            Assert.Null(owner);
        }

        [Test]
        public void NullItemGetOwner() {
            InventoryOwner owner = InventoryOwner.GetOwner((ItemDrop.ItemData)null);
            Assert.Null(owner);
        }
    }
}
