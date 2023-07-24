using System;
using UnityEditor;
using UnityEngine;

namespace VirtualGamepadKeyboard
{
    [CustomEditor(typeof(Language))]
    public class LanguageEditor : Editor
    {
        private Vector2 _scroll;
        private static float CharSelectorsDistance = 100.0f;
        private static float CharButtonsDistance = 20.0f;
        private static float InputSize = 20.0f;
        private static Vector2Int CenterCircle = new Vector2Int(250, 180);
        private static bool IsMainLayer = true;
        private static char[] DefaultChars = new char[]
        {
        'a', 'b', 'c', 'd',
        'e', 'f', 'g', 'h',
        'i', 'g', 'k', 'l',
        'm', 'n', 'o', 'p',
        'q', 'r', 's', 't',
        'u', 'v', 'w', 'x',
        '&', '`', 'z', 'y',
        '?', '!', '.', ','
        };

        private int _inspectorWidth = 100;
        private GUIContent _infoIcon;
        private GUIContent _editPinyin;
        private GUIContent _trashButton;
        private GUIContent _defaultButton;
        private void OnEnable()
        {
            _infoIcon = EditorGUIUtility.IconContent("console.infoicon.inactive.sml@2x");
            _infoIcon.text = "To change any parameter,\ndouble-click it.";

            _trashButton = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
            _defaultButton = EditorGUIUtility.IconContent("d_TreeEditor.Refresh");
            _editPinyin = EditorGUIUtility.IconContent("d_editicon.sml");
        }

        private int GetInspectorWidth()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            return (int)GUILayoutUtility.GetLastRect().width;
        }

        private void LayerButtons(Language val)
        {
            var buttonsStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            if (IsMainLayer) GUI.color = new Color(0.75f, 0.75f, 1.0f);
            else GUI.color = Color.white;
            if (GUI.Button(new Rect(CenterCircle.x - 101, 10, 100, 20), "Main", buttonsStyle)) IsMainLayer = true;
            if (!IsMainLayer) GUI.color = new Color(0.75f, 0.75f, 1.0f);
            else GUI.color = Color.white;
            if (GUI.Button(new Rect(CenterCircle.x + 1, 10, 100, 20), "Extended", buttonsStyle)) IsMainLayer = false;
            GUI.color = Color.white;

            if (GUI.Button(new Rect(_inspectorWidth - 28 - 10, 10, 28, 28), _trashButton))
            {
                if (EditorUtility.DisplayDialog(
                    "Clean the layer?",
                    "Do you really want to delete all the characters on this layer? " +
                    "Once removed, they will be replaced by spaces.",
                    "Yes", "No"))
                {
                    if (IsMainLayer) for (var i = 0; i < 8 * 4; i++) val.Chars[i] = ' ';
                    else for (var i = 0; i < 8 * 4; i++) val.CharsExtended[i] = ' ';
                }
            }
            if (GUI.Button(new Rect(_inspectorWidth - 28 - 10, 10 + 28, 28, 28), _defaultButton))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset to default?",
                    "Do you really want to return the layer to its default values?",
                    "Yes", "No"))
                {
                    if (IsMainLayer) Array.Copy(DefaultChars, val.Chars, 32);
                    else Array.Copy(DefaultChars, val.CharsExtended, 32);
                }
            }
            if (val.Pinyin) GUI.color = new Color(0.75f, 0.75f, 1.0f);
            else GUI.color = Color.white;
            if (GUI.Button(new Rect(_inspectorWidth - 28 - 10, 10 + 28 + 28, 28, 28), _editPinyin))
            {
                if (val.Pinyin == false) EditorUtility.DisplayDialog(
                    "Pinyin (Chinese)",
                    "Pinyin autocorrect is enabled for this layout.\n" +
                    "Auto-replacement is still under development!",
                    "Ok");
                val.Pinyin = !val.Pinyin;
            }
            GUI.color = Color.white;
        }

        public override void OnInspectorGUI()
        {
            var val = (Language)target;
            _inspectorWidth = GetInspectorWidth();
            CenterCircle.x = _inspectorWidth / 2;
            var inputTextStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 18,
                normal = new GUIStyleState()
                {
                    textColor = EditorGUIUtility.isProSkin ?
                        Color.white :
                        Color.black
                }
            };
            var infoTextStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState()
                {
                    textColor = EditorGUIUtility.isProSkin ?
                        Color.gray :
                        Color.gray
                }
            };
            if (val.Chars == null || val.Chars.Length < 8 * 4) val.Chars = new char[8 * 4];
            if (val.CharsExtended == null || val.CharsExtended.Length < 8 * 4) val.CharsExtended = new char[8 * 4];
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(480));

            LayerButtons(val);
            for (var i = 0; i < 8; i++)
            {
                var pos8 = new Vector2(
                    Mathf.Sin(Mathf.Deg2Rad * (i * -45.0f + 180.0f)),
                    Mathf.Cos(Mathf.Deg2Rad * (i * -45.0f + 180.0f))) * CharSelectorsDistance;

                Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.gray;
                Handles.DrawWireDisc(new Vector3(pos8.x + CenterCircle.x, pos8.y + CenterCircle.y, 0.0f), Vector3.forward, 36);
                Handles.DrawWireDisc(new Vector3(pos8.x + CenterCircle.x, pos8.y + CenterCircle.y, 0.0f), Vector3.forward, 35);

                for (var c = 0; c < 4; c++)
                {
                    var pos4 = new Vector2(
                        Mathf.Sin(Mathf.Deg2Rad * (c * -90.0f - 90.0f)),
                        Mathf.Cos(Mathf.Deg2Rad * (c * -90.0f - 90.0f))) * CharButtonsDistance;

                    Handles.color = EditorGUIUtility.isProSkin ? Color.gray : Color.gray;
                    Handles.DrawWireDisc(new Vector3(
                        pos8.x + CenterCircle.x + pos4.x,
                        pos8.y + CenterCircle.y + pos4.y, 0.0f), Vector3.forward, 13);

                    GUI.backgroundColor = Color.white;
                    if (IsMainLayer)
                    {
                        if (char.TryParse(GUI.TextField(
                            new Rect(
                                pos8.x + CenterCircle.x - InputSize / 2 + pos4.x,
                                pos8.y + CenterCircle.y - InputSize / 2 + pos4.y - 1,
                                InputSize, InputSize),
                            val.Chars[i * 4 + c].ToString(), inputTextStyle), out char newChar))
                        {
                            val.Chars[i * 4 + c] = char.ToLower(newChar);
                        }
                    }
                    else
                    {
                        if (char.TryParse(GUI.TextField(
                            new Rect(
                                pos8.x + CenterCircle.x - InputSize / 2 + pos4.x,
                                pos8.y + CenterCircle.y - InputSize / 2 + pos4.y - 1,
                                InputSize, InputSize),
                            val.CharsExtended[i * 4 + c].ToString(), inputTextStyle), out char newChar))
                        {
                            val.CharsExtended[i * 4 + c] = char.ToLower(newChar);
                        }
                    }
                }
            }
            val.Name = GUI.TextField(new Rect(
                CenterCircle.x - 50,
                CenterCircle.y - 10,
                100, 20), val.Name, inputTextStyle);

            GUI.Label(
                new Rect(CenterCircle.x - 100, CenterCircle.y * 2 - 10, 200, 40),
                _infoIcon,
                infoTextStyle);
            EditorGUILayout.EndScrollView();
            if (GUI.changed) EditorUtility.SetDirty(val);
        }
    }
}