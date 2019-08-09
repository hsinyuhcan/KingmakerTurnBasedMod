using Kingmaker;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Constructor;
using Kingmaker.UI.Journal;
using Kingmaker.UI.Overtip;
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
    public class UnitButtonManager : MonoBehaviour
    {
        private ButtonPF _button;
        private TextMeshProUGUI _label;
        private TextMeshProUGUI _activeLabel;
        private Color[] _colors;
        private Image _colorMask;
        private Image _activeColorMask;
        private GameObject _canNotPerformActionIcon;
        private GameObject _isWaitingInitiativeIcon;
        private GameObject _isFlatFootedIcon;
        private GameObject _standardActionIcon;
        private GameObject _moveActionIcon;
        private GameObject _swiftActionIcon;
        private GameObject[] _objects;
        private GameObject[] _activeObjects;

        private bool _isCurrent;
        private bool _isMouseOver;
        private float _width;

        public event Func<UnitEntityData, bool> OnClick;
        public event Action<UnitEntityData> OnEnter;
        public event Action<UnitEntityData> OnExit;

        public int Index { get; set; }

        public UnitEntityData Unit { get; private set; }

        void Awake()
        {
            _button = gameObject.GetComponent<ButtonPF>();
            _button.onClick.AddListener(new UnityAction(OnClickHandler));
            _button.OnRightClick.AddListener(new UnityAction(OnRightClickHandler));
            _button.OnEnter.AddListener(new UnityAction(OnEnterHandler));
            _button.OnExit.AddListener(new UnityAction(OnExitHandler));
            
            _label = gameObject.transform.Find("HeaderInActive").gameObject.GetComponent<TextMeshProUGUI>();
            _activeLabel = gameObject.transform.Find("HeaderActive").gameObject.GetComponent<TextMeshProUGUI>();

            _colors = new Color[]
            {
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.None).AddendumColor.linear,
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.Completed).AddendumColor.linear,
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.Failed).AddendumColor.linear,
                UIRoot.Instance.GetQuestNotificationObjectiveColor(QuestObjectiveState.Started).AddendumColor.linear
            };

            _colorMask = gameObject.transform.Find("BackgroundInActive/Highlight").gameObject.GetComponent<Image>();
            _activeColorMask = gameObject.transform.Find("BackgroundActiveHighlight").gameObject.GetComponent<Image>();
        
            _canNotPerformActionIcon = gameObject.transform.Find("CanNotPerformAction").gameObject;
            _isWaitingInitiativeIcon = gameObject.transform.Find("IsWaitingInitiative").gameObject;
            _isFlatFootedIcon = gameObject.transform.Find("IsFlatFooted").gameObject;
            _standardActionIcon = gameObject.transform.Find("StandardAction").gameObject;
            _moveActionIcon = gameObject.transform.Find("MoveAction").gameObject;
            _swiftActionIcon = gameObject.transform.Find("SwiftAction").gameObject;

            _activeObjects = new GameObject[]
            {
                gameObject.transform.Find("BackgroundActive").gameObject,
                gameObject.transform.Find("HeaderActive").gameObject,
                _activeColorMask.gameObject,
            };

            _objects = new GameObject[]
            {
                gameObject.transform.Find("BackgroundInActive").gameObject,
                gameObject.transform.Find("HeaderInActive").gameObject,
            };
        }

        void OnDestroy()
        {
            _isCurrent = false;
            _isMouseOver = false;
            UpdateUnitHighlight();
            Unit = null;
        }

        void Update()
        {
            if (IsInCombat() && Unit != null)
            {
                UpdateState();
                UpdateText();
                UpdateColorMask();
                UpdateUnitHighlight();
                UpdateCarama();
            }
        }

        public static UnitButtonManager CreateObject()
        {
            GameObject sourceObject = Game.Instance.UI.Common?.transform
                .Find("ServiceWindow/Journal").GetComponent<JournalQuestLog>().Chapter.QuestNaviElement.gameObject;
            OvertipController overtip = Game.Instance.UI.BarkManager.Overtip;

            if (sourceObject.IsNullOrDestroyed() || overtip == null)
                return null;

            GameObject tbUnitButton = Instantiate(sourceObject);
            tbUnitButton.name = "Btn_Unit";
            tbUnitButton.GetComponent<ButtonPF>().onClick = new Button.ButtonClickedEvent();
            DestroyImmediate(tbUnitButton.GetComponent<JournalQuestNaviElement>());
            DestroyImmediate(tbUnitButton.transform.Find("BackgroundInActive/Decal").gameObject);
            DestroyImmediate(tbUnitButton.transform.Find("Complied").gameObject);

            RectTransform rectUnitButton = (RectTransform)tbUnitButton.transform;
            rectUnitButton.anchorMin = new Vector2(0f, 1f);
            rectUnitButton.anchorMax = new Vector2(1f, 1f);
            rectUnitButton.pivot = new Vector2(1f, 1f);
            rectUnitButton.localPosition = new Vector3(0f, 0f, 0f);
            rectUnitButton.sizeDelta = new Vector2(0f, UNIT_BUTTON_HEIGHT);
            rectUnitButton.rotation = Quaternion.identity;

            GameObject hightlight = tbUnitButton.transform.Find("BackgroundInActive/Highlight").gameObject;
            ((RectTransform)hightlight.transform).offsetMin = new Vector2(0f, 0f);

            GameObject activeHightlight = Instantiate(hightlight, tbUnitButton.transform);
            activeHightlight.name = "BackgroundActiveHighlight";
            activeHightlight.transform.SetSiblingIndex(tbUnitButton.transform.Find("Highlight").GetSiblingIndex());
            activeHightlight.GetComponent<Image>().color = Color.white;

            Vector2 iconSize = new Vector2(UNIT_BUTTON_HEIGHT, UNIT_BUTTON_HEIGHT);

            // Icons
            // overtip.DefaultSprite
            // overtip.EquipSprite

            GameObject canNotPerformAction = tbUnitButton.transform.Find("Failed").gameObject;
            canNotPerformAction.name = "CanNotPerformAction";
            ((RectTransform)canNotPerformAction.transform).sizeDelta = iconSize;

            GameObject isWaitingInitiative = tbUnitButton.transform.Find("NeedToAttention").gameObject;
            isWaitingInitiative.name = "IsWaitingInitiative";
            isWaitingInitiative.transform.localPosition = canNotPerformAction.transform.localPosition;
            RectTransform rectIsWaitingInitiative = (RectTransform)isWaitingInitiative.transform;
            rectIsWaitingInitiative.anchoredPosition = new Vector2(-3f - UNIT_BUTTON_HEIGHT, 0.4f);
            rectIsWaitingInitiative.sizeDelta = new Vector2(UNIT_BUTTON_HEIGHT, 0f);

            GameObject isFlatFooted = tbUnitButton.transform.Find("New").gameObject;
            isFlatFooted.name = "IsFlatFooted";
            ((RectTransform)isFlatFooted.transform).sizeDelta = iconSize;

            GameObject standardAction = Instantiate(canNotPerformAction, tbUnitButton.transform);
            standardAction.name = "StandardAction";
            standardAction.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = overtip.AttackSprite;
            RectTransform rectStandardAction = (RectTransform)standardAction.transform;
            rectStandardAction.anchoredPosition = new Vector2(-3f - UNIT_BUTTON_HEIGHT * 2, 0.4f);
            rectStandardAction.sizeDelta = iconSize;

            GameObject moveAction = Instantiate(canNotPerformAction, tbUnitButton.transform);
            moveAction.name = "MoveAction";
            moveAction.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = overtip.WalkSprite;
            RectTransform rectMoveAction = (RectTransform)moveAction.transform;
            rectMoveAction.anchoredPosition = new Vector2(-3f - UNIT_BUTTON_HEIGHT, 0.4f);
            rectMoveAction.sizeDelta = iconSize;

            GameObject swiftAction = Instantiate(canNotPerformAction, tbUnitButton.transform);
            swiftAction.name = "SwiftAction";
            swiftAction.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = overtip.InteractSprite;
            ((RectTransform)swiftAction.transform).sizeDelta = iconSize;

            isWaitingInitiative.transform.SetAsLastSibling();
            isFlatFooted.transform.SetAsLastSibling();
            canNotPerformAction.transform.SetAsLastSibling();

            TextMeshProUGUI label = tbUnitButton.transform.Find("HeaderInActive").gameObject.GetComponent<TextMeshProUGUI>();
            label.enableWordWrapping = false;
            //label.fontSize = 22f;
            //label.fontSizeMax = 28f;
            //label.fontSizeMin = 14f;

            TextMeshProUGUI highlightLabel = tbUnitButton.transform.Find("HeaderActive").gameObject.GetComponent<TextMeshProUGUI>();
            highlightLabel.enableWordWrapping = false;
            //highlightLabel.fontSize = 22f;
            //highlightLabel.fontSizeMax = 28f;
            //highlightLabel.fontSizeMin = 14f;

            return tbUnitButton.AddComponent<UnitButtonManager>();
        }

        public static UnitButtonManager CreateObject(UnitButtonManager unitEntry)
        {
            return Instantiate(unitEntry.gameObject).GetComponent<UnitButtonManager>();
        }

        public void SetOrder(int order)
        {
            gameObject.transform.SetSiblingIndex(order);
        }

        public void SetUnit(UnitEntityData unit)
        {
            Unit = unit;

            UpdateState(true);
            UpdateText(true);
            UpdateColorMask();
        }

        private void OnClickHandler()
        {
            if (OnClick(Unit))
            {
                if (CameraScrollToUnitOnClickUI)
                    Unit.ScrollTo();

                if (SelectUnitOnClickUI)
                    Unit.Select();
            }
        }

        private void OnRightClickHandler()
        {
            if (ShowUnitDescriptionOnRightClickUI)
                Unit.Inspect();
        }

        private void OnEnterHandler()
        {
            _isMouseOver = true;
            OnEnter(Unit);
        }

        private void OnExitHandler()
        {
            _isMouseOver = false;
            OnExit(Unit);
        }

        private void UpdateState(bool forceUpdate = false)
        {
            bool isCurrent = Unit != null && Unit.IsCurrentUnit();

            if (forceUpdate || _isCurrent != isCurrent)
            {
                _isCurrent = isCurrent;

                for (int i = 0; i < _activeObjects.Length; i++)
                {
                    _activeObjects[i].SetActive(_isCurrent);
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

            UpdateCanNotPerformActionIcon();
            UpdateIsSurprisingIcon();
            UpdateIsFlatFooted();
        }

        private void UpdateCarama()
        {
            if (!Game.Instance.IsPaused && 
                _isCurrent && (Unit.IsDirectlyControllable ? CameraLockOnCurrentPlayerUnit : CameraLockOnCurrentNonPlayerUnit))
            {
                Unit.ScrollTo();
            }
        }

        private void UpdateUnitHighlight()
        {
            Unit.SetHighlight(_isMouseOver || (_isCurrent && HighlightCurrentUnit));
        }

        private void UpdateTimeBar()
        {
            _activeColorMask.rectTransform.anchorMax = 
                new Vector2(_isCurrent ? Mod.Core.Combat.CurrentTurn.GetRemainingTime() / 6f : 1f, 1f);
        }

        private void UpdateActionIcons()
        {
            _standardActionIcon.SetActive(_isCurrent && Unit.HasStandardAction());
            _moveActionIcon.SetActive(_isCurrent && !Unit.UsedOneMoveAction());
            _swiftActionIcon.SetActive(_isCurrent && Unit.CombatState.Cooldown.SwiftAction == 0f);
        }

        private void UpdateCanNotPerformActionIcon()
        {
            _canNotPerformActionIcon.SetActive(!_isCurrent && Unit != null && !Unit.CanPerformAction());
        }

        private void UpdateIsSurprisingIcon()
        {
            _isWaitingInitiativeIcon.SetActive(!_isCurrent && Unit != null && Unit.IsSurprising());
        }

        private void UpdateIsFlatFooted()
        {
            UnitEntityData currentUnit = Mod.Core.Combat.CurrentTurn?.Unit;
            _isFlatFootedIcon.SetActive((ShowIsFlatFootedIconOnUI || (_isMouseOver && ShowIsFlatFootedIconOnHoverUI)) &&
                !_isCurrent && Unit != null && currentUnit != null &&
                Rulebook.Trigger(new RuleCheckTargetFlatFooted(currentUnit, Unit)).IsFlatFooted);
        }

        private void UpdateColorMask()
        {
            if (Unit == null)
                _colorMask.color = _colors[0];
            else if (Game.Instance.Player.ControllableCharacters.Contains(Unit))
                _colorMask.color = _colors[1];
            else if (Unit.Group.IsEnemy(Game.Instance.Player.Group))
                _colorMask.color = _colors[2];
            else
                _colorMask.color = _colors[3];
        }

        private void UpdateText(bool froceUpdate = false)
        {
            TextMeshProUGUI label = _isCurrent ? _activeLabel : _label;
            if (froceUpdate || _width != label.rectTransform.rect.width)
            {
                _width = label.rectTransform.rect.width;

                string text = Unit?.CharacterName;

                label.text = text;
                label.ForceMeshUpdate();

                while (label.textBounds.size.x > _width)
                {
                    if (text.Length > 0)
                        text = text.Substring(0, text.Length - 1);
                    else
                        break;

                    label.text = text + "...";
                    label.ForceMeshUpdate();
                }

                (_isCurrent ? _label : _activeLabel).text = label.text;
            }
        }
    }
}
