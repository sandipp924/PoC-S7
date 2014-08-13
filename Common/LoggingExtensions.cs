using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class LoggingExtensions
    {
        public static void Debug(this Type type, string text)
        {
            LogManager.GetLogger(type).Debug(text);
        }

        public static void DebugFormat(this Type type, string formatText, params object[] formatArgs)
        {
            LogManager.GetLogger(type).DebugFormat(formatText, formatArgs);
        }

        public static void Error(this Type type, string text)
        {
            LogManager.GetLogger(type).Error(text);
        }

        public static void ErrorFormat(this Type type, string formatText, params object[] formatArgs)
        {
            LogManager.GetLogger(type).ErrorFormat(formatText, formatArgs);
        }
    }
}
