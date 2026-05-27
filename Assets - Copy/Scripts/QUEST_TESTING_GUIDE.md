# 🧪 Quest System Testing Guide

## Quick Start Testing

### 1. **Open Quest UI in Main Menu** ✅
**Steps:**
1. Launch game in Play Mode
2. In the main menu, click the **"Quests"** button
3. Quest UI should open showing all 22 quests

**Expected Result:**
- Quest list displays all quests grouped by type (Daily, Combat, Weekly, Progression, Special)
- Each quest shows: Title, Description, Progress Bar, Reward
- Main menu and agent selection hide while quest UI is open
- Close button works and restores main menu

---

### 2. **Test "First Drop" Quest** (Easiest)
**Quest:** Play 1 match (25 coins)

**Steps:**
1. Start a match from main menu
2. Join any game mode
3. Wait for match to end (you can die immediately)
4. Return to main menu
5. Open Quest UI

**Expected Result:**
- ✅ "First Drop" quest shows 1/1 progress
- ✅ Quest shows "Completed!" badge
- ✅ You receive 25 coins reward

**Console Logs to Look For:**
```
[Quest Hooks] Match started - Quest tracking active
[Quest Hooks] Match ended - Position: X/Y, Winner: false
[Quest Manager] Quest progression_veteran progress: 1/10
[Quest Manager] Quest daily_first_drop COMPLETED!
```

---

### 3. **Test "Scavenger" Quest** (Item Pickup)
**Quest:** Land and loot 3 different item boxes (40 coins)

**Steps:**
1. Start a match
2. Land from jetpack
3. Find and pick up 3 items (weapons, ammo, health packs)
4. Open quest UI (ESC → Quests or after match)

**Expected Result:**
- ✅ "Scavenger" progress shows 3/3
- ✅ Quest completes after 3rd pickup
- ✅ Reward: 40 coins

**Console Logs to Look For:**
```
[Quest Manager] Loot box collected. Progress: 1/3
[Quest Manager] Loot box collected. Progress: 2/3
[Quest Manager] Quest daily_scavenger COMPLETED!
```

---

### 4. **Test "First Blood" Quest** (Elimination)
**Quest:** Get your first elimination (100 coins)

**Steps:**
1. Start a match
2. Find and eliminate 1 enemy player
3. Check quest progress

**Expected Result:**
- ✅ "First Blood" shows 1/1
- ✅ Quest completes immediately
- ✅ Also updates "Elimination Spree" (5 total) to 1/5
- ✅ Also updates "Battle Royale Veteran" if using different weapons

**Console Logs to Look For:**
```
[Quest Hooks] Agent killed - Killer: PlayerRef(1), Victim: PlayerRef(2), Headshot: false
[Quest Manager] Elimination obtained. Progress: 1/1
[Quest Manager] Quest combat_first_blood COMPLETED!
```

---

### 5. **Test "Headhunter" Quest** (Headshot Kills)
**Quest:** Get 2 headshot eliminations (175 coins)

**Steps:**
1. Start a match
2. Get 2 kills with headshots (aim for the head)
3. Check quest progress

**Expected Result:**
- ✅ "Headhunter" shows 2/2 after 2nd headshot kill
- ✅ Each headshot also counts toward "First Blood" and "Elimination Spree"

**Console Logs to Look For:**
```
[Quest Hooks] Agent killed - Headshot: true
[Quest Manager] Headshot elimination! Progress: 1/2
[Quest Manager] Headshot elimination! Progress: 2/2
[Quest Manager] Quest combat_headhunter COMPLETED!
```

---

### 6. **Test "Marksman" Quest** (Deal Damage)
**Quest:** Deal 200 damage to enemies (125 coins)

**Steps:**
1. Start a match
2. Shoot enemies (doesn't need to kill them)
3. Deal 200+ total damage
4. Check quest progress

**Expected Result:**
- ✅ Progress bar fills as you deal damage
- ✅ Completes at 200/200 damage
- ✅ Shows partial progress like 50/200, 150/200, etc.

**Console Logs to Look For:**
```
[Quest Manager] Damage dealt: 25. Total: 25/200
[Quest Manager] Damage dealt: 30. Total: 55/200
[Quest Manager] Quest combat_marksman COMPLETED!
```

---

### 7. **Test "Survivor" Quest** (Survival Time)
**Quest:** Survive for 5 minutes in a match (50 coins)

**Steps:**
1. Start a match
2. Stay alive for 5 minutes (300 seconds)
3. Don't die early
4. Check progress periodically

**Expected Result:**
- ✅ Progress updates in real-time as time passes
- ✅ Shows time like 120/300 seconds, 240/300 seconds
- ✅ Completes at 300/300 seconds (5 minutes)

**Console Logs to Look For:**
```
[Quest Manager] Survival time: 60 seconds
[Quest Manager] Survival time: 120 seconds
[Quest Manager] Quest daily_survivor COMPLETED!
```

---

### 8. **Test "Distance Walker" Quest** (Travel Distance)
**Quest:** Travel 1000 meters on foot (35 coins)

**Steps:**
1. Start a match
2. Run around the map
3. Check progress as you move

**Expected Result:**
- ✅ Progress increases as you move
- ✅ Shows distance like 250/1000m, 500/1000m
- ✅ Completes at 1000/1000m

**Console Logs to Look For:**
```
[Quest Manager] Distance traveled: 100m. Total: 100/1000
[Quest Manager] Distance traveled: 250m. Total: 250/1000
[Quest Manager] Quest daily_distance_walker COMPLETED!
```

---

### 9. **Test "Zone Runner" Quest** (Storm Circles)
**Quest:** Survive 3 storm circles (75 coins)

**Steps:**
1. Start a match
2. Stay alive as storm shrinks
3. Survive through 3 storm circle transitions
4. Check progress after each circle

**Expected Result:**
- ✅ Progress: 1/3 after first circle shrink
- ✅ Progress: 2/3 after second circle
- ✅ Quest completes after 3rd circle: 3/3

**Console Logs to Look For:**
```
[Quest Manager] Storm circle survived: 1. Progress: 1/3
[Quest Manager] Storm circle survived: 2. Progress: 2/3
[Quest Manager] Quest daily_zone_runner COMPLETED!
```

---

### 10. **Test "Medic" Quest** (Healing Items)
**Quest:** Use healing items 10 times (100 coins)

**Steps:**
1. Start a match
2. Find health pickups or healing items
3. Get damaged and use healing items
4. Use 10 healing items total

**Expected Result:**
- ✅ Progress increases each time you use a health pickup
- ✅ Shows 1/10, 2/10, ... 10/10
- ✅ Shield items don't count (only healing)

**Console Logs to Look For:**
```
[Quest Manager] Healing item used. Progress: 1/10
[Quest Manager] Healing item used. Progress: 5/10
[Quest Manager] Quest special_medic COMPLETED!
```

---

### 11. **Test "Explorer" Quest** (Landing Locations)
**Quest:** Land in 5 different named locations (250 coins)

**Steps:**
1. Start a match
2. Land from jetpack in a named location
3. Check progress
4. In next matches, land in different locations
5. Complete 5 unique locations

**Expected Result:**
- ✅ First landing: 1/5
- ✅ Landing in same location again: still 1/5 (must be unique)
- ✅ Landing in new location: 2/5
- ✅ Quest completes after 5 unique locations

**Console Logs to Look For:**
```
[Quest Tracker] Player landed at: CityCenter
[Quest Manager] New landing location! Progress: 1/5
[Quest Manager] Quest progression_explorer COMPLETED!
```

---

### 12. **Test "Victory Royale" Quest** (Win Match)
**Quest:** Win 1 match (500 coins)

**Steps:**
1. Start a match
2. Be the last player/team standing
3. Win the match
4. Check quest after victory

**Expected Result:**
- ✅ "Victory Royale" completes: 1/1
- ✅ "Weekly Champion" progress: 1/3
- ✅ Large reward: 500 coins

**Console Logs to Look For:**
```
[Quest Hooks] Match ended - Winner: true
[Quest Manager] Match won! Victory Royale!
[Quest Manager] Quest weekly_victory_royale COMPLETED!
```

---

### 13. **Test "Perfect Game" Quest** (Win Without Storm Damage)
**Quest:** Win without taking storm damage (750 coins)

**Steps:**
1. Start a match
2. **NEVER step outside the safe zone**
3. Win the match without taking any storm damage
4. Check quest

**Expected Result:**
- ✅ If you took storm damage: quest does NOT complete
- ✅ If you didn't take storm damage AND won: quest completes
- ✅ Hardest special quest: 750 coins

**Console Logs to Look For:**
```
[Quest Manager] Match won without storm damage!
[Quest Manager] Quest special_perfect_game COMPLETED!
```

**To Fail This Quest (Testing):**
1. Step outside zone briefly
2. Take storm damage
3. Win the match anyway
4. Quest should NOT complete

---

## 🎯 Advanced Testing Scenarios

### Test Multiple Quests in One Match

**Objective:** Complete multiple quests in a single match

**Steps:**
1. Start match
2. Land in new location → +1 "Explorer"
3. Pick up 3 items → Complete "Scavenger"
4. Deal 200 damage → Complete "Marksman"
5. Get 1 kill → Complete "First Blood"
6. Survive 5 minutes → Complete "Survivor"
7. Travel 1000m → Complete "Distance Walker"
8. Survive 3 circles → Complete "Zone Runner"
9. Win match → Complete "Victory Royale"

**Expected Result:**
- ✅ 8 quests complete in ONE match!
- ✅ Huge coin reward
- ✅ All progress saved between matches

---

### Test Quest Persistence (Save/Load)

**Steps:**
1. Complete "First Drop" quest (1/1)
2. Open quest UI and verify completion
3. **Exit Play Mode**
4. **Re-enter Play Mode**
5. Open quest UI again

**Expected Result:**
- ✅ "First Drop" still shows as completed
- ✅ Progress saved via PlayerPrefs
- ✅ Coins saved and persisted

---

### Test Quest UI Real-Time Updates

**Steps:**
1. Start match
2. Open quest UI (ESC or pause menu)
3. Keep quest UI open
4. Pick up an item
5. Watch quest progress

**Expected Result:**
- ✅ Progress bar updates in real-time
- ✅ Numbers change immediately
- ✅ No need to close/reopen UI
- ✅ Quest completion shows immediately

---

## 🐛 Troubleshooting

### ❌ Quest Progress Not Updating

**Possible Causes:**
1. GameplayQuestHooks not initialized
2. Not the local player (in multiplayer)
3. Event hooks not registered

**Debug Steps:**
1. Check Console for `[Quest Hooks]` messages
2. Verify GameplayQuestHooks exists in hierarchy
3. Ensure QuestManager.Instance is not null
4. Check if you're the InputAuthority

---

### ❌ Quest UI Not Opening

**Possible Causes:**
1. UIQuestView GameObject is inactive
2. Quests button not wired up
3. Missing references in inspector

**Debug Steps:**
1. Find `/MenuUI/UIQuestView` in hierarchy
2. Check if GameObject is active
3. Run Quest UI Debugger tool (Tools → Quest Debugger)
4. Verify all inspector references are assigned

---

### ❌ Quests Complete But No Coins Awarded

**Possible Causes:**
1. Coin system not implemented yet
2. Reward not being applied in QuestManager

**Debug Steps:**
1. Check `QuestManager.CompleteQuest()` method
2. Verify coins are being added
3. Check coin display UI

---

### ❌ Progress Resets After Match

**Possible Causes:**
1. SaveProgress() not being called
2. PlayerPrefs not saving correctly
3. Quest reset logic triggering incorrectly

**Debug Steps:**
1. Check `QuestManager.SaveProgress()` calls
2. Look for PlayerPrefs save logs
3. Verify daily/weekly reset logic dates

---

## 📝 Expected Console Output (Full Match)

Here's what you should see in the console during a typical match:

```
[Quest Hooks] Match started - Quest tracking active
[Quest Tracker] Match started. Tracking active.
[Quest Manager] Match started. Active quests ready.

[Quest Hooks] Agent killed - Killer: PlayerRef(1), Victim: PlayerRef(2), Headshot: true
[Quest Manager] Elimination obtained. Progress: 1/1
[Quest Manager] Headshot elimination! Progress: 1/2
[Quest Manager] Quest combat_first_blood COMPLETED! Reward: 100 coins

[Quest Manager] Damage dealt: 50. Total: 50/200
[Quest Manager] Damage dealt: 75. Total: 125/200
[Quest Manager] Quest combat_marksman COMPLETED! Reward: 125 coins

[Quest Tracker] Player landed at: DowntownPlaza
[Quest Manager] New landing location! Progress: 1/5

[Quest Manager] Loot box collected. Progress: 1/3
[Quest Manager] Loot box collected. Progress: 2/3
[Quest Manager] Quest daily_scavenger COMPLETED! Reward: 40 coins

[Quest Manager] Storm circle survived: 1. Progress: 1/3
[Quest Manager] Storm circle survived: 2. Progress: 2/3
[Quest Manager] Quest daily_zone_runner COMPLETED! Reward: 75 coins

[Quest Manager] Survival time: 300 seconds
[Quest Manager] Quest daily_survivor COMPLETED! Reward: 50 coins

[Quest Hooks] Match ended - Position: 1/20, Winner: true
[Quest Manager] Match won! Victory Royale!
[Quest Manager] Quest weekly_victory_royale COMPLETED! Reward: 500 coins
[Quest Manager] Top 10 finish! Progress: 1/3

Total Rewards This Match: 890 coins!
Quests Completed: 7
```

---

## ✅ Final Checklist

Before submitting your quest system as complete, verify:

- [ ] All 22 quests defined in QuestManager
- [ ] All 6 gameplay hooks added to game scripts
- [ ] Quest UI opens from main menu
- [ ] Quest UI displays all quests with progress bars
- [ ] Quest UI updates in real-time during gameplay
- [ ] Quest UI closes and restores main menu
- [ ] Quest progress persists between Play Mode sessions
- [ ] Console shows quest tracking messages
- [ ] Quest completion awards coins/rewards
- [ ] Daily quests can complete in a single match
- [ ] Weekly quests track across multiple matches
- [ ] Progression quests accumulate over time
- [ ] Special quests have unique completion conditions
- [ ] Headshot kills are properly detected
- [ ] Storm damage is tracked correctly
- [ ] Landing locations are recorded
- [ ] Multiple quests can complete simultaneously
- [ ] Quest system works in multiplayer (local player only)
- [ ] No errors or warnings in console
- [ ] All quest types are represented

---

## 🎉 Success Criteria

Your quest system is **FULLY OPERATIONAL** when:

✅ You can complete "First Drop" by playing 1 match  
✅ You can complete "Scavenger" by picking up 3 items  
✅ You can complete "First Blood" by getting 1 kill  
✅ You can complete "Headhunter" by getting 2 headshot kills  
✅ You can complete "Marksman" by dealing 200 damage  
✅ You can complete "Survivor" by staying alive 5 minutes  
✅ You can complete "Distance Walker" by running 1000m  
✅ You can complete "Zone Runner" by surviving 3 circles  
✅ You can complete "Medic" by using 10 healing items  
✅ You can complete "Explorer" by landing in 5 locations  
✅ You can complete "Victory Royale" by winning a match  
✅ You can complete "Perfect Game" by winning without storm damage  

**And the quest UI shows your progress for ALL of them!** 🚀

---

## 🔥 Pro Tips

1. **Use Debug Logs:** Keep Console open to see quest updates in real-time
2. **Test Incrementally:** Test one quest type at a time
3. **Use Cheats/Editor Tools:** Speed up testing with instant kills, teleports, etc.
4. **Check Inspector:** Verify GameplayQuestHooks settings are enabled
5. **Test Multiplayer:** Make sure only YOUR player's actions count
6. **Save Often:** Quest progress saves automatically, but verify it works
7. **Read the Docs:** Refer to `QUEST_INTEGRATION_COMPLETE.md` for full details

Happy Testing! 🎮✨
