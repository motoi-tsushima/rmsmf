using System;

namespace rmsmf
{
    /// <summary>
    /// ファイル操作関連の定数定義
    /// </summary>
    public static class FileConstants
    {
        /// <summary>最大ファイルサイズ（2GB）</summary>
        public const long MaxFileSize = int.MaxValue;

        /// <summary>一時ファイル拡張子</summary>
        public const string TempFileExtension = ".RP$";

        /// <summary>バックアップファイル拡張子</summary>
        public const string BackupFileExtension = ".BACKUP$";

        /// <summary>BOM判定用バッファサイズ</summary>
        public const int BomBufferSize = 4;

        /// <summary>空ファイル判定の最小サイズ（UTF-8 BOMのサイズ）</summary>
        public const long MinFileContentSize = 3;
    }
}
