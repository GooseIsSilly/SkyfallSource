# Shop System - Visual Overview

## System Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     SHOP SYSTEM FLOW                        │
└─────────────────────────────────────────────────────────────┘

1. SETUP (You Configure)
   ↓
┌──────────────────┐
│ CharacterData    │  ← You create one per character
│ Assets           │     (soldier.asset, marine.asset, etc.)
│                  │
│ • characterID    │
│ • displayName    │
│ • icon           │
│ • price          │
│ • agentID        │
└────────┬─────────┘
         │
         ↓ (Added to)
┌──────────────────┐
│ ShopDatabase     │  ← Central list of all characters
│                  │
│ • characters[]   │
│ • startingCoins  │
└────────┬─────────┘
         │
         ↓ (Referenced by)
┌──────────────────┐
│ UIShopView       │  ← Shop UI in Menu scene
│                  │
│ • shopDatabase   │
│ • itemsList      │
│ • coinsText      │
└────────┬─────────┘
         │
         ↓ (Creates)
┌──────────────────┐
│ UIShopItem       │  ← Individual character card (spawned)
│ (multiple)       │
│                  │
│ Displays:        │
│ • Icon           │
│ • Name           │
│ • Price          │
│ • Buy Button     │
└──────────────────┘

2. RUNTIME (Player Interaction)
   ↓
┌──────────────────┐
│ Player clicks    │
│ "BUY" button     │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ CloudCoinSystem  │  ← Checks if player has enough coins
│                  │
│ • CloudCoins     │
│ • TryPurchase()  │
└────────┬─────────┘
         │
         ↓ (If successful)
┌──────────────────┐
│ ShopSystem       │  ← Tracks ownership
│                  │
│ • OwnedSkins     │
│ • TryUnlock()    │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ PlayerData       │  ← Updates selected character
│                  │
│ • AgentID        │
│ • ShopSystem     │
│ • CoinSystem     │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ Game spawns      │  ← Uses AgentSettings to find prefab
│ character        │
└──────────────────┘
```

## Data Relationships

```
CharacterData (ScriptableObject)
├─ characterID: "marine_elite"     ← Shop system identifier
├─ displayName: "Elite Marine"     ← What players see
├─ agentID: "Marine"               ← Links to AgentSettings
├─ icon: [Sprite]                  ← Shop icon
├─ price: 500                      ← Cost in CloudCoins
└─ unlockedByDefault: false        ← Starting ownership

        ↓ Used by

ShopDatabase (ScriptableObject)
└─ characters: List<CharacterData>
   ├─ [0] soldier.asset
   ├─ [1] marine.asset
   └─ [2] sniper.asset

        ↓ Used by

UIShopView (MonoBehaviour)
├─ _shopDatabase → ShopDatabase
├─ _shopItemsList → UIList
│   └─ Spawns → UIShopItem for each character
└─ _cloudCoinsText → TextMeshProUGUI

        ↓ Each UIShopItem displays

UIShopItem (MonoBehaviour)
├─ _agentIcon → Shows characterData.icon
├─ _agentName → Shows characterData.displayName
├─ _costText → Shows characterData.price
├─ _purchaseButton → Triggers purchase
└─ _ownedIndicator → Shows if player owns it

        ↓ Purchase uses

PlayerData
├─ ShopSystem → Tracks owned characters
├─ CoinSystem → Manages CloudCoins
└─ AgentID → Currently selected character
```

## Key Concepts

### Character ID vs Agent ID

```
┌────────────────────────────────────────────────────────────┐
│  CharacterData                  AgentSettings              │
│  ──────────────                 ─────────────              │
│                                                             │
│  characterID: "soldier_red"  →  (Shop identifier)          │
│  agentID: "Soldier"          →  ID: "Soldier" (Match!)     │
│                                  Prefab: SoldierPrefab     │
│                                                             │
│  characterID: "soldier_blue" →  (Shop identifier)          │
│  agentID: "Soldier"          →  ID: "Soldier" (Same!)      │
│                                  Prefab: SoldierPrefab     │
│                                                             │
│  characterID: "marine"       →  (Shop identifier)          │
│  agentID: "Marine"           →  ID: "Marine" (Match!)      │
│                                  Prefab: MarinePrefab      │
└────────────────────────────────────────────────────────────┘

This allows:
• Multiple shop entries (skins) for same character
• Easy skin system without duplicating prefabs
```

### Ownership Flow

```
New Player
   ↓
Initialize ShopSystem
   ↓
Check if has owned characters → NO
   ↓
Auto-unlock characters with unlockedByDefault = true
   ↓
Player sees shop
   ├─ Free characters: "OWNED" button
   └─ Locked characters: "BUY" button (price shown)
   ↓
Player clicks "BUY"
   ↓
Check CloudCoins ≥ price?
   ├─ YES → Deduct coins, add to owned, refresh UI
   └─ NO → Play error sound, show insufficient funds
   ↓
Player clicks "OWNED" character
   ↓
Set as active character (AgentID)
   ↓
Game spawns character using AgentSettings
```

## File Organization

```
Project
├── Assets
│   ├── Scripts
│   │   ├── CharacterData/          ← All character configs
│   │   │   ├── soldier.asset
│   │   │   ├── marine.asset
│   │   │   └── sniper.asset
│   │   ├── ShopDatabase.asset      ← Main database
│   │   ├── CharacterData.cs        ← Character config class
│   │   ├── ShopDatabase.cs         ← Database class
│   │   ├── ShopSystem.cs           ← Ownership logic
│   │   └── CloudCoinSystem.cs      ← Currency system
│   │
│   ├── Editor
│   │   ├── CharacterDataEditor.cs  ← Setup tool
│   │   ├── CharacterDataInspector.cs
│   │   └── ShopDatabaseInspector.cs
│   │
│   ├── TPSBR/Scripts/UI
│   │   ├── MenuViews
│   │   │   └── UIShopView.cs       ← Shop panel
│   │   └── Widgets
│   │       └── UIShopItem.cs       ← Character card
│   │
│   └── Materials
│       └── CharacterIcons/         ← Sprite icons
│           ├── SoldierIcon.png
│           └── MarineIcon.png
```

## Quick Reference Commands

### Create Character
```
1. TPSBR → Character & Shop Setup
2. Fill in fields
3. Create Character Data Asset
```

### View All Characters
```
1. Find ShopDatabase.asset
2. Open in Inspector
3. See list with previews
```

### Validate Setup
```
1. Select ShopDatabase.asset
2. Click "Validate Database" button
3. Check Console for issues
```

### Test Shop
```
1. Enter Play Mode
2. Open Menu → Shop
3. Verify characters appear
4. Test purchasing
```

## Common Workflows

### Adding a New Character
```
1. Create icon sprite (512x512 recommended)
2. TPSBR → Character & Shop Setup
3. Fill in all fields
4. Create asset
5. Done! (Auto-added to database)
```

### Changing Character Price
```
1. Find character asset in CharacterData/
2. Change "price" field
3. Save
4. Immediately updated in game
```

### Making Character Free/Default
```
1. Find character asset
2. Set price = 0
3. Check "Unlocked by Default"
4. Save
```

### Adding Character Variant/Skin
```
1. Create new CharacterData
2. characterID: "soldier_elite"
3. agentID: "Soldier" (same as original!)
4. Different icon and price
5. Creates alternate shop entry for same character
```
