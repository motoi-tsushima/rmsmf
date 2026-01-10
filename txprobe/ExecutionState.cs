using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace txprobe
{
    /// <summary>
    ///  プログラム実行状態(エラー時の状況を記録する)
    /// </summary>
    public static class ExecutionState
    {
        /// <summary>
        /// 正常実行状態
        /// </summary>
        public static bool isNormal = false;

        /// <summary>
        /// 管理されたエラーが発生した。
        /// (これが false のエラーは管理されていない予想外のエラーです。システムエラーとして処理します。)
        /// </summary>
        public static bool isError = false;

        /// <summary>
        /// 管理されたエラーのメッセージ
        /// </summary>
        public static string errorMessage = null;

        /// <summary>
        /// 実行行記録
        /// </summary>
        public static int stepNumber = 0;

        /// <summary>
        /// 実行中のクラスとメソッド名
        /// </summary>
        public static string className = null;
    }
}
