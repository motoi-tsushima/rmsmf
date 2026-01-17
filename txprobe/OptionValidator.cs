using System;
using System.Collections.Generic;

namespace rmsmf
{
    /// <summary>
    /// コマンドオプション検証のためのユーティリティクラス
    /// </summary>
    public static class OptionValidator
    {
        /// <summary>
        /// ファイルリストオプションとコマンドラインパラメータが同時に指定されていないか検証
        /// </summary>
        /// <param name="hasFileListOption">ファイルリストオプションが指定されているか</param>
        /// <param name="parameterCount">コマンドラインパラメータの数</param>
        /// <exception cref="RmsmfException">両方が指定されている場合</exception>
        public static void ValidateFileSpecificationNotConflicting(bool hasFileListOption, int parameterCount)
        {
            if (hasFileListOption && parameterCount > 0)
            {
                throw new RmsmfException(ValidationMessages.ConflictingFileSpecificationMethods);
            }
        }

        /// <summary>
        /// エンコーディング関連オプションの依存関係を検証
        /// </summary>
        /// <param name="hasMainOption">メインオプション（例: 置換単語ファイル）が指定されているか</param>
        /// <param name="hasEncodingOption">エンコーディングオプションが指定されているか</param>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <exception cref="RmsmfException">メインオプションなしでエンコーディングオプションが指定されている場合</exception>
        public static void ValidateEncodingOptionDependency(bool hasMainOption, bool hasEncodingOption, string errorMessage)
        {
            if (!hasMainOption && hasEncodingOption)
            {
                throw new RmsmfException(errorMessage);
            }
        }

        /// <summary>
        /// 必須のパラメータまたはオプションが指定されているか検証
        /// </summary>
        /// <param name="conditions">チェックする条件のリスト（いずれかがtrueである必要がある）</param>
        /// <exception cref="RmsmfException">すべての条件がfalseの場合</exception>
        public static void ValidateAtLeastOneCondition(params bool[] conditions)
        {
            foreach (bool condition in conditions)
            {
                if (condition)
                {
                    return;
                }
            }

            throw new RmsmfException(ValidationMessages.MissingRequiredParameters);
        }
    }
}
