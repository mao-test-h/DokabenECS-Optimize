#if ENABLE_JOBSYSTEM
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Rendering;

namespace MainContents.ParentTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(親子構造版) ※JobSystem併用
    /// </summary>
    [UpdateAfter(typeof(MeshFrustumCullingSystem))]
    public class ParentTestJobSystem : JobComponentSystem
    {
        /// <summary>
        /// 回転処理用Job
        /// </summary>
        [BurstCompile]
        struct RotationJob : IJobProcessComponentData<Rotation, DokabenRotationData, MeshCullingComponent>
        {
            // Time.deltaTime
            public float DeltaTime;

            public void Execute(ref Rotation rot, ref DokabenRotationData dokabenRotData, ref MeshCullingComponent meshCulling)
            {
                // カリングされていたら計算しない
                if (meshCulling.CullStatus == 1) { return; }

                if (dokabenRotData.DeltaTimeCounter >= Constants.ParentTest.Interval)
                {
                    dokabenRotData.CurrentRot += dokabenRotData.CurrentAngle;
                    var axis = new float3(1, 0, 0);
                    rot.Value = quaternion.axisAngle(axis, math.radians(dokabenRotData.CurrentRot));
                    dokabenRotData.FrameCounter = dokabenRotData.FrameCounter + 1;
                    if (dokabenRotData.FrameCounter >= Constants.ParentTest.Framerate)
                    {
                        dokabenRotData.CurrentAngle = -dokabenRotData.CurrentAngle;
                        dokabenRotData.FrameCounter = 0;
                    }
                    dokabenRotData.DeltaTimeCounter = 0f;
                }
                else
                {
                    dokabenRotData.DeltaTimeCounter += this.DeltaTime;
                }
            }
        }

        RotationJob _job;

        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);
            this._job = new RotationJob();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Jobの実行
            this._job.DeltaTime = Time.deltaTime;
            return this._job.Schedule(this, 7, inputDeps);
        }
    }
}
#endif
