using System;
using log4net;

namespace Core.Extensions
{
    public static class LogExtensions
    {
        public static void ErrorFormatExt(this ILog log, Exception ex, string format, object arg0)
        {
            log.Error(string.Format(format, arg0), ex);
        }

        public static void ErrorFormatExt(this ILog log, Exception ex, string format, object arg0, object arg1)
        {
            log.Error(string.Format(format, arg0, arg1), ex);
        }

        public static void ErrorFormatExt(this ILog log, Exception ex, string format, object arg0, object arg1, object arg2)
        {
            log.Error(string.Format(format, arg0, arg1, arg2), ex);
        }

        public static void ErrorFormatEx(this ILog log, Exception ex, string format, params object[] args)
        {
            log.Error(string.Format(format, args), ex);
        }
    }
}
