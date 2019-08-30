using DG.Tweening;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UI.Constructor;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using TurnBased.Controllers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.UI
{
    public class CombatTrackerManager : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private RectTransform _body;
        private VerticalLayoutGroup _bodyLayoutGroup;
        private ButtonWrapper _buttonEndTurn;
        private ButtonWrapper _buttonDelay;
        private ButtonWrapper _buttonFiveFoorStep;
        private ButtonWrapper _buttonFullAttack;
        private RectTransform _unitButtons;
        private UnitButtonManager _unitButtonTemplate;

        private bool _enabled;
        private float _scale;
        private float _width;

        private Dictionary<UnitEntityData, UnitButtonManager> _unitButtonDic = new Dictionary<UnitEntityData, UnitButtonManager>();

        public UnitEntityData HoveringUnit { get; private set; }

        public static CombatTrackerManager CreateObject()
        {
            UICommon uiCommon = Game.Instance.UI.Common;
            GameObject hudLayout = uiCommon?.transform.Find("HUDLayout")?.gameObject;
            GameObject escMenuButtonBlock = uiCommon?.transform.Find("EscMenuWindow/Window/ButtonBlock")?.gameObject;

            if (!hudLayout || !escMenuButtonBlock)
                return null;

            // initialize window
            GameObject combatTracker = new GameObject("TurnBasedCombatTracker", typeof(RectTransform), typeof(CanvasGroup));
            combatTracker.transform.SetParent(hudLayout.transform);
            combatTracker.transform.SetSiblingIndex(0);

            RectTransform rectCombatTracker = (RectTransform)combatTracker.transform;
            rectCombatTracker.anchorMin = new Vector2(0f, 0f);
            rectCombatTracker.anchorMax = new Vector2(1f, 1f);
            rectCombatTracker.pivot = new Vector2(1f, 1f);
            rectCombatTracker.position = Camera.current.ScreenToWorldPoint
                (new Vector3(Screen.width, Screen.height, Camera.current.WorldToScreenPoint(hudLayout.transform.position).z));
            rectCombatTracker.position -= rectCombatTracker.forward;
            rectCombatTracker.rotation = Quaternion.identity;

            // initialize body
            GameObject body = Instantiate(escMenuButtonBlock, combatTracker.transform, false);
            body.name = "Body";

            Image imgBody = body.GetComponent<Image>();
            Image imgMetamagic = uiCommon.transform.Find("ServiceWindow/SpellBook/ContainerNoBook/Background")?.gameObject.GetComponent<Image>();
            if (imgMetamagic)
            {
                imgBody.sprite = imgMetamagic.sprite;
            }

            RectTransform rectBody = (RectTransform)body.transform;
            rectBody.anchorMin = new Vector2(1f, 1f);
            rectBody.anchorMax = new Vector2(1f, 1f);
            rectBody.pivot = new Vector2(1f, 1f);
            rectBody.localPosition = new Vector3(0f, 0f, 0f);
            rectBody.rotation = Quaternion.identity;

            ContentSizeFitter contentSizeFitter = body.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            VerticalLayoutGroup verticalLayoutGroup = body.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childAlignment = TextAnchor.UpperRight;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childForceExpandHeight = false;

            // initialize special action buttons
            GameObject actionButtons = new GameObject("SpecialActionButtons", typeof(RectTransform));

            RectTransform rectActionButtons = (RectTransform)actionButtons.transform;
            rectActionButtons.SetParent(rectBody, false);
            rectActionButtons.pivot = new Vector2(1f, 1f);
            rectActionButtons.sizeDelta = new Vector2(0f, UNIT_BUTTON_HEIGHT * 2);
            rectActionButtons.SetSiblingIndex(0);

            void SetActionButton(ButtonPF button, string name, float left, float buttom, float right, float top)
            {
                button.name = name;
                button.transform.SetParent(rectActionButtons, false);
                RectTransform rect = (RectTransform)button.transform;
                rect.anchoredPosition = new Vector2(0f, 0f);
                rect.anchorMin = new Vector2(left, buttom);
                rect.anchorMax = new Vector2(right, top);
                rect.pivot = new Vector2(1f, 1f);
                rect.rotation = Quaternion.identity;
                rect.sizeDelta = new Vector2(0f, 0f);
            }

            // ... end turn button
            ButtonPF buttonEndTurn = body.transform.Find("Btn_Quit").gameObject.GetComponent<ButtonPF>();
            SetActionButton(buttonEndTurn, "Button_EndTurn", 0.5f, 0.5f, 1f, 1f);

            // ... delay turn button
            ButtonPF buttonDelay = body.transform.Find("Btn_Save").gameObject.GetComponent<ButtonPF>();
            SetActionButton(buttonDelay, "Button_Delay", 0.5f, 0f, 1f, 0.5f);

            // ... 5-foot step button
            ButtonPF buttonFiveFoorStep = body.transform.Find("Btn_Load").gameObject.GetComponent<ButtonPF>();
            SetActionButton(buttonFiveFoorStep, "Button_FiveFootStep", 0f, 0f, 0.5f, 0.5f);

            // ... full-attack button
            ButtonPF buttonFullAttack = body.transform.Find("Btn_Options").gameObject.GetComponent<ButtonPF>();
            SetActionButton(buttonFullAttack, "Button_FullAttack", 0f, 0.5f, 0.5f, 1f);

            // initialize separator
            RectTransform rectSeparator = body.transform.Find("Separator") as RectTransform;
            rectSeparator.pivot = new Vector2(1f, 1f);
            rectSeparator.sizeDelta = new Vector2(0f, UNIT_BUTTON_SPACE);

            // clear unused buttons
            for (int i = body.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = body.transform.GetChild(i).gameObject;
                if (child.name != "SpecialActionButtons" && child.name != "Separator")
                {
                    child.SafeDestroy();
                }
            }

            // initialize button block (unit buttons)
            GameObject unitButtons = new GameObject("UnitButtons", typeof(RectTransform));

            RectTransform rectUnitButtons = (RectTransform)unitButtons.transform;
            rectUnitButtons.SetParent(rectBody, false);
            rectUnitButtons.pivot = new Vector2(1f, 1f);

            return combatTracker.AddComponent<CombatTrackerManager>();
        }

        void Awake()
        {
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;

            _body = (RectTransform)transform.Find("Body");
            _bodyLayoutGroup = _body.GetComponent<VerticalLayoutGroup>();

            _buttonEndTurn = new ButtonWrapper(
                _body.Find("SpecialActionButtons/Button_EndTurn").gameObject.GetComponent<ButtonPF>(),
                Local["UI_Btn_EndTurn"], HandleClickEndTurn);

            _buttonDelay = new ButtonWrapper(
                _body.Find("SpecialActionButtons/Button_Delay").gameObject.GetComponent<ButtonPF>(),
                Local["UI_Btn_Delay"], HandleClickDelay);

            _buttonFiveFoorStep = new ButtonWrapper(
                _body.Find("SpecialActionButtons/Button_FiveFootStep").gameObject.GetComponent<ButtonPF>(),
                Local["UI_Btn_FiveFootStep"], HandleClickFiveFootStep);

            _buttonFullAttack = new ButtonWrapper(
                _body.Find("SpecialActionButtons/Button_FullAttack").gameObject.GetComponent<ButtonPF>(),
                Local["UI_Btn_FullAttack"], HandleClickFullAttack);

            _unitButtons = (RectTransform)_body.Find("UnitButtons");
        }

        void OnEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            HotkeyHelper.Bind(HOTKEY_FOR_END_TURN, HandleClickEndTurn);
            HotkeyHelper.Bind(HOTKEY_FOR_DELAY, HandleClickDelay);
            HotkeyHelper.Bind(HOTKEY_FOR_FIVE_FOOT_STEP, HandleClickFiveFootStep);
            HotkeyHelper.Bind(HOTKEY_FOR_FULL_ATTACK, HandleClickFullAttack);
        }

        void OnDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            HotkeyHelper.Unbind(HOTKEY_FOR_END_TURN, HandleClickEndTurn);
            HotkeyHelper.Unbind(HOTKEY_FOR_DELAY, HandleClickDelay);
            HotkeyHelper.Unbind(HOTKEY_FOR_FIVE_FOOT_STEP, HandleClickFiveFootStep);
            HotkeyHelper.Unbind(HOTKEY_FOR_FULL_ATTACK, HandleClickFullAttack);

            ClearUnits();
            HoveringUnit = null;

            _canvasGroup.DOKill();
            _body.DOKill();
        }

        void OnDestroy()
        {
            _unitButtonTemplate.SafeDestroy();
        }

        void Update()
        {
            if (IsInCombat())
            {
                UpdateUnits();
                UpdateButtons();

                ResizeScale(CombatTrackerScale);
                ResizeWidth(CombatTrackerWidth);

                if (!_enabled)
                {
                    _enabled = true;
                    _canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
                    _body.DOAnchorPosY(0f, 0.5f, false).SetUpdate(true);
                }
            }
            else
            {
                if (_enabled)
                {
                    _enabled = false;
                    _canvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
                    _body.DOAnchorPosY(_body.rect.height, 0.5f, false).SetUpdate(true);
                    ClearUnits();
                    HoveringUnit = null;
                }
            }
        }

        private void HandleClickEndTurn()
        {
            CurrentTurn()?.CommandEndTurn();
        }

        private void HandleClickDelay()
        {
            _buttonDelay.IsPressed = !_buttonDelay.IsPressed;
        }

        private void HandleClickFiveFootStep()
        {
            CurrentTurn()?.CommandToggleFiveFootStep();
        }

        private void HandleClickFullAttack()
        {
            CurrentTurn()?.CommandToggleFullAttack();
        }

        private bool HandleClickUnitButton(UnitEntityData unit)
        {
            if (_buttonDelay.IsPressed)
            {
                CurrentTurn()?.CommandDelay(unit);
                _buttonDelay.IsPressed = false;
                return false;
            }
            else
            {
                return true;
            }
        }

        private void HandleEnterUnitButton(UnitEntityData unit)
        {
            HoveringUnit = unit;
        }

        private void HandleExitUnitButton(UnitEntityData unit)
        {
            if (HoveringUnit == unit)
            {
                HoveringUnit = null;
            }
        }

        private void UpdateButtons()
        {
            TurnController currentTurn = CurrentTurn();

            // end button
            _buttonEndTurn.IsInteractable = currentTurn != null && currentTurn.CanEndTurn();

            // delay button
            if (currentTurn != null && currentTurn.CanDelay())
            {
                _buttonDelay.IsInteractable = true;
            }
            else
            {
                _buttonDelay.IsInteractable = false;
                _buttonDelay.IsPressed = false;
            }

            // 5-foot step button
            if (currentTurn != null)
            {
                _buttonFiveFoorStep.IsInteractable = currentTurn.CanToggleFiveFootStep();
                _buttonFiveFoorStep.IsPressed = currentTurn.EnabledFiveFootStep;
            }
            else
            {
                _buttonFiveFoorStep.IsInteractable = false;
                _buttonFiveFoorStep.IsPressed = false;
            }

            // full attack button
            if (currentTurn != null)
            {
                _buttonFullAttack.IsInteractable = currentTurn.CanToggleFullAttack();
                _buttonFullAttack.IsPressed = currentTurn.EnabledFullAttack;
            }
            else
            {
                _buttonFullAttack.IsInteractable = false;
                _buttonFullAttack.IsPressed = false;
            }
        }

        private void UpdateUnits()
        {
            UnitEntityData currentUnit = CurrentUnit();
            int oldCount = _unitButtonDic.Count;
            int newCount = 0;
            List<UnitButtonManager> newUnitButtons = new List<UnitButtonManager>();
            bool isDirty = false;

            // renew elements
            foreach (UnitEntityData unit in Mod.Core.Combat.SortedUnits)
            {
                if (newCount >= CombatTrackerMaxUnits)
                {
                    break;
                }

                if (!DoNotShowInvisibleUnitOnCombatTracker || unit.IsVisibleForPlayer || unit == currentUnit)
                {
                    newUnitButtons.Add(EnsureUnit(unit, newCount++, ref isDirty));
                }
            }

            if (newCount != oldCount)
            {
                // window size should have changed
                ResizeUnits(newCount);
                isDirty = true;
            }

            if (isDirty)
            {
                // remove disabled button
                foreach (UnitButtonManager button in _unitButtonDic.Values.Except(newUnitButtons).ToList())
                {
                    RemoveUnit(button);
                }

                // do move
                foreach (UnitButtonManager button in _unitButtonDic.Values.OrderBy(button => button.Index))
                {
                    button.transform.SetSiblingIndex(button.Index);
                    button.transform.DOLocalMoveY(-(UNIT_BUTTON_HEIGHT + UNIT_BUTTON_SPACE) * button.Index, 1f).SetUpdate(true);
                }
            }
        }

        private void ClearUnits()
        {
            foreach (UnitButtonManager unitButton in _unitButtonDic.Values)
            {
                unitButton.SafeDestroy();
            }
            _unitButtonDic.Clear();
        }

        private UnitButtonManager EnsureUnit(UnitEntityData unit, int index, ref bool isDirty)
        {
            if (!_unitButtonDic.TryGetValue(unit, out UnitButtonManager button))
            {
                if (!_unitButtonTemplate)
                {
                    _unitButtonTemplate = UnitButtonManager.CreateObject();
                    _unitButtonTemplate.gameObject.SetActive(false);
                    DontDestroyOnLoad(_unitButtonTemplate.gameObject);
                }

                button = Instantiate(_unitButtonTemplate);
                button.transform.SetParent(_unitButtons.transform, false);
                button.transform.localPosition = new Vector3(0f, -(UNIT_BUTTON_HEIGHT + UNIT_BUTTON_SPACE) * index, 0f);
                button.gameObject.SetActive(true);
                button.Index = index;
                button.Unit = unit;
                button.OnClick += HandleClickUnitButton;
                button.OnEnter += HandleEnterUnitButton;
                button.OnExit += HandleExitUnitButton;

                _unitButtonDic.Add(unit, button);
                isDirty = true;
            }
            else if (button.Index != index)
            {
                button.Index = index;
                isDirty = true;
            }
            return button;
        }

        private void RemoveUnit(UnitButtonManager unitButton)
        {
            _unitButtonDic.Remove(unitButton.Unit);
            unitButton.SafeDestroy();
        }

        private void ResizeScale(float scale)
        {
            if (_scale != scale)
            {
                _scale = scale;
                _body.localScale = new Vector3(scale, scale, scale);
            }
        }

        private void ResizeWidth(float width)
        {
            if (_width != width)
            {
                _width = width;
                _body.sizeDelta = new Vector2(width, _body.sizeDelta.y);
                SetPadding((int)(width * PADDING_X_PERCENT / 2f), _bodyLayoutGroup.padding.top);
            }
        }

        private void ResizeUnits(int unitsCount)
        {
            float height = (UNIT_BUTTON_HEIGHT + UNIT_BUTTON_SPACE) * unitsCount - UNIT_BUTTON_SPACE;
            _unitButtons.sizeDelta = new Vector2(_unitButtons.sizeDelta.x, height);
            SetPadding(
                _bodyLayoutGroup.padding.right, 
                (int)((UNIT_BUTTON_HEIGHT * 2 + UNIT_BUTTON_SPACE + height) * PADDING_Y_PERCENT / (1f - PADDING_Y_PERCENT) / 2f));
        }

        private void SetPadding(int x, int y)
        {
            _bodyLayoutGroup.padding = new RectOffset(x, x, y, y);
        }

        private class ButtonWrapper
        {
            private bool _isPressed;

            private readonly Color _enableColor = Color.white;
            private readonly Color _disableColor = new Color(0.7f, 0.8f, 1f);

            private readonly ButtonPF _button;
            private readonly TextMeshProUGUI _textMesh;
            private readonly Image _image;
            private readonly Sprite _defaultSprite;
            private readonly SpriteState _defaultSpriteState;
            private readonly SpriteState _pressedSpriteState;

            public bool IsInteractable {
                get => _button.interactable;
                set {
                    if (_button.interactable != value)
                    {
                        _button.interactable = value;
                        _textMesh.color = value ? _enableColor : _disableColor;
                    }
                }
            }

            public bool IsPressed {
                get => _isPressed;
                set {
                    if (_isPressed != value)
                    {
                        _isPressed = value;
                        if (value)
                        {
                            _button.spriteState = _pressedSpriteState;
                            _image.sprite = _pressedSpriteState.pressedSprite;
                        }
                        else
                        {
                            _button.spriteState = _defaultSpriteState;
                            _image.sprite = _defaultSprite;
                        }
                    }
                }
            }

            public ButtonWrapper(ButtonPF button, string text, Action onClick)
            {
                _button = button;
                _button.onClick = new Button.ButtonClickedEvent();
                _button.onClick.AddListener(new UnityAction(onClick));
                _textMesh = _button.GetComponentInChildren<TextMeshProUGUI>();
                _textMesh.fontSize = 20;
                _textMesh.fontSizeMax = 72;
                _textMesh.fontSizeMin = 18;
                _textMesh.text = text;
                _textMesh.color = _button.interactable ? _enableColor : _disableColor;
                _image = _button.gameObject.GetComponent<Image>();
                _defaultSprite = _image.sprite;
                _defaultSpriteState = _button.spriteState;
                _pressedSpriteState = _defaultSpriteState;
                _pressedSpriteState.disabledSprite = _pressedSpriteState.pressedSprite;
                _pressedSpriteState.highlightedSprite = _pressedSpriteState.pressedSprite;
            }
        }
    }
}