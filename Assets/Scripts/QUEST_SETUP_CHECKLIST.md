# Quest System Setup Checklist

Use this checklist to ensure your quest system is fully set up and working.

---

## ✅ Phase 1: Core System Setup (5 minutes)

- [ ] **Open Setup Wizard**
  - Go to `TPSBR > Quest System Setup Wizard` in Unity menu
  
- [ ] **Initialize Quest System**
  - Click "Create Quest System Initializer in Current Scene"
  - Verify GameObject appears in Menu scene hierarchy
  
- [ ] **Verify Auto-Creation**
  - Enter Play Mode
  - Check Console for "[Quest System] Initialization complete!"
  - Exit Play Mode

**✓ Phase 1 Complete** - Core managers are ready!

---

## ✅ Phase 2: UI Setup (15-20 minutes)

### A. Quest Menu View

- [ ] **Create UIQuestView GameObject**
  - In Menu scene, duplicate existing UI view (e.g., UIShopView)
  - Rename to "UIQuestView"
  
- [ ] **Setup View Structure**
  ```
  UIQuestView
  ├── Background (Image)
  ├── CloseButton (Button)  
  ├── TitleText (TextMeshProUGUI)
  └── Content
      └── QuestListContainer (Vertical Layout Group)
  ```

- [ ] **Add UIQuestView Component**
  - Add `UIQuestView` script to root GameObject
  
- [ ] **Link References**
  - Quest List Container → Content/QuestListContainer
  - Close Button → CloseButton
  - Title Text → TitleText
  - Set title to "QUESTS"

### B. Quest Item Prefab

- [ ] **Create Prefab Structure**
  ```
  QuestItemPrefab
  ├── Background (Image)
  ├── QuestNameText (TextMeshProUGUI)
  ├── QuestDescriptionText (TextMeshProUGUI)
  ├── ProgressBar (Slider)
  ├── ProgressText (TextMeshProUGUI)
  ├── RewardText (TextMeshProUGUI)
  ├── ClaimButton (Button)
  └── CompletedIndicator (Image/GameObject)
  ```

- [ ] **Add UIQuestItem Component**
  - Add `UIQuestItem` script to prefab root
  
- [ ] **Link Prefab References**
  - Assign all UI elements in Inspector
  - Set colors for quest types (optional)
  
- [ ] **Save as Prefab**
  - Drag to /Assets/Prefabs/ folder
  
- [ ] **Link to UIQuestView**
  - Assign prefab to UIQuestView's "Quest Item Prefab" field

### C. Menu Button

- [ ] **Add Quests Button**
  - Duplicate existing menu button (e.g., Shop button)
  - Rename to "QuestsButton"
  - Update button text to "QUESTS"
  
- [ ] **Hook Up Button**
  - Add click handler to open UIQuestView
  - Test button opens quest menu

**✓ Phase 2 Complete** - UI is ready!

---

## ✅ Phase 3: Game Integration (20-30 minutes)

### Essential Integrations (Required)

- [ ] **Match Start**
  - Open `BattleRoyaleGameplayMode.cs`
  - Find `OnActivate()` method
  - Add: `QuestIntegrationPatches.PatchGameplayModeActivated();`
  
- [ ] **Match End**
  - Find `FinishGameplay()` or similar method
  - Calculate player position, total players, isWinner
  - Add: `QuestIntegrationPatches.PatchGameplayModeFinished(pos, total, winner);`
  
- [ ] **Combat/Damage**
  - Open `Health.cs`
  - Find damage application method
  - Add: `QuestIntegrationPatches.PatchHealthDamage(this, hitData);`
  
- [ ] **Item Pickups**
  - Open `StaticPickup.cs` or `DynamicPickup.cs`
  - Find `OnPickup()` method
  - Add: `QuestIntegrationPatches.PatchItemPickup(agent);`

### Recommended Integrations (Highly Recommended)

- [ ] **Storm Damage**
  - Open `ShrinkingArea.cs` or `DamageArea.cs`
  - Find storm damage application
  - Add: `QuestIntegrationPatches.PatchStormDamage(agent);`
  
- [ ] **Healing Items**
  - Open healing/consumable script
  - Find usage method
  - Add: `QuestIntegrationPatches.PatchHealingItemUsed(agent);`
  
- [ ] **NetworkGame Reference**
  - Open `NetworkGame.cs`
  - In `Spawned()` method
  - Add: `QuestEventIntegration.Instance?.SetNetworkGame(this);`
  
- [ ] **GameplayMode Reference**
  - Open `GameplayMode.cs` or `BattleRoyaleGameplayMode.cs`
  - In `Spawned()` method
  - Add: `QuestEventIntegration.Instance?.SetGameplayMode(this);`

### Optional Integrations (Nice to Have)

- [ ] **Player Landing**
  - Track where players land after drop
  - Add: `QuestIntegrationPatches.PatchPlayerLanded(agent, position);`
  
- [ ] **Storm Circles**
  - Track storm circle progression
  - Add: `QuestIntegrationPatches.PatchShrinkingAreaChanged(circleNum);`

**✓ Phase 3 Complete** - Game tracking is hooked up!

---

## ✅ Phase 4: Testing (10 minutes)

### Editor Tests

- [ ] **Validate Setup**
  - Open Setup Wizard
  - Click "Validate Quest System Setup"
  - Verify all systems show ✓ checkmarks
  
- [ ] **Check Components**
  - Click "Find Quest System Components"
  - Verify all components are found
  
- [ ] **Test Quest Progress**
  - Enter Play Mode
  - Select QuestSystemInitializer in Hierarchy
  - Right-click → "Test Complete Quest"
  - Check Console for success message

### Gameplay Tests

- [ ] **Test Match Tracking**
  - Enter Play Mode
  - Start a match
  - Check Console for "[Quest Hooks] Match started"
  - Complete match
  - Verify match quest progress updates
  
- [ ] **Test Combat Tracking**
  - Get an elimination
  - Check quest progress for combat quests
  - Deal damage
  - Verify damage quest updates
  
- [ ] **Test Item Tracking**
  - Pick up items/loot boxes
  - Check scavenger quest progress
  - Use healing item
  - Verify medic quest progress
  
- [ ] **Test Quest Menu**
  - Open quest menu from main menu
  - Verify quests are displayed
  - Check progress bars update
  - Complete a quest
  - Claim reward
  - Verify CloudCoins increase

### UI Tests

- [ ] **Quest Display**
  - Open quest menu
  - Verify 4-5 quests are shown
  - Check quest names, descriptions, rewards display
  
- [ ] **Progress Display**
  - Verify progress bars work
  - Check progress text (e.g., "1/3")
  - Confirm colors match quest types
  
- [ ] **Claim Rewards**
  - Complete a quest
  - Claim button appears
  - Click claim button
  - Verify coins added
  - Quest marked as complete

**✓ Phase 4 Complete** - Everything works!

---

## ✅ Phase 5: Verification (5 minutes)

### Final Checks

- [ ] **All Managers Present**
  - QuestManager initialized ✓
  - CloudCoinManager initialized ✓
  - QuestEventIntegration initialized ✓
  
- [ ] **UI Functional**
  - Quest menu opens ✓
  - Quests display correctly ✓
  - Progress updates in real-time ✓
  - Rewards can be claimed ✓
  
- [ ] **Tracking Works**
  - Match events tracked ✓
  - Combat events tracked ✓
  - Item events tracked ✓
  - Progress saves/loads ✓
  
- [ ] **No Errors**
  - Check Console for errors
  - Verify no missing references
  - Test multiple play sessions

### Performance Check

- [ ] **No Frame Drops**
  - Quest system doesn't affect FPS
  
- [ ] **No Memory Leaks**
  - Quest data properly managed
  
- [ ] **Save System Works**
  - Progress persists between sessions
  - Daily reset works (test by changing system time)

**✓ Phase 5 Complete** - Production ready!

---

## 🎉 Setup Complete!

### You Now Have:

✅ **Fully functional quest system**  
✅ **21 unique quests**  
✅ **4-5 daily quests rotating**  
✅ **CloudCoin rewards working**  
✅ **Complete UI**  
✅ **Automatic tracking**  
✅ **Persistent save system**  

---

## 📋 Quick Reference

| Need | Location |
|------|----------|
| Complete setup guide | QUEST_SYSTEM_README.md |
| Integration examples | QUEST_INTEGRATION_GUIDE.md |
| Quick overview | QUEST_SYSTEM_SUMMARY.md |
| Setup wizard | TPSBR > Quest System Setup Wizard |
| This checklist | QUEST_SETUP_CHECKLIST.md |

---

## 🐛 Troubleshooting

If something doesn't work:

1. **Check Console** - Look for error messages
2. **Validate Setup** - Use Setup Wizard validation
3. **Verify Integration** - Ensure patches are called
4. **Check References** - All UI references assigned?
5. **Read Docs** - Check README for solutions

---

## 🚀 Next Steps

- [ ] Customize quest rewards
- [ ] Adjust UI colors/styling
- [ ] Add more quest types (optional)
- [ ] Tune reset timers (optional)
- [ ] Add quest notifications (optional)
- [ ] Integrate with analytics (optional)

---

**Congratulations! Your quest system is ready for production! 🎮✨**

Print this checklist and check off items as you complete them!
