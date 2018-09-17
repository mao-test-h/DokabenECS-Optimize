using UnityEngine;
using UnityEngine.Assertions;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Rendering;

using System;

namespace MainContents.ParentTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(親子構造版)
    /// </summary>
    public sealed class ParentTestSystem : ComponentSystem
    {
        // 回転用親ノード
        private readonly EntityArchetypeQuery query = new EntityArchetypeQuery
        {
            All = new[] { ComponentType.Create<Rotation>(), ComponentType.Create<DokabenRotationData>() },
            None = Array.Empty<ComponentType>(),
            Any = Array.Empty<ComponentType>(),
        };
        private readonly NativeList<EntityArchetype> foundArchetypes = new NativeList<EntityArchetype>(Allocator.Persistent);
        private EntityManager manager;
        protected override void OnCreateManager() => manager = EntityManager;
        protected override void OnDestroyManager() => foundArchetypes.Dispose();

        protected override unsafe void OnUpdate()
        {
            float deltaTime = Time.deltaTime;
            manager.AddMatchingArchetypes(query, foundArchetypes);
            var RotationTypeRW = manager.GetArchetypeChunkComponentType<Rotation>(false);
            var DokabenRotationDataTypeRW = manager.GetArchetypeChunkComponentType<DokabenRotationData>(false);
            using (var chunks = manager.CreateArchetypeChunkArray(foundArchetypes, Allocator.TempJob))
            {
                for (int i = 0; i < chunks.Length; ++i)
                {
                    var rots = chunks[i].GetNativeArray(RotationTypeRW);
                    if (rots.Length == 0) continue;
                    var dokabens = chunks[i].GetNativeArray(DokabenRotationDataTypeRW);
                    var rotPtr = (Rotation*)NativeArrayUnsafeUtility.GetUnsafePtr(rots);
                    var dokabenPtr = (DokabenRotationData*)NativeArrayUnsafeUtility.GetUnsafePtr(dokabens);
                    for (int j = 0; j < rots.Length; ++j, ++rotPtr, ++dokabenPtr)
                    {
                        if (dokabenPtr->DeltaTimeCounter < Constants.ParentTest.Interval)
                        {
                            dokabenPtr->DeltaTimeCounter += deltaTime;
                            continue;
                        }
                        dokabenPtr->DeltaTimeCounter = 0;
                        dokabenPtr->CurrentRot += dokabenPtr->CurrentAngle;
                        rotPtr->Value = quaternion.AxisAngle(new float3(1, 0, 0), math.radians(dokabenPtr->CurrentRot));
                        ++dokabenPtr->FrameCounter;
                        if (dokabenPtr->FrameCounter >= Constants.ParentTest.Framerate)
                        {
                            dokabenPtr->CurrentAngle = -dokabenPtr->CurrentAngle;
                            dokabenPtr->FrameCounter = 0;
                        }
                    }
                }
            }
        }
    }
}