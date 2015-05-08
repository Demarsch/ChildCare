using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Класс расширений
    /// </summary>
    public static class StuffExtension
    {
        static StuffExtension() { }

        #region Методы расширения для класса object

        public static T CastByExample<T>(this object obj, T example)
        {
            return (T)obj;
        }

        /// <summary>
        /// Значение свойства
        /// </summary>
        public static object GetPropertyValue(this object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        /// <summary>
        /// Значение свойства
        /// </summary>
        public static void SetPropertyValue(this object src, string propName, object value)
        {
            src.GetType().GetProperty(propName).SetValue(src, value, null);
        }

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

        #endregion

        #region Методы расширений для класса int

        public static double PerCent(this int value, int count)
        {
            if (count > 0) return Math.Round((value.ToDouble() / count.ToDouble()) * 100.0, 2);
            return 0.0;
        }

        public static string PerCentStr(this int value, int count)
        {
            if (count > 0) return Math.Round((value.ToDouble() / count.ToDouble()) * 100.0, 2).ToString() + "%";
            return "";
        }

        #endregion

        #region Методы расширений для класса IEnumerable<int>, int[]

        /// <summary>
        /// возвращает первый неиспользованный в последовательности int, начиная с 1
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static int FirstUnused(this IEnumerable<int> numbers)
        {
            int i = 1;
            foreach (int t in numbers.Distinct().OrderBy(x => x))
            {
                if (t != i) break;
                i++;
            }
            return i;
        }

        /// <summary>
        /// возвращает первый неиспользованный в последовательности int, начиная с 1
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static int FirstUnused(this int[] numbers)
        {
            int i = 1;
            foreach (int t in numbers.Distinct().OrderBy(x => x))
            {
                if (t != i) break;
                i++;
            }
            return i;
        }

        #endregion

        #region Методы расширения для класса string

        /// <summary>
        /// фильтрация строки
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chars">нужные символы из строки</param>
        /// <param name="literals">оставлять ли буквы</param>
        /// <param name="digits">оставлять ли цифры</param>
        /// <param name="trimall"></param>
        /// <returns></returns>
        public static string FilterChars(this string str, string chars = " ,.-/", bool literals = true, bool digits = true, bool trimall = true)
        {
            string r = "";
            foreach (char c in str.ToCharArray()) if (chars.Contains(c) || (literals && char.IsLetter(c)) || (digits && char.IsDigit(c))) r += c;
            if (trimall)
            {
                r = r.Trim();
                while (r.Contains("  ")) r = r.Replace("  ", " ");
            }
            return r;
        }

        /// <summary>
        /// Проверяет наличие в указанной строке любого образца
        /// </summary>
        /// <param name="currentString"></param>
        /// <param name="strings">массив образцов</param>
        /// <returns></returns>
        public static bool ContainsAny(this string currentString, params string[] strings)
        {
            foreach (string item in strings)
                if (currentString.Contains(item))
                    return true;
            return false;
        }

        /// <summary>
        /// Проверяет наличие в указанной строке любого образца с учетом или без учета регистра
        /// </summary>
        /// <param name="currentString"></param>
        /// <param name="ignoreCase">true, если не нужно учитывать регистр, false - в противном случае</param>
        /// <param name="strings">массив образцов</param>
        /// <returns></returns>
        public static bool ContainsAny(this string currentString, bool ignoreCase, params string[] strings)
        {
            if (!ignoreCase)
                return ContainsAny(currentString, strings);
            string ignoreCaseCurrentString = currentString.ToLower();
            foreach (string item in strings)
                if (ignoreCaseCurrentString.Contains(item.ToLower()))
                    return true;
            return false;
        }
        
        /// <summary>
        /// Проверяет, является ли текущая строка null'ом, пустой или содержащей только пробельные символы
        /// </summary>
        /// <returns>true - если в строке нет полезной информации, false - если есть хотя бы один полезный символ</returns>
        public static bool HasNoData(this string currentString)
        {
            if (string.IsNullOrEmpty(currentString)) return true;
            if (currentString.Trim() == "") return true;
            return false;
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
        
        /// <summary>
        /// Like-нечеткое сравнение строк (возвращает степень вхождения в строку строки str)
        /// </summary>
        public static int Like(this string cur, string str)
        {
            string s1 = cur.ToLower();
            string s2 = str.ToLower();
            int i, j, c, r = 0, l1 = cur.Length, l2 = s2.Length;
            for (i = 0; i < l1; i++) for (j = 0; j < l2; j++)
                {
                    c = 0;
                    while (i + c < l1 && j + c < l2 && s1[i + c] == s2[j + c]) c++;
                    r += c * c * (i == 0 ? (c + 1) : 1) * (j == 0 ? (c + 1) : 1);
                }
            return r;
        }

        /// <summary>
        /// Parse to int (0 - if can't parse)
        /// </summary>
        public static int ToInt(this string str)
        {
            int r = 0;
            if (int.TryParse(str.Trim(), out r)) return r;
            return 0;
        }

        /// <summary>
        /// Parse to double (0 - if can't parse)
        /// </summary>
        public static double ToDouble(this string str)
        {
            double r = 0;
            if (double.TryParse(str.Trim(), out r)) return r;
            return 0;
        }

        /// <summary>
        /// приводит ФИО к нормальному виду (удаляет все левые символы первые буквы делает большими)
        /// </summary>
        public static string AutoInitial(this string str)
        {
            string r = "";
            foreach (var c in str.TrimStart(' ', '-'))
            {
                if (char.IsLetter(c))
                {
                    if (r.Length == 0 || r[r.Length - 1] == ' ' || r[r.Length - 1] == '-') r += char.ToUpper(c);
                    else r += char.ToLower(c);
                }
                else if (c == ' ' || c == '-')
                {
                    if (r.Length > 0)
                    {
                        if (r[r.Length - 1] == ' ' || r[r.Length - 1] == '-') r = r.Substring(0, r.Length - 1) + c;
                        else r += c;
                    }
                }
            }
            r = r.Replace(" Сан ", " сан ").Replace(" Оглы ", " оглы ").Replace(" Де ", " де ").Replace(" Ибн ", " ибн ");
            return r;
        }

        /// <summary>
        /// Определяет наличие в строке всех слов из строки words
        /// </summary>
        /// <param name="str"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public static bool ContainsWords(this string str, string words)
        {
            foreach (string s in words.Split(words.ToCharArray().Where(x => !char.IsLetterOrDigit(x)).Distinct().ToArray(), StringSplitOptions.RemoveEmptyEntries)) if (str.IndexOf(s) < 0) return false;
            return true;
        }

        /// <summary>
        /// Определяет наличие в строке слова, начинающегося с txt
        /// </summary>
        /// <param name="str"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool ContainsWordBeginWith(this string str, string txt)
        {
            int i = str.IndexOf(txt);
            if (i < 0) return false;
            if (i == 0) return true;
            return (!char.IsLetterOrDigit(str[i - 1]));
        }

        #endregion

        #region Методы расширений для класса IEnumerable<string>

        /// <summary>
        /// соединение строк в одну строку
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string CombineStrings(this IEnumerable<string> strings, string separator)
        {
            StringBuilder result = new StringBuilder();
            if (strings != null)
            {
                foreach (string item in strings)
                    result.Append(item).Append(separator);
                if (result.Length != 0)
                    result.Remove(result.Length - separator.Length, separator.Length);
            }
            return result.ToString();

        }

        /// <summary>
        /// фильтрация строк по словам из filter
        /// </summary>
        /// <param name="q"></param>
        /// <param name="filter">слова для фильтрации</param>
        /// <returns></returns>
        public static IEnumerable<string> Filter(this IEnumerable<string> q, string filter)
        {
            IEnumerable<string> r = q;
            bool emptyfilter = false;

            foreach (string s in filter.Split(filter.ToCharArray().Where(x => !char.IsLetterOrDigit(x)).Distinct().ToArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string st = s;
                r = r.Where(x => (' ' + x).IndexOf(' ' + st) >= 0);
                emptyfilter = false;
            }

            if (emptyfilter) r = r.Where(x => false);

            return r;
        }

        #endregion

        #region Методы расширения для класса IQueryable<string>

        /// <summary>
        /// фильтрация строк по словам из filter
        /// </summary>
        /// <param name="q"></param>
        /// <param name="filter">слова для фильтрации</param>
        /// <returns></returns>
        public static IQueryable<string> Filter(this IQueryable<string> q, string filter)
        {
            IQueryable<string> r = q;
            bool emptyfilter = true;

            foreach (string s in filter.Split(filter.ToCharArray().Where(x => !char.IsLetterOrDigit(x)).Distinct().ToArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string st = " " + s;
                r = r.Where(x => (" " + x).IndexOf(st) >= 0);
                emptyfilter = false;
            }

            if (emptyfilter) r = r.Where(x => false);

            return r;
        }

        #endregion

        #region Методы расширений для класса char

        public static bool IsEnglish(this char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
        }

        #endregion

        #region Методы расширения для класса bool
        public static int ToInt(this bool obj)
        {
            return obj ? 1 : 0;
        }
        #endregion

        #region Методы расширений для класса TimeSpan

        public static string ToString(this TimeSpan time, bool useLeadingZero)
        {
            if (useLeadingZero)
                return string.Format("{0:D2}:{1:D2}", time.Hours, time.Minutes);
            return string.Format("{0}:{1:D2}", time.Hours, time.Minutes);
        }

        #endregion

        #region Методы расширения для класса DateTime
        /// <summary>
        /// Возвращает дату и время в формате дд.ММ.гггг ЧЧ:мм
        /// </summary>
        public static string ToFullString(this DateTime thisDate)
        {
            return thisDate.ToString("dd.MM.yyyy HH:mm");
        }
        /// <summary>
        /// Возвращает первое число начала квартала, к которому относится текущая дата
        /// </summary>
        public static DateTime GetQuarterBeginning(this DateTime thisDate)
        {
            int quarterMonth = 0;
            if (thisDate.Month >= 1 && thisDate.Month <= 3)
                quarterMonth = 1;
            if (thisDate.Month >= 4 && thisDate.Month <= 6)
                quarterMonth = 4;
            if (thisDate.Month >= 7 && thisDate.Month <= 9)
                quarterMonth = 7;
            if (thisDate.Month >= 10 && thisDate.Month <= 12)
                quarterMonth = 10;
            return new DateTime(thisDate.Year, quarterMonth, 1);
        }
        /// <summary>
        /// Возвращает начало недели, к которой относится текущая дата
        /// </summary>
        public static DateTime GetWeekBegininng(this DateTime thisDate)
        {
            return thisDate.Date.AddDays((int)DayOfWeek.Monday - (thisDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)thisDate.DayOfWeek));
        }
        /// <summary>
        /// Возвращает первое число текущего месяца
        /// </summary>
        public static DateTime GetMonthBeginning(this DateTime thisDate)
        {
            return new DateTime(thisDate.Year, thisDate.Month, 1);
        }

        public static int GetQuarterIndex(this DateTime thisDate)
        {
            if (thisDate.Month >= 1 && thisDate.Month <= 3)
                return 1;
            if (thisDate.Month >= 4 && thisDate.Month <= 6)
                return 2;
            if (thisDate.Month >= 7 && thisDate.Month <= 9)
                return 3;
            if (thisDate.Month >= 10 && thisDate.Month <= 12)
                return 4;
            throw new ArgumentOutOfRangeException(thisDate.ToShortDateString() + " не относится ни к одному кварталу");
        }

        #endregion

        #region Методы расширения для класса IQueryable

        /// <summary>
        /// Получает из запроса первый объект, Id которого равен заданному значению
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TSource ById<TSource>(this IQueryable<TSource> query, int id) where TSource : class
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");
            Expression expression = Expression.Property(parameter, "Id");
            expression = Expression.Equal(expression, Expression.Constant(id));
            Expression<Func<TSource, bool>> resultEx = Expression.Lambda<Func<TSource, bool>>(expression, parameter);
            return query.FirstOrDefault(resultEx);
        }

        /// <summary>
        /// Проверяет, есть ли в последовательности элемент с заданным Id
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="query"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Any<TSource>(this IQueryable<TSource> query, int id) where TSource : class
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");
            Expression expression = Expression.Property(parameter, "Id");
            expression = Expression.Equal(expression, Expression.Constant(id));
            Expression<Func<TSource, bool>> resultEx = Expression.Lambda<Func<TSource, bool>>(expression, parameter);
            return query.Any(resultEx);
        }

        #endregion

        #region Методы расширения для класса BackgroundWorker

        public static void Setup(this BackgroundWorker worker, bool reportProgress = true, bool supportCancelation = true)
        {
            worker.WorkerSupportsCancellation = supportCancelation;
            worker.WorkerReportsProgress = reportProgress;
        }

        public static void WaitForComplete(this BackgroundWorker worker)
        {
            while (worker.IsBusy) System.Threading.Thread.Sleep(100);
        }
        
        public static void WaitForAbort(this BackgroundWorker worker)
        {
            if (worker.WorkerSupportsCancellation) worker.CancelAsync();
            while (worker.IsBusy) System.Threading.Thread.Sleep(100);
        }

        public static void WaitForRestart(this BackgroundWorker worker)
        {
            if (worker.WorkerSupportsCancellation) worker.CancelAsync();
            while (worker.IsBusy) System.Threading.Thread.Sleep(100);
            worker.RunWorkerAsync();
        }

        public static void WaitForRestart(this BackgroundWorker worker, object argument)
        {
            if (worker.WorkerSupportsCancellation) worker.CancelAsync();
            while (worker.IsBusy) System.Threading.Thread.Sleep(100);
            worker.RunWorkerAsync(argument);
        }
        
        #endregion

        #region Методы расширений для класса Comparer

        public static IEnumerable<T> DistinctBy<T, TIdentity>(this IEnumerable<T> source, Func<T, TIdentity> identitySelector)
        {
            return source.Distinct(By(identitySelector));
        }

        public static IEqualityComparer<TSource> By<TSource, TIdentity>(Func<TSource, TIdentity> identitySelector)
        {
            return new DelegateComparer<TSource, TIdentity>(identitySelector);
        }

        private class DelegateComparer<T, TIdentity> : IEqualityComparer<T>
        {
            private readonly Func<T, TIdentity> identitySelector;

            public DelegateComparer(Func<T, TIdentity> identitySelector)
            {
                this.identitySelector = identitySelector;
            }

            public bool Equals(T x, T y)
            {
                return Equals(identitySelector(x), identitySelector(y));
            }

            public int GetHashCode(T obj)
            {
                return identitySelector(obj).GetHashCode();
            }
        }

        #endregion

    }
}
