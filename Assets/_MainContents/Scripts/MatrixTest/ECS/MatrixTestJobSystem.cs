#if ENABLE_JOBSYSTEM
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Burst;

namespace MainContents.MatrixTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(回転行列演算版) ※JobSystem併用
    /// </summary>
    /// <remarks>JobComponentSystemを継承した実装</remarks>
    [UpdateAfter(typeof(MeshInstanceRendererSystem))]   // MeshInstanceRendererSystemの処理負荷軽減の為に後に回す
    public sealed class MatrixTestJobSystem : JobComponentSystem
    {
        /// <summary>
        /// 回転行列演算用Job
        /// </summary>
        [BurstCompile]
#if ENABLE_FRUSTUM_CULLING
        struct RotateJob : IJobProcessComponentData<MatrixTestComponentData, TransformMatrix, MeshCullingComponent>
#else
        struct RotateJob : IJobProcessComponentData<MatrixTestComponentData, TransformMatrix>
#endif
        {
            // Time.time
            public float Time;

            // Jobで実行されるコード
#if ENABLE_FRUSTUM_CULLING
            public void Execute(ref MatrixTestComponentData data, ref TransformMatrix transform, ref MeshCullingComponent meshCulling)
            {
                // カリングされていたら計算しない
                if (meshCulling.CullStatus == 1) { return; }
#else
            public void Execute(ref MatrixTestComponentData data, ref TransformMatrix transform)
            {
#endif
                float4x4 m = float4x4.identity;

                // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                float sinTime = math.sin(this.Time * Constants.MatrixTest.AnimationSpeed) + data.AnimationHeader;

                // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                float normal = (sinTime + 1f) / 2f;

                // X軸に0~90度回転
                float rot = 0f;
                // BurstCompileを有効にする場合、通常の配列にはアクセスできないのでスタック上に静的確保を行う形にする。
                unsafe
                {
                    float* AnimationTable = stackalloc float[16];
                    const int AnimationTableLength = 16;
                    for (int i = 0; i < AnimationTableLength; ++i)
                    {
                        float calc = (1f - (i * 0.06666666666666667f));
                        AnimationTable[i] = (calc <= 0f) ? 0f : calc;
                    }
                    var animIndex = (int)math.round(normal * (AnimationTableLength - 1));
                    rot = AnimationTable[animIndex] * math.radians(90f);
                }

                // 任意の原点周りにX軸回転を行う(原点を-0.5ずらして下端に設定)
                float y = 0f, z = 0f;
                float halfY = y - 0.5f;
                float sin = math.sin(rot);
                float cos = math.cos(rot);
                m.c1.yz = new float2(cos, sin);
                m.c2.yz = new float2(-sin, cos);
                m.c3.yz = new float2(halfY - halfY * cos + z * sin, z - halfY * sin - z * cos);

                // 移動
                m.c3.xyz += data.Position.xyz;

                // 計算結果の反映
                transform.Value = m;
            }
        }

        RotateJob _job;

        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);
            this._job = new RotateJob();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Jobの実行
            this._job.Time = Time.time;
            return this._job.Schedule(this, 7, inputDeps);
        }
    }
}
#endif
