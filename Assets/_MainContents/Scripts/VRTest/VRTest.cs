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

        /// <summary>
        /// EntityManager
        /// </summary>
        EntityManager _entityManager;

        /// <summary>
        /// カメラ情報参照用Entity
        /// </summary>
        Entity _sharedCameraDataEntity;

        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        void Start()
        {
            //UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 0.5f;

            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // Root Entityのアーキタイプ
            var rootArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(Rotation),
                typeof(EnableBillboard),
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

            // カメラ情報参照用Entityの生成
            var sharedCameraDataArchetype = entityManager.CreateArchetype(
                typeof(SharedCameraData));

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

            // カメラ情報参照用Entityの生成
            var sharedCameraDataEntity = entityManager.CreateEntity(sharedCameraDataArchetype);
            entityManager.SetComponentData(sharedCameraDataEntity, new SharedCameraData());
            entityManager.AddSharedComponentData(sharedCameraDataEntity, new CameraRotation { Value = this.GetBillboardRotation(this._cameraTrs.rotation) });
            this._sharedCameraDataEntity = sharedCameraDataEntity;

            this._entityManager = entityManager;
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
            this._entityManager.SetSharedComponentData(this._sharedCameraDataEntity, new CameraRotation { Value = this.GetBillboardRotation(this._cameraTrs.rotation) });
        }

        quaternion GetBillboardRotation(Quaternion rot)
        {
            var euler = rot.eulerAngles;
            return Quaternion.Euler(new Vector3(euler.x, euler.y, 0f));
        }
    }
}
