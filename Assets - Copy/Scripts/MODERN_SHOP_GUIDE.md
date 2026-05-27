# 🎨 Modern Shop System - Complete Guide

## ✨ What's New

A completely redesigned shop system featuring:
- **Sleek card-based UI** with modern design
- **Skin Rarity System** (Common, Rare, Epic, Legendary, Mythic)
- **Dynamic rarity colors** and glow effects
- **Clean architecture** - no more NullReferenceExceptions!
- **Auto-population** from your existing CharacterData

## 🎯 Rarity System

### Rarity Tiers
1. **Common** - Gray (0.7, 0.7, 0.7)
2. **Rare** - Blue (0.2, 0.5, 1.0)
3. **Epic** - Purple (0.6, 0.2, 0.9)
4. **Legendary** - Orange (1.0, 0.6, 0.0)
5. **Mythic** - Red (1.0, 0.2, 0.3)

### Setting Rarities
1. Select any CharacterData asset (e.g., `marine.asset`, `soldier66.asset`)
2. In the Inspector, find the **Rarity** dropdown
3. Choose the rarity tier
4. The shop will automatically apply colored borders and glows!

## 🚀 Quick Setup

### Step 1: Create the Modern Shop UI
1. Open Unity Editor
2. Go to menu: **TPSBR → 🎨 Create Modern Shop UI**
3. This will:
   - Create a `ModernShop` GameObject in your Menu scene
   - Generate the shop card prefab at `Assets/Prefabs/ModernShopCard.prefab`
   - Auto-wire all references
   - Save the scene

### Step 2: Configure Character Rarities
1. Open `Assets/Scripts/CharacterData/marine.asset`
   - Set **Rarity** to `Legendary` or `Epic`
   - Set **Price** to something like `750`
   
2. Open `Assets/Scripts/CharacterData/soldier66.asset`
   - Set **Rarity** to `Common` or `Rare`
   - Keep **Price** at `0` (free)
   - Keep **Unlocked By Default** checked

### Step 3: Open the Shop
Add the `ModernShopToggle` script to the `ModernShop` GameObject:
1. Select `ModernShop` in the Hierarchy
2. Add Component → `ModernShopToggle`
3. Drag the `ModernShop` CanvasGroup to the `Shop Canvas Group` field
4. Set **Toggle Key** to `B` (or any key you prefer)

### Step 4: Test It!
1. Press **Play ▶️**
2. Press **B** to open the shop
3. You should see:
   - Character cards with icons
   - Rarity-colored borders and glows
   - Buy buttons with prices
   - Your current CloudCoins

## 📦 Component Breakdown

### ModernShopManager
The main shop controller that:
- Loads characters from `ShopDatabase`
- Creates shop cards dynamically
- Handles purchases and selections
- Updates coin display
- Sorts by rarity (highest first)

### ModernShopCard
Individual card component that:
- Displays character info
- Shows rarity with colored border/glow
- Updates button states (BUY/SELECT/SELECTED)
- Handles button clicks
- Shows owned/selected badges

### CharacterData (Enhanced)
Your existing ScriptableObject now includes:
- `SkinRarity rarity` - The rarity tier
- `GetRarityColor()` - Returns the color for this rarity
- `GetRarityText()` - Returns "COMMON", "RARE", etc.

## 🎨 Customization

### Changing Card Appearance
Edit the prefab at `Assets/Prefabs/ModernShopCard.prefab`:
- Card size: Adjust RectTransform
- Colors: Edit Image components
- Fonts: Swap TextMeshProUGUI fonts
- Layout: Move child objects

### Adding More Characters
1. Right-click in Project → **Create → TPSBR → Character Data**
2. Set the **Character ID**, **Display Name**, **Icon**, **Rarity**, **Price**
3. Add it to your `ShopDatabase.asset` characters list
4. Cards will automatically appear in the shop!

### Custom Rarity Colors
Edit `CharacterData.cs`, find the `GetRarityColor()` method:
```csharp
public Color GetRarityColor()
{
    return rarity switch
    {
        SkinRarity.Common => new Color(0.7f, 0.7f, 0.7f),     // Change these!
        SkinRarity.Rare => new Color(0.2f, 0.5f, 1f),
        // ... etc
    };
}
```

### Changing Grid Layout
Select `ModernShop/Content/ScrollView/Viewport/Grid`:
- **Cell Size** - Card dimensions
- **Spacing** - Gap between cards
- **Constraint Count** - Cards per row

## 🔧 Integration

### Opening from Existing UI
If you have a shop button in `UIShopView` or another menu:

```csharp
// Get reference to ModernShop
GameObject modernShop = GameObject.Find("ModernShop");
ModernShopToggle toggle = modernShop.GetComponent<ModernShopToggle>();

// Open it
toggle.OpenShop();

// Close it
toggle.CloseShop();
```

### Replacing Old Shop
Once you're happy with the new shop:
1. Delete or disable the old `UIShopView` GameObject
2. Wire your shop button to call `ModernShopToggle.OpenShop()`
3. Remove old shop scripts if needed

## ✅ Verification Checklist

After setup, verify:
- [ ] `ModernShop` GameObject exists in Menu scene
- [ ] `ModernShopCard.prefab` exists at `Assets/Prefabs/`
- [ ] `ShopDatabase.asset` is assigned to `ModernShopManager`
- [ ] Characters have rarities set
- [ ] Can open shop with toggle key
- [ ] Cards display with correct colors
- [ ] Can purchase and select characters
- [ ] Coins update correctly

## 🐛 Troubleshooting

**Shop is empty**
- Check that `ShopDatabase` has characters in the list
- Verify `ModernShopManager._shopDatabase` is assigned
- Check Console for errors

**Cards don't show rarities**
- Make sure you set the `rarity` field on CharacterData assets
- Re-open the shop to refresh

**Can't buy characters**
- Check `PlayerData.Instance` exists
- Verify `CoinSystem.CloudCoins` has enough funds
- Check `ShopSystem.IsCharacterOwned()` logic

**Cards look wrong**
- Edit `Assets/Prefabs/ModernShopCard.prefab`
- Check that all child objects are properly parented
- Verify `ModernShopCard` script references are assigned

## 🎮 Example Character Setup

```
Common Character (Soldier66):
- Rarity: Common
- Price: 0
- Unlocked By Default: ✓
- Result: Gray border, free, already owned

Legendary Character (Marine):
- Rarity: Legendary  
- Price: 750
- Unlocked By Default: ✗
- Result: Orange border with glow, costs 750 coins

Mythic Character (Future):
- Rarity: Mythic
- Price: 5000
- Unlocked By Default: ✗
- Result: Red border with intense glow, premium skin
```

## 📚 Architecture Benefits

Why this system is better:
1. **No UIList/UIListBase complexity** - Simple MonoBehaviour components
2. **Direct prefab instantiation** - No serialization issues
3. **Clean separation** - Manager, Card, and Data are independent
4. **Easy to extend** - Add new card features without touching existing code
5. **Visual clarity** - See exactly what's in the scene

## 🚀 Next Steps

Consider adding:
- **Animations** - Card hover effects, purchase animations
- **Filters** - Show only owned, sort by price/rarity
- **Preview** - 3D character preview on selection
- **Sound effects** - Purchase sounds, UI feedback
- **Particles** - Glow particles for rare items
- **Scroll snap** - Smooth scrolling between cards

---

**Created by:** Bezi AI Assistant  
**Date:** 2024  
**Compatible with:** Unity 6000.0, URP
