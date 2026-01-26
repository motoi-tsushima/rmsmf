using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using UtfUnknown;

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

    public static class EncodingJudgmentControl
    {
        public static EncodingInfomation JudgeEncoding(byte[] buffer)
        {
            EncodingInfomation encInfo;
            EncodingJudgment encJudg = new EncodingJudgment(buffer);
            encInfo = encJudg.Judgment();
            return encInfo;
        }
        public static EncodingInfomation JudgeUtfUnknown(byte[] buffer)
        {
            EncodingInfomation encInfo = new EncodingInfomation();

            var result = CharsetDetector.DetectFromBytes(buffer);
            if (result != null)
            {
                if (result.Detected.Confidence > 0.5)
                {
                    encInfo.EncodingName = result.Detected.EncodingName;
                    encInfo.CodePage = result.Detected.Encoding.CodePage;
                }
                else
                {
                    encInfo.CodePage = -1;
                }
            }
            else
            {
                encInfo.CodePage = -1;
            }
            return encInfo;
        }

        public static EncodingInfomation NormalJudgeEncoding(byte[] buffer)
        {
            EncodingInfomation encInfo;

            encInfo = JudgeEncoding(buffer);
            if (encInfo.CodePage < 0)
            {
                encInfo = JudgeUtfUnknown(buffer);
            }
            return encInfo;
        }
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
        
        /// <summary>コードページ：ISO-2022-KR</summary>
        private const int CodePageIso2022Kr = 50225;
        
        /// <summary>コードページ：ISO-2022-CN (x-cp50227)</summary>
        private const int CodePageIso2022Cn = 50227;
        
        /// <summary>コードページ：ISO-2022-TW</summary>
        private const int CodePageIso2022Tw = 50229;
        
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
        
        /// <summary>コードページ：EUC-KR</summary>
        private const int CodePageEucKr = 51949;
        
        /// <summary>コードページ：EUC-CN (GB2312)</summary>
        private const int CodePageEucCn = 51936;
        
        /// <summary>コードページ：EUC-TW</summary>
        private const int CodePageEucTw = 51950;
        
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
                case 50225:
                    encodingName = "iso-2022-kr";
                    break;
                case 50227:
                    encodingName = "x-cp50227";
                    break;
                case 50229:
                    encodingName = "iso-2022-tw";
                    break;
                case 65001:
                    encodingName = "utf-8";
                    break;
                case 20932:
                    encodingName = "euc-jp";
                    break;
                case 51936:
                    encodingName = "euc-cn";
                    break;
                case 51949:
                    encodingName = "euc-kr";
                    break;
                case 51950:
                    encodingName = "euc-tw";
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

            // ISO-2022 または ASCII 判定
            bool isISO2022;
            int iso2022CodePage;
            outOfSpecification = ISO2022_Judgment(out isISO2022, out iso2022CodePage);

            if (outOfSpecification == false)
            {
                if (isISO2022 == true)
                {
                    encInfo.CodePage = iso2022CodePage;
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

            // EUC 判定 (EUC-JP/KR/CN/TW)
            int eucCodePage;
            outOfSpecification = EUCxx_Judgment(out eucCodePage);

            if (outOfSpecification == false)
            {
                encInfo.CodePage = eucCodePage;
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
        /// ISO-2022 (JP/KR/CN/TW) 又はASCIIであるか判定する
        /// </summary>
        /// <param name="isISO2022">true=ISO-2022である(出力引数)</param>
        /// <param name="codePage">判別した文字エンコーディングのコードページ（判別できない場合は-1）</param>
        /// <returns>true=ISO-2022又はASCIIでは無い</returns>
        public bool ISO2022_Judgment(out bool isISO2022, out int codePage)
        {
            codePage = -1;
            isISO2022 = false;
            bool outOfSpecification = false;    //規格外フラグ
            bool escExist = false;  //ESCシーケンス存在フラグ
            
            // 各ISO-2022バリアントの特徴的なESCシーケンスをスコアリング
            int jpScore = 0;   // ISO-2022-JP
            int krScore = 0;   // ISO-2022-KR
            int cnScore = 0;   // ISO-2022-CN
            int twScore = 0;   // ISO-2022-TW
            
            // 共通のESCシーケンス一覧
            byte[][] commonESC = new byte[][] {
                new byte[] { 0x1B, 0x28, 0x42 }, //ASCII
                new byte[] { 0x1B, 0x2E, 0x41 }, //ISO-8859-1（G2指示）
                new byte[] { 0x1B, 0x2E, 0x46 }  //ISO-8859-7（ギリシャ文字、G2指示）
            };
            
            // ISO-2022-JP 特有のESCシーケンス
            byte[][] jpESC = new byte[][] {
                new byte[] { 0x1B, 0x24, 0x42 },       //JIS X 0208-1983（新JIS）
                new byte[] { 0x1B, 0x24, 0x40 },       //JIS X 0208-1978（旧JIS）
                new byte[] { 0x1B, 0x28, 0x4A },       //JIS X 0201 ローマ字
                new byte[] { 0x1B, 0x24, 0x28, 0x44 }, //JIS X 0212-1990（補助漢字）
                new byte[] { 0x1B, 0x24, 0x28, 0x4F }, //JIS X 0213:2000 第1面
                new byte[] { 0x1B, 0x24, 0x28, 0x50 }, //JIS X 0213:2004 第1面
                new byte[] { 0x1B, 0x24, 0x28, 0x51 }, //JIS X 0213 第2面
                new byte[] { 0x1B, 0x28, 0x49 }        //JIS X 0201 カタカナ
            };
            
            // ISO-2022-KR 特有のESCシーケンス
            byte[][] krESC = new byte[][] {
                new byte[] { 0x1B, 0x24, 0x29, 0x43 }  //KS X 1001（韓国語）
            };
            
            // ISO-2022-CN 特有のESCシーケンス
            byte[][] cnESC = new byte[][] {
                new byte[] { 0x1B, 0x24, 0x29, 0x41 }, //GB2312（中国語簡体字）
                new byte[] { 0x1B, 0x24, 0x41 },       //GB2312（中国語簡体字、別形式）
                new byte[] { 0x1B, 0x24, 0x29, 0x47 }, //CNS 11643-1992 Plane 1
                new byte[] { 0x1B, 0x24, 0x2A, 0x48 }  //CNS 11643-1992 Plane 2
            };
            
            // ISO-2022-TW 特有のESCシーケンス
            byte[][] twESC = new byte[][] {
                new byte[] { 0x1B, 0x24, 0x29, 0x47 }, //CNS 11643-1992 Plane 1
                new byte[] { 0x1B, 0x24, 0x2A, 0x48 }, //CNS 11643-1992 Plane 2
                new byte[] { 0x1B, 0x24, 0x2B, 0x49 }, //CNS 11643-1992 Plane 3
                new byte[] { 0x1B, 0x24, 0x2B, 0x4A }, //CNS 11643-1992 Plane 4
                new byte[] { 0x1B, 0x24, 0x2B, 0x4B }, //CNS 11643-1992 Plane 5
                new byte[] { 0x1B, 0x24, 0x2B, 0x4C }, //CNS 11643-1992 Plane 6
                new byte[] { 0x1B, 0x24, 0x2B, 0x4D }  //CNS 11643-1992 Plane 7
            };
            
            byte[] backESC = { 0, 0, 0, 0 };    //直近4バイト保存バッファ
            bool hasShiftFunction = false;      // SO/SI の存在（KR/CN/TW用）

            // ISO-2022はすべて7bitのみ

            for (int i = 0; i < this.BufferSize; i++)
            {
                if (0x80 <= this._buffer[i])
                {
                    outOfSpecification = true;
                    break;
                }
                
                // SO (Shift Out: 0x0E) / SI (Shift In: 0x0F) の検出
                if (this._buffer[i] == 0x0E || this._buffer[i] == 0x0F)
                {
                    hasShiftFunction = true;
                    // KR, CN, TW で使用される
                    krScore += 5;
                    cnScore += 3;
                    twScore += 3;
                }
                
                //直近4バイト保存
                backESC[0] = backESC[1];
                backESC[1] = backESC[2];
                backESC[2] = backESC[3];
                backESC[3] = this._buffer[i];

                //ESCシーケンス照合
                
                // 共通ESCシーケンス
                for (int j = 0; j < commonESC.Length; j++)
                {
                    if (IsMatched(backESC, commonESC[j]))
                    {
                        escExist = true;
                        break;
                    }
                }
                
                // ISO-2022-JP
                for (int j = 0; j < jpESC.Length; j++)
                {
                    if (IsMatched(backESC, jpESC[j]))
                    {
                        escExist = true;
                        jpScore += 10;
                        break;
                    }
                }
                
                // ISO-2022-KR
                for (int j = 0; j < krESC.Length; j++)
                {
                    if (IsMatched(backESC, krESC[j]))
                    {
                        escExist = true;
                        krScore += 10;
                        break;
                    }
                }
                
                // ISO-2022-CN
                for (int j = 0; j < cnESC.Length; j++)
                {
                    if (IsMatched(backESC, cnESC[j]))
                    {
                        escExist = true;
                        cnScore += 10;
                        break;
                    }
                }
                
                // ISO-2022-TW
                for (int j = 0; j < twESC.Length; j++)
                {
                    if (IsMatched(backESC, twESC[j]))
                    {
                        escExist = true;
                        twScore += 10;
                        break;
                    }
                }
            }

            if (escExist)
            {
                isISO2022 = true;
                
                // 最高スコアのバリアントを選択
                int maxScore = Math.Max(Math.Max(jpScore, krScore), Math.Max(cnScore, twScore));
                
                if (maxScore == jpScore)
                {
                    codePage = CodePageJis;
                }
                else if (maxScore == krScore)
                {
                    codePage = CodePageIso2022Kr;
                }
                else if (maxScore == cnScore)
                {
                    codePage = CodePageIso2022Cn;
                }
                else if (maxScore == twScore)
                {
                    codePage = CodePageIso2022Tw;
                }
                else
                {
                    // スコアがすべて0の場合（共通ESCのみ）、デフォルトでJPを選択
                    codePage = CodePageJis;
                }
            }
            else
            {
                isISO2022 = false;
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

            // NULLバイトパターンチェックによるUTF-16判定の強化
            if (!outOfSpecification)
            {
                int leNullPattern = 0;
                int beNullPattern = 0;

                for (int i = 0; i < this.BufferSize; i++)
                {
                    byte currentByte = this._buffer[i];
                    
                    // ASCII範囲のバイトをチェック
                    if (IsAsciiRangeByte(currentByte))
                    {
                        // UTF-16LE パターン: XX 00
                        if (i % 2 == 0 && i + 1 < this.BufferSize && this._buffer[i + 1] == 0x00)
                        {
                            leNullPattern++;
                        }
                        
                        // UTF-16BE パターン: 00 XX
                        if (i % 2 == 1 && i - 1 >= 0 && this._buffer[i - 1] == 0x00)
                        {
                            beNullPattern++;
                        }
                    }
                }

                // NULLパターンが見つからない場合はUTF-16ではない
                if (leNullPattern == 0 && beNullPattern == 0)
                {
                    outOfSpecification = true;
                }
                else
                {
                    // NULLパターンに基づいてエンディアンを判定
                    isLittleEndian = (leNullPattern >= beNullPattern);
                }
            }
            else if (!outOfSpecification)
            {
                // スコアのみでエンディアン判定
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

            // NULLバイトパターンチェックによるUTF-32判定の強化
            if (!outOfSpecification)
            {
                int leNullPattern = 0;
                int beNullPattern = 0;

                for (int i = 0; i < this.BufferSize; i++)
                {
                    byte currentByte = this._buffer[i];
                    
                    // ASCII範囲のバイトをチェック
                    if (IsAsciiRangeByte(currentByte))
                    {
                        // UTF-32LE パターン: XX 00 00 00
                        if (i % 4 == 0 && i + 3 < this.BufferSize &&
                            this._buffer[i + 1] == 0x00 &&
                            this._buffer[i + 2] == 0x00 &&
                            this._buffer[i + 3] == 0x00)
                        {
                            leNullPattern++;
                        }
                        
                        // UTF-32BE パターン: 00 00 00 XX
                        if (i % 4 == 3 && i - 3 >= 0 &&
                            this._buffer[i - 3] == 0x00 &&
                            this._buffer[i - 2] == 0x00 &&
                            this._buffer[i - 1] == 0x00)
                        {
                            beNullPattern++;
                        }
                    }
                }

                // NULLパターンが見つからない場合はUTF-32ではない
                if (leNullPattern == 0 && beNullPattern == 0)
                {
                    outOfSpecification = true;
                }
                else
                {
                    // NULLパターンに基づいてエンディアンを判定
                    isLittleEndian = (leNullPattern >= beNullPattern);
                }
            }
            else if (!outOfSpecification)
            {
                // スコアのみでエンディアン判定
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
        /// ASCII範囲のバイトかどうかを判定
        /// </summary>
        /// <param name="b">判定するバイト</param>
        /// <returns>true=ASCII範囲のバイト、false=それ以外</returns>
        private bool IsAsciiRangeByte(byte b)
        {
            // 0x20-0x7E: 印字可能文字
            // 0x09: タブ
            // 0x0A: LF
            // 0x0D: CR
            return (b >= 0x20 && b <= 0x7E) || b == 0x09 || b == 0x0A || b == 0x0D;
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
        /// EUCバイト種別
        /// </summary>
        public enum BYTECODE : byte { OneByteCode, TwoByteCode, CodeSet2, CodeSet3 }

        /// <summary>
        /// EUC-JP/KR/CN/TWであるか判定する
        /// </summary>
        /// <param name="codePage">判別した文字エンコーディングのコードページ（判別できない場合は-1）</param>
        /// <returns>true=EUCでは無い</returns>
        public bool EUCxx_Judgment(out int codePage)
        {
            codePage = -1;
            bool outOfSpecification = false;

            // 各EUCの特徴をスコアリング
            int jpScore = 0;  // EUC-JP
            int krScore = 0;  // EUC-KR
            int cnScore = 0;  // EUC-CN
            int twScore = 0;  // EUC-TW

            bool isDefinitelyJP = false;
            bool isDefinitelyTW = false;

            BYTECODE beforeCode = BYTECODE.OneByteCode;
            int byteCharCount = 0;

            for (int i = 0; i < this.BufferSize; i++)
            {
                byte currentByte = this._buffer[i];

                // ステップ1：構造によるフィルタリング（確実性が高い）
                
                // 0x8E (SS2) の検出
                if (currentByte == 0x8E)
                {
                    if (i + 1 < this.BufferSize)
                    {
                        byte nextByte = this._buffer[i + 1];
                        
                        // EUC-TW: 0x8E + 0xA1〜0xB0 + 0xA1〜0xFE + 0xA1〜0xFE (4バイト構造)
                        if (nextByte >= 0xA1 && nextByte <= 0xB0 && i + 3 < this.BufferSize)
                        {
                            byte byte3 = this._buffer[i + 2];
                            byte byte4 = this._buffer[i + 3];
                            if (byte3 >= 0xA1 && byte3 <= 0xFE && byte4 >= 0xA1 && byte4 <= 0xFE)
                            {
                                isDefinitelyTW = true;
                                twScore += 100;
                                i += 3; // 4バイト分スキップ
                                beforeCode = BYTECODE.OneByteCode;
                                byteCharCount = 0;
                                continue;
                            }
                        }
                        
                        // EUC-JP: 0x8E + 0xA1〜0xDF (半角カナ)
                        if (nextByte >= 0xA1 && nextByte <= 0xDF)
                        {
                            isDefinitelyJP = true;
                            jpScore += 50;
                            i += 1; // 2バイト分スキップ
                            beforeCode = BYTECODE.OneByteCode;
                            byteCharCount = 0;
                            continue;
                        }
                        
                        // EUC-JP: 0x8E + 0xB1〜0xDF (特にこの範囲は半角カナとして使われる)
                        if (nextByte >= 0xB1 && nextByte <= 0xFE)
                        {
                            jpScore += 10;
                            i += 1;
                            beforeCode = BYTECODE.OneByteCode;
                            byteCharCount = 0;
                            continue;
                        }
                    }
                    
                    // 0x8Eの後に適切なバイトが続かない場合は規格外
                    if (i + 1 >= this.BufferSize || 
                        (this._buffer[i + 1] < 0xA1 || this._buffer[i + 1] > 0xFE))
                    {
                        outOfSpecification = true;
                        break;
                    }
                }
                
                // 0x8F (SS3) の検出 - EUC-JP確定
                if (currentByte == 0x8F)
                {
                    if (i + 2 < this.BufferSize)
                    {
                        byte byte2 = this._buffer[i + 1];
                        byte byte3 = this._buffer[i + 2];
                        if (byte2 >= 0xA1 && byte2 <= 0xFE && byte3 >= 0xA1 && byte3 <= 0xFE)
                        {
                            isDefinitelyJP = true;
                            jpScore += 100;
                            i += 2; // 3バイト分スキップ
                            beforeCode = BYTECODE.OneByteCode;
                            byteCharCount = 0;
                            continue;
                        }
                    }
                    
                    // 0x8Fの後に適切なバイトが続かない場合は規格外
                    if (i + 2 >= this.BufferSize || 
                        this._buffer[i + 1] < 0xA1 || this._buffer[i + 1] > 0xFE ||
                        this._buffer[i + 2] < 0xA1 || this._buffer[i + 2] > 0xFE)
                    {
                        outOfSpecification = true;
                        break;
                    }
                }

                // ステップ2：2バイト文字（G1領域）の範囲チェック
                
                // 2バイトコード (0xA1〜0xFE)
                if (0xA1 <= currentByte && currentByte <= 0xFE)
                {
                    // 2バイト文字の1バイト目の場合
                    if (beforeCode != BYTECODE.TwoByteCode || byteCharCount == 2)
                    {
                        // EUC-CN: 1バイト目が 0xB0〜0xF7 (常用漢字域) に集中する
                        if (currentByte >= 0xB0 && currentByte <= 0xF7)
                        {
                            cnScore += 2;
                        }
                        
                        // EUC-KR: 1バイト目が 0xB0〜0xC8 (ハングル域) に集中する
                        if (currentByte >= 0xB0 && currentByte <= 0xC8)
                        {
                            krScore += 2;
                        }
                        
                        // EUC-JP/TW: 漢字域が広く分布
                        if (currentByte >= 0xA1 && currentByte <= 0xFE)
                        {
                            jpScore += 1;
                            twScore += 1;
                        }
                    }

                    if (beforeCode == BYTECODE.TwoByteCode)
                    {
                        if (byteCharCount == 1)
                            byteCharCount = 2;
                        else if (byteCharCount == 2)
                            byteCharCount = 1;
                    }
                    else
                    {
                        byteCharCount = 1;
                    }

                    beforeCode = BYTECODE.TwoByteCode;
                }
                // 1バイトコード (ASCII)
                else if (currentByte <= 0x7F)
                {
                    if (beforeCode == BYTECODE.TwoByteCode && byteCharCount == 1)
                    {
                        outOfSpecification = true;
                        break;
                    }

                    beforeCode = BYTECODE.OneByteCode;
                    byteCharCount = 1;
                }
                // 0x80〜0x8D, 0x90〜0x9F, 0xFF など不正なバイト
                else if (currentByte != 0x8E && currentByte != 0x8F)
                {
                    outOfSpecification = true;
                    break;
                }
            }

            // 規格外の場合は終了
            if (outOfSpecification)
            {
                return true;
            }

            // ステップ3：最終判定
            
            // 確定的な特徴がある場合はそれを優先
            if (isDefinitelyTW)
            {
                codePage = CodePageEucTw;
                return false;
            }
            
            if (isDefinitelyJP)
            {
                codePage = CodePageEucJp;
                return false;
            }

            // スコアリングによる判定
            int maxScore = Math.Max(Math.Max(jpScore, krScore), Math.Max(cnScore, twScore));
            
            // スコアが低すぎる場合はEUCではないと判定
            if (maxScore <= 0)
            {
                return true;
            }

            // 各スコアをリスト化して上位候補を抽出
            var scores = new List<Tuple<int, int>>
            {
                Tuple.Create(jpScore, CodePageEucJp),
                Tuple.Create(krScore, CodePageEucKr),
                Tuple.Create(cnScore, CodePageEucCn),
                Tuple.Create(twScore, CodePageEucTw)
            };
            
            // スコアの降順でソート
            scores.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            
            int topScore = scores[0].Item1;
            int secondScore = scores.Count > 1 ? scores[1].Item1 : 0;
            
            // スコア差の閾値（この値以下なら「同点に近い」と判断）
            const int scoreThreshold = 5;
            
            // スコアが同点または非常に近い場合、OSカルチャー情報を参照
            if (topScore - secondScore <= scoreThreshold)
            {
                int cultureBasedCodePage = GetEucCodePageFromOsCulture();
                
                // OSカルチャーに基づくコードページが候補に含まれているか確認
                if (cultureBasedCodePage != -1)
                {
                    // 上位2つの候補のスコアが近い場合、OSカルチャーを優先
                    foreach (var score in scores)
                    {
                        if (score.Item2 == cultureBasedCodePage && score.Item1 >= topScore - scoreThreshold)
                        {
                            codePage = cultureBasedCodePage;
                            return false;
                        }
                    }
                }
            }

            // OSカルチャー情報による判定ができない場合、最高スコアのエンコーディングを選択
            codePage = scores[0].Item2;

            return false;
        }

        /// <summary>
        /// OSカルチャー情報から推定されるEUCコードページを取得
        /// </summary>
        /// <returns>EUCコードページ（該当なしの場合は-1）</returns>
        private int GetEucCodePageFromOsCulture()
        {
            try
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                string cultureName = currentCulture.Name;

                // カルチャー名から判定
                // 日本語
                if (cultureName.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
                {
                    return CodePageEucJp;
                }
                // 韓国語
                else if (cultureName.StartsWith("ko", StringComparison.OrdinalIgnoreCase))
                {
                    return CodePageEucKr;
                }
                // 中国語（簡体字）
                else if (cultureName.Equals("zh-CN", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-SG", StringComparison.OrdinalIgnoreCase))
                {
                    return CodePageEucCn;
                }
                // 中国語（繁体字・台湾）
                else if (cultureName.Equals("zh-TW", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-HK", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-MO", StringComparison.OrdinalIgnoreCase))
                {
                    return CodePageEucTw;
                }
                
                // 該当なし
                return -1;
            }
            catch
            {
                // エラーが発生した場合は判定不可
                return -1;
            }
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
