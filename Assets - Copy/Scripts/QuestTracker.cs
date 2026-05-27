using UnityEngine;
using Fusion;

namespace TPSBR
{
    public class QuestTracker : MonoBehaviour
    {
        private QuestManager _questManager;
        private Player _localPlayer;
        private Agent _localAgent;
        
        private float _survivalTimeCounter = 0f;
        private Vector3 _lastPosition;
        private float _totalDistanceTraveled = 0f;
        private int _lastCircleCount = 0;
        private bool _matchActive = false;

        private void Start()
        {
            _questManager = QuestManager.Instance;
            if (_questManager == null)
            {
                Debug.LogWarning("[QuestTracker] QuestManager not found. Quest tracking will be disabled.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (!_matchActive || _questManager == null)
                return;

            TrackSurvivalTime();
            TrackTravelDistance();
        }

        public void OnMatchStart()
        {
            _matchActive = true;
            _survivalTimeCounter = 0f;
            _totalDistanceTraveled = 0f;
            _lastCircleCount = 0;
            
            if (_questManager != null)
            {
                _questManager.OnMatchStarted();
            }

            FindLocalAgent();
        }

        public void OnMatchEnd(int position, int totalPlayers, bool isWinner)
        {
            _matchActive = false;
            
            if (_questManager != null)
            {
                _questManager.OnMatchEnded(position, totalPlayers, isWinner);
                _questManager.OnPlayerSurvived();
            }
        }

        private void FindLocalAgent()
        {
            var networkGame = FindObjectOfType<NetworkGame>();
            if (networkGame != null)
            {
                var runner = networkGame.Runner;
                if (runner != null)
                {
                    _localPlayer = networkGame.GetPlayer(runner.LocalPlayer);
                    if (_localPlayer != null)
                    {
                        _localAgent = _localPlayer.ActiveAgent;
                    }
                }
            }
        }

        private void TrackSurvivalTime()
        {
            _survivalTimeCounter += Time.deltaTime;
            
            if (Mathf.FloorToInt(_survivalTimeCounter) % 60 == 0 && _survivalTimeCounter > 1f)
            {
                _questManager.UpdateSurvivalTime(_survivalTimeCounter);
            }
        }

        private void TrackTravelDistance()
        {
            if (_localAgent == null || _localAgent.Character == null)
            {
                FindLocalAgent();
                if (_localAgent != null && _localAgent.Character != null)
                {
                    _lastPosition = _localAgent.Character.transform.position;
                }
                return;
            }

            Vector3 currentPosition = _localAgent.Character.transform.position;
            float distance = Vector3.Distance(_lastPosition, currentPosition);
            
            if (distance > 0.1f && distance < 50f)
            {
                _totalDistanceTraveled += distance;
                _questManager.UpdateTravelDistance(_totalDistanceTraveled);
            }
            
            _lastPosition = currentPosition;
        }

        public void OnKillObtained(KillData killData, float distance, string weaponType)
        {
            if (_questManager == null)
                return;

            bool isHeadshot = killData.Headshot;
            _questManager.OnEliminationObtained(isHeadshot, distance, weaponType);
        }

        public void OnDamageDealt(int damage)
        {
            if (_questManager == null)
                return;

            _questManager.OnDamageDealt(damage);
        }

        public void OnStormDamageTaken()
        {
            if (_questManager == null)
                return;

            _questManager.OnStormDamageTaken();
        }

        public void OnItemPickedUp()
        {
            if (_questManager == null)
                return;

            _questManager.OnItemLooted();
        }

        public void OnHealingItemUsed()
        {
            if (_questManager == null)
                return;

            _questManager.OnHealingItemUsed();
        }

        public void OnPlayerLanded(Vector3 landingPosition)
        {
            if (_questManager == null)
                return;

            string locationName = DetermineLocationName(landingPosition);
            _questManager.OnPlayerLanded(locationName);
        }

        public void OnStormCircleChanged(int circleNumber)
        {
            if (_questManager == null || circleNumber <= _lastCircleCount)
                return;

            _lastCircleCount = circleNumber;
            _questManager.OnStormCircleSurvived();

            if (circleNumber >= 6)
            {
                _questManager.OnFinalCircleReached();
            }
        }

        private string DetermineLocationName(Vector3 position)
        {
            return $"Location_{Mathf.FloorToInt(position.x / 100f)}_{Mathf.FloorToInt(position.z / 100f)}";
        }
    }
}
