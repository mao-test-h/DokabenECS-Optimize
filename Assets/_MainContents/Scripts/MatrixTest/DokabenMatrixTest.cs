using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

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

            var archetype = entityManager.CreateArchetype(
                typeof(MatrixTestComponentData),
                typeof(MeshCullingComponent),
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
