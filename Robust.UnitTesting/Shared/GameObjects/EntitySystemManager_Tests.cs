using NUnit.Framework;
using Robust.Client.Reflection;
using Robust.Shared.GameObjects;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Interfaces.GameObjects.Systems;
using Robust.Shared.Interfaces.Log;
using Robust.Shared.Interfaces.Reflection;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.UnitTesting.Shared.Reflection;

namespace Robust.UnitTesting.Shared.GameObjects
{
    [TestFixture, TestOf(typeof(EntitySystemManager))]
    public class EntitySystemManager_Tests: RobustUnitTest
    {

        public abstract class ESystemBase : IEntitySystem
        {
            public void Initialize() { }
            public void Shutdown() { }
            public void Update(float frameTime) { }
            public void FrameUpdate(float frameTime) { }
        }
        public class ESystemA : ESystemBase { }
        public class ESystemC : ESystemA { }
        public abstract class ESystemBase2 : ESystemBase { }
        public class ESystemB : ESystemBase2 { }

        /*
         ESystemBase (Abstract)
           - ESystemA
             - ESystemC
           - EsystemBase2 (Abstract)
             - ESystemB

         */

        [Test]
        public void GetsByTypeOrSupertype()
        {
            var esm = IoCManager.Resolve<IEntitySystemManager>();
            esm.Initialize();

            // getting type by the exact type should work fine
            Assert.AreEqual(esm.GetEntitySystem<ESystemB>().GetType(), typeof(ESystemB));

            // getting type by an abstract supertype should work fine
            // because there are no other subtypes of that supertype it would conflict with
            // it should return the only concrete subtype
            Assert.AreEqual(esm.GetEntitySystem<ESystemBase2>().GetType(), typeof(ESystemB));

            // getting ESystemA type by its exact type should work fine,
            // even though EsystemC is a subtype - it should return an instance of ESystemA
            var esysA = esm.GetEntitySystem<ESystemA>();
            Assert.AreEqual(esysA.GetType(), typeof(ESystemA));
            Assert.AreNotEqual(esysA.GetType(), typeof(ESystemC));

            var esysC = esm.GetEntitySystem<ESystemC>();
            Assert.AreEqual(esysC.GetType(), typeof(ESystemC));

            // this should not work - it's abstract and there are multiple
            // concrete subtypes
            Assert.Throws<InvalidEntitySystemException>(() =>
            {
                esm.GetEntitySystem<ESystemBase>();
            });
        }

    }
}
