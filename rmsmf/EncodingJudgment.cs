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
        /// <summary>コードページ</summary>
        public int CodePage { get; set; }
        
        /// <summary>エンコーディング名</summary>
        public string EncodingName { get; set; }
        
        /// <summary>BOMの有無</summary>
        public bool Bom { get; set; }
        
        /// <summary>エンコーディング</summary>
        public Encoding Encoding { get; set; }
    }

    /// <summary>
    /// 文字エンコーディング判定
    /// </summary>
    public class EncodingJudgment
    {
        /// <summary>コードページ：US-ASCII</summary>
        private const int CodePageAscii = 20127;
        
        /// <summary>コードページ：ISO-2022-JP (JIS)</summary>
        private const int CodePageJis = 50220;
        
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
        
        /// <summary>コードページ：EUC-JP</summary>
        private const int CodePageEucJp = 20932;
        
        /// <summary>コードページ：Shift_JIS</summary>
        private const int CodePageShiftJis = 932;
        
        /// <summary>マジックナンバー：バッファインデックス2</summary>
        private const int BufferIndex2 = 2;
        
        /// <summary>マジックナンバー：バッファインデックス3</summary>
        private const int BufferIndex3 = 3;
        
        /// <summary>マジックナンバー：0xFF</summary>
        private const byte DefaultBufferValue = 0xFF;

        /// <summary>バッファサイズ</summary>
        public int BufferSize { get; private set; }

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
                this.BufferSize = 0;
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
            this.BufferSize = buffer.Length;
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
                long fileLength = fs.Length;
                
                // ファイルサイズ検証：2GB以上のファイルはエラー
                if (fileLength > int.MaxValue)
                {
                    throw new RmsmfException($"ファイル {fileName} が大きすぎます（最大 2GB）。");
                }
                
                this.BufferSize = (int)fileLength;
                this._buffer = new byte[this.BufferSize];
                
                // ゼロサイズのUTF-16 LE/BE 対応
                this._buffer[BufferIndex2] = DefaultBufferValue;
                this._buffer[BufferIndex3] = DefaultBufferValue;

                int readCount = fs.Read(this._buffer, 0, this.BufferSize);

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

            // BOMチェック
            if (bomJudg.IsBOM(this._buffer))
            {
                encInfo.CodePage = bomJudg.CodePage;
                encInfo.EncodingName = this.EncodingName(encInfo.CodePage);
                encInfo.Bom = true;
                return encInfo;
            }
            else
            {
                encInfo.CodePage = -1;
                encInfo.Bom = false;
            }

            // ISO-2022-JP または ASCII 判定
            bool isJIS;
            outOfSpecification = JIS_Judgment(out isJIS);

            if (outOfSpecification == false)
            {
                if (isJIS == true)
                {
                    encInfo.CodePage = CodePageJis;
                    encInfo.Bom = false;
                }
                else
                {
                    encInfo.CodePage = CodePageAscii;
                    encInfo.Bom = false;
                }

                encInfo.EncodingName = this.EncodingName(encInfo.CodePage);

                return encInfo;
            }

            // UTF-32 判定（UTF-16より先に判定）
            bool isLittleEndian32;
            outOfSpecification = Utf32_Judgment(out isLittleEndian32);

            if (outOfSpecification == false)
            {
                if (isLittleEndian32)
                {
                    encInfo.CodePage = CodePageUtf32Le;
                }
                else
                {
                    encInfo.CodePage = CodePageUtf32Be;
                }
                encInfo.EncodingName = this.EncodingName(encInfo.CodePage);
                encInfo.Bom = false;

                return encInfo;
            }

            // UTF-16 判定
            bool isLittleEndian16;
            outOfSpecification = Utf16_Judgment(out isLittleEndian16);

            if (outOfSpecification == false)
            {
                if (isLittleEndian16)
                {
                    encInfo.CodePage = CodePageUtf16Le;
                }
                else
                {
                    encInfo.CodePage = CodePageUtf16Be;
                }
                encInfo.EncodingName = this.EncodingName(encInfo.CodePage);
                encInfo.Bom = false;

                return encInfo;
            }

            // UTF-8 判定
            outOfSpecification = Utf8_Judgment();

            if (outOfSpecification == false)
            {
                encInfo.CodePage = CodePageUtf8;
                encInfo.EncodingName = this.EncodingName(encInfo.CodePage);
                encInfo.Bom = false;

                return encInfo;
            }

            // EUC-JP 判定
            outOfSpecification = EUCJP_Judgment();

            if (outOfSpecification == false)
            {
                encInfo.CodePage = CodePageEucJp;
                encInfo.EncodingName = this.EncodingName(encInfo.CodePage);
                encInfo.Bom = false;

                return encInfo;
            }

            // Shift_JIS 判定
            outOfSpecification = SJIS_Judgment();

            if (outOfSpecification == false)
            {
                encInfo.CodePage = CodePageShiftJis;
                encInfo.EncodingName = this.EncodingName(encInfo.CodePage);
                encInfo.Bom = false;
                return encInfo;
            }

            // 不明
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

            for (int i = 0; i < this.BufferSize; i++)
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
        /// UTF-16であるか判定する
        /// </summary>
        /// <param name="isLittleEndian">true=Little Endian、false=Big Endian(出力引数)</param>
        /// <returns>true=UTF-16では無い</returns>
        public bool Utf16_Judgment(out bool isLittleEndian)
        {
            isLittleEndian = true;
            bool outOfSpecification = false;

            // ファイルサイズが奇数の場合は対象外
            if (this.BufferSize % 2 != 0)
            {
                return true;
            }

            // ファイルサイズが0または2バイト未満の場合は判定不可
            if (this.BufferSize < 2)
            {
                return true;
            }

            // LE/BEのヒューリスティック判定用カウンタ
            int leScore = 0;
            int beScore = 0;

            // サロゲートペア状態管理
            bool expectLowSurrogate = false;
            ushort highSurrogate = 0;

            // 2バイト単位で読み取り
            for (int i = 0; i < this.BufferSize; i += 2)
            {
                // Little Endian として読み取り
                ushort valueLe = (ushort)(this._buffer[i] | (this._buffer[i + 1] << 8));
                // Big Endian として読み取り
                ushort valueBe = (ushort)((this._buffer[i] << 8) | this._buffer[i + 1]);

                // まずLEとして検証
                if (IsValidUtf16CodeUnit(valueLe, ref expectLowSurrogate, ref highSurrogate, out bool isLeValid))
                {
                    leScore++;
                    // ASCII範囲の文字はスコアを加算（テキストファイルらしさ）
                    if (valueLe < 0x80)
                    {
                        leScore++;
                    }
                }
                else if (!isLeValid)
                {
                    // 明らかに不正な並び
                    leScore = -1000;
                }

                // リセットしてBEとして検証
                expectLowSurrogate = false;
                highSurrogate = 0;

                if (IsValidUtf16CodeUnit(valueBe, ref expectLowSurrogate, ref highSurrogate, out bool isBeValid))
                {
                    beScore++;
                    if (valueBe < 0x80)
                    {
                        beScore++;
                    }
                }
                else if (!isBeValid)
                {
                    beScore = -1000;
                }
            }

            // 最後に高サロゲートが残っていたら不正
            if (expectLowSurrogate)
            {
                outOfSpecification = true;
            }

            // スコアが低すぎる場合はUTF-16ではない
            if (leScore < 0 && beScore < 0)
            {
                outOfSpecification = true;
            }

            // LE/BEの判定
            if (!outOfSpecification)
            {
                isLittleEndian = (leScore >= beScore);
            }

            return outOfSpecification;
        }

        /// <summary>
        /// UTF-16コードユニットの妥当性を検証
        /// </summary>
        /// <param name="value">検証するコードユニット</param>
        /// <param name="expectLowSurrogate">低サロゲート待ちフラグ</param>
        /// <param name="highSurrogate">保持中の高サロゲート</param>
        /// <param name="isValid">明らかに不正かどうか</param>
        /// <returns>true=妥当、false=不正の可能性</returns>
        private bool IsValidUtf16CodeUnit(ushort value, ref bool expectLowSurrogate, ref ushort highSurrogate, out bool isValid)
        {
            isValid = true;

            // 非文字（Noncharacters）のチェック
            if (value == 0xFFFE || value == 0xFFFF || (value >= 0xFDD0 && value <= 0xFDEF))
            {
                isValid = false;
                return false;
            }

            // 高サロゲート (D800-DBFF)
            if (value >= 0xD800 && value <= 0xDBFF)
            {
                if (expectLowSurrogate)
                {
                    // 高サロゲートの後に高サロゲート → 不正
                    isValid = false;
                    return false;
                }
                expectLowSurrogate = true;
                highSurrogate = value;
                return true;
            }

            // 低サロゲート (DC00-DFFF)
            if (value >= 0xDC00 && value <= 0xDFFF)
            {
                if (!expectLowSurrogate)
                {
                    // 低サロゲートが単独で出現 → 不正
                    isValid = false;
                    return false;
                }
                // 正しいサロゲートペア
                expectLowSurrogate = false;
                highSurrogate = 0;
                return true;
            }

            // BMP文字
            if (expectLowSurrogate)
            {
                // 高サロゲートの後に非サロゲート → 不正
                isValid = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// UTF-32であるか判定する
        /// </summary>
        /// <param name="isLittleEndian">true=Little Endian、false=Big Endian(出力引数)</param>
        /// <returns>true=UTF-32では無い</returns>
        public bool Utf32_Judgment(out bool isLittleEndian)
        {
            isLittleEndian = true;
            bool outOfSpecification = false;

            // ファイルサイズが4の倍数でない場合は対象外
            if (this.BufferSize % 4 != 0)
            {
                return true;
            }

            // ファイルサイズが0または4バイト未満の場合は判定不可
            if (this.BufferSize < 4)
            {
                return true;
            }

            // LE/BEのヒューリスティック判定用カウンタ
            int leScore = 0;
            int beScore = 0;

            // 4バイト単位で読み取り
            for (int i = 0; i < this.BufferSize; i += 4)
            {
                // Little Endian として読み取り
                uint valueLe = (uint)(this._buffer[i] | 
                                     (this._buffer[i + 1] << 8) | 
                                     (this._buffer[i + 2] << 16) | 
                                     (this._buffer[i + 3] << 24));
                
                // Big Endian として読み取り
                uint valueBe = (uint)((this._buffer[i] << 24) | 
                                     (this._buffer[i + 1] << 16) | 
                                     (this._buffer[i + 2] << 8) | 
                                     this._buffer[i + 3]);

                // LEとして検証
                if (IsValidUtf32CodePoint(valueLe))
                {
                    leScore++;
                    // ASCII範囲の文字はスコアを加算
                    if (valueLe < 0x80)
                    {
                        leScore++;
                    }
                }
                else
                {
                    leScore = -1000;
                }

                // BEとして検証
                if (IsValidUtf32CodePoint(valueBe))
                {
                    beScore++;
                    if (valueBe < 0x80)
                    {
                        beScore++;
                    }
                }
                else
                {
                    beScore = -1000;
                }
            }

            // スコアが低すぎる場合はUTF-32ではない
            if (leScore < 0 && beScore < 0)
            {
                outOfSpecification = true;
            }

            // LE/BEの判定
            if (!outOfSpecification)
            {
                isLittleEndian = (leScore >= beScore);
            }

            return outOfSpecification;
        }

        /// <summary>
        /// UTF-32コードポイントの妥当性を検証
        /// </summary>
        /// <param name="value">検証するコードポイント</param>
        /// <returns>true=妥当、false=不正</returns>
        private bool IsValidUtf32CodePoint(uint value)
        {
            // Unicodeの最大値を超える
            if (value > 0x10FFFF)
            {
                return false;
            }

            // サロゲート範囲 (D800-DFFF) はUTF-32では使用禁止
            if (value >= 0xD800 && value <= 0xDFFF)
            {
                return false;
            }

            // 非文字（Noncharacters）
            if ((value & 0xFFFE) == 0xFFFE) // nFFFE, nFFFF
            {
                return false;
            }

            if (value >= 0xFDD0 && value <= 0xFDEF)
            {
                return false;
            }

            return true;
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

            for (int i = 0; i < this.BufferSize; i++)
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

            for (int i = 0; i < this.BufferSize; i++)
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

            for (int i = 0; i < this.BufferSize; i++)
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
                    //Console.WriteLine("SJIS Spec Unkown byte found : 0x{0:X2}", this._buffer[i]);
                }

                // バイト種別規格判定
                if(sjisByte == SJIS_BYTECODE.OutOfSpec)
                {
                    outOfSpecification = true;
                    break;
                }

                if (sjisByte == SJIS_BYTECODE.TwoByteBefore)
                {
                    if(i == (this.BufferSize - 1))
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
