using UnityEngine;

namespace VirtualGamepadKeyboard
{
    internal static class CheckReferences
    {
        internal static bool CheckAllReferences(
            Transform selfTransform, Language[] languages, ref Transform rootObject, GameObject charButton,
            GameObject circleLangSelector, GameObject keyboardSelector)
        {
            if (languages.Length < 1)
            {
                Debug.LogWarning("VirtualGamepadKeyboard: Language list is empty!");
                return false;
            }
            if (rootObject == null)
            {
                Debug.LogWarning("VirtualGamepadKeyboard: Root Object is null! A fallback root object will be created.");
                var gm = new GameObject("FallBackRootObject");
                gm.AddComponent<RectTransform>();
                gm.transform.SetParent(selfTransform, false);
                rootObject = gm.transform;
            }
            if (charButton == null)
            {
                Debug.LogWarning("VirtualGamepadKeyboard: Char Button is null!");
                return false;
            }
            if (circleLangSelector == null)
            {
                Debug.LogWarning("VirtualGamepadKeyboard: Circle Lang Selector is null!");
                return false;
            }
            if (keyboardSelector == null)
            {
                Debug.LogWarning("VirtualGamepadKeyboard: Keyboard Selector is null!");
                return false;
            }
            return true;
        }
    }
}