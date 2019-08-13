using DG.Tweening;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UI.Constructor;
using ModMaker.Utility;
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
        private RectTransform _buttonBlock;
        private VerticalLayoutGroup _buttonBlockLayoutGroup;
        private ButtonPF _buttonEndTurn;
        private ButtonPF _buttonFiveFoorStep;
        private ButtonPF _buttonDelay;
        private TextMeshProUGUI _buttonEndTurnText;
        private TextMeshProUGUI _buttonFiveFoorStepText;
        private TextMeshProUGUI _buttonDelayText;
        private Image _buttonFiveFoorStepImage;
        private Image _buttonDelayImage;
        private Sprite _buttonNormalSprite;
        private SpriteState _buttonNormalSpriteState;
        private RectTransform _unitButtonBlock;
        private UnitButtonManager _unitButtonTemplate;

        private bool _enabled;
        private float _scale;
        private float _width;

        private bool _toggledFiveFoorStep;
        private bool _toggledDelay;
        private Dictionary<UnitEntityData, UnitButtonManager> _unitButtonDic = new Dictionary<UnitEntityData, UnitButtonManager>();

        public UnitEntityData HoveringUnit { get; private set; }

        void Awake()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;

            _buttonBlock = (RectTransform)transform.Find("ButtonBlock");
            _buttonBlock.anchoredPosition = new Vector2(0f, _buttonBlock.rect.height);
            _buttonBlockLayoutGroup = _buttonBlock.GetComponent<VerticalLayoutGroup>();

            _buttonEndTurn = _buttonBlock.Find("Btn_EndTurn").gameObject.GetComponent<ButtonPF>();
            _buttonEndTurn.onClick.AddListener(new UnityAction(HandleClickEndTurn));

            _buttonFiveFoorStep = _buttonBlock.Find("SpecialActionButtons/Btn_FiveFootStep").gameObject.GetComponent<ButtonPF>();
            _buttonFiveFoorStep.onClick.AddListener(new UnityAction(HandleClickFiveFootStep));

            _buttonDelay = _buttonBlock.Find("SpecialActionButtons/Btn_Delay").gameObject.GetComponent<ButtonPF>();
            _buttonDelay.onClick.AddListener(new UnityAction(HandleClickDelay));

            _buttonEndTurnText = _buttonEndTurn.GetComponentInChildren<TextMeshProUGUI>();
            _buttonFiveFoorStepText = _buttonFiveFoorStep.GetComponentInChildren<TextMeshProUGUI>();
            _buttonDelayText = _buttonDelay.GetComponentInChildren<TextMeshProUGUI>();

            _buttonFiveFoorStepImage = _buttonFiveFoorStep.gameObject.GetComponent<Image>();
            _buttonDelayImage = _buttonDelay.gameObject.GetComponent<Image>();
            _buttonNormalSprite = _buttonDelayImage.sprite;
            _buttonNormalSpriteState = _buttonDelay.spriteState;

            _unitButtonBlock = (RectTransform)_buttonBlock.Find("UnitButtonBlock");

            HotkeyHelper.Bind(HOTKEY_FOR_FIVE_FOOT_STEP, HandleClickFiveFootStep);
            HotkeyHelper.Bind(HOTKEY_FOR_DELAY, HandleClickDelay);
            HotkeyHelper.Bind(HOTKEY_FOR_END_TURN, HandleClickEndTurn);
        }

        void OnDestroy()
        {
            HotkeyHelper.Unbind(HOTKEY_FOR_FIVE_FOOT_STEP, HandleClickFiveFootStep);
            HotkeyHelper.Unbind(HOTKEY_FOR_DELAY, HandleClickDelay);
            HotkeyHelper.Unbind(HOTKEY_FOR_END_TURN, HandleClickEndTurn);

            if (!_unitButtonTemplate.IsNullOrDestroyed())
                Destroy(_unitButtonTemplate.gameObject);

            HoveringUnit = null;
            ClearUnits();

            _canvasGroup.DOKill();
            _buttonBlock.DOKill();
        }

        void Update()
        {
            if (IsInCombat())
            {
                CombatController roundController = Mod.Core.Combat;

                UpdateUnits(roundController.GetSortedUnits());
                UpdateButtons(roundController.CurrentTurn);

                ResizeScale(CombatTrackerScale);
                ResizeWidth(CombatTrackerWidth);

                if (!_enabled)
                {
                    _enabled = true;
                    _canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
                    _buttonBlock.DOAnchorPosY(0f, 0.5f, false).SetUpdate(true);
                }
            }
            else
            {
                if (_enabled)
                {
                    _enabled = false;
                    _canvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
                    _buttonBlock.DOAnchorPosY(_buttonBlock.rect.height, 0.5f, false).SetUpdate(true);
                    ClearUnits();
                }
            }
        }

        public static CombatTrackerManager CreateObject()
        {
            UICommon uiCommon = Game.Instance.UI.Common;
            GameObject hudLayout = uiCommon?.transform.Find("HUDLayout")?.gameObject;
            GameObject escMenuButtonBlock = uiCommon?.transform.Find("EscMenuWindow/Window/ButtonBlock")?.gameObject;

            if (hudLayout.IsNullOrDestroyed() || escMenuButtonBlock.IsNullOrDestroyed())
                return null;

            // initialize window
            GameObject tbCombatTracker = new GameObject("TurnBasedCombatTracker", typeof(RectTransform), typeof(CanvasGroup));
            tbCombatTracker.transform.SetParent(hudLayout.transform);
            tbCombatTracker.transform.SetSiblingIndex(0);

            RectTransform rectCombatTracker = (RectTransform)tbCombatTracker.transform;
            rectCombatTracker.anchorMin = new Vector2(0f, 0f);
            rectCombatTracker.anchorMax = new Vector2(1f, 1f);
            rectCombatTracker.pivot = new Vector2(1f, 1f);
            rectCombatTracker.position = Camera.current.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height,
                Camera.current.WorldToScreenPoint(hudLayout.transform.position).z));
            rectCombatTracker.position = rectCombatTracker.position - rectCombatTracker.forward;
            rectCombatTracker.rotation = Quaternion.identity;

            // initialize button block
            GameObject tbButtonBlock = Instantiate(escMenuButtonBlock, tbCombatTracker.transform, false);
            tbButtonBlock.name = "ButtonBlock";

            Image bigbackbutton = tbButtonBlock.GetComponent<Image>();
            Image metamagicback = uiCommon?.transform.Find("ServiceWindow/SpellBook/ContainerNoBook/Background")?.gameObject.GetComponent<Image>();

            if (metamagicback != null)
                bigbackbutton.sprite = metamagicback.sprite;

            RectTransform rectButtonBlock = (RectTransform)tbButtonBlock.transform;
            rectButtonBlock.anchorMin = new Vector2(1f, 1f);
            rectButtonBlock.anchorMax = new Vector2(1f, 1f);
            rectButtonBlock.pivot = new Vector2(1f, 1f);
            rectButtonBlock.localPosition = new Vector3(0f, 0f, 0f);
            rectButtonBlock.rotation = Quaternion.identity;

            ContentSizeFitter contentSizeFitter = tbButtonBlock.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            VerticalLayoutGroup verticalLayoutGroup = tbButtonBlock.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childAlignment = TextAnchor.UpperRight;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childForceExpandHeight = false;

            // initialize end turn button
            ButtonPF tbButtonEndTurn = tbButtonBlock.transform.Find("Btn_Quit").gameObject.GetComponent<ButtonPF>();
            tbButtonEndTurn.name = "Btn_EndTurn";
            tbButtonEndTurn.onClick = new Button.ButtonClickedEvent();
            tbButtonEndTurn.GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";

            RectTransform rectButtonEndTurn = (RectTransform)tbButtonEndTurn.transform;
            rectButtonEndTurn.pivot = new Vector2(1f, 1f);
            rectButtonEndTurn.sizeDelta = new Vector2(0f, UNIT_BUTTON_HEIGHT);
            rectButtonEndTurn.SetSiblingIndex(0);

            // initialize special action buttons
            GameObject tbSpecialButtons = new GameObject("SpecialActionButtons", typeof(RectTransform));

            RectTransform rectSpecialButtons = (RectTransform)tbSpecialButtons.transform;
            rectSpecialButtons.SetParent(rectButtonBlock, false);
            rectSpecialButtons.pivot = new Vector2(1f, 1f);
            rectSpecialButtons.sizeDelta = new Vector2(0f, UNIT_BUTTON_HEIGHT);
            rectSpecialButtons.SetSiblingIndex(1);

            // initialize 5-foot step button
            ButtonPF tbButtonFFT = Instantiate(tbButtonBlock.transform.Find("Btn_Save").gameObject.GetComponent<ButtonPF>(), rectSpecialButtons, false);
            tbButtonFFT.name = "Btn_FiveFootStep";
            tbButtonFFT.onClick = new Button.ButtonClickedEvent();
            tbButtonFFT.GetComponentInChildren<TextMeshProUGUI>().text = "5-F. STEP";

            RectTransform rectButtonFFT = (RectTransform)tbButtonFFT.transform;
            rectButtonFFT.anchorMin = new Vector2(0f, 0f);
            rectButtonFFT.anchorMax = new Vector2(0.5f, 1f);
            rectButtonFFT.pivot = new Vector2(1f, 1f);
            rectButtonFFT.localPosition = new Vector3(0f, 0f, 0f);
            rectButtonFFT.rotation = Quaternion.identity;
            rectButtonFFT.sizeDelta = new Vector2(0f, 0f);

            // initialize delay turn button
            ButtonPF tbButtonDelay = Instantiate(tbButtonBlock.transform.Find("Btn_Save").gameObject.GetComponent<ButtonPF>(), rectSpecialButtons, false);
            tbButtonDelay.name = "Btn_Delay";
            tbButtonDelay.onClick = new Button.ButtonClickedEvent();
            tbButtonDelay.GetComponentInChildren<TextMeshProUGUI>().text = "Delay";

            RectTransform rectButtonDelay = (RectTransform)tbButtonDelay.transform;
            rectButtonDelay.anchorMin = new Vector2(0.5f, 0f);
            rectButtonDelay.anchorMax = new Vector2(1f, 1f);
            rectButtonDelay.pivot = new Vector2(1f, 1f);
            rectButtonDelay.localPosition = new Vector3(0f, 0f, 0f);
            rectButtonDelay.rotation = Quaternion.identity;
            rectButtonDelay.sizeDelta = new Vector2(0f, 0f);

            // initialize separator
            RectTransform rectSeparator = tbButtonBlock.transform.Find("Separator") as RectTransform;
            rectSeparator.pivot = new Vector2(1f, 1f);
            rectSeparator.sizeDelta = new Vector2(0f, UNIT_BUTTON_SPACE);

            // clear unused buttons
            for (int i = 0; i < tbButtonBlock.transform.childCount; i++)
            {
                GameObject child = tbButtonBlock.transform.GetChild(i).gameObject;
                if (child.name != "Btn_EndTurn" && child.name != "SpecialActionButtons" && child.name != "Separator")
                {
                    child.transform.SetParent(null, false);
                    Destroy(child);
                    i--;
                }
            }

            // initialize button block (unit buttons)
            GameObject tbUnitButtonBlock = new GameObject("UnitButtonBlock", typeof(RectTransform));

            RectTransform rectUnitButtonBlock = (RectTransform)tbUnitButtonBlock.transform;
            rectUnitButtonBlock.SetParent(rectButtonBlock, false);
            rectUnitButtonBlock.pivot = new Vector2(1f, 1f);

            return tbCombatTracker.AddComponent<CombatTrackerManager>();
        }

        private void HandleClickFiveFootStep()
        {
            Mod.Core.Combat?.CurrentTurn?.CommandToggleFiveFootStep();
        }

        private void HandleClickDelay()
        {
            ToggleDelay(!_toggledDelay);
        }

        private void HandleClickEndTurn()
        {
            Mod.Core.Combat?.CurrentTurn?.CommandEndTurn();
        }

        private bool HandleClickUnitButton(UnitEntityData unit)
        {
            if (_toggledDelay && unit != null)
            {
                Mod.Core.Combat?.CurrentTurn?.CommandDelay(unit);
                ToggleDelay(false);
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

        private void ToggleFiveFootStep(bool toggle)
        {
            if (_toggledFiveFoorStep != toggle)
            {
                if (toggle)
                {
                    SpriteState spriteState = _buttonFiveFoorStep.spriteState;
                    spriteState.highlightedSprite = spriteState.pressedSprite;
                    _buttonFiveFoorStep.spriteState = spriteState;
                    _buttonFiveFoorStepImage.sprite = spriteState.pressedSprite;
                }
                else
                {
                    _buttonFiveFoorStep.spriteState = _buttonNormalSpriteState;
                    _buttonFiveFoorStepImage.sprite = _buttonNormalSprite;
                }
                _toggledFiveFoorStep = toggle;
            }
        }

        private void ToggleDelay(bool toggle)
        {
            if (_toggledDelay != toggle)
            {
                if (toggle)
                {
                    SpriteState spriteState = _buttonDelay.spriteState;
                    spriteState.highlightedSprite = spriteState.pressedSprite;
                    _buttonDelay.spriteState = spriteState;
                    _buttonDelayImage.sprite = spriteState.pressedSprite;
                }
                else
                {
                    _buttonDelay.spriteState = _buttonNormalSpriteState;
                    _buttonDelayImage.sprite = _buttonNormalSprite;
                }
                _toggledDelay = toggle;
            }
        }

        private void UpdateButtons(TurnController currentTurn)
        {
            UnitEntityData currentUnit = currentTurn?.Unit;

            // 5-foot step button
            if (currentUnit != null && currentTurn.CanToggleFiveFootStep())
            {
                if (!_buttonFiveFoorStep.interactable)
                {
                    _buttonFiveFoorStep.interactable = true;
                }
                ToggleFiveFootStep(currentTurn.EnabledFiveFootStep);
            }
            else
            {
                if (_buttonFiveFoorStep.interactable)
                {
                    _buttonFiveFoorStep.interactable = false;
                    ToggleFiveFootStep(false);
                }
            }

            // delay button
            if (currentUnit != null && currentTurn.CanDelay())
            {
                if (!_buttonDelay.interactable)
                {
                    _buttonDelay.interactable = true;
                    ToggleDelay(false);
                }
            }
            else
            {
                if (_buttonDelay.interactable)
                {
                    _buttonDelay.interactable = false;
                    ToggleDelay(false);
                }
            }

            // end button
            if (currentUnit != null && currentTurn.CanEndTurn())
            {
                if (!_buttonEndTurn.interactable)
                {
                    _buttonEndTurn.interactable = true;
                }
            }
            else
            {
                if (_buttonEndTurn.interactable)
                {
                    _buttonEndTurn.interactable = false;
                }
            }

            UpdateButtonsColor();
        }

        private void UpdateButtonsColor()
        {
            Color enable = Color.white;
            Color disable = new Color(0.7f, 0.8f, 1f);
            _buttonEndTurnText.color = _buttonEndTurn.interactable ? enable : disable;
            _buttonFiveFoorStepText.color = _buttonFiveFoorStep.interactable ? enable : disable;
            _buttonDelayText.color = _buttonDelay.interactable ? enable : disable;
        }

        private void UpdateUnits(IEnumerable<UnitEntityData> units)
        {
            UnitEntityData currentUnit = Mod.Core.Combat.CurrentTurn?.Unit;
            bool isDirty = false;
            List<UnitButtonManager> newUnits = new List<UnitButtonManager>();

            int oldCount = _unitButtonDic.Count;
            int newCount = 0;

            // renew elements
            foreach (UnitEntityData unit in units)
            {
                if (newCount >= CombatTrackerMaxUnits)
                {
                    break;
                }

                if (!DoNotShowInvisibleUnitOnCombatTracker || unit.IsVisibleForPlayer || unit == currentUnit)
                {
                    newUnits.Add(EnsureUnit(unit, newCount, ref isDirty));
                    newCount++;
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
                foreach (UnitButtonManager button in _unitButtonDic.Values.Except(newUnits).ToList())
                {
                    RemoveUnit(button);
                }

                // do move
                foreach (UnitButtonManager button in _unitButtonDic.Values.OrderBy(button => button.Index))
                {
                    button.transform.SetSiblingIndex(2 + button.Index);
                    button.transform.DOLocalMoveY(-(UNIT_BUTTON_HEIGHT + UNIT_BUTTON_SPACE) * button.Index, 1f).SetUpdate(true);
                }
            }
        }

        private void ClearUnits()
        {
            foreach (UnitButtonManager unitButton in _unitButtonDic.Values.ToList())
            {
                RemoveUnit(unitButton);
            }
        }

        private UnitButtonManager EnsureUnit(UnitEntityData unit, int index, ref bool isDirty)
        {
            if (!_unitButtonDic.TryGetValue(unit, out UnitButtonManager button))
            {
                if (_unitButtonTemplate.IsNullOrDestroyed())
                {
                    _unitButtonTemplate = UnitButtonManager.CreateObject();
                    _unitButtonTemplate.gameObject.SetActive(false);
                    DontDestroyOnLoad(_unitButtonTemplate.gameObject);
                }

                button = UnitButtonManager.CreateObject(_unitButtonTemplate);
                button.transform.SetParent(_unitButtonBlock.transform, false);
                ((RectTransform)button.transform).localPosition = 
                    new Vector3(0f, -(UNIT_BUTTON_HEIGHT + UNIT_BUTTON_SPACE) * index, 0f);
                button.gameObject.SetActive(true);
                button.Index = index;
                button.SetUnit(unit);
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
            unitButton.transform.SetParent(null, false);
            Destroy(unitButton.gameObject);
        }

        private void ResizeScale(float scale)
        {
            if (_scale != scale)
            {
                _scale = scale;
                _buttonBlock.localScale = new Vector3(scale, scale, scale);
            }
        }

        private void ResizeWidth(float width)
        {
            if (_width != width)
            {
                _width = width;
                _buttonBlock.sizeDelta = new Vector2(width, _buttonBlock.sizeDelta.y);
                SetPadding(
                    (int)(width * DEFAULT_BLOCK_PADDING.x / DEFAULT_BLOCK_SIZE.x / 2f),
                    _buttonBlockLayoutGroup.padding.top);
            }
        }

        private void ResizeUnits(int unitsCount)
        {
            float height = (UNIT_BUTTON_HEIGHT + UNIT_BUTTON_SPACE) * unitsCount - UNIT_BUTTON_SPACE;
            _unitButtonBlock.sizeDelta = new Vector2(_unitButtonBlock.sizeDelta.x, height);
            SetPadding(
                _buttonBlockLayoutGroup.padding.right, 
                (int)((height + UNIT_BUTTON_HEIGHT * 2 + UNIT_BUTTON_SPACE) * 
                DEFAULT_BLOCK_PADDING.y / (DEFAULT_BLOCK_SIZE.y - DEFAULT_BLOCK_PADDING.y) / 2f));
        }

        private void SetPadding(int x, int y)
        {
            _buttonBlockLayoutGroup.padding = new RectOffset(x, x, y, y);
        }
    }
}
