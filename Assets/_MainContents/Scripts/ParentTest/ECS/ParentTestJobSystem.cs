using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MainContents.ParentTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(親子構造版) ※JobSystem併用
    /// </summary>
    public sealed class ParentTestJobSystem : JobComponentSystem
    {
        /// <summary>
        /// 回転処理用Job
        /// </summary>
        [BurstCompile]
        struct RotationJob : IJobProcessComponentData<Rotation, DokabenRotationData>
        {
            // Time.deltaTime
            public float DeltaTime;

            public void Execute(ref Rotation rot, ref DokabenRotationData dokabenRotData)
            {
                if (dokabenRotData.DeltaTimeCounter < Constants.ParentTest.Interval)
                {
                    dokabenRotData.DeltaTimeCounter += this.DeltaTime;
                    return;
                }
                dokabenRotData.DeltaTimeCounter = 0f;
                dokabenRotData.CurrentRot += dokabenRotData.CurrentAngle;
                rot.Value = quaternion.AxisAngle(new float3(1, 0, 0), math.radians(dokabenRotData.CurrentRot));
                if (++dokabenRotData.FrameCounter >= Constants.ParentTest.Framerate)
                {
                    dokabenRotData.CurrentAngle = -dokabenRotData.CurrentAngle;
                    dokabenRotData.FrameCounter = 0;
                }
            }
        }

        RotationJob _rotationjob = default;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Jobの実行
            this._rotationjob.DeltaTime = Time.deltaTime;
            return this._rotationjob.Schedule(this, inputDeps);
        }
    }
}