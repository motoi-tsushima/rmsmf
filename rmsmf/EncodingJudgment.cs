using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace rmsmf
{
    /// <summary>
    /// 文字エンコーディング判定情報
    /// </summary>
    public class EncodingInfomation
    {
        public int codePage = 0;
        public string encodingName = null;
        public bool bom = false;
        public Encoding encoding = null;
    }

    /// <summary>
    /// 文字エンコーディング判定
    /// </summary>
    public class EncodingJudgment
    {
        public int bufferSize = 0;

        /// <summary>
        /// ファイルバイナリ配列
        /// </summary>
        private byte[] _buffer = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EncodingJudgment(int filesize)
        {
            if(filesize <= 0)
            {
                this.bufferSize = 0;
                this._buffer = null;
            }
            else
            {
                this._buffer = new byte[filesize];
            }
        }

        /// <summary>
        /// バッファを呼び主が与えるコンストラクタ
        /// </summary>
        /// <param name="buffer"></param>
        public EncodingJudgment(byte[] buffer)
        {
            this._buffer = buffer;
            this.bufferSize = buffer.Length;
        }

        /// <summary>
        /// コードページからエンコーディング名を取得する
        /// </summary>
        /// <param name="codePage">コードページ</param>
        /// <returns>エンコーディング名</returns>
        public string EncodingName(int codePage)
        {
            string encodingName;

            switch (codePage)
            {
                case 20127:
                    encodingName = "us-ascii";
                    break;
                case 50220:
                    encodingName = "iso-2022-jp";
                    break;
                case 65001:
                    encodingName = "utf-8";
                    break;
                case 20932:
                    encodingName = "euc-jp";
                    break;
                case 932:
                    encodingName = "shift_jis";
                    break;
                case 1200:
                    encodingName = "utf-16";
                    break;
                case 1201:
                    encodingName = "unicodeFFFE";
                    break;
                case 12000:
                    encodingName = "utf-32";
                    break;
                case 12001:
                    encodingName = "utf-32BE";
                    break;
                default:
                    encodingName = "I do not know.";
                    break;
            }

            return encodingName;
        }

        private bool IsMatched(byte[] data, byte[] bom)
        {
            if (data == null || data.Length < bom.Length)
                return false;

            bool result = true;

            for (int i = 0; i < bom.Length; i++)
            {
                if (bom[i] != data[i])
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// ファイルを読んで判定実行
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>文字エンコーディング判定情報</returns>
        public EncodingInfomation Judgment(string fileName)
        {
            EncodingInfomation encInfo = null;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                this.bufferSize = (int)fs.Length;
                this._buffer = new byte[this.bufferSize];
                //ゼロサイズのutf-16LE.BE 対応
                this._buffer[2] = 0xFF;
                this._buffer[3] = 0xFF;

                int readCount = fs.Read(this._buffer, 0, this.bufferSize);

                encInfo = Judgment();

                //Console.WriteLine("EncodingJudgment : Encoding = {0} , Codepage = {1} , BOM = {2}", encInfo.encodingName, encInfo.codePage, encInfo.bom);
            }

            return encInfo;
        }

        /// <summary>
        /// 判定実行
        /// </summary>
        /// <returns>文字エンコーディング判定情報</returns>
        public EncodingInfomation Judgment()
        {
            bool outOfSpecification;
            EncodingInfomation encInfo = new EncodingInfomation();
            ByteOrderMarkJudgment bomJudg = new ByteOrderMarkJudgment();

            // Check BOM
            if (bomJudg.IsBOM(this._buffer))
            {
                encInfo.codePage = bomJudg.CodePage;
                encInfo.encodingName = this.EncodingName(encInfo.codePage);
                encInfo.bom = true;
                return encInfo;
            }
            else
            {
                encInfo.codePage = -1;
                encInfo.bom = false;
            }

            // if ISO-2022-JP or ASCII
            bool isJIS;
            outOfSpecification = JIS_Judgment(out isJIS);

            if (outOfSpecification == false)
            {
                if (isJIS == true)
                {
                    encInfo.codePage = 50220; //iso-2022-jp : Japanese (JIS)
                    encInfo.bom = false;
                }
                else
                {
                    encInfo.codePage = 20127; //us-ascii : US-ASCII
                    encInfo.bom = false;
                }

                encInfo.encodingName = this.EncodingName(encInfo.codePage);

                return encInfo;
            }

            // else if UTF-8
            outOfSpecification = Utf8_Judgment();

            if (outOfSpecification == false)
            {
                encInfo.codePage = 65001; //utf-8 : Unicode (UTF-8)
                encInfo.encodingName = this.EncodingName(encInfo.codePage);
                encInfo.bom = false;

                return encInfo;
            }

            // else if EUC-JP
            outOfSpecification = EUCJP_Judgment();

            if (outOfSpecification == false)
            {
                encInfo.codePage = 20932; //EUC-JP : Japanese (JIS 0208-1990 and 0212-1990)	
                encInfo.encodingName = this.EncodingName(encInfo.codePage);
                encInfo.bom = false;

                return encInfo;
            }

            // else if Shift_JIS
            outOfSpecification = SJIS_Judgment();

            if (outOfSpecification == false)
            {
                encInfo.codePage = 932; //shift_jis : Japanese (Shift-JIS)
                encInfo.encodingName = this.EncodingName(encInfo.codePage);
                encInfo.bom = false;
                return encInfo;
            }

            // I do not know.
            return encInfo;
        }


        /// <summary>
        /// JIS又はASCIIであるか判定する
        /// </summary>
        /// <param name="isJIS">true=JISである(出力引数)</param>
        /// <returns>true=JIS又はASCIIでは無い</returns>
        public bool JIS_Judgment(out bool isJIS)
        {
            bool outOfSpecification = false;    //規格外フラグ
            bool escExist = false;  //ESCシーケンス存在フラグ
            byte[][] byteESC = new byte[][] {   //ESCシーケンス一覧
                  new byte[] { 0x1B, 0x28, 0x42 } //ASCII
                , new byte[] { 0x1B, 0x24, 0x42 } //JIS X 0208-1983（新JIS）
                , new byte[] { 0x1B, 0x24, 0x40 } //JIS X 0208-1978（旧JIS）
                , new byte[] { 0x1B, 0x28, 0x4A } //JIS X 0201 ローマ字
                , new byte[] { 0x1B, 0x24, 0x28, 0x44 } //JIS X 0212-1990（補助漢字）
                , new byte[] { 0x1B, 0x24, 0x28, 0x4F } //JIS X 0213:2000 第1面
                , new byte[] { 0x1B, 0x24, 0x28, 0x50 } //JIS X 0213:2004 第1面
                , new byte[] { 0x1B, 0x24, 0x28, 0x51 } //JIS X 0213 第2面
                , new byte[] { 0x1B, 0x24, 0x41 }   //GB2312（中国語簡体字）
                , new byte[] { 0x1B, 0x24, 0x28, 0x43 } //KS X 1001（韓国語）
                , new byte[] { 0x1B, 0x2E, 0x41 } //ISO-8859-1（G2指示）
                , new byte[] { 0x1B, 0x2E, 0x46 } //ISO-8859-7（ギリシャ文字、G2指示）
                , new byte[] { 0x1B, 0x28, 0x49 } //JIS X 0201 カタカナ
            };
             
            byte[] backESC = { 0, 0, 0, 0 };    //直近4バイト保存バッファ

            // if ISO-2022-JP

            for (int i = 0; i < this.bufferSize; i++)
            {
                if (0x80 <= this._buffer[i])
                {
                    outOfSpecification = true;
                    break;
                }
                else
                {
                    //直近4バイト保存
                    backESC[0] = backESC[1];
                    backESC[1] = backESC[2];
                    backESC[2] = backESC[3];
                    backESC[3] = this._buffer[i];

                    //ESCシーケンス照合
                    for (int j=0; j< byteESC.Length; j++)
                    {
                        //ESCシーケンス長さ分だけ比較
                        if (IsMatched(backESC, byteESC[j]))
                        {
                            //ESCシーケンスが存在した
                            escExist = true;    //ESCシーケンス存在フラグON
                            break;
                        }
                    }
                }
            }

            if (escExist)
            {
                isJIS = true;
            }
            else
            {
                isJIS = false;
            }

            return outOfSpecification;
        }

        /// <summary>
        /// UTF8であるか判定する
        /// </summary>
        /// <returns>true=UTF8では無い</returns>
        public bool Utf8_Judgment()
        {
            bool outOfSpecification;

            outOfSpecification = false;
            uint[] byteChar = new uint[6];
            int byteCharCount = 0;

            for (int i = 0; i < this.bufferSize; i++)
            {
                //２バイト文字以上である
                if ((uint)0x80 <= (uint)this._buffer[i])
                {
                    //２バイト文字
                    uint char2byte = (uint)0b11100000 & (uint)this._buffer[i];
                    if (char2byte == 0b11000000)
                    {
                        //セカンドコード数が規格より少なければ規格外
                        outOfSpecification = Utf8OutOfSpecification(byteChar[0], byteCharCount, false);
                        if (outOfSpecification)
                        {
                            break;
                        }

                        byteChar[0] = char2byte;
                        byteCharCount = 1;
                        continue;
                    }

                    //3バイト文字
                    uint char3byte = (uint)0b11110000 & (uint)this._buffer[i];
                    if (char3byte == 0b11100000)
                    {
                        //セカンドコード数が規格より少なければ規格外
                        outOfSpecification = Utf8OutOfSpecification(byteChar[0], byteCharCount, false);
                        if (outOfSpecification)
                        {
                            break;
                        }

                        byteChar[0] = char3byte;
                        byteCharCount = 1;
                        continue;
                    }

                    //4バイト文字
                    uint char4byte = (uint)0b11111000 & (uint)this._buffer[i];
                    if (char4byte == 0b11110000)
                    {
                        //セカンドコード数が規格より少なければ規格外
                        outOfSpecification = Utf8OutOfSpecification(byteChar[0], byteCharCount, false);
                        if (outOfSpecification)
                        {
                            break;
                        }

                        byteChar[0] = char4byte;
                        byteCharCount = 1;
                        continue;
                    }

                    //２バイト目以降のコード
                    uint charSecond = (uint)0b11000000 & (uint)this._buffer[i];
                    if (charSecond == 0b10000000)
                    {
                        // 文字の先頭がセカンドコードなら規格外
                        if (byteCharCount < 1)
                        {
                            outOfSpecification = true;
                            break;
                        }
                        // 1文字が4バイトを超えるなら規格外
                        if (4 < byteCharCount)
                        {
                            outOfSpecification = true;
                            break;
                        }

                        //セカンドコードを保存
                        byteChar[byteCharCount] = charSecond;
                        byteCharCount++;

                        //セカンドコード数が規格より多ければ規格外
                        outOfSpecification = Utf8OutOfSpecification(byteChar[0], byteCharCount, true);
                        if (outOfSpecification)
                        {
                            break;
                        }

                        continue;
                    }

                    //どれにも当てはまらない
                    outOfSpecification = true;
                    break;
                }
                else
                {
                    // 7bit文字
                    byteChar[0] = 0;
                    byteChar[1] = 0;
                    byteChar[2] = 0;
                    byteChar[3] = 0;
                    byteChar[4] = 0;
                    byteChar[5] = 0;
                    byteCharCount = 0;
                }
            }

            return outOfSpecification;
        }

        /// <summary>
        /// UTF8セカンドコード規格判定
        /// </summary>
        /// <param name="topByteChar"></param>
        /// <param name="byteCharCount"></param>
        /// <param name="checkBig"></param>
        /// <returns>true=UTF-8では無い</returns>
        private bool Utf8OutOfSpecification(uint topByteChar, int byteCharCount, bool checkBig)
        {
            bool outOfSpecification = false;

            //セカンドコード数が規格より多ければ規格外
            if (topByteChar == 0b11000000)
            {
                if (checkBig == true)
                {
                    if (byteCharCount > 2) outOfSpecification = true;
                }
                else
                {
                    if (byteCharCount < 2) outOfSpecification = true;
                }
            }
            else if (topByteChar == 0b11100000)
            {
                if (checkBig == true)
                {
                    if (byteCharCount > 3) outOfSpecification = true;
                }
                else
                {
                    if (byteCharCount < 3) outOfSpecification = true;
                }
            }
            else if (topByteChar == 0b11110000)
            {
                if (checkBig == true)
                {
                    if (byteCharCount > 4) outOfSpecification = true;
                }
                else
                {
                    if (byteCharCount < 4) outOfSpecification = true;
                }
            }

            return outOfSpecification;
        }

        /// <summary>
        /// EUC-JPバイト種別
        /// </summary>
        public enum BYTECODE : byte { OneByteCode, TwoByteCode, KanaOneByte }

        /// <summary>
        /// EUC-JPであるか判定する
        /// </summary>
        /// <returns>true=EUC-JPでは無い</returns>
        public bool EUCJP_Judgment()
        {
            bool outOfSpecification = false;


            BYTECODE beforeCode = BYTECODE.OneByteCode;
            int byteCharCount = 0;

            for (int i = 0; i < this.bufferSize; i++)
            {
                // 2バイトコード
                if (0xA1 <= this._buffer[i] && this._buffer[i] <= 0xFE)
                {
                    if (beforeCode == BYTECODE.KanaOneByte)
                    {
                        if (byteCharCount == 1)
                        {
                            byteCharCount = 2;
                        }
                        else
                        {
                            outOfSpecification = true;
                            break;
                        }
                    }

                    if (beforeCode == BYTECODE.TwoByteCode)
                    {
                        if (byteCharCount == 1)
                            byteCharCount = 2;
                        else if (byteCharCount == 2)
                            byteCharCount = 1;
                    }

                    beforeCode = BYTECODE.TwoByteCode;
                }
                // 1バイトコード
                else if (this._buffer[i] <= 0x7F)
                {
                    if (beforeCode == BYTECODE.TwoByteCode && byteCharCount == 1)
                    {
                        outOfSpecification = true;
                        break;
                    }

                    beforeCode = BYTECODE.OneByteCode;
                    byteCharCount = 1;
                }
                // 半角カタカナ2バイトコード
                else if (this._buffer[i] == 0x8E && byteCharCount == 1)
                {
                    beforeCode = BYTECODE.KanaOneByte;
                    byteCharCount = 1;
                }
                // あり得ない
                else
                {
                    outOfSpecification = true;
                    break;
                }
            }

            return outOfSpecification;
        }

        /// <summary>
        /// Shift-JISバイト種別
        /// </summary>
        public enum SJIS_BYTECODE : byte { OneByteCode, TwoByteBefore, TwoByteAfter, KanaOneByte, OutOfSpec, Unknown }

        /// <summary>
        /// Shift-JIS であるか判定する
        /// </summary>
        /// <returns>true=Shift-JISでは無い</returns>
        public bool SJIS_Judgment()
        {
            bool outOfSpecification = false; ;
            SJIS_BYTECODE beforeSjisByte = SJIS_BYTECODE.OneByteCode;
            SJIS_BYTECODE sjisByte = SJIS_BYTECODE.OneByteCode;

            // if SJIS

            for (int i = 0; i < this.bufferSize; i++)
            {
                // バイト種別判定
                if (beforeSjisByte != SJIS_BYTECODE.TwoByteBefore
                    && this._buffer[i] <= 0x7F )
                {
                    sjisByte = SJIS_BYTECODE.OneByteCode;
                }
                else if (
                    (beforeSjisByte != SJIS_BYTECODE.TwoByteBefore)
                    && (0xA1 <= this._buffer[i] && this._buffer[i] <= 0xDF)
                    )
                {
                    sjisByte = SJIS_BYTECODE.KanaOneByte;
                }
                else if ((beforeSjisByte != SJIS_BYTECODE.TwoByteBefore) 
                    && (0x81 <= this._buffer[i] && this._buffer[i] <= 0x9F)
                    )
                {
                    sjisByte = SJIS_BYTECODE.TwoByteBefore;
                }
                else if ((beforeSjisByte != SJIS_BYTECODE.TwoByteBefore)
                    && (0xE0 <= this._buffer[i] && this._buffer[i] <= 0xFC)
                    )
                {
                    sjisByte = SJIS_BYTECODE.TwoByteBefore;
                }
                else if ((beforeSjisByte == SJIS_BYTECODE.TwoByteBefore)
                        && (0x40 <= this._buffer[i] && this._buffer[i] <= 0x7E)
                        )
                {
                    sjisByte = SJIS_BYTECODE.TwoByteAfter;
                }
                else if ((beforeSjisByte == SJIS_BYTECODE.TwoByteBefore)
                        && (0x80 <= this._buffer[i] && this._buffer[i] <= 0xFC)
                        )
                {
                    sjisByte = SJIS_BYTECODE.TwoByteAfter;
                }
                else if (this._buffer[i] == 0x80 || this._buffer[i] == 0xA0)
                {
                    sjisByte = SJIS_BYTECODE.OutOfSpec;
                }
                else if (0xFD <= this._buffer[i]  && this._buffer[i] <= 0xFF)
                {
                    sjisByte = SJIS_BYTECODE.OutOfSpec;
                }
                else if (this._buffer[i] == 0x7F)
                {
                    sjisByte = SJIS_BYTECODE.OutOfSpec;
                }
                else
                {
                    sjisByte = SJIS_BYTECODE.Unknown;
                    Console.WriteLine("SJIS Spec Unkown byte found : 0x{0:X2}", this._buffer[i]);
                }

                // バイト種別規格判定
                if(sjisByte == SJIS_BYTECODE.OutOfSpec)
                {
                    outOfSpecification = true;
                    break;
                }

                if (sjisByte == SJIS_BYTECODE.TwoByteBefore)
                {
                    if(i == (this.bufferSize - 1))
                    {
                        // 最後のバイトで2バイト文字の前半が来たら規格外
                        outOfSpecification = true;
                        break;
                    }
                }
                
                if (beforeSjisByte == SJIS_BYTECODE.TwoByteBefore)
                {
                    // 直前が2バイト文字の前半の場合
                    if (sjisByte != SJIS_BYTECODE.TwoByteAfter)
                    {
                        outOfSpecification = true;
                        break;
                    }
                }

                // 一つ前のバイト種別の保存
                beforeSjisByte = sjisByte;
            }

            return outOfSpecification;
        }

    }
}
