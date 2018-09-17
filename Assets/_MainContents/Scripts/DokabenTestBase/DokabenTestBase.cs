using UnityEngine;
using UnityEngine.Events;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MainContents
{
    /// <summary>
    /// ドカベンロゴテスト ベース
    /// </summary>
    public abstract class DokabenTestBase : MonoBehaviour
    {
        /// <summary>
        /// ドカベン表示用データ
        /// </summary>
        [SerializeField] protected MeshInstanceRendererData _dokabenRenderData;

        /// <summary>
        /// 表示領域のサイズ
        /// </summary>
        [SerializeField] Vector3 _boundSize = new Vector3(256f, 256f, 256f);

        /// <summary>
        /// 最大オブジェクト数
        /// </summary>
        [SerializeField] int _maxObjectNum = 100000;

        /// <summary>
        /// Entityをランダムな位置に生成
        /// </summary>
        /// <param name="onCreateEntity">Entity生成毎に呼ばれるコールバック</param>
        protected void CreateEntitiesFromRandomPosition(UnityAction<Entity, float3> onCreateEntity)
        {
            var look = Utility.CreateMeshInstanceRenderer(this._dokabenRenderData);
            var halfX = this._boundSize.x / 2;
            var halfY = this._boundSize.y / 2;
            var halfZ = this._boundSize.z / 2;
            var manager = World.Active.GetExistingManager<EntityManager>();
            var entities = new NativeArray<Entity>(_maxObjectNum, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            try
            {
                entities[0] = manager.CreateEntity(Archetype);
                manager.SetSharedComponentData(entities[0], look);
                unsafe
                {
                    var ptr = (Entity*)NativeArrayUnsafeUtility.GetUnsafePtr(entities);
                    var rest = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Entity>(ptr + 1, entities.Length - 1, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref rest, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
                    manager.Instantiate(entities[0], rest);
                }
                for (int i = 0; i < entities.Length; ++i)
                    onCreateEntity(entities[i], new float3(UnityEngine.Random.Range(-halfX, halfX), UnityEngine.Random.Range(-halfY, halfY), UnityEngine.Random.Range(-halfZ, halfZ)));
            }
            finally
            {
                entities.Dispose();
            }
        }

        /// <summary>
        /// ドカベンロゴを表示するEntityのEntityArchetype
        /// 最低限MeshInstanceRendererComponentを含まなくてはならぬ
        /// </summary>
        protected abstract EntityArchetype Archetype { get; }

        protected virtual void Start()
        {
            World.Active = new World("default world");
            World.Active.CreateManager(typeof(EntityManager));
            World.Active.CreateManager<MeshInstanceRendererSystem>().ActiveCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
        }
        void OnDestroy()
        {
            World.Active.Dispose();
        }
    }
}