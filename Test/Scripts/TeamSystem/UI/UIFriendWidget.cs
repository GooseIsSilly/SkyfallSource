using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR.UI
{
    public class UIFriendWidget : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _nicknameText;
        [SerializeField]
        private Image _statusIndicator;
        [SerializeField]
        private UIButton _inviteButton;
        [SerializeField]
        private UIButton _removeButton;
        [SerializeField]
        private Color _onlineColor = Color.green;
        [SerializeField]
        private Color _offlineColor = Color.gray;

        private FriendData _friendData;
        private Action<string> _onInviteClicked;

        public void SetData(FriendData friendData, Action<string> onInviteClicked)
        {
            _friendData = friendData;
            _onInviteClicked = onInviteClicked;

            if (_nicknameText != null)
            {
                _nicknameText.text = friendData.Nickname;
            }

            if (_statusIndicator != null)
            {
                _statusIndicator.color = friendData.IsOnline ? _onlineColor : _offlineColor;
            }

            if (_inviteButton != null)
            {
                _inviteButton.interactable = friendData.IsOnline && friendData.InLobby;
                _inviteButton.onClick.RemoveAllListeners();
                _inviteButton.onClick.AddListener(OnInviteClicked);
            }

            if (_removeButton != null)
            {
                _removeButton.onClick.RemoveAllListeners();
                _removeButton.onClick.AddListener(OnRemoveClicked);
            }
        }

        private void OnInviteClicked()
        {
            _onInviteClicked?.Invoke(_friendData.UserID);
        }

        private void OnRemoveClicked()
        {
            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.RemoveFriend(_friendData.UserID);
                Destroy(gameObject);
            }
        }
    }
}
