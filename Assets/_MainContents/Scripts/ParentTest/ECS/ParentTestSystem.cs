#if !ENABLE_JOBSYSTEM
using UnityEngine;
using UnityEngine.Assertions;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

namespace MainContents.ParentTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(親子構造版)
    /// </summary>
    [UpdateAfter(typeof(MeshFrustumCullingSystem))]
    public class ParentTestSystem : ComponentSystem
    {
        struct Group
        {
            public readonly int Length;
            public ComponentDataArray<Rotation> Rotation;
            public ComponentDataArray<DokabenRotationData> DokabenRotationData;
        }

        struct CullingGroup
        {
            public readonly int Length;
            [ReadOnly] public ComponentDataArray<TransformParent> Dummy;    // 子要素識別用
            [ReadOnly] public ComponentDataArray<MeshCullingComponent> MeshCulling;
        }

        [Inject] Group _group;
        [Inject] CullingGroup _cullingGroup;

        protected override void OnUpdate()
        {
            float deltaTime = Time.deltaTime;
            Assert.IsTrue(
                this._group.Length == this._cullingGroup.Length,
                "parent : " + this._group.Length + " child : " + this._cullingGroup.Length);
            for (int i = 0; i < this._group.Length; i++)
            {
                // カリングされていたら計算しない
                var culling = this._cullingGroup.MeshCulling[i];
                if (culling.CullStatus == 1) { return; }

                var rot = this._group.Rotation[i];
                var dokabenRotData = this._group.DokabenRotationData[i];

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
                    dokabenRotData.DeltaTimeCounter += deltaTime;
                }

                this._group.Rotation[i] = rot;
                this._group.DokabenRotationData[i] = dokabenRotData;
            }
        }
    }
}
#endif
