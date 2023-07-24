//#define USE_TMP

#if USE_TMP
using TMPro;
#endif
using System.Linq;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Microsoft.International.Converters.PinYinConverter;

namespace VirtualGamepadKeyboard
{
    public class VirtualGamepadKeyboard : MonoBehaviour
    {
        [HideInInspector] public bool ActivedVirtualKeyBoard { get; private set; } = false;

        [Header("Scene references:")]
#if USE_TMP
        [SerializeField] private TMP_InputField input = null;
#else
    [SerializeField] private InputField input = null;
#endif
        [SerializeField] private Transform rootObject = null;
        [SerializeField] private AudioSource clickSound = null;

        [Header("Prefabs:")]
        [SerializeField] private GameObject charButton;
        [SerializeField] private GameObject circleLangSelector;
        [SerializeField] private GameObject keyboardSelector;

        [Header("Setup:")]
        [SerializeField] private Language[] languages;
        [SerializeField] private ColorSetup colors;
        [SerializeField] private float charButtonsDistance = 40.0f;
        [SerializeField] private float charSelectorsDistance = 240.0f;

        private Vector2 _stickMove;
        private char[] _currentKeyBoard;
        private Transform _rotationVector;
        private IEnumerator _updateCoroutine;
        private IEnumerator _keyboardSelectorCoroutine;
        private LangSelector[] _8dirSelectors = new LangSelector[8];
        private RectTransform[] _keyboardSelectors;
        internal static float UpdateRate = 1.0f / 45.0f;
        internal static WaitForSeconds UpdateRateWaiter = new WaitForSeconds(UpdateRate);

        private int _8dirSelectedPanel = -1;
        private int _keyBoardSelectedIndex = 0;

        private bool _extended = false;
        private bool _upperShift = false;

        private bool _buttonLeft = false;
        private float _buttonLeftTimer = 0.0f;

        private bool _buttonRight = false;
        private float _buttonRightTimer = 0.0f;

        private float _leftTriggerValue = 0.0f;
        private float _rightTriggerValue = 0.0f;

        private void OnEnable() => Show(input);
        private void OnDisable() => Hide();

        private bool _openedKeyboard = false;
#if USE_TMP
        public void SetInputField(TMP_InputField inputField) => input = inputField;
#else
    public void SetInputField(InputField inputField) => input = inputField;
#endif

        public void Switch(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            _openedKeyboard = !_openedKeyboard;
            if (_openedKeyboard) Show(null);
            else Hide();
        }

#if USE_TMP
        public void Show(TMP_InputField inputField)
#else
    public void Show(InputField inputField)
#endif
        {
            if (ActivedVirtualKeyBoard) return;
            if (!CheckReferences.CheckAllReferences(
                transform, languages, ref rootObject, charButton,
                circleLangSelector, keyboardSelector)) return;
            if (inputField == null)
            {
                if (input == null) return;
            }
            else input = inputField;
            ActivedVirtualKeyBoard = true;
            rootObject.gameObject.SetActive(true);
            if (languages.Length <= _keyBoardSelectedIndex) _keyBoardSelectedIndex = 0;
            _currentKeyBoard = languages[_keyBoardSelectedIndex].Chars;
            input.caretWidth = 2;
            input.caretBlinkRate = 3.0f;
            input.Select();
            input.caretPosition = input.text.Length;
            _rotationVector = new GameObject("LangSelectorVector").transform;
            _rotationVector.SetParent(rootObject);
            for (var i = 0; i < 8; i++)
            {
                var lang = Instantiate(circleLangSelector, rootObject);
                var rect = lang.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(
                    Mathf.Sin(Mathf.Deg2Rad * i * 45.0f),
                    Mathf.Cos(Mathf.Deg2Rad * i * 45.0f)) * charSelectorsDistance;
                var langSelector = new LangSelector()
                {
                    Index = i,
                    LangPanel = lang.GetComponent<Image>(),
                    LangChars = new LangChar[4]
                };
                for (var c = 0; c < 4; c++)
                {
                    var charInstance = Instantiate(charButton, lang.transform);
                    charInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                        Mathf.Sin(Mathf.Deg2Rad * (c * 90.0f - 90.0f)),
                        Mathf.Cos(Mathf.Deg2Rad * (c * 90.0f - 90.0f))) * charButtonsDistance;
                    var lChar = new LangChar()
                    {
                        Index = c,
                        CharPoint = charInstance.GetComponent<Image>(),
                        Selection = charInstance.transform.GetChild(0).GetComponent<Image>(),
#if USE_TMP
                        CharText = charInstance.transform.GetChild(1).GetComponent<TMP_Text>()
#else
                    CharText = charInstance.transform.GetChild(1).GetComponent<Text>()
#endif


                    };
                    lChar.Selection.color = colors.charUnselectedColor;
                    lChar.CharPoint.color = colors.charSelectedColor;
                    langSelector.LangChars[c] = lChar;
                }
                _8dirSelectors[i] = langSelector;
            }
            ChangeChars();
            _keyboardSelectors = new RectTransform[languages.Length];
            for (var i = 0; i < _keyboardSelectors.Length; i++)
            {
                var selector = Instantiate(keyboardSelector, rootObject);
                var rect = selector.GetComponent<RectTransform>();
#if USE_TMP
                selector.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = languages[i].Name;
#else
            selector.transform.GetChild(0).gameObject.GetComponent<Text>().text = languages[i].Name;
#endif
                _keyboardSelectors[i] = rect;
            }
            ChangeKeyboard.ChangeKeyboardSelector(
                this, ref _keyboardSelectorCoroutine, _keyBoardSelectedIndex, _keyboardSelectors,
                keyboardSelector.GetComponent<RectTransform>().sizeDelta);

            if (_updateCoroutine != null) StopCoroutine(_updateCoroutine);
            _updateCoroutine = UpdateCoroutine();
            StartCoroutine(_updateCoroutine);
        }
        public void Hide()
        {
            if (!ActivedVirtualKeyBoard) return;
            ActivedVirtualKeyBoard = false;
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
            if (_rotationVector != null) Destroy(_rotationVector.gameObject);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            for (var i = 0; i < 8; i++)
            {
                for (var c = 0; c < 4; c++)
                {
                    var langChar = _8dirSelectors[i].LangChars[c];
                    if (langChar.Selection != null) Destroy(langChar.Selection.gameObject);
                    if (langChar.CharText != null) Destroy(langChar.CharText.gameObject);
                    if (langChar.CharPoint != null) Destroy(langChar.CharPoint.gameObject);
                }
                if (_8dirSelectors[i].LangPanel != null) Destroy(_8dirSelectors[i].LangPanel.gameObject);
            }
            for (var i = 0; i < _keyboardSelectors.Length; i++)
                if (_keyboardSelectors[i] != null)
                    Destroy(_keyboardSelectors[i].gameObject);
            if (rootObject != null && rootObject.gameObject != null)
                rootObject.gameObject.SetActive(false);
            _extended = false;
            _upperShift = false;
            _buttonLeft = false;
            _buttonLeftTimer = 0.0f;
            _buttonRight = false;
            _buttonRightTimer = 0.0f;
            _leftTriggerValue = 0.0f;
            _rightTriggerValue = 0.0f;
            _8dirSelectedPanel = -1;
        }

        private IEnumerator UpdateCoroutine()
        {
            while (ActivedVirtualKeyBoard)
            {
                if (_buttonRight)
                {
                    _buttonRightTimer += UpdateRate;
                    if (_buttonRightTimer > 1.0f)
                    {
                        InsertChar(' ');
                        _buttonRightTimer = 0.9f;
                    }
                }
                if (_buttonLeft)
                {
                    _buttonLeftTimer += UpdateRate;
                    if (_buttonLeftTimer > 1.0f)
                    {
                        DeleteByCaret(Mathf.Clamp(Mathf.RoundToInt(_leftTriggerValue * 6.0f), 1, 6));
                        _buttonLeftTimer = 0.9f;
                    }
                }
                if (_stickMove.magnitude > 0.25f && _rotationVector != null)
                {
                    _rotationVector.rotation = Quaternion.LookRotation(new Vector3(_stickMove.x, 0.0f, _stickMove.y), Vector3.up);
                    _8dirSelectedPanel = Mathf.Clamp(Mathf.RoundToInt(_rotationVector.eulerAngles.y / 360.0f * 8.0f), 0, 7);
                    var langPanel = _8dirSelectors[_8dirSelectedPanel].LangPanel;
                    if (langPanel != null) langPanel.color = colors.langSelectorActiveColor;
                    CharButtons.SetCharButtonsColor(_8dirSelectors[_8dirSelectedPanel], colors);
                }
                else _8dirSelectedPanel = -1;

                for (var i = 0; i < _8dirSelectors.Length; i++)
                {
                    if (i == _8dirSelectedPanel) continue;
                    var langPanel = _8dirSelectors[i].LangPanel;
                    if (langPanel != null) langPanel.color = colors.langSelectorInactiveColor;
                    for (var c = 0; c < _8dirSelectors[i].LangChars.Length; c++)
                    {
                        var charPoint = _8dirSelectors[i].LangChars[c].CharPoint;
                        if (charPoint != null) charPoint.color = colors.charButtonInactiveColor;
                    }
                }
                yield return UpdateRateWaiter;
            }
        }

        private void SelectLang(Vector2Int vect)
        {
            if (_8dirSelectedPanel == -1) return;
            var index = 0;
            if (vect == Vector2Int.left) index = 0;
            if (vect == Vector2Int.up) index = 1;
            if (vect == Vector2Int.right) index = 2;
            if (vect == Vector2Int.down) index = 3;

            if (input == null)
            {
                Hide();
                return;
            }
            input.Select();
            char upperChar;
            if (_leftTriggerValue > 0.2f)
            {
                StartCoroutine(VibrateMotor(0.4f));
                upperChar = char.ToUpper(_currentKeyBoard[index + _8dirSelectedPanel * 4]);
            }
            else
            {
                StartCoroutine(VibrateMotor(0.1f));
                upperChar = char.ToLower(_currentKeyBoard[index + _8dirSelectedPanel * 4]);
            }
            InsertChar(upperChar);
            StartCoroutine(CharButtons.UpButtonFade(_8dirSelectors[_8dirSelectedPanel].LangChars[index], colors));
        }
        private void InsertChar(char charToInsert)
        {
            if (input == null)
            {
                Hide();
                return;
            }
            var selectedChar = new[] { charToInsert };
            var lastText = input.text;
            input.text = new string(
                lastText.Take(input.caretPosition)
                .Concat(selectedChar)
                .Concat(lastText.Skip(input.caretPosition)).ToArray());
            input.caretPosition++;

            if (languages[_keyBoardSelectedIndex].Pinyin)
            {
                var splited = input.text.Split(' ');
                if (splited.Length > 1)
                {
                    var sb = new StringBuilder();
                    for (var i = 0; i < splited.Length; i++)
                    {
                        if (ChineseChar.IsValidPinyin(splited[i])) sb.Append(ChineseChar.GetChars(splited[i]));
                        else sb.Append(splited[i]);
                    }
                    input.text = sb.ToString();
                }
            }
        }
        private void DeleteByCaret(int i)
        {
            if (input == null)
            {
                Hide();
                return;
            }
            var lastText = input.text;
            var lastCarretPos = input.caretPosition;
            input.text = new string(
                lastText.Take(Mathf.Clamp(lastCarretPos - i, 0, int.MaxValue))
                .Concat(lastText.Skip(lastCarretPos)).ToArray());
            input.caretPosition = lastCarretPos - i;
        }
        private void ChangeChars()
        {
            for (var i = 0; i < 8; i++)
                for (var c = 0; c < 4; c++)
                    //_8dirSelectors[i].LangChars[c].CharText.text = $"{i}{c}";
                    _8dirSelectors[i].LangChars[c].CharText.text = _upperShift ?
                        _currentKeyBoard[c + i * 4].ToString().ToUpper() :
                        _currentKeyBoard[c + i * 4].ToString().ToLower();
        }
        private void SwitchKeyboard()
        {
            if (_extended) _currentKeyBoard = languages[_keyBoardSelectedIndex].CharsExtended;
            else _currentKeyBoard = languages[_keyBoardSelectedIndex].Chars;
            StartCoroutine(VibrateMotor(0.4f));
            ChangeChars();
        }
        private IEnumerator VibrateMotor(float weight)
        {
            if (clickSound != null) clickSound.Play();
            if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0.0f, weight);
            yield return new WaitForSeconds(0.1f);
            if (Gamepad.current != null) Gamepad.current?.SetMotorSpeeds(0.0f, 0.0f);
        }
        public void DPadCursor(InputAction.CallbackContext context)
        {
            if (!ActivedVirtualKeyBoard) return;
            if (context.started || context.canceled) return;
            var vec = context.ReadValue<Vector2>();
            var intVect = new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));

            var lastKeyBoardSelected = _keyBoardSelectedIndex;
            if (intVect.y > 0) _keyBoardSelectedIndex--;
            else if (intVect.y < 0) _keyBoardSelectedIndex++;
            if (_keyBoardSelectedIndex >= languages.Length) _keyBoardSelectedIndex = 0;
            else if (_keyBoardSelectedIndex < 0) _keyBoardSelectedIndex = languages.Length - 1;
            if (lastKeyBoardSelected != _keyBoardSelectedIndex)
            {
                SwitchKeyboard();
                ChangeKeyboard.ChangeKeyboardSelector(
                    this, ref _keyboardSelectorCoroutine, _keyBoardSelectedIndex, _keyboardSelectors,
                    keyboardSelector.GetComponent<RectTransform>().sizeDelta);
            }

            if (input == null) return;
            input.Select();
            if (intVect.x > 0) input.caretPosition = input.caretPosition + 1 + Mathf.RoundToInt(6.0f * _leftTriggerValue);
            else if (intVect.x < 0) input.caretPosition = input.caretPosition - 1 - Mathf.RoundToInt(6.0f * _leftTriggerValue);
#if !USE_TMP
        input.ForceLabelUpdate();
#endif
        }
        public void ButtonLeft(InputAction.CallbackContext context)
        {
            if (!ActivedVirtualKeyBoard) return;
            if (context.performed) _buttonLeft = true;
            else if (context.canceled)
            {
                _buttonLeft = false;
                _buttonLeftTimer = 0.0f;
            }
            if (context.started || context.canceled) return;
            DeleteByCaret(Mathf.Clamp(Mathf.RoundToInt(_leftTriggerValue * 6.0f), 1, 6));
            StartCoroutine(VibrateMotor(0.4f));
        }
        public void ButtonRight(InputAction.CallbackContext context)
        {
            if (!ActivedVirtualKeyBoard) return;
            if (context.performed) _buttonRight = true;
            else if (context.canceled)
            {
                _buttonRight = false;
                _buttonRightTimer = 0.0f;
            }
            if (context.started || context.canceled) return;
            InsertChar(' ');
            StartCoroutine(VibrateMotor(0.4f));
        }
        public void TriggerLeft(InputAction.CallbackContext context)
        {
            if (!ActivedVirtualKeyBoard) return;
            _leftTriggerValue = context.ReadValue<float>();
            var lastUpperShift = _upperShift;
            _upperShift = Mathf.RoundToInt(_leftTriggerValue) == 1 ? true : false;
            if (lastUpperShift != _upperShift) SwitchKeyboard();
        }
        public void TriggerRight(InputAction.CallbackContext context)
        {
            if (!ActivedVirtualKeyBoard) return;
            _rightTriggerValue = context.ReadValue<float>();
            var lastExtended = _extended;
            _extended = Mathf.RoundToInt(_rightTriggerValue) == 1 ? true : false;
            if (lastExtended != _extended) SwitchKeyboard();
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!ActivedVirtualKeyBoard) return;
            _stickMove = context.ReadValue<Vector2>();
        }
        public void XYAB(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled || !ActivedVirtualKeyBoard) return;
            var v = context.ReadValue<Vector2>();
            SelectLang(new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y)));
        }
    }
}