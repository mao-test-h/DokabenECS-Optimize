using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Rendering;

using System;

namespace MainContents.MatrixTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(回転行列演算版)
    /// </summary>
    // [UpdateAfter(typeof(MeshFrustumCullingSystem))]
    public sealed class MatrixTestSystem : ComponentSystem
    {
        private readonly EntityArchetypeQuery query = new EntityArchetypeQuery
        {
            All = new[] { ComponentType.Create<MatrixTestComponentData>(), ComponentType.Create<LocalToWorld>() },
            Any = Array.Empty<ComponentType>(),
            None = Array.Empty<ComponentType>(),
        };
        private readonly NativeList<EntityArchetype> foundArchetypes = new NativeList<EntityArchetype>(Allocator.Persistent);
        protected override void OnDestroyManager() => foundArchetypes.Dispose();
        protected override unsafe void OnUpdate()
        {
            var time = Time.time;
            var animLength = Constants.MatrixTest.AnimationTable.Length;
            var entityManager = EntityManager;
            entityManager.AddMatchingArchetypes(query, foundArchetypes);
            var MatrixTestComponentDataTypeRW = entityManager.GetArchetypeChunkComponentType<MatrixTestComponentData>(false);
            var LocalToWorldTypeRW = entityManager.GetArchetypeChunkComponentType<LocalToWorld>(false);
            var sinTimeBase = Math.Sin(time * Constants.MatrixTest.AnimationSpeed);
            using (var chunks = entityManager.CreateArchetypeChunkArray(foundArchetypes, Allocator.TempJob))
            {
                for (int i = 0; i < chunks.Length; ++i)
                {
                    //// カリングに関してはMeshInstanceRendererSystemがVisibleLocalToWorldコンポーネントを見てゴニョゴニョしてくれる
                    var matrixTests = chunks[i].GetNativeArray(MatrixTestComponentDataTypeRW);
                    var localToWorlds = chunks[i].GetNativeArray(LocalToWorldTypeRW);
                    var matrixTestComponentPtr = (MatrixTestComponentData*)NativeArrayUnsafeUtility.GetUnsafePtr(matrixTests);
                    var localToWorldPtr = (LocalToWorld*)NativeArrayUnsafeUtility.GetUnsafePtr(localToWorlds);
                    for (int j = 0; j < matrixTests.Length; ++j, ++matrixTestComponentPtr, ++localToWorldPtr)
                    {
                        // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                        var sinTime = sinTimeBase + matrixTestComponentPtr->AnimationHeader;

                        // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                        var normal = sinTime * 0.5 + 0.5;
                        var index = (int)Math.Round(normal * (animLength - 1));

                        //// そもそも最初からAnimationTableの内部をラジアンにすべきでは？
                        // 任意の原点周りにX軸回転を行う(原点を-0.5ずらして下端に設定)
                        // ※最新のUnity.Mathematicsだとfloat4x4も列優先みたいなので注意(少し前は行優先だった)
                        var rot = Constants.MatrixTest.AnimationTable[index] * Math.PI * 0.5;
                        // 任意の原点周りにX軸回転を行う(原点を-0.5ずらして下端に設定)
                        // ※最新のUnity.Mathematicsだとfloat4x4も列優先みたいなので注意(少し前は行優先だった)
                        var y = 0f;
                        var z = 0f;
                        var halfY = y - 0.5f;
                        var sin = (float)Math.Sin(rot);
                        var cos = (float)Math.Cos(rot);
                        ref var m = ref localToWorldPtr->Value;
                        m.c0.x = 1;
                        m.c3.w = 1;
                        m.c1.yz = new float2(cos, sin);
                        m.c2.yz = new float2(-sin, cos);
                        m.c3.yz = new float2(halfY - halfY * cos + z * sin, z - halfY * sin - z * cos);
                        m.c3.x = 0;
                        // 移動
                        m.c3.xyz += matrixTestComponentPtr->Position;

                        //// 原文では再代入しているが、弄っていないから不要では？
                        // 計算結果の保持
                        //     this._group.Data[i] = data;
                    }
                }
            }
        }
    }
}