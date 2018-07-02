using System;
using System.Linq;
using UnityEngine;
using Unity.Rendering;

namespace MainContents
{
    /// <summary>
    /// 共通定数
    /// </summary>
    public class Constants
    {
        public class MatrixTest
        {
            /// <summary>
            /// コマ落ちアニメーションテーブル
            /// </summary>
            public static readonly float[] AnimationTable =
                new float[] {
                1f,
                0.9333333333333333f,
                0.8666666666666667f,
                0.8f,
                0.7333333333333333f,
                0.6666666666666666f,
                0.6f,
                0.5333333333333333f,
                0.4666666666666667f,
                0.4f,
                0.3333333333333333f,
                0.26666666666666666f,
                0.2f,
                0.13333333333333333f,
                0.06666666666666667f,
                0f };

            /// <summary>
            /// アニメーションの再生速度
            /// </summary>
            public const float AnimationSpeed = 1f;
        }

        public class ParentTest
        {
            /// <summary>
            /// コマ数
            /// </summary>
            public const int Framerate = 9;

            /// <summary>
            /// 1コマに於ける回転角度
            /// </summary>
            public const float Angle = (90f / Framerate); // 90度をコマ数で分割

            /// <summary>
            /// コマ中の待機時間
            /// </summary>
            public const float Interval = 0.2f;
        }
    }

    /// <summary>
    /// MeshInstanceRendererに渡すデータ
    /// </summary>
    [Serializable]
    public sealed class MeshInstanceRendererData
    {
        /// <summary>
        /// 表示スプライト
        /// </summary>
        [SerializeField] public Sprite Sprite;

        /// <summary>
        /// 表示マテリアル
        /// </summary>
        [SerializeField] public Material Material;
    }

    public static class Utility
    {
        /// <summary>
        /// MeshInstanceRendererの生成
        /// </summary>
        /// <param name="data">表示データ</param>
        /// <returns>生成したMeshInstanceRenderer</returns>
        public static MeshInstanceRenderer CreateMeshInstanceRenderer(MeshInstanceRendererData data)
        {
            // Sprite to Mesh
            var mesh = new Mesh();
            var sprite = data.Sprite;
            mesh.SetVertices(Array.ConvertAll(sprite.vertices, _ => (Vector3)_).ToList());
            mesh.SetUVs(0, sprite.uv.ToList());
            mesh.SetTriangles(Array.ConvertAll(sprite.triangles, _ => (int)_), 0);

            var matInst = new Material(data.Material);

            // 渡すマテリアルはGPU Instancingに対応させる必要がある
            var meshInstanceRenderer = new MeshInstanceRenderer();
            meshInstanceRenderer.mesh = mesh;
            meshInstanceRenderer.material = matInst;
            return meshInstanceRenderer;
        }
    }
}
