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
    [UpdateAfter(typeof(MeshInstanceRendererSystem))]   // MeshInstanceRendererSystemでJob待ち?が発生するっぽいので後に実行。しかし毎フレーム解決されるわけではない...
    public class MatrixTestJobSystem : JobComponentSystem
    {
        /// <summary>
        /// 回転行列演算用Job
        /// </summary>
        //[BurstCompile]  // TODO BurstCompolerについて、static、配列などが使えないので作り変える必要ありそう感
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
                var animIndex = (int)math.round(normal * (Constants.MatrixTest.AnimationTable.Length - 1));
                float rot = Constants.MatrixTest.AnimationTable[animIndex] * math.radians(90f);

                // 任意の原点周りにX軸回転を行う(原点を-0.5ずらして下端に設定)
                // ※最新のUnity.Mathematicsだとfloat4x4も列優先みたいなので注意(少し前は行優先だった)
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
