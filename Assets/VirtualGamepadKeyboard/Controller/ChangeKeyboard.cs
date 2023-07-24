using UnityEngine;
using System.Collections;

namespace VirtualGamepadKeyboard
{
    internal static class ChangeKeyboard
    {
        internal static void ChangeKeyboardSelector(
            MonoBehaviour self,
            ref IEnumerator current,
            int _keyBoardSelectedIndex,
            RectTransform[] _keyboardSelectors,
            Vector2 size)
        {
            if (current != null) self.StopCoroutine(current);
            current = ChangeKeyboardSelectorCoroutine(_keyBoardSelectedIndex, _keyboardSelectors, size);
            self.StartCoroutine(current);
        }

        internal static IEnumerator ChangeKeyboardSelectorCoroutine(
            int _keyBoardSelectedIndex,
            RectTransform[] _keyboardSelectors,
            Vector2 size)
        {
            var timer = 0.0f;
            while (true)
            {
                for (var i = 0; i < _keyboardSelectors.Length; i++)
                {
                    var sel = _keyboardSelectors[i];
                    if (sel == null) yield break;
                    sel.anchoredPosition = Vector2.Lerp(
                        sel.anchoredPosition,
                        new Vector2(0.0f, (size.y + 4.0f) * -(i - _keyBoardSelectedIndex)),
                        14.0f * VirtualGamepadKeyboard.UpdateRate);

                    if (i == _keyBoardSelectedIndex) KeyboardSizeLerp(sel, size, 8.0f * VirtualGamepadKeyboard.UpdateRate);
                    else KeyboardSizeLerp(sel, size - Vector2.right * 20.0f, 8.0f * VirtualGamepadKeyboard.UpdateRate);
                    sel.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f - Vector2.Distance(Vector2.zero, sel.anchoredPosition * 1.0f / (size * 1.3f));
                }
                timer += VirtualGamepadKeyboard.UpdateRate;
                if (timer > 0.75f)
                {
                    for (var i = 0; i < _keyboardSelectors.Length; i++)
                    {
                        var sel = _keyboardSelectors[i];
                        if (sel == null) yield break;
                        sel.anchoredPosition = new Vector2(0.0f, (size.y + 4.0f) * -(i - _keyBoardSelectedIndex));
                        if (i == _keyBoardSelectedIndex) KeyboardSizeLerp(sel, size, 1.0f);
                        else KeyboardSizeLerp(sel, size - Vector2.right * 20.0f, 1.0f);
                        sel.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f - Vector2.Distance(Vector2.zero, sel.anchoredPosition * 1.0f / (size * 1.3f));
                    }
                    yield break;
                }
                yield return VirtualGamepadKeyboard.UpdateRateWaiter;
            }
        }

        internal static void KeyboardSizeLerp(RectTransform sel, Vector2 size, float lerpTime)
        {
            sel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Mathf.Lerp(sel.sizeDelta.x, size.x, lerpTime));

            sel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Lerp(sel.sizeDelta.y, size.y, lerpTime));
        }
    }
}