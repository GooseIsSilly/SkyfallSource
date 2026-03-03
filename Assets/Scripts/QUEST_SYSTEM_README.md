# Quest System - Complete Implementation Guide

## Overview
This quest system provides a complete daily/weekly quest solution with CloudCoin rewards for your battle royale game. Players receive 4-5 random daily quests that refresh every 24 hours, along with weekly and progression quests.

## Features
✅ **4-5 Random Daily Quests** - Refreshes every 24 hours
✅ **Quest Categories**: Daily, Combat, Weekly, Progression, Special
✅ **21 Unique Quests** with varied objectives
✅ **CloudCoin Rewards** - 25 to 1000 coins per quest
✅ **Automatic Progress Tracking** - Tracks player actions during gameplay
✅ **Persistent Save System** - Quest progress saved between sessions
✅ **UI System** - Full quest menu and in-game HUD
✅ **Event Integration** - Hooks into game events automatically

---

## Quick Start Guide

### Step 1: Initial Setup

1. **Create Quest System GameObject in Menu Scene**
   - Open `Menu.unity` scene
   - Create an empty GameObject named "QuestSystemInitializer"
   - Add the `QuestSystemInitializer` component
   - Leave "Initialize On Awake" checked
   - Leave "Persist Across Scenes" checked

2. **The system will auto-create these managers:**
   - QuestManager
   - CloudCoinManager
   - QuestEventIntegration

### Step 2: UI Setup

#### A. Quest Menu UI (Main Menu)

1. **Create Quest Menu View**
   - In Menu scene hierarchy, duplicate an existing view (like UIShopView)
   - Rename it to "UIQuestView"
   - Add the `UIQuestView` component to the root

2. **Setup UI Elements:**
   ```
   UIQuestView (GameObject)
   ├── Background (Image)
   ├── CloseButton (Button)
   ├── TitleText (TextMeshProUGUI) - Set text to "QUESTS"
   └── Content (GameObject)
       ├── QuestListContainer (Vertical Layout Group)
       └── ScrollView (Optional)
   ```

3. **Create Quest Item Prefab**
   - Create a new GameObject "QuestItemPrefab"
   - Add the `UIQuestItem` component
   - Setup structure:
   ```
   QuestItemPrefab
   ├── Background (Image)
   ├── QuestNameText (TextMeshProUGUI)
   ├── QuestDescriptionText (TextMeshProUGUI)
   ├── ProgressBar (Slider)
   ├── ProgressText (TextMeshProUGUI)
   ├── RewardText (TextMeshProUGUI)
   ├── ClaimButton (Button)
   └── CompletedIndicator (GameObject/Image)
   ```

4. **Link References in UIQuestView:**
   - Quest List Container → The container GameObject
   - Quest Item Prefab → Your created prefab
   - Close Button → The close button
   - Title Text → The title text

#### B. In-Game Quest HUD (Optional)

1. **In Gameplay Scene**, add to HUD:
   - Create GameObject "QuestProgressHUD"
   - Add `UIQuestProgressHUD` component
   - Create similar structure to quest items but simplified

### Step 3: Add Quest Button to Main Menu

1. **Add Quests Button** (copy from existing buttons):
   ```csharp
   // In your main menu script, add:
   [SerializeField] private UIButton _questsButton;
   
   private void OnQuestsButtonClicked()
   {
       // Show quest view
       UIQuestView questView = FindObjectOfType<UIQuestView>(true);
       if (questView != null)
       {
           questView.Open();
       }
   }
   ```

### Step 4: Game Event Integration

The quest system needs to track player actions. Add these integration points:

#### A. Health/Damage Tracking
In `Health.cs`, find where damage is applied and add:
```csharp
using TPSBR;

// In the method where damage is dealt (e.g., ProcessHit or ApplyDamage):
QuestIntegrationPatches.PatchHealthDamage(this, hitData);
```

#### B. Storm Damage Tracking
In `ShrinkingArea.cs` or `DamageArea.cs`, where storm damage is applied:
```csharp
// When applying storm damage to an agent:
QuestIntegrationPatches.PatchStormDamage(agent);
```

#### C. Item Pickup Tracking
In pickup scripts (e.g., `StaticPickup.cs`, `DynamicPickup.cs`):
```csharp
// When item is picked up:
QuestIntegrationPatches.PatchItemPickup(agent);
```

#### D. Healing Item Usage
In healing/consumable item scripts:
```csharp
// When healing item is used:
QuestIntegrationPatches.PatchHealingItemUsed(agent);
```

#### E. Match Start/End Tracking
In `GameplayMode.cs` or `BattleRoyaleGameplayMode.cs`:

```csharp
// In OnActivate() method:
protected override void OnActivate()
{
    base.OnActivate();
    QuestIntegrationPatches.PatchGameplayModeActivated();
}

// In FinishGameplay() or similar method:
protected override void FinishGameplay()
{
    // Calculate player position and winner
    int playerPosition = CalculatePlayerPosition();
    int totalPlayers = GetTotalPlayers();
    bool isWinner = CheckIfWinner();
    
    QuestIntegrationPatches.PatchGameplayModeFinished(playerPosition, totalPlayers, isWinner);
    
    base.FinishGameplay();
}
```

#### F. NetworkGame Integration
In `NetworkGame.cs`, add in initialization:
```csharp
public override void Spawned()
{
    base.Spawned();
    
    // Set network game reference for quest system
    if (QuestEventIntegration.Instance != null)
    {
        QuestEventIntegration.Instance.SetNetworkGame(this);
    }
}
```

#### G. GameplayMode Reference
In the GameplayMode setup:
```csharp
public override void Spawned()
{
    base.Spawned();
    
    if (QuestEventIntegration.Instance != null)
    {
        QuestEventIntegration.Instance.SetGameplayMode(this);
    }
}
```

#### H. Storm Circle Tracking
In `ShrinkingArea.cs`, when circles change:
```csharp
private void OnShrinkingAreaChanged(int circleNumber)
{
    QuestIntegrationPatches.PatchShrinkingAreaChanged(circleNumber);
}
```

---

## Quest List

### Daily Quests (25-75 coins)
1. **First Drop** - Play 1 match (25 coins)
2. **Survivor** - Survive for 5 minutes (50 coins)
3. **Scavenger** - Loot 3 item boxes (40 coins)
4. **Top Half** - Finish in top 50% (60 coins)
5. **Distance Walker** - Travel 1000 meters (35 coins)
6. **Zone Runner** - Survive 3 storm circles (75 coins)

### Combat Quests (75-150 coins)
7. **First Blood** - Get 1 elimination (100 coins)
8. **Marksman** - Deal 200 damage (125 coins)
9. **Close Combat** - Elimination within 10 meters (150 coins)
10. **Headhunter** - Get 2 headshot eliminations (175 coins)

### Weekly Quests (200-500 coins)
11. **Victory Royale** - Win 1 match (500 coins)
12. **Top 10 Streak** - Top 10 in 3 matches (250 coins)
13. **Elimination Spree** - 5 total eliminations (200 coins)
14. **Zone Master** - Reach final circle 2 times (300 coins)

### Progression Quests (250-400 coins)
15. **Battle Royale Veteran** - Play 10 matches (300 coins)
16. **Storm Survivor** - Take storm damage and survive 5 times (400 coins)
17. **Weapon Master** - Eliminations with 3 weapon types (350 coins)
18. **Explorer** - Land in 5 different locations (250 coins)

### Special Quests (100-1000 coins)
19. **Weekly Champion** - Win 3 matches this week (1000 coins)
20. **Perfect Game** - Win without storm damage (750 coins)
21. **Medic** - Use 10 healing items (100 coins)

---

## API Reference

### QuestManager
```csharp
// Get quest manager instance
QuestManager questManager = QuestManager.Instance;

// Manually update quest progress
questManager.UpdateQuestProgress(QuestRequirementType.GetEliminations, 1);

// Get active quests
List<QuestDefinition> activeQuests = questManager.GetActiveQuests();

// Get quest progress
QuestProgress progress = questManager.GetQuestProgress("quest_id");

// Claim quest reward
questManager.ClaimQuestReward("quest_id");

// Event subscriptions
questManager.OnQuestsUpdated += OnQuestsRefreshed;
questManager.OnQuestProgressUpdated += OnQuestProgress;
questManager.OnQuestCompleted += OnQuestComplete;
```

### CloudCoinManager
```csharp
// Get coin manager instance
CloudCoinManager coinManager = CloudCoinManager.Instance;

// Add coins
coinManager.AddCoins(100);

// Try purchase
bool success = coinManager.TryPurchase(50);

// Get current coins
int coins = coinManager.GetCurrentCoins();
```

### Manual Quest Tracking
```csharp
QuestManager qm = QuestManager.Instance;

// Match events
qm.OnMatchStarted();
qm.OnMatchEnded(position: 5, totalPlayers: 100, isWinner: false);

// Combat events
qm.OnEliminationObtained(isHeadshot: true, distance: 15f, weaponType: "AssaultRifle");
qm.OnDamageDealt(damage: 50);

// Survival events
qm.OnStormDamageTaken();
qm.OnStormCircleSurvived();
qm.OnFinalCircleReached();

// Loot/Items
qm.OnItemLooted();
qm.OnHealingItemUsed();

// Exploration
qm.OnPlayerLanded(locationName: "TiltedTowers");

// Time/Distance
qm.UpdateSurvivalTime(seconds: 300f);
qm.UpdateTravelDistance(meters: 1000f);
```

---

## Testing the System

### In Editor Testing
1. Select the QuestSystemInitializer GameObject
2. Use the context menu (right-click in Inspector):
   - **Force Initialize Quest System** - Reinitialize all managers
   - **Test Add Coins** - Add 100 test coins
   - **Test Complete Quest** - Simulate quest completion

### Runtime Testing
```csharp
// Add to any test script:
void TestQuests()
{
    var qm = QuestManager.Instance;
    
    // Simulate playing a match
    qm.OnMatchStarted();
    qm.UpdateQuestProgress(QuestRequirementType.PlayMatches, 1);
    qm.OnMatchEnded(10, 100, false);
    
    // Simulate getting kills
    qm.OnEliminationObtained(true, 20f, "AssaultRifle");
    
    // Simulate dealing damage
    qm.OnDamageDealt(150);
    
    Debug.Log($"Active Quests: {qm.GetActiveQuests().Count}");
}
```

---

## Troubleshooting

### Quests not tracking?
- Ensure QuestSystemInitializer is in the scene
- Check that integration patches are called in game code
- Verify QuestManager.Instance is not null
- Check console for "[Quest System]" log messages

### Coins not being added?
- Verify CloudCoinManager is initialized
- Check Global.PlayerService.PlayerData exists
- Ensure quests are being claimed (not just completed)

### UI not showing?
- Check UIQuestView component is attached
- Verify all UI references are assigned in Inspector
- Make sure quest item prefab is assigned

### Quest progress resets?
- Quest data is saved to PlayerPrefs
- Daily quests reset every 24 hours (intended)
- Weekly quests reset every 7 days (intended)

---

## File Structure

```
/Assets/Scripts/
├── QuestData.cs                    - Quest definitions and data structures
├── QuestManager.cs                 - Core quest management logic
├── QuestTracker.cs                 - Gameplay progress tracking
├── QuestEventIntegration.cs        - Game event hooks
├── QuestIntegrationPatches.cs      - Helper integration methods
├── QuestSystemInitializer.cs       - Auto-initialization system
├── CloudCoinManager.cs             - Currency management
├── UIQuestView.cs                  - Main quest menu UI
├── UIQuestItem.cs                  - Individual quest display
├── UIQuestProgressHUD.cs           - In-game HUD tracker
└── QUEST_SYSTEM_README.md          - This file
```

---

## Customization

### Adding New Quests
1. Open `QuestManager.cs`
2. Find the `InitializeQuests()` method
3. Add new quest definition:
```csharp
_allQuests.Add(new QuestDefinition(
    "quest_id",
    "Quest Name",
    "Quest Description",
    QuestType.Daily,
    QuestRequirementType.PlayMatches,
    requiredAmount: 5,
    coinReward: 100
));
```

### Changing Daily Quest Count
In `QuestManager.cs`, modify:
```csharp
private const int DAILY_QUESTS_PER_DAY = 5; // Change this number
```

### Adjusting Reset Times
In `QuestManager.cs`, modify:
```csharp
long oneDayInSeconds = 86400;    // 24 hours
long oneWeekInSeconds = 604800;  // 7 days
```

---

## Support

For issues or questions:
1. Check the console for error messages
2. Verify all integration points are implemented
3. Test with the provided testing methods
4. Review this README for setup steps

**System is fully functional and ready to use!** 🎮✨
