using Kingmaker;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Constructor;
using Kingmaker.UI.Journal;
using Kingmaker.UI.Overtip;
using Kingmaker.View;
using ModMaker.Utility;
using System;
using System.Linq;
using TMPro;
using TurnBased.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.UI
{
    public class UnitButtonManager : 
        MonoBehaviour,
        IUnitDirectHoverUIHandler
    {
        private ButtonPF _button;
        private TextMeshProUGUI _label;
        private TextMeshProUGUI _labelActive;
        private Color[] _colors;
        private Image _mask;
        private Image _maskActive;
        private GameObject _markIsNextRound;
        private GameObject _iconCanNotPerformAction;
        private GameObject _iconIsSurprising;
        private GameObject _iconIsFlatFooted;
        private GameObject _iconStandardAction;
        private GameObject _iconMoveAction;
        private GameObject _iconSwiftAction;
        private GameObject[] _objects;
        private GameObject[] _objectsActive;

        private bool _isCurrent;
        private bool _isMouseOver;
        private string _previousText;
        private float _width;

        public event Func<UnitEntityData, bool> OnClick;
        public event Action<UnitEntityData> OnEnter;
        public event Action<UnitEntityData> OnExit;

        public int Index { get; set; }

        public UnitEntityData Unit { get; set; }

        public static UnitButtonManager CreateObject()
        {
            GameObject sourceObject = Game.Instance.UI.Common?.transform
                .Find("ServiceWindow/Journal").GetComponent<JournalQuestLog>().Chapter.QuestNaviElement.gameObject;
            OvertipController overtip = Game.Instance.UI.BarkManager?.Overtip;

            if (!sourceObject || !overtip)
                return null;

            GameObject buttonUnit = Instantiate(sourceObject);
            buttonUnit.name = "Button_Unit";
            buttonUnit.GetComponent<ButtonPF>().onClick = new Button.ButtonClickedEvent();
            DestroyImmediate(buttonUnit.GetComponent<JournalQuestNaviElement>());
            buttonUnit.transform.Find("Complied").SafeDestroy();

            RectTransform rectButtonUnit = (RectTransform)buttonUnit.transform;
            rectButtonUnit.anchorMin = new Vector2(0f, 1f);
            rectButtonUnit.anchorMax = new Vector2(1f, 1f);
            rectButtonUnit.pivot = new Vector2(1f, 1f);
            rectButtonUnit.localPosition = new Vector3(0f, 0f, 0f);
            rectButtonUnit.sizeDelta = new Vector2(0f, UNIT_BUTTON_HEIGHT);
            rectButtonUnit.rotation = Quaternion.identity;

            GameObject hightlight = buttonUnit.transform.Find("BackgroundInActive/Highlight").gameObject;
            ((RectTransform)hightlight.transform).offsetMin = new Vector2(0f, 0f);

            GameObject hightlightActive = Instantiate(hightlight, buttonUnit.transform.Find("BackgroundActive"));
            hightlightActive.name = "Highlight";
            hightlightActive.GetComponent<Image>().color = Color.white;

            Vector2 iconSize = new Vector2(UNIT_BUTTON_HEIGHT, UNIT_BUTTON_HEIGHT);

            GameObject canNotPerformAction = buttonUnit.transform.Find("Failed").gameObject;
            canNotPerformAction.name = "CanNotPerformAction";
            ((RectTransform)canNotPerformAction.transform).sizeDelta = iconSize;

            GameObject isSurprising = buttonUnit.transform.Find("NeedToAttention").gameObject;
            isSurprising.name = "IsSurprising";
            isSurprising.transform.localPosition = canNotPerformAction.transform.localPosition;
            RectTransform rectIsSurprising = (RectTransform)isSurprising.transform;
            rectIsSurprising.anchoredPosition = new Vector2(-3f - UNIT_BUTTON_HEIGHT, 0.4f);
            rectIsSurprising.sizeDelta = new Vector2(UNIT_BUTTON_HEIGHT, 0f);

            GameObject isFlatFooted = buttonUnit.transform.Find("New").gameObject;
            isFlatFooted.name = "IsFlatFooted";
            ((RectTransform)isFlatFooted.transform).sizeDelta = iconSize;

            GameObject standardAction = Instantiate(canNotPerformAction, buttonUnit.transform);
            standardAction.name = "StandardAction";
            standardAction.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = overtip.AttackSprite;
            RectTransform rectStandardAction = (RectTransform)standardAction.transform;
            rectStandardAction.anchoredPosition = new Vector2(-3f - UNIT_BUTTON_HEIGHT * 2, 0.4f);
            rectStandardAction.sizeDelta = iconSize;

            GameObject moveAction = Instantiate(canNotPerformAction, buttonUnit.transform);
            moveAction.name = "MoveAction";
            moveAction.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = overtip.WalkSprite;
            RectTransform rectMoveAction = (RectTransform)moveAction.transform;
            rectMoveAction.anchoredPosition = new Vector2(-3f - UNIT_BUTTON_HEIGHT, 0.4f);
            rectMoveAction.sizeDelta = iconSize;

            GameObject swiftAction = Instantiate(canNotPerformAction, buttonUnit.transform);
            swiftAction.name = "SwiftAction";
            swiftAction.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = overtip.InteractSprite;
            ((RectTransform)swiftAction.transform).sizeDelta = iconSize;

            isSurprising.transform.SetAsLastSibling();
            isFlatFooted.transform.SetAsLastSibling();
            canNotPerformAction.transform.SetAsLastSibling();

            TextMeshProUGUI label = buttonUnit.transform.Find("HeaderInActive").gameObject.GetComponent<TextMeshProUGUI>();
            label.enableWordWrapping = false;

            TextMeshProUGUI labelActive = buttonUnit.transform.Find("HeaderActive").gameObject.GetComponent<TextMeshProUGUI>();
            labelActive.enableWordWrapping = false;

            return buttonUnit.AddComponent<UnitButtonManager>();
        }

        void Awake()
        {
            _button = gameObject.GetComponent<ButtonPF>();
            _button.onClick.AddListener(new UnityAction(HandleOnClick));
            _button.OnRightClick.AddListener(new UnityAction(HandleOnRightClick));
            _button.OnEnter.AddListener(new UnityAction(HandleOnEnter));
            _button.OnExit.AddListener(new UnityAction(HandleOnExit));
            
            _label = gameObject.transform.Find("HeaderInActive").gameObject.GetComponent<TextMeshProUGUI>();
            _labelActive = gameObject.transform.Find("HeaderActive").gameObject.GetComponent<TextMeshProUGUI>();

            _colors = new Color[]
            {
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.None).AddendumColor.linear,
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.Completed).AddendumColor.linear,
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.Failed).AddendumColor.linear,
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.Started).AddendumColor.linear
            };

            _mask = gameObject.transform.Find("BackgroundInActive/Highlight").gameObject.GetComponent<Image>();
            _maskActive = gameObject.transform.Find("BackgroundActive/Highlight").gameObject.GetComponent<Image>();

            _markIsNextRound = gameObject.transform.Find("BackgroundInActive/Decal").gameObject;

            _iconCanNotPerformAction = gameObject.transform.Find("CanNotPerformAction").gameObject;
            _iconIsSurprising = gameObject.transform.Find("IsSurprising").gameObject;
            _iconIsFlatFooted = gameObject.transform.Find("IsFlatFooted").gameObject;
            _iconStandardAction = gameObject.transform.Find("StandardAction").gameObject;
            _iconMoveAction = gameObject.transform.Find("MoveAction").gameObject;
            _iconSwiftAction = gameObject.transform.Find("SwiftAction").gameObject;

            _objects = new GameObject[]
            {
                gameObject.transform.Find("BackgroundInActive").gameObject,
                gameObject.transform.Find("HeaderInActive").gameObject,
            };

            _objectsActive = new GameObject[]
            {
                gameObject.transform.Find("BackgroundActive").gameObject,
                gameObject.transform.Find("HeaderActive").gameObject,
            };
        }

        void OnEnable()
        {
            EventBus.Subscribe(this);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(this);

            Unit?.SetHighlight(false);
            Unit = null;
        }

        void Update()
        {
            if (IsInCombat())
            {
                UpdateState();
                UpdateText();
                UpdateColorMask();
                UpdateUnitHighlight();
                UpdateCarama();
            }
        }

        public void HandleHoverChange(UnitEntityView unitEntityView, bool isHover)
        {
            if (unitEntityView.EntityData == Unit)
            {
                if (isHover)
                {
                    _button.OnSelect(null);
                    OnEnter(Unit);
                }
                else
                {
                    _button.OnDeselect(null);
                    OnExit(Unit);
                }
            }
        }

        private void HandleOnClick()
        {
            if (OnClick(Unit))
            {
                if (CameraScrollToUnitOnClickUI)
                    Unit.ScrollTo();

                if (SelectUnitOnClickUI)
                    Unit.Select();
            }
        }

        private void HandleOnRightClick()
        {
            if (ShowUnitDescriptionOnRightClickUI)
                Unit.Inspect();
        }

        private void HandleOnEnter()
        {
            _isMouseOver = true;
            OnEnter(Unit);
        }

        private void HandleOnExit()
        {
            _isMouseOver = false;
            OnExit(Unit);
        }

        private void UpdateState()
        {
            bool isCurrent = Unit != null && Unit.IsCurrentUnit();

            if (_isCurrent != isCurrent)
            {
                _isCurrent = isCurrent;

                for (int i = 0; i < _objectsActive.Length; i++)
                {
                    _objectsActive[i].SetActive(_isCurrent);
                }

                for (int i = 0; i < _objects.Length; i++)
                {
                    _objects[i].SetActive(!_isCurrent);
                }

                UpdateTimeBar();
                UpdateActionIcons();
            }
            else if (_isCurrent)
            {
                UpdateTimeBar();
                UpdateActionIcons();
            }

            UpdateStateIcons();
        }

        private void UpdateCarama()
        {
            if (!Game.Instance.IsPaused && _isCurrent && 
                (Unit.IsDirectlyControllable ? CameraLockOnCurrentPlayerUnit : CameraLockOnCurrentNonPlayerUnit))
                Unit.ScrollTo();
        }

        private void UpdateUnitHighlight()
        {
            if (Unit != null)
                Unit.SetHighlight(_isMouseOver || (_isCurrent && HighlightCurrentUnit));
        }

        private void UpdateTimeBar()
        {
            _maskActive.rectTransform.anchorMax = 
                new Vector2(_isCurrent ? Mod.Core.Combat.CurrentTurn.GetRemainingTime() / 6f : 1f, 1f);
        }

        private void UpdateActionIcons()
        {
            bool canActive = _isCurrent && Unit.CanPerformAction();
            _iconStandardAction.SetActive(canActive && Unit.HasStandardAction());
            _iconMoveAction.SetActive(canActive && !Unit.UsedOneMoveAction());
            _iconSwiftAction.SetActive(canActive && Unit.CombatState.Cooldown.SwiftAction == 0f);
        }

        private void UpdateStateIcons()
        {
            UnitEntityData currentUnit;
            _markIsNextRound.SetActive(Unit != null && !_isCurrent && Unit.GetTimeToNextTurn() < Mod.Core.Combat.TimeToNextRound);
            _iconIsSurprising.SetActive(Unit != null && !_isCurrent && Unit.IsSurprising());
            _iconIsFlatFooted.SetActive(ShowIsFlatFootedIconOnUI &&
                Unit != null && !_isCurrent && (currentUnit = Mod.Core.Combat.CurrentTurn?.Unit) != null &&
                Rulebook.Trigger(new RuleCheckTargetFlatFooted(currentUnit, Unit)).IsFlatFooted);
            _iconCanNotPerformAction.SetActive(Unit != null && !Unit.CanPerformAction());
        }

        private void UpdateColorMask()
        {
            if (Unit == null)
                _mask.color = _colors[0];
            else if (Game.Instance.Player.ControllableCharacters.Contains(Unit))
                _mask.color = _colors[1];
            else if (Unit.Group.IsEnemy(Game.Instance.Player.Group))
                _mask.color = _colors[2];
            else
                _mask.color = _colors[3];
        }

        private void UpdateText()
        {
            TextMeshProUGUI label = _isCurrent ? _labelActive : _label;
            string text = Unit == null ? string.Empty : 
                (!DoNotShowInvisibleUnitOnCombatTracker || Unit.IsVisibleForPlayer) ? Unit.CharacterName : "Unknown";

            if (text != _previousText || _width != label.rectTransform.rect.width)
            {
                _previousText = text;
                _width = label.rectTransform.rect.width;

                label.text = text;
                label.ForceMeshUpdate();

                for (int i = text.Length - 1; i >= 0 && label.textBounds.size.x > _width; i--)
                {
                    label.text = text.Substring(0, i) + "...";
                    label.ForceMeshUpdate();
                }

                (_isCurrent ? _label : _labelActive).text = label.text;
            }
        }
    }
}