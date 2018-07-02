using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

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

            // 親Entityのアーキタイプ
            // →親EntityはComponentSystem上で実行されるのでJobの有無で分ける
            var parentArchetype = base._enableJobSystem
                ? entityManager.CreateArchetype(
                        typeof(EnableJobSystemData),
                        typeof(DokabenRotationData),
                        typeof(Position),
                        typeof(Rotation),
                        typeof(TransformMatrix))
                : entityManager.CreateArchetype(
                        typeof(DisableJobSystemData),
                        typeof(DokabenRotationData),
                        typeof(Position),
                        typeof(Rotation),
                        typeof(TransformMatrix));

            // 子Entityのアーキタイプ
            var childArchetype = entityManager.CreateArchetype(
                typeof(LocalPosition),
                typeof(LocalRotation),
                typeof(TransformParent),
                typeof(TransformMatrix));

            base.CreateEntitiesFromRandomPosition((randomPosition, look) =>
                {
                    // 親Entityの生成
                    var parentEntity = entityManager.CreateEntity(parentArchetype);
                    entityManager.SetComponentData(parentEntity, new Position { Value = randomPosition });
                    entityManager.SetComponentData(parentEntity, new Rotation { Value = quaternion.identity });
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
