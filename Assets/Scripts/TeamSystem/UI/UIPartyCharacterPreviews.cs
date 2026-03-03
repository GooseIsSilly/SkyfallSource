using System.Collections.Generic;
using UnityEngine;

namespace TPSBR.UI
{
    public class UIPartyCharacterPreviews : UIWidget
    {
        [Header("Preview Settings")]
        [SerializeField]
        private Transform _previewContainer;
        [SerializeField]
        private float _spacing = 150f;
        [SerializeField]
        private Vector3 _basePosition = new Vector3(-200f, -100f, 5f);
        [SerializeField]
        private Vector3 _rotation = new Vector3(0f, 160f, 0f);
        
        [Header("Camera")]
        [SerializeField]
        private Camera _previewCamera;
        
        [Header("Prefab")]
        [SerializeField]
        private GameObject _playerPreviewPrefab;

        private List<GameObject> _activePreviews = new List<GameObject>();
        private PartyLobbyManager _partyManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            if (_previewContainer == null)
            {
                var containerObj = new GameObject("CharacterPreviewContainer");
                containerObj.transform.SetParent(transform.parent, false);
                _previewContainer = containerObj.transform;
            }
            
            _partyManager = PartyLobbyManager.Instance;
            
            if (_partyManager != null)
            {
                _partyManager.OnPartyUpdated += OnPartyUpdated;
            }
            
            RefreshPreviews();
        }

        protected override void OnDeinitialize()
        {
            if (_partyManager != null)
            {
                _partyManager.OnPartyUpdated -= OnPartyUpdated;
            }
            
            ClearPreviews();
            
            base.OnDeinitialize();
        }

        protected override void OnTick()
        {
            base.OnTick();
        }

        private void OnPartyUpdated(TeamData party)
        {
            RefreshPreviews();
        }

        private void RefreshPreviews()
        {
            ClearPreviews();
            
            if (_partyManager == null)
                return;
                
            var party = _partyManager.GetCurrentParty();
            
            if (party == null || party.MemberUserIDs.Count == 0)
            {
                CreateLocalPlayerPreview();
                return;
            }
            
            int count = party.MemberUserIDs.Count;
            float totalWidth = (count - 1) * _spacing;
            float startX = _basePosition.x - (totalWidth * 0.5f);
            
            for (int i = 0; i < party.MemberUserIDs.Count; i++)
            {
                string userID = party.MemberUserIDs[i];
                Vector3 position = _basePosition + new Vector3(startX + (i * _spacing), 0, 0);
                
                CreateCharacterPreview(userID, position);
            }
        }

        private void CreateLocalPlayerPreview()
        {
            if (Context == null || Context.PlayerData == null)
                return;
                
            CreateCharacterPreview(Context.PlayerData.UserID, _basePosition);
        }

        private void CreateCharacterPreview(string userID, Vector3 position)
        {
            GameObject preview = null;
            
            if (_playerPreviewPrefab != null)
            {
                preview = Instantiate(_playerPreviewPrefab, _previewContainer);
            }
            else
            {
                preview = CreateDefaultPreview(userID);
            }
            
            if (preview != null)
            {
                preview.transform.localPosition = position;
                preview.transform.localRotation = Quaternion.Euler(_rotation);
                preview.transform.localScale = Vector3.one;
                
                _activePreviews.Add(preview);
                
                var previewComponent = preview.GetComponent<CharacterPreview>();
                if (previewComponent != null)
                {
                    previewComponent.SetCharacter(userID, GetCharacterForUser(userID));
                }
            }
        }

        private GameObject CreateDefaultPreview(string userID)
        {
            var previewObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            previewObj.name = $"Preview_{userID}";
            previewObj.transform.SetParent(_previewContainer, false);
            
            var renderer = previewObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = GetColorForUser(userID);
            }
            
            var nicknameObj = new GameObject("Nickname");
            nicknameObj.transform.SetParent(previewObj.transform, false);
            nicknameObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            
            var canvas = nicknameObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            var canvasRect = nicknameObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 50);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(nicknameObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = GetNicknameForUser(userID);
            text.fontSize = 36;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = Color.white;
            
            return previewObj;
        }

        private void ClearPreviews()
        {
            foreach (var preview in _activePreviews)
            {
                if (preview != null)
                {
                    Destroy(preview);
                }
            }
            
            _activePreviews.Clear();
        }

        private string GetCharacterForUser(string userID)
        {
            if (Context != null && Context.PlayerData != null && userID == Context.PlayerData.UserID)
            {
                return Context.PlayerData.AgentID;
            }
            
            var friend = _partyManager?.GetFriend(userID);
            if (friend != null)
            {
                return "Agent1";
            }
            
            return "Agent1";
        }

        private string GetNicknameForUser(string userID)
        {
            if (Context != null && Context.PlayerData != null && userID == Context.PlayerData.UserID)
            {
                return Context.PlayerData.Nickname;
            }
            
            var friend = _partyManager?.GetFriend(userID);
            if (friend != null)
            {
                return friend.Nickname;
            }
            
            return "Player";
        }

        private Color GetColorForUser(string userID)
        {
            int hash = userID.GetHashCode();
            Random.InitState(hash);
            return new Color(Random.value, Random.value, Random.value);
        }
    }

    public class CharacterPreview : MonoBehaviour
    {
        private string _userID;
        private string _character;

        public void SetCharacter(string userID, string character)
        {
            _userID = userID;
            _character = character;
        }
    }
}
