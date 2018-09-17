using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

using MainContents.ParentTest.ECS;
using MainContents.Billboard.ECS;

namespace MainContents.VRTest
{
    public sealed class VRTest : DokabenTestBase
    {
        /// <summary>
        /// 子オブジェクトのオフセット
        /// </summary>
        [SerializeField] Vector3 _childOffset;

        /// <summary>
        /// CameraのTransformの参照
        /// </summary>
        [SerializeField] Transform _cameraTrs;

        [SerializeField] bool useJobSystem;

        /// <summary>
        /// EntityManager
        /// </summary>
        EntityManager _entityManager;

        /// <summary>
        /// カメラ情報参照用Entity
        /// </summary>
        Entity _sharedCameraDataEntity;

        private EntityArchetype childArchetype;
        protected override EntityArchetype Archetype => childArchetype;

        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // Root Entityのアーキタイプ
            var rootArchetype = _entityManager.CreateArchetype(
                ComponentType.Create<Position>(),
                ComponentType.Create<Rotation>(),
                ComponentType.Create<EnableBillboard>());

            // 親Entityのアーキタイプ
            var parentArchetype = _entityManager.CreateArchetype(
                ComponentType.Create<DokabenRotationData>(),
                ComponentType.Create<Position>(),
                ComponentType.Create<Rotation>());

            // 子Entityのアーキタイプ
            childArchetype = _entityManager.CreateArchetype(
                ComponentType.Create<Position>(),
                ComponentType.Create<Rotation>(),
                ComponentType.Create<MeshInstanceRenderer>());

            // カメラ情報参照用Entityの生成
            var sharedCameraDataArchetype = _entityManager.CreateArchetype(
                ComponentType.Create<SharedCameraData>(),
                ComponentType.Create<CameraRotation>());

            //// Attach
            var attachmentArchetype = _entityManager.CreateArchetype(
                ComponentType.Create<Attach>());

            // ドカベンロゴの生成
            base.CreateEntitiesFromRandomPosition((childEntity, randomPosition) =>
                {
                    var rootEntity = _entityManager.CreateEntity(rootArchetype);
                    _entityManager.SetComponentData(rootEntity, new Position { Value = randomPosition });
                    _entityManager.SetComponentData(rootEntity, new Rotation { Value = quaternion.identity });

                    // 親Entityの生成
                    var parentEntity = _entityManager.CreateEntity(parentArchetype);
                    _entityManager.SetComponentData(parentEntity, new Position { Value = new float3(0f, 0f, 0f) });
                    _entityManager.SetComponentData(parentEntity, new Rotation { Value = quaternion.identity });
                    _entityManager.SetComponentData(parentEntity, new DokabenRotationData
                    {
                        CurrentAngle = Constants.ParentTest.Angle,
                        DeltaTimeCounter = 0f,
                        FrameCounter = 0,
                        CurrentRot = 0f,
                    });

                    // 子Entityの設定
                    _entityManager.SetComponentData(childEntity, new Position { Value = this._childOffset });
                    _entityManager.SetComponentData(childEntity, new Rotation { Value = quaternion.identity });

                    //// 親子関係の構築
                    var attach0 = _entityManager.CreateEntity(attachmentArchetype);
                    _entityManager.SetComponentData(attach0, new Attach
                    {
                        Parent = parentEntity,
                        Child = childEntity,
                    });
                    var attach1 = _entityManager.CreateEntity(attachmentArchetype);
                    _entityManager.SetComponentData(attach1, new Attach
                    {
                        Parent = rootEntity,
                        Child = parentEntity,
                    });
                });

            // カメラ情報参照用Entityの生成
            var sharedCameraDataEntity = _entityManager.CreateEntity(sharedCameraDataArchetype);
            _entityManager.SetComponentData(sharedCameraDataEntity, new SharedCameraData());
            _entityManager.SetSharedComponentData(sharedCameraDataEntity, new CameraRotation { Value = this._cameraTrs.rotation });
            this._sharedCameraDataEntity = sharedCameraDataEntity;

            World.Active.CreateManager(typeof(EndFrameTransformSystem));
            if (useJobSystem)
                World.Active.CreateManager(typeof(ParentTestBillboardJobSystem));
            else World.Active.CreateManager(typeof(ParentTestBillboardSystem));
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
        }

        /// <summary>
        /// MonoBehaviour.Update
        /// </summary>
        void Update()
        {
            // こればかりはUpdate内で更新...

            // 但し全てのEntityに対してCamera座標を持たせた上で更新を行う形で実装すると、
            // Update内でとんでもない数のEntityを面倒見無くてはならなくなるので、
            // 予めカメラ情報参照用のEntityを一つだけ生成し、そいつのみに更新情報を渡す形にする。
            // →その上で必要なComponentSystem内でカメラ情報参照用のEntityをInjectして参照すること。
            this._entityManager.SetSharedComponentData(this._sharedCameraDataEntity, new CameraRotation { Value = this._cameraTrs.rotation });
        }
    }
}