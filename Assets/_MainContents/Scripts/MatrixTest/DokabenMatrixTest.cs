using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

using MainContents.MatrixTest.ECS;

namespace MainContents.MatrixTest
{
    public sealed class DokabenMatrixTest : DokabenTestBase
    {
        public bool useJobSystem;
        private EntityArchetype archetype;
        protected override EntityArchetype Archetype => archetype;

        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
            var entityManager = World.Active.GetExistingManager<EntityManager>();

            archetype = entityManager.CreateArchetype(
                ComponentType.Create<MatrixTestComponentData>(),
                ComponentType.Create<LocalToWorld>(),
                ComponentType.Create<MeshInstanceRenderer>());

            CreateEntitiesFromRandomPosition((entity, randomPosition) =>
            {
                entityManager.SetComponentData(entity, new MatrixTestComponentData
                {
                    // AnimationHeader = 0f,
                    Position = randomPosition,
                });
            });
            if (useJobSystem)
                World.Active.CreateManager(typeof(MatrixTestJobSystem));
            else
                World.Active.CreateManager(typeof(MatrixTestSystem));
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
        }
    }
}