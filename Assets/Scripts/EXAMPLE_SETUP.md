# Complete Setup Example

This is a step-by-step walkthrough showing exactly how to set up 3 characters in your shop.

## Starting Fresh

Let's say you want to set up:
1. **Soldier** - Free starting character
2. **Marine** - Costs 500 CloudCoins
3. **Sniper** - Costs 1000 CloudCoins

## Step-by-Step Setup

### 1. Open the Character Setup Tool

```
Unity Menu Bar → TPSBR → Character & Shop Setup
```

You'll see a window with three sections:
- Shop Database
- Create New Character
- Quick Setup Actions

### 2. Create the Shop Database

First time only:

```
Click: "Create New Shop Database" button
```

Result:
- File created at: `/Assets/Scripts/ShopDatabase.asset`
- The tool automatically finds and assigns it
- You'll see "Characters in Database: 0"

### 3. Create the Soldier Character (Free)

In the "Create New Character" section, fill in:

```
Character ID:         soldier
Display Name:         Soldier
Agent ID:             Soldier
Icon:                 [Drag SoldierIcon.png from your Materials folder]
Price:                0
Unlocked by Default:  ✓ (checked)
```

Click: **"Create Character Data Asset"**

Result:
- File created: `/Assets/Scripts/CharacterData/soldier.asset`
- Automatically added to ShopDatabase
- You'll see "Characters in Database: 1"

### 4. Create the Marine Character (500 coins)

Clear the form and fill in:

```
Character ID:         marine
Display Name:         Marine
Agent ID:             Marine
Icon:                 [Drag MarineIcon.png]
Price:                500
Unlocked by Default:  ☐ (unchecked)
```

Click: **"Create Character Data Asset"**

Result:
- File created: `/Assets/Scripts/CharacterData/marine.asset`
- Added to database
- "Characters in Database: 2"

### 5. Create the Sniper Character (1000 coins)

Clear the form and fill in:

```
Character ID:         sniper
Display Name:         Sniper
Agent ID:             Sniper
Icon:                 [Drag SniperIcon.png]
Price:                1000
Unlocked by Default:  ☐ (unchecked)
```

Click: **"Create Character Data Asset"**

Result:
- File created: `/Assets/Scripts/CharacterData/sniper.asset`
- Added to database
- "Characters in Database: 3"

### 6. Verify the Database

```
Click: "Validate Shop Database" button
```

You should see in Console:
```
✓ No warnings (all good!)
```

If you see warnings, they'll tell you what to fix.

### 7. View Your Characters

```
In Project window:
  Find: Assets/Scripts/ShopDatabase.asset
  Select it
```

In the Inspector you'll see:
```
┌─────────────────────────────────────────┐
│ Shop Database Configuration             │
├─────────────────────────────────────────┤
│ Database Summary                        │
│   Total Characters: 3                   │
│   Default Unlocked: 1                   │
│   Total Cost: 1500 CloudCoins           │
├─────────────────────────────────────────┤
│ Starting Cloud Coins: 100               │
├─────────────────────────────────────────┤
│ Shop Characters:                        │
│                                         │
│ [Icon] Soldier                          │
│        FREE (Default)                   │
│                                         │
│ [Icon] Marine                           │
│        💰 500 CloudCoins                │
│                                         │
│ [Icon] Sniper                           │
│        💰 1000 CloudCoins               │
└─────────────────────────────────────────┘
```

### 8. Setup the UI (One Time)

Open Menu scene:
```
File → Open Scene → Assets/TPSBR/Scenes/Menu.unity
```

Find the UIShopView GameObject in Hierarchy:
```
Canvas/MenuUI/UIShopView
```

In the Inspector, assign references:

```
UIShopView Component:
  Shop Configuration:
    ┌─ Shop Database → [Drag ShopDatabase.asset here]
    
  UI References:
    ┌─ Shop Items List → [Reference to UIList GameObject]
    └─ Cloud Coins Text → [Reference to TextMeshProUGUI]
    
  Display Settings:
    └─ Cloud Coins Format: "CloudCoins: {0}"
    
  Audio:
    ┌─ Purchase Sound → [Your purchase sound]
    └─ Insufficient Funds Sound → [Your error sound]
```

Save the scene:
```
Ctrl + S (Windows) or Cmd + S (Mac)
```

### 9. Test It!

Enter Play Mode:
```
Press Play button
```

Open the shop:
```
Menu → Shop Button
```

You should see:
```
┌──────────────────── AGENT SHOP ────────────────────┐
│                                                     │
│              CloudCoins: 100                        │
│                                                     │
├─────────────────────────────────────────────────────┤
│                                                     │
│  [Icon]  Soldier                    ┌─────────────┐│
│          FREE                       │   OWNED     ││
│                                     └─────────────┘│
│                                                     │
│  [Icon]  Marine                     ┌─────────────┐│
│          500 CloudCoins             │    BUY      ││
│                                     └─────────────┘│
│                                                     │
│  [Icon]  Sniper                     ┌─────────────┐│
│          1000 CloudCoins (Red)      │    BUY      ││
│          (Greyed out - can't afford)└─────────────┘│
└─────────────────────────────────────────────────────┘
```

Try clicking:
- **OWNED** on Soldier → Should select the character
- **BUY** on Marine → Should deduct 500 coins, unlock it
- **BUY** on Sniper → Should play error sound (not enough coins)

After buying Marine:
```
CloudCoins: -400  (100 - 500 = -400... wait that's wrong!)
```

Actually, if you only have 100 coins, you CAN'T buy Marine yet!

Let me show you how to test with more coins...

### 10. Testing with Debug Coins

You have two options:

**Option A: Change Starting Coins**
```
1. Open ShopDatabase.asset
2. Change "Starting Cloud Coins" to 2000
3. Save
4. Test again
```

**Option B: Give Yourself Coins in Play Mode**
```
Create a debug script or use console commands
(depends on your existing debug system)
```

With 2000 coins, you can now:
- Buy Marine for 500 → You have 1500 left
- Buy Sniper for 1000 → You have 500 left
- All three characters show "OWNED"

## What You've Created

### File Structure
```
Assets/
  Scripts/
    CharacterData/
      soldier.asset      ← Soldier config
      marine.asset       ← Marine config
      sniper.asset       ← Sniper config
    ShopDatabase.asset   ← Main database
  
  Materials/
    SoldierIcon.png      ← Icon images
    MarineIcon.png
    SniperIcon.png
```

### In-Game Result

Players will:
1. Start with Soldier (free)
2. See Marine (500 coins) - can buy with starting 100 + earned coins
3. See Sniper (1000 coins) - need to earn more coins
4. Purchase unlocks characters
5. Click owned character to select it
6. Selected character spawns in game

## Customizing Your Setup

### Want Different Prices?
```
Open CharacterData assets:
  soldier.asset → price: 0
  marine.asset → price: 750 (changed from 500)
  sniper.asset → price: 1200 (changed from 1000)
Save changes → Immediately updated in game!
```

### Want More Starting Characters?
```
Open marine.asset:
  ✓ Check "Unlocked by Default"
  Save
Now both Soldier and Marine start unlocked!
```

### Want to Add a 4th Character?
```
TPSBR → Character & Shop Setup
Fill in form:
  Character ID: commando
  Display Name: Commando
  Agent ID: Commando
  Icon: CommandoIcon.png
  Price: 1500
  Unlocked: ☐
Create → Done!
Appears in shop automatically!
```

### Want Character Skins/Variants?
```
Create two CharacterData for same character:

soldier.asset:
  Character ID: soldier_default
  Agent ID: Soldier  ← Same!
  Price: 0
  
soldier_elite.asset:
  Character ID: soldier_elite
  Agent ID: Soldier  ← Same!
  Price: 300
  (Different icon)

Now shop shows both, but they spawn same prefab!
Perfect for cosmetic variants!
```

## Advanced: Linking to AgentSettings

The **Agent ID** field must match your AgentSettings:

```
AgentSettings.asset contains:
  Agents array:
    [0] ID: "Soldier"    Prefab: SoldierPrefab
    [1] ID: "Marine"     Prefab: MarinePrefab
    [2] ID: "Sniper"     Prefab: SniperPrefab

Your CharacterData must use these exact IDs:
  soldier.asset → agentID: "Soldier"  ✓ Matches!
  marine.asset → agentID: "Marine"    ✓ Matches!
  sniper.asset → agentID: "Sniper"    ✓ Matches!
```

If they don't match:
```
CharacterData:
  agentID: "soldier" (lowercase)

AgentSettings:
  ID: "Soldier" (capitalized)

Result: ❌ Character won't spawn!
```

## Next Steps

Now that you have the basic setup:

1. ✅ Add your own character icons
2. ✅ Balance the prices for your game
3. ✅ Configure starting CloudCoins amount
4. ✅ Add more characters as you create them
5. ✅ Customize the UI colors and text
6. ✅ Add sound effects for purchase/error
7. ✅ Implement ways for players to earn CloudCoins

The system is now super easy to expand!
