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

    /// <summary>
    /// JobSystem非有効識別子用 ダミーデータ
    /// </summary>
    public struct DisableJobSystemData : IComponentData { }

    /// <summary>
    /// JobSystem有効識別子用 ダミーデータ
    /// </summary>
    public struct EnableJobSystemData : IComponentData { }
}
