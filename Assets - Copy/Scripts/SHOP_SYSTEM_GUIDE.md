# Shop System - Setup Guide

## Overview

The new shop system uses a **ScriptableObject-based** approach that makes it super easy to add and configure characters without touching code!

## System Architecture

### Core Components

1. **CharacterData** - Individual character configuration (icon, price, model reference)
2. **ShopDatabase** - Central database of all available characters
3. **UIShopView** - The shop UI that displays all characters
4. **UIShopItem** - Individual character UI card/widget
5. **CloudCoinSystem** - Currency management
6. **ShopSystem** - Ownership and purchase logic

## Quick Start Guide

### Step 1: Open the Character Setup Tool

1. In Unity, go to menu: `TPSBR → Character & Shop Setup`
2. This opens the Character Setup Tool window

### Step 2: Create Shop Database (First Time Only)

1. In the Character Setup Tool, click **"Create New Shop Database"**
2. This creates a `ShopDatabase.asset` in `/Assets/Scripts/`
3. The database is automatically assigned in the tool

### Step 3: Create Your Characters

For each character you want in the shop:

1. Fill in the character details:
   - **Character ID**: Unique identifier (e.g., "soldier", "marine") - used internally
   - **Display Name**: Name shown in the shop (e.g., "Elite Soldier", "Space Marine")
   - **Agent ID**: Must match the ID in `AgentSettings` (the actual game prefab ID)
   - **Icon**: Drag & drop a sprite for the character icon
   - **Price**: Cost in CloudCoins (0 = free)
   - **Unlocked by Default**: Check this for starting characters

2. Click **"Create Character Data Asset"**
3. The character is automatically added to the Shop Database!

### Step 4: Setup the UI

1. Open your Menu scene
2. Find or create the `UIShopView` GameObject
3. In the `UIShopView` component inspector:
   - Assign the **Shop Database** you created
   - Assign the **Shop Items List** (UIList component)
   - Assign the **Cloud Coins Text** (TextMeshProUGUI)
4. Make sure you have a `UIShopItem` prefab with all the required UI elements

## Character ID vs Agent ID

**Important Distinction:**

- **Character ID**: Internal identifier for the shop system (e.g., "soldier_premium")
- **Agent ID**: References the actual character prefab in `AgentSettings` (e.g., "Soldier")

These can be the same, but separating them allows you to:
- Have multiple shop entries for the same character (different skins)
- Use more descriptive shop IDs while keeping agent IDs simple

## Example Setup

### Example 1: Free Starting Character

```
Character ID: soldier_default
Display Name: Standard Soldier
Agent ID: Soldier
Icon: [SoldierIcon.png]
Price: 0
Unlocked by Default: ✓
```

### Example 2: Premium Character

```
Character ID: marine_elite
Display Name: Elite Marine
Agent ID: Marine
Icon: [MarineIcon.png]
Price: 500
Unlocked by Default: ✗
```

## Managing Characters

### Adding New Characters

Use the Character Setup Tool (fastest way) OR:

1. Right-click in Project window
2. Create → TPSBR → Character Data
3. Fill in all the fields
4. Manually add it to the Shop Database's character list

### Editing Characters

1. Find the CharacterData asset in `/Assets/Scripts/CharacterData/`
2. Edit values directly in the Inspector
3. Changes apply immediately!

### Removing Characters

1. Open the Shop Database
2. Remove the character from the list
3. (Optional) Delete the CharacterData asset

## Testing

1. Enter Play Mode
2. Open the shop
3. You should see all characters from the database
4. Characters marked "Unlocked by Default" should show as "OWNED"
5. Other characters show their price and "BUY" button

## Troubleshooting

### "Shop Database is not assigned"
- Select your UIShopView GameObject
- Assign the ShopDatabase in the inspector

### Characters don't appear
- Check that the Shop Database has characters in its list
- Validate the database using: TPSBR → Character & Shop Setup → "Validate Shop Database"

### Icons don't show
- Make sure sprites are assigned to CharacterData assets
- Check sprite import settings (Texture Type: Sprite 2D)

### Wrong character spawns when purchased
- Verify that the **Agent ID** in CharacterData matches the ID in `AgentSettings`
- Agent IDs are case-sensitive!

### Player starts with no characters
- At least one character should have "Unlocked by Default" checked
- Check the Console for warnings from Shop Database validation

## UI Customization

The `UIShopItem` has many customizable fields:

- **Cost Format**: How prices are displayed (e.g., "{0} CloudCoins")
- **Free Text**: Text shown for free items
- **Button Text**: "BUY", "OWNED", "SELECTED" labels
- **Colors**: Can afford vs cannot afford colors

Edit these in the `UIShopItem` prefab to match your game's style!

## Advanced: Custom Validation

The Shop Database automatically validates:
- Duplicate character IDs
- Missing character references
- Missing icons
- At least one default unlocked character

Check the Console after making changes to catch setup errors early!
