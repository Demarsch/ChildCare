using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns empty string if current object is null else calls ToString()
        /// </summary>
        public static string ToSafeString(this object obj)
        {
            return obj == null ? "" : obj.ToString();
        }

        /// <summary>
        /// Returns empty string if current object is zero else calls ToString()
        /// </summary>
        public static string ToEmptyString(this object obj)
        {
            return obj.ToDouble() == 0.0 ? "" : obj.ToString();
        }

        /// <summary>
        /// Returns default value if current object is empty
        /// </summary>
        public static string ToDefaultString(this object obj, string defaultValue)
        {
            return ((obj.ToSafeString().HasData() && obj.ToSafeString() != "0") ? obj.ToString() : defaultValue);
        }

        /// <summary>
        /// Returns null without throwing a NullReferenceException if current object is null else calls ToString()
        /// </summary>
        public static string ToNullString(this object obj)
        {
            return obj == null ? null : obj.ToString();
        }

        /// <summary>
        /// Parse to int (0 - if can't parse)
        /// </summary>
        public static int ToInt(this object obj)
        {
            if (obj is int) return (int)obj;
            if (obj is double) return Convert.ToInt32((double)obj);
            if (obj is decimal) return Convert.ToInt32((decimal)obj);
            if (obj is float) return Convert.ToInt32((float)obj);
            try
            {
                return int.Parse(obj.ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Parse to double (0 - if can't parse)
        /// </summary>
        public static double ToDouble(this object obj)
        {
            if (obj is int) return Convert.ToDouble((int)obj);
            if (obj is double) return (double)obj;
            if (obj is decimal) return Convert.ToDouble((decimal)obj);
            if (obj is float) return Convert.ToDouble((float)obj);
            try
            {
                return double.Parse(obj.ToString());
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Проверяет, чтобы строка не являлась null'ом, пустой или содержащей только пробельные символы
        /// </summary>
        /// <returns>false - если в строке нет полезной информации, true - если есть хотя бы один полезный символ</returns>
        public static bool HasData(this string currentString)
        {
            if (string.IsNullOrEmpty(currentString)) return false;
            if (currentString.Trim() == "") return false;
            return true;
        }
    }
}
