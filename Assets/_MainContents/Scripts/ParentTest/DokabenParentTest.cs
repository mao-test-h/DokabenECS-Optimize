using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

using MainContents.ParentTest.ECS;

namespace MainContents.ParentTest
{
    public sealed class DokabenParentTest : DokabenTestBase
    {
        /// <summary>
        /// 子オブジェクトのオフセット
        /// </summary>
        [SerializeField] Vector3 _childOffset;
        [SerializeField] bool useJobSystem;

        private EntityArchetype archetype;
        protected override EntityArchetype Archetype => archetype;

        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // 子Entityのアーキタイプ
            archetype = entityManager.CreateArchetype(
                ComponentType.Create<Position>(),
                ComponentType.Create<Rotation>(),
                ComponentType.Create<MeshInstanceRenderer>());

            // 親Entityのアーキタイプ
            var parentArchetype = entityManager.CreateArchetype(
                ComponentType.Create<DokabenRotationData>(),
                ComponentType.Create<Position>(),
                ComponentType.Create<Rotation>());

            // Root Entityのアーキタイプ
            var childArchetype = entityManager.CreateArchetype(
                ComponentType.Create<Position>(),
                ComponentType.Create<Static>(),
                ComponentType.Create<Rotation>());

            //// Attachmentのアーキタイプ
            var attachmentArchetype = entityManager.CreateArchetype(ComponentType.Create<Attach>());

            // ドカベンロゴの生成
            base.CreateEntitiesFromRandomPosition((childEntity, randomPosition) =>
                {
                    // 子Entityの設定
                    entityManager.SetComponentData(childEntity, new Position { Value = this._childOffset });
                    entityManager.SetComponentData(childEntity, new Rotation { Value = quaternion.identity });

                    // ルートEntityの生成
                    var rootEntity = entityManager.CreateEntity(childArchetype);
                    entityManager.SetComponentData(rootEntity, new Position { Value = randomPosition });
                    entityManager.SetComponentData(rootEntity, new Rotation { Value = quaternion.identity });

                    // 親Entityの生成
                    var parentEntity = entityManager.CreateEntity(parentArchetype);
                    entityManager.SetComponentData(parentEntity, new Rotation { Value = quaternion.identity });
                    entityManager.SetComponentData(parentEntity, new DokabenRotationData { CurrentAngle = Constants.ParentTest.Angle });

                    //// 親子関係構築
                    var attach0 = entityManager.CreateEntity(attachmentArchetype);
                    entityManager.SetComponentData(attach0, new Attach
                    {
                        Parent = parentEntity,
                        Child = childEntity,
                    });
                    var attach1 = entityManager.CreateEntity(attachmentArchetype);
                    entityManager.SetComponentData(attach1, new Attach
                    {
                        Parent = rootEntity,
                        Child = parentEntity,
                    });
                });

            World.Active.CreateManager(typeof(EndFrameTransformSystem));
            if (useJobSystem)
                World.Active.CreateManager(typeof(ParentTestJobSystem));
            else World.Active.CreateManager(typeof(ParentTestSystem));
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
        }
    }
}