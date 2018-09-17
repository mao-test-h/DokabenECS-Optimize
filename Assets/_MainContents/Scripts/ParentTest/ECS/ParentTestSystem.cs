// #if !ENABLE_JOBSYSTEM
// using UnityEngine;
// using UnityEngine.Assertions;

// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using Unity.Collections;
// using Unity.Rendering;

// namespace MainContents.ParentTest.ECS
// {
//     /// <summary>
//     /// ドカベンロゴ回転システム(親子構造版)
//     /// </summary>
//     [UpdateAfter(typeof(MeshFrustumCullingSystem))]
//     public class ParentTestSystem : ComponentSystem
//     {
//         // 回転用親ノード
//         struct ParentGroup
//         {
//             public readonly int Length;
//             public ComponentDataArray<LocalRotation> LocalRotation;
//             public ComponentDataArray<DokabenRotationData> DokabenRotationData;
//             [ReadOnly] public ComponentDataArray<MeshCullingComponent> MeshCulling;
//         }

//         [Inject] ParentGroup _parentGroup;

//         protected override void OnUpdate()
//         {
//             float deltaTime = Time.deltaTime;
//             for (int i = 0; i < this._parentGroup.Length; i++)
//             {
//                 // カリングされていたら計算しない
//                 var culling = this._parentGroup.MeshCulling[i];
//                 if (culling.CullStatus == 1) { return; }

//                 var rot = this._parentGroup.LocalRotation[i];
//                 var dokabenRotData = this._parentGroup.DokabenRotationData[i];

//                 if (dokabenRotData.DeltaTimeCounter >= Constants.ParentTest.Interval)
//                 {
//                     dokabenRotData.CurrentRot += dokabenRotData.CurrentAngle;
//                     var axis = new float3(1, 0, 0);
//                     rot.Value = quaternion.axisAngle(axis, math.radians(dokabenRotData.CurrentRot));
//                     dokabenRotData.FrameCounter = dokabenRotData.FrameCounter + 1;
//                     if (dokabenRotData.FrameCounter >= Constants.ParentTest.Framerate)
//                     {
//                         dokabenRotData.CurrentAngle = -dokabenRotData.CurrentAngle;
//                         dokabenRotData.FrameCounter = 0;
//                     }
//                     dokabenRotData.DeltaTimeCounter = 0f;
//                 }
//                 else
//                 {
//                     dokabenRotData.DeltaTimeCounter += deltaTime;
//                 }

//                 this._parentGroup.LocalRotation[i] = rot;
//                 this._parentGroup.DokabenRotationData[i] = dokabenRotData;
//             }
//         }
//     }
// }
// #endif