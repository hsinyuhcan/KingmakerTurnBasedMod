using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual;
using Kingmaker.Visual.Decals;
using UnityEngine;

namespace TurnBased.UI
{
    public class RangeIndicatorManager : MonoBehaviour
    {
        private bool _visible;
        private Color _currentColor;
        private Color _visibleColor;
        private GUIDecal[] _decals;

        public Color VisibleColor {
            get => _visibleColor;
            set {
                _visibleColor = value;

                if (_visible)
                {
                    DOTween.Pause(this);
                    DOTween.Kill(this, false);
                    SetColor(_visibleColor);
                }
            }
        }

        void Awake()
        {
            _decals = gameObject.GetComponentsInChildren<GUIDecal>();
        }

        void OnDestroy()
        {
            DOTween.Pause(this);
            DOTween.Kill(this, false);
        }

        public static RangeIndicatorManager CreateObject(GameObject aoeRange, string name, bool hasBackground = true)
        {
            GameObject tbRange = Instantiate(aoeRange);
            tbRange.name = name;
            tbRange.SetActive(false);

            if (hasBackground)
                tbRange.transform.Find("AoERangeBack").gameObject.name = name + "Back";
            else
                DestroyImmediate(tbRange.transform.Find("AoERangeBack").gameObject);

            DestroyImmediate(tbRange.GetComponent<ScreenSpaceDecalGroup>());

            return tbRange.AddComponent<RangeIndicatorManager>();
        }

        public void SetPosition(UnitEntityData unit)
        {
            transform.position = unit.Position;
        }

        public void SetRadius(float meters)
        {
            float radius = meters * 2f;
            transform.localScale = new Vector3(radius, transform.localScale.y, radius);
        }

        private void SetColor(Color color)
        {
            _currentColor = color;

            foreach (GUIDecal decal in _decals)
                decal.MaterialProperties.SetBaseColor(color);
        }

        public void SetVisible(bool visible)
        {
            if (_visible != visible)
            {
                _visible = visible;

                DOTween.Pause(this);
                DOTween.Kill(this, false);
                TweenerCore<Color, Color, ColorOptions> tweenerCore =
                    DOTween.To(() => _currentColor, (Color color) => SetColor(color), visible ? VisibleColor : Color.clear, 0.3f);

                if (visible)
                {
                    gameObject.SetActive(true);
                }
                else
                {
                    tweenerCore.OnComplete(() => gameObject.SetActive(false));
                }

                tweenerCore.SetAutoKill();
                tweenerCore.SetTarget(this);
                tweenerCore.SetUpdate(true);
            }
        }
    }
}
