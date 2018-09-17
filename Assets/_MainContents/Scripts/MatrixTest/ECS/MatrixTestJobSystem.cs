using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MainContents.MatrixTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(回転行列演算版) ※JobSystem併用
    /// </summary>
    /// <remarks>JobComponentSystemを継承した実装</remarks>
    [UpdateAfter(typeof(MeshInstanceRendererSystem))]   // MeshInstanceRendererSystemでJob待ち?が発生するっぽいので後に実行。しかし毎フレーム解決されるわけではない...
    public sealed class MatrixTestJobSystem : JobComponentSystem
    {
        private readonly NativeArray<float> _AnimationTable;
        public unsafe MatrixTestJobSystem()
        {
            _AnimationTable = new NativeArray<float>(Constants.MatrixTest.AnimationTable.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            fixed (float* srcPtr = Constants.MatrixTest.AnimationTable)
            {
                UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafePtr(_AnimationTable), srcPtr, _AnimationTable.Length * sizeof(float));
            }
        }
        /// <summary>
        /// 回転行列演算用Job
        /// </summary>
        [BurstCompile]
        unsafe struct RotateJob : IJobProcessComponentData<MatrixTestComponentData, LocalToWorld>
        {
            public RotateJob(NativeArray<float> AnimationTable, float AnimationSpeed)
            {
                this.Time = 0f;
                this.AnimationTable = AnimationTable;
                this.AnimationSpeed = AnimationSpeed;
                this.Identity = float4x4.identity;
            }
            // Time.time
            public float Time;
            [ReadOnly] public readonly NativeArray<float> AnimationTable;
            public readonly float AnimationSpeed;
            public readonly float4x4 Identity;

            // Jobで実行されるコード
            public void Execute(ref MatrixTestComponentData matrixTestComponent, ref LocalToWorld localToWorld)
            {
                // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                var sinTime = math.sin(Time * AnimationSpeed) + matrixTestComponent.AnimationHeader;
                // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                var normal = (sinTime + 1f) / 2f;
                // X軸に0~90度回転
                var animIndex = (int)math.round(normal * (AnimationTable.Length - 1));
                //// Mathf.PIは定数であるので問題ない
                var rot = AnimationTable[animIndex] * Mathf.PI * 0.5f;

                // 任意の原点周りにX軸回転を行う(原点を-0.5ずらして下端に設定)
                // ※最新のUnity.Mathematicsだとfloat4x4も列優先みたいなので注意(少し前は行優先だった)
                var z = 0f;
                var halfY = -0.5f;
                var sin = math.sin(rot);
                var cos = math.cos(rot);
                localToWorld.Value.c0.x = 1;
                localToWorld.Value.c3.w = 1;
                localToWorld.Value.c1.yz = new float2(cos, sin);
                localToWorld.Value.c2.yz = new float2(-sin, cos);
                localToWorld.Value.c3.yz = new float2(halfY - halfY * cos + z * sin, z - halfY * sin - z * cos);
                localToWorld.Value.c3.x = matrixTestComponent.Position.x;
                localToWorld.Value.c3.yz += matrixTestComponent.Position.yz;
            }
        }

        RotateJob _job;

        protected override void OnCreateManager() => this._job = new RotateJob(_AnimationTable, Constants.MatrixTest.AnimationSpeed);
        protected override void OnDestroyManager() => _AnimationTable.Dispose();

        protected override unsafe JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Jobの実行
            this._job.Time = Time.time;
            return this._job.Schedule(this, inputDeps);
        }
    }
}