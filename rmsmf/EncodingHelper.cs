using System;
using System.IO;
using System.Text;
using SnowStack.EncodingProbe;

namespace rmsmf
{
    /// <summary>
    /// エンコーディング判定結果
    /// </summary>
    public class EncodingDetectionResult
    {
        /// <summary>判定されたエンコーディング（判定不可の場合はnull）</summary>
        public Encoding Encoding { get; set; }

        /// <summary>BOMの有無</summary>
        public bool BomExists { get; set; }

        /// <summary>コードページ</summary>
        public int CodePage { get; set; }

        /// <summary>改行コードの種類</summary>
        public LineBreakType LineBreak { get; set; } = LineBreakType.None;

        /// <summary>
        /// SnowStack.EncodingProbeによるエンコーディング判定情報
        /// （読み取りエンコーディングが明示指定された場合はnull）
        /// </summary>
        public EncodingInformation EncodingInfo { get; set; }
    }

    /// <summary>
    /// エンコーディング判定ヘルパークラス
    /// ファイルのエンコーディング判定ロジックを共通化
    /// </summary>
    public static class EncodingHelper
    {
        /// <summary>
        /// ファイルストリームからエンコーディングを判定または指定されたエンコーディングを使用
        /// </summary>
        /// <param name="fs">ファイルストリーム</param>
        /// <param name="fileName">ファイル名（エラーメッセージ用）</param>
        /// <param name="specifiedEncoding">指定されたエンコーディング（nullの場合は自動判定）</param>
        /// <param name="detectionMode">自動判定モード</param>
        /// <returns>エンコーディング判定結果</returns>
        public static EncodingDetectionResult DetectOrUseSpecifiedEncoding(
            FileStream fs,
            string fileName,
            Encoding specifiedEncoding,
            CommandOptions.EncodingDetectionType detectionMode)
        {
            var result = new EncodingDetectionResult();

            byte[] buffer = ReadEntireFile(fs, fileName);

            // 読み込みエンコーディングの有無で分岐
            if (specifiedEncoding == null)
            {
                // エンコーディング指定が無い場合：自動判定
                DetectEncodingFromBuffer(buffer, fileName, detectionMode, result);
            }
            else
            {
                // エンコーディング指定が有る場合
                UseSpecifiedEncoding(buffer, specifiedEncoding, result);
            }

            return result;
        }

        /// <summary>
        /// ファイルを全て読み取ってバイト配列にする
        /// </summary>
        private static byte[] ReadEntireFile(FileStream fs, string fileName)
        {
            long fileLength = fs.Length;

            // ファイルサイズ検証：2GB以上のファイルはエラー
            if (fileLength > FileConstants.MaxFileSize)
            {
                throw new RmsmfException(string.Format(ValidationMessages.FileTooLarge, fileName));
            }

            int fileSize = (int)fileLength;
            byte[] buffer = new byte[fileSize];
            fs.Read(buffer, 0, fileSize);

            // ファイルポジションを先頭に戻す（StreamReaderが正しく読めるようにする）
            fs.Position = 0;

            return buffer;
        }

        /// <summary>
        /// バイト配列から自動的にエンコーディングを判定
        /// </summary>
        private static void DetectEncodingFromBuffer(
            byte[] buffer,
            string fileName,
            CommandOptions.EncodingDetectionType detectionMode,
            EncodingDetectionResult result)
        {
            var options = new EncodingDetectorOptions
            {
                Strategy = ToDetectionStrategy(detectionMode)
            };

            result.EncodingInfo = EncodingProbe.Detect(buffer, options);
            result.BomExists = result.EncodingInfo.Bom;
            result.CodePage = result.EncodingInfo.CodePage;
            result.LineBreak = result.EncodingInfo.LineBreak;

            // エンコーディングオブジェクトの作成
            result.Encoding = CreateEncodingFromCodePage(result.CodePage, fileName);
        }

        /// <summary>
        /// 指定されたエンコーディングを使用
        /// （BOMの有無と改行コードの種類はバイト列から判定する）
        /// </summary>
        private static void UseSpecifiedEncoding(
            byte[] buffer,
            Encoding specifiedEncoding,
            EncodingDetectionResult result)
        {
            result.Encoding = specifiedEncoding;
            result.CodePage = specifiedEncoding.CodePage;

            // BOMと改行コードはバイト列から機械的に判定できるため、
            // エンコーディング指定の有無にかかわらず独自実装のみで判定する
            var options = new EncodingDetectorOptions { Strategy = DetectionStrategy.NativeOnly };
            EncodingInformation detected = EncodingProbe.Detect(buffer, options);

            result.BomExists = detected.Bom;
            result.LineBreak = detected.LineBreak;

            // EncodingInfoは指定エンコーディングとは無関係の判定結果になるため保持しない
            // （GetEncodingNameが指定エンコーディングの名前を正しく返せるようにするため）
            result.EncodingInfo = null;
        }

        /// <summary>
        /// 自動判定モードをSnowStack.EncodingProbeの判定戦略に変換する
        /// </summary>
        private static DetectionStrategy ToDetectionStrategy(CommandOptions.EncodingDetectionType detectionMode)
        {
            switch (detectionMode)
            {
                case CommandOptions.EncodingDetectionType.FirstParty:
                    return DetectionStrategy.NativeOnly;

                case CommandOptions.EncodingDetectionType.ThirdParty:
                    return DetectionStrategy.UtfUnknownOnly;

                case CommandOptions.EncodingDetectionType.Normal:
                default:
                    return DetectionStrategy.Combined;
            }
        }

        /// <summary>
        /// コードページからエンコーディングオブジェクトを作成
        /// </summary>
        private static Encoding CreateEncodingFromCodePage(int codePage, string fileName)
        {
            if (codePage <= 0)
            {
                return null;
            }

            try
            {
                return Encoding.GetEncoding(codePage);
            }
            catch (ArgumentException)
            {
                // サポートされていないコードページの場合はnullを設定
                // （例: EUC-TW (51950) は .NET Framework 4.8/4.8.1 でサポートされていない）
                Console.WriteLine($"Warning: Code page {codePage} is not supported. Skipping {fileName}");
                return null;
            }
            catch (NotSupportedException)
            {
                Console.WriteLine($"Warning: Code page {codePage} is not supported. Skipping {fileName}");
                return null;
            }
        }

        /// <summary>
        /// エンコーディング名を取得（表示用）
        /// </summary>
        public static string GetEncodingName(Encoding encoding, EncodingInformation encInfo)
        {
            if (encoding == null)
            {
                return "encoding Unknown";
            }

            // encInfo.EncodingWebNameが設定されている場合はそれを優先使用
            if (encInfo != null && !string.IsNullOrEmpty(encInfo.EncodingWebName))
            {
                return encInfo.EncodingWebName;
            }

            return encoding.WebName;
        }

        /// <summary>
        /// BOM表示文字列を取得
        /// </summary>
        public static string GetBomDisplayString(bool bomExists)
        {
            return bomExists ? "BOM exists" : "No BOM";
        }

        /// <summary>
        /// 改行コード種類の表示文字列を取得
        /// </summary>
        public static string GetLineBreakDisplayString(LineBreakType lineBreak)
        {
            switch (lineBreak)
            {
                case LineBreakType.None:
                    return "No";
                case LineBreakType.CrLf:
                    return "CR-LF";
                case LineBreakType.Lf:
                    return "LF";
                case LineBreakType.Cr:
                    return "CR";
                case LineBreakType.LfAndCrLf:
                    return "LF & CR-LF";
                case LineBreakType.CrAndCrLf:
                    return "CR & CR-LF";
                case LineBreakType.LfAndCr:
                    return "LF & CR";
                case LineBreakType.LfAndCrAndCrLf:
                    return "LF & CR & CR-LF";
                default:
                    return "EOL Unknown";
            }
        }

        /// <summary>
        /// エンコーディング判定結果の表示行を生成（エンコーディング不明時用）
        /// </summary>
        public static string CreateUnknownEncodingDisplayLine(
            string fileName,
            EncodingDetectionResult encodingResult)
        {
            string dispBOM = GetBomDisplayString(encodingResult.BomExists);
            string lineBreakType = GetLineBreakDisplayString(encodingResult.LineBreak);

            string encodingName = "encoding Unknown";
            if (encodingResult.EncodingInfo != null && !string.IsNullOrEmpty(encodingResult.EncodingInfo.EncodingWebName))
            {
                encodingName = encodingResult.EncodingInfo.EncodingWebName;
            }

            return $"{fileName}\t,{encodingName}\t,{lineBreakType}\t,{dispBOM}";
        }
    }
}
