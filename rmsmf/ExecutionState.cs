using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rmsmf
{
    public static class ExecutionState
    {
        public static bool isNormal = false;
        public static bool isError = false;
        public static string errorMessage = null;
        public static int stepNumber = 0;
        public static string className = null;
    }
}
