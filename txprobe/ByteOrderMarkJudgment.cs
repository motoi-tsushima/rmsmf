using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace txprobe
{
    public class ByteOrderMarkJudgment
    {
        private static byte[] bomUTF8 = { 0xEF, 0xBB, 0xBF };
        private static byte[] bomUTF16Little = { 0xFF, 0xFE };
        private static byte[] bomUTF16Big = { 0xFE, 0xFF };
        private static byte[] bomUTF32Little = { 0xFF, 0xFE, 0x00, 0x00 };
        private static byte[] bomUTF32Big = { 0x00, 0x00, 0xFE, 0xFF };

        public static byte[] BOM_UTF8
        {
            get { return ByteOrderMarkJudgment.bomUTF8; }
        }

        public static byte[] BOM_UTF16Little
        {
            get { return ByteOrderMarkJudgment.bomUTF16Little; }
        }

        public static byte[] BOM_UTF16Big
        {
            get { return ByteOrderMarkJudgment.BOM_UTF16Big; }
        }

        public static byte[] BOM_UTF32Little
        {
            get { return ByteOrderMarkJudgment.BOM_UTF32Little; }
        }

        public static byte[] BOM_UTF32Big
        {
            get { return ByteOrderMarkJudgment.BOM_UTF32Big; }
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
        /// <returns>True=BOM有り。False=BOMなし。</returns>
        public bool IsBOM(byte[] bomByte)
        {
            bool result;

            if (IsMatched(bomByte, bomUTF8))
            {
                result = true;
                this._codePage = 65001; //utf-8,Unicode (UTF-8)
            }

            else if (IsMatched(bomByte, bomUTF32Little))
            {
                result = true;
                this._codePage = 12000; //utf-32,Unicode (UTF-32)
            }

            else if (IsMatched(bomByte, bomUTF32Big))
            {
                result = true;
                this._codePage = 12001; //utf-32BE,Unicode (UTF-32 Big-Endian) 
            }

            else if (IsMatched(bomByte, bomUTF16Little))
            {
                result = true;
                this._codePage = 1200; //utf-16,Unicode
            }

            else if (IsMatched(bomByte, bomUTF16Big))
            {
                result = true;
                this._codePage = 1201; //utf-16BE,Unicode (Big-Endian) 
            }

            else
            {
                result = false;
                this._codePage = 932; //shift_jis,Japanese (Shift-JIS)
            }

            return result;
        }

        public bool IsMatched(byte[] data, byte[] bom)
        {
            if (data == null || data.Length < bom.Length)
                return false;

            bool result = true;

            for (int i = 0; i < bom.Length; i++)
            {
                if (bom[i] != data[i])
                    result = false;
            }

            return result;
        }

    }
}
