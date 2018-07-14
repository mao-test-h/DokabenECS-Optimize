using UnityEngine;
using UnityEngine.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

namespace MainContents.VRTest2
{
    public struct AnimationData : IComponentData
    {
        // アニメーションテーブル内に於ける再生位置
        public float AnimationHeader;
        // 位置
        public float3 Position;
    }

    /// <summary>
    /// カメラ情報参照用Entity生成用のダミーデータ
    /// </summary>
    public struct SharedCameraData : IComponentData { }

    /// <summary>
    /// カメラの回転情報
    /// </summary>
    public struct CameraTRS : ISharedComponentData
    {
        public float3 Position;
    }



    [UpdateAfter(typeof(MeshInstanceRendererSystem))]   // MeshInstanceRendererSystemでJob待ち?が発生するっぽいので後に実行。しかし毎フレーム解決されるわけではない...
    public sealed class MatrixTestJobSystem : JobComponentSystem
    {
        /// <summary>
        /// カメラ情報参照用Entity
        /// </summary>
        struct SharedCameraDataGroup
        {
            public readonly int Length;
            [ReadOnly] public ComponentDataArray<SharedCameraData> Dummy;   // これはInject用の識別子みたいなもの
            [ReadOnly] public SharedComponentDataArray<CameraTRS> CameraTRS;
        }

        //[BurstCompile]    // TODO: Constants.MatrixTestの参照を何とかする必要がある
#if ENABLE_FRUSTUM_CULLING
        struct RotateJob : IJobProcessComponentData<AnimationData, TransformMatrix, MeshCullingComponent>
#else
        struct RotateJob : IJobProcessComponentData<AnimationData, TransformMatrix>
#endif
        {
            // Time.time
            public float Time;
            public CameraTRS CameraTRS;

#if ENABLE_FRUSTUM_CULLING
            public void Execute(ref AnimationData data, ref TransformMatrix transform, ref MeshCullingComponent meshCulling)
            {
                // カリングされていたら計算しない
                if (meshCulling.CullStatus == 1) { return; }
#else
            public void Execute(ref AnimationData data, ref TransformMatrix transform)
            {
#endif
                // Billboard Quaternion
                var target = data.Position - this.CameraTRS.Position;
                var billboardQuat = quaternion.lookRotation(target, new float3(0, 1, 0));

                // 軸回転行列
                float4x4 axisRotationMatrix = float4x4.identity;

                // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                float sinTime = math.sin(this.Time * Constants.MatrixTest.AnimationSpeed) + data.AnimationHeader;

                // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                float normal = (sinTime + 1f) / 2f;

                // X軸に0~90度回転
                var animIndex = (int)math.round(normal * (Constants.MatrixTest.AnimationTable.Length - 1));
                float rot = Constants.MatrixTest.AnimationTable[animIndex] * math.radians(90f);

                // 任意の原点周りにX軸回転を行う(原点を-0.5ずらして下端に設定)
                float y = 0f, z = 0f;
                float halfY = y - 0.5f;
                float sin = math.sin(rot);
                float cos = math.cos(rot);
                axisRotationMatrix.c1.yz = new float2(cos, sin);
                axisRotationMatrix.c2.yz = new float2(-sin, cos);
                axisRotationMatrix.c3.yz = new float2(halfY - halfY * cos + z * sin, z - halfY * sin - z * cos);

                // 最後にビルドード+移動行列と軸回転行列を掛け合わせる
                var ret = math.mul(new float4x4(billboardQuat, data.Position), axisRotationMatrix);

                // 計算結果の反映
                transform.Value = ret;
            }
        }

        [Inject] SharedCameraDataGroup _sharedCameraDataGroup;
        RotateJob _rotateJob;

        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);
            this._rotateJob = new RotateJob();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // 1個しか無い想定
            Assert.IsTrue(this._sharedCameraDataGroup.Length == 1);

            // Jobの実行
            this._rotateJob.Time = Time.time;
            this._rotateJob.CameraTRS = this._sharedCameraDataGroup.CameraTRS[0];
            return this._rotateJob.Schedule(this, 7, inputDeps);
        }
    }
}