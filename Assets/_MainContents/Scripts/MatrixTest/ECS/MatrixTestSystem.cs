#if !ENABLE_JOBSYSTEM
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

namespace MainContents.MatrixTest.ECS
{
    /// <summary>
    /// ドカベンロゴ回転システム(回転行列演算版)
    /// </summary>
    [UpdateAfter(typeof(MeshFrustumCullingSystem))]
    public class MatrixTestSystem : ComponentSystem
    {
        struct Group
        {
            public readonly int Length;
            public ComponentDataArray<MatrixTestComponentData> Data;
            public ComponentDataArray<TransformMatrix> Transforms;
#if ENABLE_FRUSTUM_CULLING
            [ReadOnly] public ComponentDataArray<MeshCullingComponent> MeshCulling;
#endif
        }

        [Inject] Group _group;

        protected override void OnUpdate()
        {
            float time = Time.time;
            int animLength = Constants.MatrixTest.AnimationTable.Length;

            for (int i = 0; i < this._group.Length; i++)
            {
#if ENABLE_FRUSTUM_CULLING
                // カリングされていたら計算しない
                var culling = this._group.MeshCulling[i];
                if (culling.CullStatus == 1) { return; }
#endif

                var data = this._group.Data[i];
                float4x4 m = float4x4.identity;

                // 時間の正弦を算出(再生位置を加算することで角度をずらせるように設定)
                float sinTime = math.sin(time * Constants.MatrixTest.AnimationSpeed) + data.AnimationHeader;

                // _SinTime0~1に正規化→0~15(コマ数分)の範囲にスケールして要素数として扱う
                float normal = (sinTime + 1f) / 2f;

                // X軸に0~90度回転
                var index = (int)math.round(normal * (animLength - 1));
                float rot = Constants.MatrixTest.AnimationTable[index] * math.radians(90f);

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

                // 計算結果の保持
                this._group.Data[i] = data;

                var trs = this._group.Transforms[i];
                trs.Value = m;
                this._group.Transforms[i] = trs;
            }
        }
    }
}
#endif
