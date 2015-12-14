namespace Core.Extensions
{
    public static class CharExtensions
    {
        public static bool IsRussianLetter(this char source)
        {
            var charCode = (int)source;
            return (charCode >= 1072 && charCode <= 1103) //Small letters
                || (charCode >= 1040 && charCode <= 1071) //Capital letters
                || charCode == 1105                       //Small ё
                || charCode == 1025;                      //Capital Ё
        }
    }
}
