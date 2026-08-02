using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

namespace rmsmf.Tests
{
    /// <summary>
    /// SnowStack.EncodingProbe導入によって対応した、韓国語・繁体字中国語・簡体字中国語の
    /// 文字エンコーディング自動判定のテスト
    /// </summary>
    [TestClass]
    public class EncodingProbeLanguageTests
    {
        private string _tempFile;
        private CultureInfo _originalCulture;

        [TestInitialize]
        public void Initialize()
        {
            _tempFile = "test_lang_" + Guid.NewGuid().ToString() + ".txt";
            _originalCulture = Thread.CurrentThread.CurrentCulture;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Thread.CurrentThread.CurrentCulture = _originalCulture;

            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }

        /// <summary>
        /// 判定対象のファイルを作成し、EncodingHelperで判定を行う
        /// </summary>
        private EncodingDetectionResult DetectFile(Encoding fileEncoding, string text, CommandOptions.EncodingDetectionType detectionMode)
        {
            File.WriteAllBytes(_tempFile, fileEncoding.GetBytes(text));

            using (FileStream fs = new FileStream(_tempFile, FileMode.Open))
            {
                return EncodingHelper.DetectOrUseSpecifiedEncoding(fs, _tempFile, null, detectionMode);
            }
        }

        #region 韓国語 (CP949 / EUC-KR)

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_KoreanCP949WithKoreanCulture_DetectsCorrectly()
        {
            // Arrange
            // /det:1（FirstParty=NativeOnly）は、SnowStack.EncodingProbe内部の独自実装が
            // カルチャーに応じて対象言語の判定を行うため、韓国語カルチャーを設定する
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ko-KR");
            var cp949 = Encoding.GetEncoding(949);

            // Act
            var result = DetectFile(cp949, "안녕하세요. 이것은 테스트 파일입니다.", CommandOptions.EncodingDetectionType.FirstParty);

            // Assert
            Assert.IsNotNull(result.Encoding);
            Assert.AreEqual(949, result.CodePage);
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_KoreanEucKrWithKoreanCulture_DetectsCorrectly()
        {
            // Arrange
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ko-KR");
            var eucKr = Encoding.GetEncoding(51949);

            // Act
            var result = DetectFile(eucKr, "안녕하세요. 이것은 테스트 파일입니다.", CommandOptions.EncodingDetectionType.FirstParty);

            // Assert
            Assert.IsNotNull(result.Encoding);
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_KoreanCP949DefaultCulture_DetectsViaCombinedFallback()
        {
            // Arrange
            // カルチャーを変更しない、通常運用時のデフォルト（Normal=Combined）判定
            var cp949 = Encoding.GetEncoding(949);

            // Act
            var result = DetectFile(cp949, "안녕하세요. 이것은 테스트 파일입니다.", CommandOptions.EncodingDetectionType.Normal);

            // Assert
            Assert.IsNotNull(result.Encoding);
        }

        #endregion

        #region 繁体字中国語 (Big5/CP950 / EUC-TW)

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_TraditionalChineseBig5WithZhTwCulture_DetectsCorrectly()
        {
            // Arrange
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-TW");
            var big5 = Encoding.GetEncoding(950);

            // Act
            var result = DetectFile(big5, "這是一個用繁體中文編寫的測試檔案。", CommandOptions.EncodingDetectionType.FirstParty);

            // Assert
            Assert.IsNotNull(result.Encoding);
            Assert.AreEqual(950, result.CodePage);
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_TraditionalChineseBig5DefaultCulture_DetectsViaCombinedFallback()
        {
            // Arrange
            var big5 = Encoding.GetEncoding(950);

            // Act
            var result = DetectFile(big5, "這是一個用繁體中文編寫的測試檔案。", CommandOptions.EncodingDetectionType.Normal);

            // Assert
            Assert.IsNotNull(result.Encoding);
        }

        #endregion

        #region 簡体字中国語 (GBK/CP936, GB18030 / EUC-CN)

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_SimplifiedChineseGBKWithZhCnCulture_DetectsCorrectly()
        {
            // Arrange
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
            var gbk = Encoding.GetEncoding(936);

            // Act
            var result = DetectFile(gbk, "这是一个用简体中文编写的测试文件。", CommandOptions.EncodingDetectionType.FirstParty);

            // Assert
            Assert.IsNotNull(result.Encoding);
            Assert.AreEqual(936, result.CodePage);
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_SimplifiedChineseGB18030WithZhCnCulture_DetectsCorrectly()
        {
            // Arrange
            // GB18030は4バイトシーケンスを含む拡張文字を使うことで判定される
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
            var gb18030 = Encoding.GetEncoding(54936);

            // Act
            // 𠀀（U+20000、GB18030では4バイト表現）を含めてGBKと区別する
            var result = DetectFile(gb18030, "这是一个用简体中文编写的测试文件。\U00020000", CommandOptions.EncodingDetectionType.FirstParty);

            // Assert
            Assert.IsNotNull(result.Encoding);
            Assert.AreEqual(54936, result.CodePage);
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_SimplifiedChineseGBKDefaultCulture_DetectsViaCombinedFallback()
        {
            // Arrange
            var gbk = Encoding.GetEncoding(936);

            // Act
            var result = DetectFile(gbk, "这是一个用简体中文编写的测试文件。", CommandOptions.EncodingDetectionType.Normal);

            // Assert
            Assert.IsNotNull(result.Encoding);
        }

        #endregion
    }
}
