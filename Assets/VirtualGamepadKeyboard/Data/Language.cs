using System;
using UnityEngine;

namespace VirtualGamepadKeyboard
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewLanguage.asset", menuName = "Language asset")]
    public class Language : ScriptableObject
    {
        public string Name = "New Layer";
        public char[] Chars = new char[8 * 4];
        public char[] CharsExtended = new char[8 * 4];
        public bool Pinyin = false;
    }
}