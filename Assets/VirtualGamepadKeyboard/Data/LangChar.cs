//#define USE_TMP

#if USE_TMP
using TMPro;
#endif
using UnityEngine.UI;

namespace VirtualGamepadKeyboard
{
    public class LangChar
    {
        public int Index;
        public Image CharPoint;
        public Image Selection;
#if USE_TMP
    public TMP_Text CharText;
#else
        public Text CharText;
#endif
    }
}