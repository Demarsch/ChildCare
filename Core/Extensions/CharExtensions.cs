using System.Collections.Generic;

namespace Core.Extensions
{
    public static class CharExtensions
    {
        public static bool IsRussianLetter(this char source)
        {
            var charCode = (int)source;
            return (charCode >= 1072 && charCode <= 1103) //Small letters
                   || (charCode >= 1040 && charCode <= 1071) //Capital letters
                   || charCode == 1105 //Small ё
                   || charCode == 1025; //Capital Ё
        }
        
        private static readonly HashSet<char> RomanNumberLetters = new HashSet<char>(new[] { 'i', 'I', 'v', 'V', 'x', 'X', 'c', 'C', 'l', 'L', 'd', 'D', 'm', 'M' });

        public static bool IsRomanNumber(this char source)
        {
            return RomanNumberLetters.Contains(source);
        }

        private static readonly HashSet<char> DateSeparators = new HashSet<char>(new[] { '-', '.', ' ', '/', '\\' }); 

        public static bool IsDateSeparator(this char source)
        {
            return DateSeparators.Contains(source);
        }
    }
}