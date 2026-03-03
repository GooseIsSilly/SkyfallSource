# Shop System - Quick Setup Checklist ✅

## Initial Setup (Do Once)

### 1. Create Shop Database
- [ ] Open: `TPSBR → Character & Shop Setup`
- [ ] Click: **"Create New Shop Database"**
- [ ] Database created at: `/Assets/Scripts/ShopDatabase.asset`

### 2. Create Your First Character (Free/Default)
- [ ] In Character Setup Tool, fill in:
  - Character ID: `soldier`
  - Display Name: `Soldier`
  - Agent ID: `Soldier` (must match AgentSettings!)
  - Icon: Drag your soldier sprite
  - Price: `0`
  - Unlocked by Default: `✓` (checked)
- [ ] Click: **"Create Character Data Asset"**

### 3. Create Your Second Character (Purchasable)
- [ ] Fill in:
  - Character ID: `marine`
  - Display Name: `Marine`
  - Agent ID: `Marine` (must match AgentSettings!)
  - Icon: Drag your marine sprite
  - Price: `500`
  - Unlocked by Default: `☐` (unchecked)
- [ ] Click: **"Create Character Data Asset"**

### 4. Setup the Shop UI
- [ ] Open your Menu scene
- [ ] Find/Create `UIShopView` GameObject
- [ ] In Inspector, assign:
  - **Shop Database**: The database you created
  - **Shop Items List**: UIList component reference
  - **Cloud Coins Text**: TextMeshProUGUI reference
- [ ] Create `UIShopItem` prefab with required UI elements
- [ ] Assign UIShopItem prefab to UIList component

## Adding More Characters (Anytime)

For each new character:
- [ ] Open: `TPSBR → Character & Shop Setup`
- [ ] Fill in character details
- [ ] Click: **"Create Character Data Asset"**
- [ ] Done! It's automatically added to the database

## Testing

- [ ] Enter Play Mode
- [ ] Open the shop
- [ ] Verify:
  - All characters appear
  - Default character shows "OWNED"
  - Other characters show price and "BUY" button
  - Can purchase characters with enough coins
  - Cannot purchase with insufficient coins
  - Purchased characters show "OWNED"
  - Can select owned characters

## Common Setup Mistakes

### ❌ Agent ID doesn't match AgentSettings
**Problem**: Wrong character spawns or nothing spawns
**Solution**: Make sure `Agent ID` in CharacterData exactly matches the ID in `AgentSettings`

### ❌ No default unlocked characters
**Problem**: Players start with no characters
**Solution**: Set at least one character's "Unlocked by Default" to checked

### ❌ Shop Database not assigned in UIShopView
**Problem**: Shop is empty or crashes
**Solution**: Assign the ShopDatabase in UIShopView inspector

### ❌ UIShopItem prefab missing references
**Problem**: Characters show up but UI is broken
**Solution**: Make sure UIShopItem prefab has all UI elements assigned

## File Structure

After setup, you'll have:

```
/Assets
  /Scripts
    /CharacterData
      soldier.asset
      marine.asset
      (more characters...)
    ShopDatabase.asset
    CharacterData.cs
    ShopDatabase.cs
    ShopSystem.cs
    CloudCoinSystem.cs
  /TPSBR
    /Scripts
      /UI
        /MenuViews
          UIShopView.cs
        /Widgets
          UIShopItem.cs
  /Materials
    (character icons/sprites)
  /Prefabs
    (character models)
```

## Quick Validation

Run these checks anytime:
1. Open ShopDatabase asset
2. Click **"Validate Database"** button
3. Check Console for warnings/errors
4. Fix any issues reported

## Need Help?

Check these files for detailed information:
- `/Assets/Scripts/SHOP_SYSTEM_GUIDE.md` - Complete documentation
- `TPSBR → Character & Shop Setup` - Visual setup tool
- ShopDatabase Inspector - Shows summary and validation

## Pro Tips 💡

1. **Naming Convention**: Use lowercase IDs with underscores (e.g., `elite_marine`)
2. **Icon Size**: Use 512x512 or 256x256 sprites for crisp icons
3. **Price Balance**: Start with reasonable prices based on your game economy
4. **Test Early**: Create 2-3 characters first, test thoroughly, then add more
5. **Validation**: Run validation after any changes to catch errors early
