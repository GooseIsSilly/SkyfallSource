using UnityEngine;

namespace TPSBR.Examples
{
    public class CustomReviveBehavior : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField]
        private AudioClip _downedSound;
        [SerializeField]
        private AudioClip _revivedSound;
        [SerializeField]
        private AudioClip _bledOutSound;

        [Header("Effects")]
        [SerializeField]
        private GameObject _downedVFX;
        [SerializeField]
        private GameObject _revivedVFX;

        private ReviveSystem _reviveSystem;
        private AudioSource _audioSource;

        private void Awake()
        {
            _reviveSystem = GetComponent<ReviveSystem>();
            _audioSource = GetComponent<AudioSource>();

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            if (_reviveSystem != null)
            {
                _reviveSystem.OnPlayerDowned += OnPlayerDowned;
                _reviveSystem.OnReviveStarted += OnReviveStarted;
                _reviveSystem.OnReviveCompleted += OnReviveCompleted;
                _reviveSystem.OnReviveCancelled += OnReviveCancelled;
                _reviveSystem.OnPlayerBledOut += OnPlayerBledOut;
            }
        }

        private void OnDisable()
        {
            if (_reviveSystem != null)
            {
                _reviveSystem.OnPlayerDowned -= OnPlayerDowned;
                _reviveSystem.OnReviveStarted -= OnReviveStarted;
                _reviveSystem.OnReviveCompleted -= OnReviveCompleted;
                _reviveSystem.OnReviveCancelled -= OnReviveCancelled;
                _reviveSystem.OnPlayerBledOut -= OnPlayerBledOut;
            }
        }

        private void OnPlayerDowned(Player player)
        {
            Debug.Log($"{player.Nickname} is down!");

            PlaySound(_downedSound);
            SpawnEffect(_downedVFX, player.transform.position);
        }

        private void OnReviveStarted(Player downedPlayer, Player reviver)
        {
            Debug.Log($"{reviver.Nickname} started reviving {downedPlayer.Nickname}");
        }

        private void OnReviveCompleted(Player revivedPlayer, Player reviver)
        {
            Debug.Log($"{revivedPlayer.Nickname} was revived by {reviver.Nickname}!");

            PlaySound(_revivedSound);
            SpawnEffect(_revivedVFX, revivedPlayer.transform.position);
        }

        private void OnReviveCancelled(Player player)
        {
            Debug.Log($"Revive of {player.Nickname} was cancelled");
        }

        private void OnPlayerBledOut(Player player)
        {
            Debug.Log($"{player.Nickname} bled out and died");

            PlaySound(_bledOutSound);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        private void SpawnEffect(GameObject effectPrefab, Vector3 position)
        {
            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, position, Quaternion.identity);
            }
        }
    }
}
