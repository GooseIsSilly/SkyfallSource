using UnityEngine;
using TMPro;

namespace TPSBR
{
    public class TeammateVisualizer : ContextBehaviour
    {
        [Header("Name Tag")]
        [SerializeField]
        private GameObject _nameTagPrefab;
        [SerializeField]
        private Vector3 _nameTagOffset = new Vector3(0, 2.5f, 0);
        [SerializeField]
        private float _nameTagVisibleDistance = 50f;

        [Header("Glow Effect")]
        [SerializeField]
        private Color _teammateGlowColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField]
        private Color _downedGlowColor = new Color(1f, 0.5f, 0f, 0.8f);
        [SerializeField]
        private float _glowVisibleDistance = 20f;
        [SerializeField]
        private float _downedGlowVisibleDistance = 50f;
        [SerializeField]
        private Material _outlineMaterial;

        private Player _localPlayer;
        private GameObject _nameTagInstance;
        private TextMeshProUGUI _nameText;
        private Canvas _nameTagCanvas;
        private Player _owner;

        private void Awake()
        {
            _owner = GetComponent<Player>();
        }

        private void Update()
        {
            if (Context == null || Context.NetworkGame == null)
                return;

            UpdateLocalPlayer();

            if (_localPlayer == null || _owner == null)
                return;

            if (_localPlayer == _owner)
            {
                HideVisuals();
                return;
            }

            if (!_localPlayer.IsTeammateWith(_owner))
            {
                HideVisuals();
                return;
            }

            if (_localPlayer.ActiveAgent == null)
            {
                HideVisuals();
                return;
            }

            float distance = Vector3.Distance(_localPlayer.ActiveAgent.transform.position, transform.position);

            UpdateNameTag(distance);
            UpdateGlow(distance);
        }

        private void UpdateLocalPlayer()
        {
            if (_localPlayer != null)
                return;

            if (Context != null && Context.NetworkGame != null)
            {
                _localPlayer = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
            }
        }

        private void UpdateNameTag(float distance)
        {
            var reviveSystem = _owner != null ? _owner.GetComponent<ReviveSystem>() : null;
            bool isDowned = reviveSystem != null && reviveSystem.IsDown;
            
            bool shouldShow = distance <= _nameTagVisibleDistance || (isDowned && distance <= _downedGlowVisibleDistance);

            if (shouldShow && _nameTagInstance == null)
            {
                CreateNameTag();
            }

            if (_nameTagInstance != null)
            {
                _nameTagInstance.SetActive(shouldShow);

                if (shouldShow && _nameTagCanvas != null)
                {
                    Vector3 worldPosition = transform.position + _nameTagOffset;
                    _nameTagInstance.transform.position = worldPosition;

                    if (Camera.main != null)
                    {
                        _nameTagInstance.transform.rotation = Quaternion.LookRotation(_nameTagInstance.transform.position - Camera.main.transform.position);
                    }

                    if (_nameText != null && _owner != null)
                    {
                        string nameText = _owner.Nickname;
                        if (isDowned)
                        {
                            nameText += " (DOWNED)";
                            _nameText.color = _downedGlowColor;
                        }
                        else
                        {
                            _nameText.color = Color.white;
                        }
                        _nameText.text = nameText;
                    }
                }
            }
        }

        private void CreateNameTag()
        {
            _nameTagInstance = new GameObject("TeammateNameTag");
            _nameTagInstance.transform.SetParent(transform);

            _nameTagCanvas = _nameTagInstance.AddComponent<Canvas>();
            _nameTagCanvas.renderMode = RenderMode.WorldSpace;

            var canvasScaler = _nameTagInstance.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10f;

            var rectTransform = _nameTagInstance.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200f, 50f);
            rectTransform.localScale = Vector3.one * 0.01f;

            var textObj = new GameObject("NameText");
            textObj.transform.SetParent(_nameTagInstance.transform);

            _nameText = textObj.AddComponent<TextMeshProUGUI>();
            _nameText.alignment = TextAlignmentOptions.Center;
            _nameText.fontSize = 24;
            _nameText.color = Color.white;
            _nameText.fontStyle = FontStyles.Bold;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private void UpdateGlow(float distance)
        {
            var reviveSystem = _owner != null ? _owner.GetComponent<ReviveSystem>() : null;
            bool isDowned = reviveSystem != null && reviveSystem.IsDown;
            
            bool shouldGlow = distance <= _glowVisibleDistance || (isDowned && distance <= _downedGlowVisibleDistance);

            if (_owner == null || _owner.ActiveAgent == null)
                return;

            var renderers = _owner.ActiveAgent.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer == null)
                    continue;

                if (shouldGlow)
                {
                    ApplyGlowEffect(renderer);
                }
                else
                {
                    RemoveGlowEffect(renderer);
                }
            }
        }

        private void ApplyGlowEffect(Renderer renderer)
        {
            var materials = renderer.materials;
            bool hasOutline = false;

            foreach (var mat in materials)
            {
                if (mat != null && mat.name.Contains("Outline"))
                {
                    hasOutline = true;
                    break;
                }
            }

            if (!hasOutline && _outlineMaterial != null)
            {
                var newMaterials = new Material[materials.Length + 1];
                materials.CopyTo(newMaterials, 0);
                newMaterials[materials.Length] = _outlineMaterial;
                renderer.materials = newMaterials;
            }
        }

        private void RemoveGlowEffect(Renderer renderer)
        {
            var materials = renderer.materials;
            var newMaterials = new Material[0];
            int count = 0;

            foreach (var mat in materials)
            {
                if (mat != null && !mat.name.Contains("Outline"))
                {
                    count++;
                }
            }

            if (count < materials.Length)
            {
                newMaterials = new Material[count];
                int index = 0;
                foreach (var mat in materials)
                {
                    if (mat != null && !mat.name.Contains("Outline"))
                    {
                        newMaterials[index++] = mat;
                    }
                }
                renderer.materials = newMaterials;
            }
        }

        private void HideVisuals()
        {
            if (_nameTagInstance != null)
            {
                _nameTagInstance.SetActive(false);
            }

            if (_owner != null && _owner.ActiveAgent != null)
            {
                var renderers = _owner.ActiveAgent.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    RemoveGlowEffect(renderer);
                }
            }
        }

        private void OnDestroy()
        {
            if (_nameTagInstance != null)
            {
                Destroy(_nameTagInstance);
            }
        }
    }
}
