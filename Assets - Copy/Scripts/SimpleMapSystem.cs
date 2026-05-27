using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using Fusion;

namespace TPSBR
{
    public class SimpleMapSystem : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private Sprite _mapImage;
        [SerializeField] private Vector2 _mapWorldSize = new Vector2(100f, 100f);
        [SerializeField] private Vector2 _mapWorldCenter = Vector2.zero;

        [Header("UI References")]
        [SerializeField] private GameObject _mapPanel;
        [SerializeField] private Image _mapDisplay;
        [SerializeField] private RectTransform _playerMarker;
        [SerializeField] private GameObject _poiMarkerPrefab;
        [SerializeField] private Transform _poiMarkersContainer;

        [Header("Zone Visualization")]
        [SerializeField] private bool _showZones = true;
        [SerializeField] private Image _currentZoneCircle;
        [SerializeField] private Image _nextZoneCircle;
        [SerializeField] private Image _dangerZoneOverlay;
        [SerializeField] private Image _guidanceLine;
        [SerializeField] private Color _currentZoneColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color _nextZoneColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Color _dangerZoneColor = new Color(1f, 0f, 0f, 0.6f);
        [SerializeField] private Color _guideLineColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private float _guideLineWidth = 3f;

        private Material _dangerZoneMaterial;

        private bool _isMapOpen = false;
        private Transform _playerTransform;
        private ShrinkingArea _shrinkingArea;
        private readonly List<MapPOI> _pois = new List<MapPOI>();
        private readonly Dictionary<MapPOI, RectTransform> _poiMarkers = new Dictionary<MapPOI, RectTransform>();

        private void Start()
        {
            SetupMap();
            FindPOIs();
            FindShrinkingArea();
            SetupZoneCircles();

            if (_mapPanel != null)
            {
                _mapPanel.SetActive(false);
            }
        }

        private void SetupMap()
        {
            if (_mapDisplay != null && _mapImage != null)
            {
                _mapDisplay.sprite = _mapImage;
            }
        }

        private void FindPOIs()
        {
            _pois.Clear();
            _pois.AddRange(FindObjectsByType<MapPOI>(FindObjectsSortMode.None));
            CreatePOIMarkers();
        }

        private void CreatePOIMarkers()
        {
            if (_poiMarkerPrefab == null || _poiMarkersContainer == null)
                return;

            foreach (var kvp in _poiMarkers)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _poiMarkers.Clear();

            foreach (var poi in _pois)
            {
                GameObject markerObj = Instantiate(_poiMarkerPrefab, _poiMarkersContainer);
                RectTransform markerRect = markerObj.GetComponent<RectTransform>();

                if (markerRect != null)
                {
                    Text markerText = markerObj.GetComponent<Text>();
                    if (markerText != null)
                    {
                        markerText.text = poi.POIName;
                        markerText.color = poi.POIColor;
                    }
                    else
                    {
                        TextMeshProUGUI markerTMP = markerObj.GetComponent<TextMeshProUGUI>();
                        if (markerTMP != null)
                        {
                            markerTMP.text = poi.POIName;
                            markerTMP.color = poi.POIColor;
                        }
                    }

                    _poiMarkers[poi] = markerRect;
                }
            }
        }

        private void Update()
        {
            HandleInput();

            if (_isMapOpen)
            {
                UpdateMarkers();
            }
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.mKey.wasPressedThisFrame)
            {
                ToggleMap();
            }
        }

        public void ToggleMap()
        {
            _isMapOpen = !_isMapOpen;

            if (_mapPanel != null)
            {
                _mapPanel.SetActive(_isMapOpen);
            }
        }

        private void UpdateMarkers()
        {
            if (_playerTransform == null)
            {
                FindPlayer();
            }
            else
            {
                Agent agent = _playerTransform.GetComponent<Agent>();
                if (agent == null || !agent.HasInputAuthority)
                {
                    _playerTransform = null;
                    FindPlayer();
                }
            }

            if (_playerTransform != null && _playerMarker != null)
            {
                UpdatePlayerMarker();
            }

            UpdatePOIMarkers();
            UpdateZoneCircles();
        }

        private void FindShrinkingArea()
        {
            _shrinkingArea = FindFirstObjectByType<ShrinkingArea>();
        }

        private void SetupZoneCircles()
        {
            if (_currentZoneCircle != null)
            {
                _currentZoneCircle.color = _currentZoneColor;
                _currentZoneCircle.type = Image.Type.Simple;
                _currentZoneCircle.gameObject.SetActive(false);
            }

            if (_nextZoneCircle != null)
            {
                _nextZoneCircle.color = _nextZoneColor;
                _nextZoneCircle.type = Image.Type.Simple;
                _nextZoneCircle.gameObject.SetActive(false);
            }

            if (_dangerZoneOverlay != null)
            {
                _dangerZoneOverlay.color = Color.white;
                _dangerZoneOverlay.type = Image.Type.Simple;
                _dangerZoneOverlay.gameObject.SetActive(false);
                _dangerZoneOverlay.raycastTarget = false;

                Shader dangerShader = Shader.Find("UI/DangerZoneOverlay");
                if (dangerShader != null)
                {
                    _dangerZoneMaterial = new Material(dangerShader);
                    _dangerZoneMaterial.SetColor("_Color", _dangerZoneColor);
                    _dangerZoneOverlay.material = _dangerZoneMaterial;
                }
            }

            if (_guidanceLine != null)
            {
                _guidanceLine.color = _guideLineColor;
                _guidanceLine.type = Image.Type.Filled;
                _guidanceLine.fillMethod = Image.FillMethod.Horizontal;
                _guidanceLine.gameObject.SetActive(false);
                _guidanceLine.raycastTarget = false;
            }
        }

        private void UpdateZoneCircles()
        {
            if (!_showZones || _mapDisplay == null)
            {
                if (_currentZoneCircle != null) _currentZoneCircle.gameObject.SetActive(false);
                if (_nextZoneCircle != null) _nextZoneCircle.gameObject.SetActive(false);
                if (_dangerZoneOverlay != null) _dangerZoneOverlay.gameObject.SetActive(false);
                if (_guidanceLine != null) _guidanceLine.gameObject.SetActive(false);
                return;
            }

            if (_shrinkingArea == null || !_shrinkingArea.IsActive)
            {
                if (_currentZoneCircle != null) _currentZoneCircle.gameObject.SetActive(false);
                if (_nextZoneCircle != null) _nextZoneCircle.gameObject.SetActive(false);
                if (_dangerZoneOverlay != null) _dangerZoneOverlay.gameObject.SetActive(false);
                if (_guidanceLine != null) _guidanceLine.gameObject.SetActive(false);
                return;
            }

            if (_currentZoneCircle != null)
            {
                _currentZoneCircle.gameObject.SetActive(true);
                UpdateZoneCircle(_currentZoneCircle, _shrinkingArea.Center, _shrinkingArea.Radius);
            }

            bool hasNextZone = _shrinkingArea.IsAnnounced;
            
            if (_nextZoneCircle != null)
            {
                _nextZoneCircle.gameObject.SetActive(true);
                if (hasNextZone)
                {
                    UpdateZoneCircle(_nextZoneCircle, _shrinkingArea.ShrinkCenter, _shrinkingArea.ShrinkRadius);
                }
                else
                {
                    UpdateZoneCircle(_nextZoneCircle, _shrinkingArea.Center, _shrinkingArea.Radius);
                }
            }

            UpdateDangerZone(hasNextZone);
            UpdateGuidanceLine(hasNextZone);
        }

        private void UpdateDangerZone(bool hasNextZone)
        {
            if (_dangerZoneOverlay == null || _mapDisplay == null || _shrinkingArea == null)
                return;

            _dangerZoneOverlay.gameObject.SetActive(true);

            RectTransform overlayRect = _dangerZoneOverlay.GetComponent<RectTransform>();
            RectTransform mapRect = _mapDisplay.GetComponent<RectTransform>();

            overlayRect.anchoredPosition = Vector2.zero;
            overlayRect.sizeDelta = mapRect.sizeDelta;

            if (_dangerZoneMaterial != null)
            {
                Vector3 targetZoneCenter = hasNextZone ? _shrinkingArea.ShrinkCenter : _shrinkingArea.Center;
                float targetZoneRadius = hasNextZone ? _shrinkingArea.ShrinkRadius : _shrinkingArea.Radius;

                float normalizedX = (targetZoneCenter.x - _mapWorldCenter.x + _mapWorldSize.x / 2f) / _mapWorldSize.x;
                float normalizedZ = (targetZoneCenter.z - _mapWorldCenter.y + _mapWorldSize.y / 2f) / _mapWorldSize.y;

                float radiusNormalized = targetZoneRadius / Mathf.Max(_mapWorldSize.x, _mapWorldSize.y);

                _dangerZoneMaterial.SetVector("_ZoneCenter", new Vector4(normalizedX, normalizedZ, 0, 0));
                _dangerZoneMaterial.SetFloat("_ZoneRadius", radiusNormalized);
                _dangerZoneMaterial.SetVector("_MapSize", new Vector4(mapRect.rect.width, mapRect.rect.height, 0, 0));
                _dangerZoneMaterial.SetColor("_Color", _dangerZoneColor);
            }
        }

        private void UpdateGuidanceLine(bool hasNextZone)
        {
            if (_guidanceLine == null || _playerTransform == null || _shrinkingArea == null || _mapDisplay == null)
            {
                if (_guidanceLine != null)
                    _guidanceLine.gameObject.SetActive(false);
                return;
            }

            Vector3 targetZoneCenter = hasNextZone ? _shrinkingArea.ShrinkCenter : _shrinkingArea.Center;
            float targetZoneRadius = hasNextZone ? _shrinkingArea.ShrinkRadius : _shrinkingArea.Radius;

            Vector3 playerWorldPos = _playerTransform.position;
            Vector3 directionToZone = targetZoneCenter - playerWorldPos;
            directionToZone.y = 0f;
            float distanceToZoneCenter = directionToZone.magnitude;

            bool isOutsideZone = distanceToZoneCenter > targetZoneRadius;

            if (!isOutsideZone)
            {
                _guidanceLine.gameObject.SetActive(false);
                return;
            }

            _guidanceLine.gameObject.SetActive(true);

            Vector2 playerMapPos = WorldToMapPosition(playerWorldPos);
            Vector2 zoneMapPos = WorldToMapPosition(targetZoneCenter);
            
            Vector2 lineDirection = (zoneMapPos - playerMapPos).normalized;
            float lineLength = Vector2.Distance(playerMapPos, zoneMapPos);

            RectTransform lineRect = _guidanceLine.GetComponent<RectTransform>();
            lineRect.anchoredPosition = playerMapPos;
            lineRect.sizeDelta = new Vector2(lineLength, _guideLineWidth);

            float angle = Mathf.Atan2(lineDirection.y, lineDirection.x) * Mathf.Rad2Deg;
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);

            lineRect.pivot = new Vector2(0f, 0.5f);
        }

        private void UpdateZoneCircle(Image circleImage, Vector3 worldCenter, float worldRadius)
        {
            if (circleImage == null || _mapDisplay == null)
                return;

            RectTransform circleRect = circleImage.GetComponent<RectTransform>();
            if (circleRect == null)
                return;

            Vector2 mapPosition = WorldToMapPosition(worldCenter);
            circleRect.anchoredPosition = mapPosition;

            RectTransform mapRect = _mapDisplay.GetComponent<RectTransform>();
            float mapDisplayWidth = mapRect.rect.width;

            float circleSize = (worldRadius * 2f / _mapWorldSize.x) * mapDisplayWidth;

            circleRect.sizeDelta = new Vector2(circleSize, circleSize);
        }

        private void FindPlayer()
        {
            Agent[] allAgents = FindObjectsByType<Agent>(FindObjectsSortMode.None);
            
            foreach (Agent agent in allAgents)
            {
                if (agent.HasInputAuthority)
                {
                    _playerTransform = agent.transform;
                    return;
                }
            }
        }

        private void UpdatePlayerMarker()
        {
            if (_playerTransform == null || _playerMarker == null || _mapDisplay == null)
                return;

            Vector2 mapPosition = WorldToMapPosition(_playerTransform.position);
            _playerMarker.anchoredPosition = mapPosition;

            float rotation = -_playerTransform.eulerAngles.y;
            _playerMarker.localRotation = Quaternion.Euler(0, 0, rotation);
        }

        private void UpdatePOIMarkers()
        {
            foreach (var kvp in _poiMarkers)
            {
                MapPOI poi = kvp.Key;
                RectTransform marker = kvp.Value;

                if (poi != null && marker != null)
                {
                    Vector2 mapPosition = WorldToMapPosition(poi.transform.position);
                    marker.anchoredPosition = mapPosition;
                }
            }
        }

        private Vector2 WorldToMapPosition(Vector3 worldPosition)
        {
            if (_mapDisplay == null)
                return Vector2.zero;

            float normalizedX = (worldPosition.x - _mapWorldCenter.x + _mapWorldSize.x / 2f) / _mapWorldSize.x;
            float normalizedZ = (worldPosition.z - _mapWorldCenter.y + _mapWorldSize.y / 2f) / _mapWorldSize.y;

            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedZ = Mathf.Clamp01(normalizedZ);

            RectTransform mapRect = _mapDisplay.GetComponent<RectTransform>();
            float displayWidth = mapRect.rect.width;
            float displayHeight = mapRect.rect.height;

            float x = (normalizedX - 0.5f) * displayWidth;
            float y = (normalizedZ - 0.5f) * displayHeight;

            return new Vector2(x, y);
        }
    }
}
