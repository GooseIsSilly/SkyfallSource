using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR
{
    public class BattlePassUIManager : MonoBehaviour
    {
        public int rewardsPerPage = 7;
        public int currentPage = 0;

        [Header("UI")]
        public GameObject freeTierReward, premiumTierReward, tierNumber;
        public Transform freeTierHolder, premiumTierHolder, tiersHolder;
        public TMP_Text currentTier;

        List<GameObject> spawnedObjectsForThisPage = new List<GameObject>();

        void Start()
        {
            UpdatePage(0);
        }

        void Update()
        {
            currentTier.text = BattlePassManager.Instance.CurrentTier.ToString();
        }

        public void UpdatePage(int page)
        {
            currentPage = page;
            foreach (GameObject go in spawnedObjectsForThisPage)
            {
                Destroy(go);
            }
            spawnedObjectsForThisPage.Clear();

            for (int i = 0; i < rewardsPerPage; i++)
            {
                BattlePassTierEntry thisReward = BattlePassManager.Instance.SeasonData.Tiers[i + currentPage * rewardsPerPage];

                GameObject newFreeTier = Instantiate(freeTierReward, freeTierHolder);
                newFreeTier.transform.GetChild(1).GetComponent<Image>().sprite = thisReward.FreeReward.Icon;

                GameObject newPremiumTier = Instantiate(premiumTierReward, premiumTierHolder);
                newPremiumTier.transform.GetChild(1).GetComponent<Image>().sprite = thisReward.PremiumReward.Icon;

                GameObject newTierNumber = Instantiate(tierNumber, tiersHolder);
                newTierNumber.transform.GetChild(1).GetComponent<TMP_Text>().text = (i + currentPage * rewardsPerPage + 1).ToString();

                spawnedObjectsForThisPage.Add(newTierNumber);
                spawnedObjectsForThisPage.Add(newPremiumTier);
                spawnedObjectsForThisPage.Add(newFreeTier);
            }
        }
        public void NextPage()
        {
            if (currentPage < (BattlePassManager.Instance.SeasonData.Tiers.Count / rewardsPerPage) - 1)
            {
                UpdatePage(currentPage + 1);
            }
        }
        public void BackPage()
        {
            if (currentPage > 0)
            {
                UpdatePage(currentPage - 1);
            }
        }
    }
}