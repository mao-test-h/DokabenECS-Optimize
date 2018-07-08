using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainContents.MonoBehaviourTest
{
    public sealed class VRTest_MonoBehaviour : MonoBehaviour
    {
        // ------------------------------
        #region // Defines

        public sealed class Dokaben
        {
            float _deltaTimeCounter;
            int _frameCounter;
            float _currentAngle;
            float _currentRot;

            Transform _cameraTrs;
            Transform _rootNode;
            Transform _rotateNode;

            // 回転軸
            readonly Vector3 RightVector = Vector3.right;

            public Dokaben(Transform cameraTrs, Transform rootNode, Transform rotateNode, float currentAngle)
            {
                this._currentAngle = currentAngle;
                this._deltaTimeCounter = 0f;
                this._frameCounter = 0;
                this._currentRot = 0f;
                this._cameraTrs = cameraTrs;
                this._rootNode = rootNode;
                this._rotateNode = rotateNode;
            }

            public void Rotate(float deltaTime)
            {
                if (this._deltaTimeCounter >= Constants.ParentTest.Interval)
                {
                    this._currentRot += this._currentAngle;
                    this._rotateNode.localRotation = Quaternion.AngleAxis(this._currentRot, RightVector);
                    this._frameCounter = this._frameCounter + 1;
                    if (this._frameCounter >= Constants.ParentTest.Framerate)
                    {
                        this._currentAngle = -this._currentAngle;
                        this._frameCounter = 0;
                    }
                    this._deltaTimeCounter = 0f;
                }
                else
                {
                    this._deltaTimeCounter += deltaTime;
                }
                this._rootNode.localRotation = this._cameraTrs.localRotation;
            }
        }

        #endregion  // Defines

        // ------------------------------
        #region // Private Members(Editable)

        /// <summary>
        /// ベースのPrefab
        /// </summary>
        [SerializeField] GameObject _basePrefab;

        /// <summary>
        /// 表示領域のサイズ
        /// </summary>
        [SerializeField] Vector3 _boundSize = new Vector3(256f, 256f, 256f);

        /// <summary>
        /// 最大オブジェクト数
        /// </summary>
        [SerializeField] int _maxObjectNum = 100000;

        /// <summary>
        /// カメラのTransformの参照
        /// </summary>
        [SerializeField] Transform _cameraTrs;

        #endregion // Private Members(Editable)

        // ------------------------------
        #region // Private Members

        /// <summary>
        /// 生成したドカベンロゴ一覧
        /// </summary>
        Dokaben[] _dokabens = null;

        #endregion // Private Members


        // ----------------------------------------------------
        #region // Unity Events

        void Start()
        {
            this._dokabens = new Dokaben[this._maxObjectNum];
            var halfX = this._boundSize.x / 2;
            var halfY = this._boundSize.y / 2;
            var halfZ = this._boundSize.z / 2;
            for (int i = 0; i < this._maxObjectNum; ++i)
            {
                var pos = new Vector3(
                    Random.Range(-halfX, halfX),
                    Random.Range(-halfY, halfY),
                    Random.Range(-halfZ, halfZ));
                var obj = Instantiate<GameObject>(this._basePrefab, pos, Quaternion.identity);
                var rootTrs = obj.transform;
                var childTrs = rootTrs.GetChild(0);
                this._dokabens[i] = new Dokaben(this._cameraTrs, rootTrs, childTrs, Constants.ParentTest.Angle);
            }
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < this._maxObjectNum; ++i)
            {
                this._dokabens[i].Rotate(deltaTime);
            }
        }

        #endregion // Unity Events
    }
}