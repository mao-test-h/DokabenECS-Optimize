using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MainContents.ParentTest.ECS
{
    public struct DokabenRotationData :  IComponentData
    {
        /// <summary>
        /// 経過時間計測用
        /// </summary>
        public float DeltaTimeCounter;

        /// <summary>
        /// コマ数のカウンタ
        /// </summary>
        public int FrameCounter;

        /// <summary>
        /// 1コマに於ける回転角度
        /// </summary>
        public float CurrentAngle;

        /// <summary>
        /// 現在の回転角度
        /// </summary>
        public float CurrentRot;
    }
}
