using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

using MainContents.MatrixTest.ECS;

namespace MainContents.MatrixTest
{
    public sealed class DokabenMatrixTest : DokabenTestBase
    {
        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        void Start()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            var archetype = base._enableJobSystem
                ? entityManager.CreateArchetype(
                    typeof(EnableJobSystemData),
                    typeof(MatrixTestComponentData),
                    typeof(TransformMatrix))
                : entityManager.CreateArchetype(
                    typeof(DisableJobSystemData),
                    typeof(MatrixTestComponentData),
                    typeof(TransformMatrix));

            base.CreateEntitiesFromRandomPosition((randomPosition, look) =>
                {
                    var entity = entityManager.CreateEntity(archetype);
                    entityManager.SetComponentData(
                        entity,
                        new MatrixTestComponentData
                        {
                            AnimationHeader = 0f,
                            Position = randomPosition,
                        });
                    entityManager.AddSharedComponentData(entity, look);
                });
        }
    }
}
