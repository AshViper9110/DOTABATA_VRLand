using MessagePack;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTABATA_VRLand.Shared.Models.Entities {
    /// <summary>
    /// MagicOnionでTransformを使用可能にするクラス
    /// </summary>
    [MessagePackObject]
    public class SimpleTransform {
        /// <summary>
        /// ローカル座標
        /// </summary>
        [Key(0)] public Vector3 localPosition;
        /// <summary>
        /// ローカルローテーション
        /// </summary>
        [Key(1)] public Quaternion localRotation;
        /// <summary>
        /// ローカルスケール
        /// </summary>
        [Key(2)] public Vector3 localScale;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SimpleTransform() {
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            localScale = Vector3.one;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SimpleTransform(Vector3 lp, Quaternion lr, Vector3 ls) {
            this.localPosition = lp;
            this.localRotation = lr;
            this.localScale = ls;
        }
    }
}
