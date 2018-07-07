using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

using MainContents.ParentTest.ECS;

namespace MainContents.ParentTest
{
    public class DokabenParentTest : DokabenTestBase
    {
        /// <summary>
        /// 子オブジェクトのオフセット
        /// </summary>
        [SerializeField] Vector3 _childOffset;

        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        void Start()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // Root Entityのアーキタイプ
            var rootArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(Rotation),
#if ENABLE_FRUSTUM_CULLING
                typeof(MeshCullingComponent),
#endif
                typeof(TransformMatrix));

            // 親Entityのアーキタイプ
            var parentArchetype = entityManager.CreateArchetype(
                typeof(DokabenRotationData),
                typeof(LocalPosition),
                typeof(LocalRotation),
                typeof(TransformParent),
#if ENABLE_FRUSTUM_CULLING
                typeof(MeshCullingComponent),
#endif
                typeof(TransformMatrix));

            // 子Entityのアーキタイプ
            var childArchetype = entityManager.CreateArchetype(
                typeof(LocalPosition),
                typeof(LocalRotation),
#if ENABLE_FRUSTUM_CULLING
                typeof(MeshCullingComponent),
#endif
                typeof(TransformParent),
                typeof(TransformMatrix));

            // ドカベンロゴの生成
            base.CreateEntitiesFromRandomPosition((randomPosition, look) =>
                {
                    var rootEntity = entityManager.CreateEntity(rootArchetype);
                    entityManager.SetComponentData(rootEntity, new Position { Value = randomPosition });
                    entityManager.SetComponentData(rootEntity, new Rotation { Value = quaternion.identity });

                    // 親Entityの生成
                    var parentEntity = entityManager.CreateEntity(parentArchetype);
                    entityManager.SetComponentData(parentEntity, new LocalPosition { Value = new float3(0f, 0f, 0f) });
                    entityManager.SetComponentData(parentEntity, new LocalRotation { Value = quaternion.identity });
                    entityManager.SetComponentData(parentEntity, new TransformParent { Value = rootEntity });
                    entityManager.SetComponentData(parentEntity, new DokabenRotationData
                    {
                        CurrentAngle = Constants.ParentTest.Angle,
                        DeltaTimeCounter = 0f,
                        FrameCounter = 0,
                        CurrentRot = 0f,
                    });

                    // 子Entityの生成
                    var childEntity = entityManager.CreateEntity(childArchetype);
                    entityManager.SetComponentData(childEntity, new LocalPosition { Value = this._childOffset });
                    entityManager.SetComponentData(childEntity, new LocalRotation { Value = quaternion.identity });
                    entityManager.SetComponentData(childEntity, new TransformParent { Value = parentEntity });
                    entityManager.AddSharedComponentData(childEntity, look);
                });
        }
    }
}
