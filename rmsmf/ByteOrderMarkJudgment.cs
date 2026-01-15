using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rmsmf
{
    /// <summary>
    /// BOM（Byte Order Mark）の判定を行うクラス
    /// </summary>
    public class ByteOrderMarkJudgment
    {
        /// <summary>UTF-8のBOM</summary>
        private static readonly byte[] _bomUtf8 = { 0xEF, 0xBB, 0xBF };
        
        /// <summary>UTF-16 Little Endianのbom</summary>
        private static readonly byte[] _bomUtf16Little = { 0xFF, 0xFE };
        
        /// <summary>UTF-16 Big EndianのBOM</summary>
        private static readonly byte[] _bomUtf16Big = { 0xFE, 0xFF };
        
        /// <summary>UTF-32 Little EndianのBOM</summary>
        private static readonly byte[] _bomUtf32Little = { 0xFF, 0xFE, 0x00, 0x00 };
        
        /// <summary>UTF-32 Big EndianのBOM</summary>
        private static readonly byte[] _bomUtf32Big = { 0x00, 0x00, 0xFE, 0xFF };

        /// <summary>コードページ：UTF-8</summary>
        private const int CodePageUtf8 = 65001;
        
        /// <summary>コードページ：UTF-16 Little Endian</summary>
        private const int CodePageUtf16Le = 1200;
        
        /// <summary>コードページ：UTF-16 Big Endian</summary>
        private const int CodePageUtf16Be = 1201;
        
        /// <summary>コードページ：UTF-32 Little Endian</summary>
        private const int CodePageUtf32Le = 12000;
        
        /// <summary>コードページ：UTF-32 Big Endian</summary>
        private const int CodePageUtf32Be = 12001;
        
        /// <summary>コードページ：Shift_JIS（デフォルト）</summary>
        private const int CodePageShiftJis = 932;

        /// <summary>UTF-8のBOMを取得</summary>
        public static byte[] BomUtf8
        {
            get { return _bomUtf8; }
        }

        /// <summary>UTF-16 Little EndianのBOMを取得</summary>
        public static byte[] BomUtf16Little
        {
            get { return _bomUtf16Little; }
        }

        /// <summary>UTF-16 Big EndianのBOMを取得</summary>
        public static byte[] BomUtf16Big
        {
            get { return _bomUtf16Big; }
        }

        /// <summary>UTF-32 Little EndianのBOMを取得</summary>
        public static byte[] BomUtf32Little
        {
            get { return _bomUtf32Little; }
        }

        /// <summary>UTF-32 Big EndianのBOMを取得</summary>
        public static byte[] BomUtf32Big
        {
            get { return _bomUtf32Big; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ByteOrderMarkJudgment()
        {

        }

        /// <summary>
        /// コードページ
        /// </summary>
        private int _codePage = 0;

        /// <summary>
        /// コードページ
        /// </summary>
        public int CodePage
        {
            get { return this._codePage; }
        }

        /// <summary>
        /// BOM判定
        /// </summary>
        /// <param name="bomByte">判定対象バイト配列</param>
        /// <returns>true=BOM有り、false=BOMなし</returns>
        public bool IsBOM(byte[] bomByte)
        {
            bool result;

            if (IsMatched(bomByte, _bomUtf8))
            {
                result = true;
                this._codePage = CodePageUtf8;
            }
            else if (IsMatched(bomByte, _bomUtf32Little))
            {
                result = true;
                this._codePage = CodePageUtf32Le;
            }
            else if (IsMatched(bomByte, _bomUtf32Big))
            {
                result = true;
                this._codePage = CodePageUtf32Be;
            }
            else if (IsMatched(bomByte, _bomUtf16Little))
            {
                result = true;
                this._codePage = CodePageUtf16Le;
            }
            else if (IsMatched(bomByte, _bomUtf16Big))
            {
                result = true;
                this._codePage = CodePageUtf16Be;
            }
            else
            {
                result = false;
                this._codePage = CodePageShiftJis;
            }

            return result;
        }

        /// <summary>
        /// バイト配列が指定されたBOMと一致するか判定
        /// </summary>
        /// <param name="data">判定対象データ</param>
        /// <param name="bom">BOMバイト配列</param>
        /// <returns>true=一致、false=不一致</returns>
        private bool IsMatched(byte[] data, byte[] bom)
        {
            if (data == null || data.Length < bom.Length)
                return false;

            for (int i = 0; i < bom.Length; i++)
            {
                if (bom[i] != data[i])
                {
                    return false;
                }
            }

            return true;
        }

    }
}
