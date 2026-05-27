# ⚡ Quest System - ULTRA QUICK START

## 🎯 Get Up and Running in 5 Minutes!

---

## Step 1: Open the Setup Wizard (30 seconds)

1. In Unity, go to: **`TPSBR > Quest System Setup Wizard`**
2. The wizard window will open

---

## Step 2: Create Core Systems (30 seconds)

1. Click **"Create Quest System Initializer in Current Scene"**
2. Done! ✅ Core systems ready

---

## Step 3: Generate UI Automatically (1 minute)

1. Make sure **Menu scene is open**
2. In the Setup Wizard, click **"AUTO-GENERATE QUEST UI (One Click!)"**
3. Click "Generate" when prompted
4. Wait for success message
5. Done! ✅ Complete UI created automatically!

**What this creates:**
- ✅ UIQuestView in Menu scene
- ✅ QuestItemPrefab in /Assets/Prefabs/
- ✅ Quests button in main menu
- ✅ All references auto-linked!

---

## Step 4: Save the Scene! (10 seconds)

**IMPORTANT:** Press `Ctrl+S` (or `Cmd+S` on Mac) to save the Menu scene!

---

## Step 5: Test It! (2 minutes)

1. **Enter Play Mode**
2. **Check Console** - Look for:
   - `[Quest System] Initialization complete!`
   - `[Quest System] X quests assigned for today`
3. **Click "QUESTS" button** in main menu
4. **See your quests!** 4-5 daily quests should be displayed
5. **Test Quest Progress:**
   - In Hierarchy, find and select `QuestSystemInitializer`
   - Right-click → Select **"Test Complete Quest"**
   - Click Quests button again
   - See the quest completed with CLAIM button! 💰

---

## Step 6: Add Game Integration (Optional - 15 minutes)

To make quests track REAL gameplay, add these 3 simple lines to your game code:

### A. Match Start
In `BattleRoyaleGameplayMode.cs`, find `OnActivate()`:
```csharp
protected override void OnActivate()
{
    base.OnActivate();
    
    // ADD THIS LINE:
    QuestIntegrationPatches.PatchGameplayModeActivated();
    
    // ... rest of code
}
```

### B. Match End
In `BattleRoyaleGameplayMode.cs`, find `FinishGameplay()`:
```csharp
protected override void FinishGameplay()
{
    // ... existing finish code ...
    
    // ADD THESE LINES:
    if (HasInputAuthority || HasStateAuthority)
    {
        var localPlayer = Context?.NetworkGame?.GetPlayer(Runner.LocalPlayer);
        if (localPlayer != null)
        {
            var stats = localPlayer.Statistics;
            QuestIntegrationPatches.PatchGameplayModeFinished(
                stats.Position, 
                Context.NetworkGame.ActivePlayers.Count, 
                stats.Position == 1
            );
        }
    }
    
    base.FinishGameplay();
}
```

### C. Combat Tracking
In `Health.cs`, find where damage is applied:
```csharp
public void ProcessHit(ref HitData hitData)
{
    // ... existing damage code ...
    
    // ADD THIS LINE:
    QuestIntegrationPatches.PatchHealthDamage(this, hitData);
    
    // ... rest of method ...
}
```

**That's it! Quests will now track matches, combat, and more!** 🎮

---

## 🎉 Done!

### You Now Have:
✅ **21 working quests**  
✅ **Auto-generated UI**  
✅ **CloudCoin rewards**  
✅ **Progress tracking**  
✅ **Persistent saves**  
✅ **Daily rotation**

---

## 🧪 Quick Test Checklist

- [ ] Quest System Initializer exists in Menu scene
- [ ] UIQuestView exists in MenuUI
- [ ] Quests button appears in main menu
- [ ] Clicking Quests opens quest window
- [ ] 4-5 quests are displayed
- [ ] Quest names, descriptions, rewards shown
- [ ] Progress bars visible
- [ ] Test quest completion works
- [ ] CLAIM button appears and works
- [ ] CloudCoins increase when claimed

**All checked? You're ready to go! 🚀**

---

## ⚠️ Troubleshooting

### Issue: "UIQuestView already exists"
**Solution:** Click "Yes" to replace it with the new version

### Issue: "Could not find MenuUI"
**Solution:** Make sure Menu scene (`Assets/TPSBR/Scenes/Menu.unity`) is open

### Issue: Quests not showing
**Solution:** 
1. Use Setup Wizard → "Validate Quest System Setup"
2. Check Console for errors
3. Verify UIQuestView has all references assigned

### Issue: Button doesn't work
**Solution:** 
1. Select the Quests button in Hierarchy
2. Check Button component has onClick event
3. Re-run "AUTO-GENERATE QUEST UI" to fix

---

## 📚 Need More Help?

- **Complete Guide:** `QUEST_SYSTEM_README.md`
- **Integration Examples:** `QUEST_INTEGRATION_GUIDE.md`
- **Full Reference:** `QUEST_SYSTEM_SUMMARY.md`
- **Step-by-Step:** `QUEST_SETUP_CHECKLIST.md`

---

## 🎮 What's Next?

1. **Play a match** and complete quests naturally
2. **Customize rewards** in `QuestManager.cs`
3. **Add more integration points** (see Integration Guide)
4. **Style the UI** to match your game's theme
5. **Ship it!** The system is production-ready

---

## ⏱️ Total Time: ~5 Minutes

| Step | Time |
|------|------|
| Open Wizard | 30s |
| Create Systems | 30s |
| Generate UI | 1m |
| Save Scene | 10s |
| Test | 2m |
| **TOTAL** | **~5 minutes** |

**Optional Integration:** +15 minutes

---

**That's it! You now have a complete quest system! 🎉**

Remember to **save your scene** after generating the UI!
