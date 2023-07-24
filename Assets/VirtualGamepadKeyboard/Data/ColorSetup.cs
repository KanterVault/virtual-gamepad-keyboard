using System;
using UnityEngine;

namespace VirtualGamepadKeyboard
{
    [Serializable]
    public class ColorSetup
    {
        [Space(20)]
        public Color langSelectorInactiveColor;
        public Color langSelectorActiveColor;

        [Space(20)]
        public Color charButtonInactiveColor;
        public Color colorX;
        public Color colorY;
        public Color colorB;
        public Color colorA;

        public Color psColorX;
        public Color psColorY;
        public Color psColorB;
        public Color psColorA;

        [Space(20)]
        public Color charUnselectedColor;
        public Color charSelectedColor;
    }
}