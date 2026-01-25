using System;

namespace rmsmf
{
    /// <summary>
    /// RMSMFアプリケーション固有の例外クラス
    /// </summary>
    public class RmsmfException : Exception
    {
        public RmsmfException()
        {
        }

        public RmsmfException(string message)
            : base(message)
        {
        }

        public RmsmfException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
