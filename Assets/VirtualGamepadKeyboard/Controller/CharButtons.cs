using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem;

namespace VirtualGamepadKeyboard
{
    internal static class CharButtons
    {
        internal static IEnumerator UpButtonFade(LangChar langChar, ColorSetup colors)
        {
            var timer = 0.0f;
            if (langChar.Selection != null) langChar.Selection.color = colors.charSelectedColor;
            while (true)
            {
                yield return VirtualGamepadKeyboard.UpdateRateWaiter;
                if (langChar.Selection == null) yield break;
                langChar.Selection.color = Color.Lerp(langChar.Selection.color, colors.charUnselectedColor, 4.0f * VirtualGamepadKeyboard.UpdateRate);
                if (timer > 1.0f)
                {
                    langChar.Selection.color = colors.charUnselectedColor;
                    yield break;
                }
                timer += VirtualGamepadKeyboard.UpdateRate;
            }
        }

        internal static void SetCharButtonsColor(LangSelector selector, ColorSetup colors)
        {
            var gamePad = Gamepad.current;
            if (gamePad != null)
            {
                if (gamePad is DualShockGamepad)
                {
                    if (selector.LangChars[0].CharPoint != null) selector.LangChars[0].CharPoint.color = colors.psColorX;
                    if (selector.LangChars[1].CharPoint != null) selector.LangChars[1].CharPoint.color = colors.psColorY;
                    if (selector.LangChars[2].CharPoint != null) selector.LangChars[2].CharPoint.color = colors.psColorB;
                    if (selector.LangChars[3].CharPoint != null) selector.LangChars[3].CharPoint.color = colors.psColorA;
                }
                else //(gamePad is XInputController)
                {
                    if (selector.LangChars[0].CharPoint != null) selector.LangChars[0].CharPoint.color = colors.colorX;
                    if (selector.LangChars[1].CharPoint != null) selector.LangChars[1].CharPoint.color = colors.colorY;
                    if (selector.LangChars[2].CharPoint != null) selector.LangChars[2].CharPoint.color = colors.colorB;
                    if (selector.LangChars[3].CharPoint != null) selector.LangChars[3].CharPoint.color = colors.colorA;
                }
            }
        }
    }
}