namespace Core.Misc
{
    //TODO: we probably should completely remove this class and use only date-time formats defined in the current culture
    //TODO: however XAML has some issues with using custom formats (he use default formats defined for current culture)
    public static class DateTimeFormats
    {
        public static readonly string ShortDateFormat = "dd.MM.yyyy";

        public static readonly string ShortDateTimeFormat = "dd.MM.yyyy HH:mm";
    }
}
