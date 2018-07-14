using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;

namespace MainContents.VRTest2
{
    public sealed class VRTest2 : DokabenTestBase
    {
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
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            var archetype = entityManager.CreateArchetype(
                typeof(AnimationData),
#if ENABLE_FRUSTUM_CULLING
                typeof(MeshCullingComponent),
#endif
                typeof(TransformMatrix));

            // カメラ情報参照用Entityの生成
            var sharedCameraDataArchetype = entityManager.CreateArchetype(
                typeof(SharedCameraData));

            base.CreateEntitiesFromRandomPosition((randomPosition, look) =>
                {
                    var entity = entityManager.CreateEntity(archetype);
                    entityManager.SetComponentData(
                        entity,
                        new AnimationData
                        {
                            AnimationHeader = 0f,
                            Position = randomPosition,
                        });
                    entityManager.AddSharedComponentData(entity, look);
                });

            // カメラ情報参照用Entityの生成
            var sharedCameraDataEntity = entityManager.CreateEntity(sharedCameraDataArchetype);
            entityManager.SetComponentData(sharedCameraDataEntity, new SharedCameraData());
            entityManager.AddSharedComponentData(
                sharedCameraDataEntity,
                new CameraTRS
                {
                    Position = this._cameraTrs.localPosition,
                });

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
            this._entityManager.SetSharedComponentData(
                this._sharedCameraDataEntity,
                new CameraTRS
                {
                    Position = this._cameraTrs.localPosition,
                });
        }
    }
}