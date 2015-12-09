using System.Runtime.InteropServices;
using System.Text;

namespace Core.Misc
{
    public static class WinApiInteraction
    {
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,

            MAPVK_VSC_TO_VK = 0x1,

            MAPVK_VK_TO_CHAR = 0x2,

            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);
    }
}