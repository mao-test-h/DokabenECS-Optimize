using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace MainContents.ParentTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(親子構造版)
    /// </summary>
    public class ParentTestSystem : ComponentSystem
    {
        struct Group
        {
            public readonly int Length;
            public ComponentDataArray<Rotation> Rotation;
            public ComponentDataArray<DokabenRotationData> DokabenRotationData;
            [ReadOnly] public ComponentDataArray<DisableJobSystemData> Dummy;
        }

        [Inject] Group _group;

        protected override void OnUpdate()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < this._group.Length; i++)
            {
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
