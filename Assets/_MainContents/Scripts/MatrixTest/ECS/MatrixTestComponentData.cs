using Unity.Entities;
using Unity.Mathematics;

namespace MainContents.MatrixTest.ECS
{
    /// <summary>
    /// 回転行列演算テスト
    /// </summary>
    public struct MatrixTestComponentData : IComponentData
    {
        /// <summary>
        /// アニメーションテーブル内に於ける再生位置
        /// </summary>
        public float AnimationHeader;
        /// <summary>
        /// 位置
        /// </summary>
        public float3 Position;
    }
}
