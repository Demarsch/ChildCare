using System.Text;
using System.Windows.Input;
using Core.Misc;

namespace Core.Wpf.Misc
{
    public static class KeyToCharTranslator
    {
        public static char GetCharFromKey(Key key)
        {
            var ch = '\0';
            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            WinApiInteraction.GetKeyboardState(keyboardState);
            var scanCode = WinApiInteraction.MapVirtualKey((uint)virtualKey, WinApiInteraction.MapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new StringBuilder(2);
            var result = WinApiInteraction.ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }
    }
}
