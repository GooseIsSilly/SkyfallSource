using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR
{
    [Serializable]
    public class ChallengeEntryData
    {
        public string Title;
        public string Description;
        public int XPReward;
        public int CurrentProgress;
        public int TargetProgress;
        public bool IsComplete;
    }

    /// <summary>Placeholder challenge list panel shown when the Challenges tab is active.</summary>
    public class UIBattlePassChallengesTab : MonoBehaviour
    {
        // ── Serialized ─────────────────────────────────────────────────────────

        [Header("Layout")]
        [SerializeField] private Transform   _rowContainer;
        [SerializeField] private GameObject  _rowPrefab;
        [SerializeField] private TextMeshProUGUI _emptyLabel;

        // ── Private State ──────────────────────────────────────────────────────

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Populates the challenge list with the provided entries.</summary>
        public void Populate(List<ChallengeEntryData> challenges)
        {
            ClearRows();

            bool isEmpty = challenges == null || challenges.Count == 0;

            if (_emptyLabel != null)
                _emptyLabel.gameObject.SetActive(isEmpty);

            if (isEmpty || _rowPrefab == null || _rowContainer == null) return;

            foreach (ChallengeEntryData challenge in challenges)
                SpawnRow(challenge);
        }

        // ── Private Helpers ────────────────────────────────────────────────────

        private void SpawnRow(ChallengeEntryData data)
        {
            GameObject row = Instantiate(_rowPrefab, _rowContainer);
            _spawnedRows.Add(row);

            // Bind title
            TextMeshProUGUI title = row.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
                title.text = data.IsComplete ? $"✓ {data.Title}" : data.Title;

            // Bind description
            TextMeshProUGUI desc = row.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            if (desc != null)
                desc.text = data.Description;

            // Bind XP reward
            TextMeshProUGUI xpLabel = row.transform.Find("XPText")?.GetComponent<TextMeshProUGUI>();
            if (xpLabel != null)
                xpLabel.text = $"+{data.XPReward} XP";

            // Bind progress bar
            Slider progress = row.GetComponentInChildren<Slider>();
            if (progress != null)
            {
                progress.minValue = 0;
                progress.maxValue = Mathf.Max(1, data.TargetProgress);
                progress.value    = data.CurrentProgress;
            }

            // Bind progress label
            TextMeshProUGUI progressLabel = row.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            if (progressLabel != null)
                progressLabel.text = $"{data.CurrentProgress}/{data.TargetProgress}";
        }

        private void ClearRows()
        {
            foreach (GameObject row in _spawnedRows)
            {
                if (row != null) Destroy(row);
            }
            _spawnedRows.Clear();
        }
    }
}
