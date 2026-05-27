# ✅ Purchase System Fixed!

## Issues Fixed

### 1. ❌ Can't Buy Characters
**Problem:** The button click handler was calling BOTH purchase AND select callbacks every time, regardless of ownership status.

**Solution:** ✅ Updated `ModernShopCard.OnActionButtonClicked()` to check ownership first:
- If owned → call select callback
- If not owned → call purchase callback

### 2. ❌ Shop Button Opens Wrong View
**Problem:** The shop button was trying to open the old `UIShopView` instead of the new `ModernShopManager`.

**Solution:** ✅ Updated `UIMainMenuView.OnShopButton()` to:
- Try to open `ModernShopManager` first
- Automatically close `UIAgentSelectionView` if it's open
- Fall back to old `UIShopView` if ModernShop isn't found
- Show helpful error if neither exists

### 3. ✨ Added Close Button Support
**Problem:** No way to close the modern shop.

**Solution:** ✅ Changed `ModernShopManager` to inherit from `UICloseView`:
- Automatically wires up the close button
- Supports back navigation
- Properly cleans up on close

## What Changed

### Files Modified:
1. `/Assets/Scripts/ModernShopCard.cs`
   - Fixed `OnActionButtonClicked()` to check ownership
   - Store `_playerData` reference for ownership checks

2. `/Assets/TPSBR/Scripts/UI/MenuViews/UIMainMenuView.cs`
   - Updated `OnShopButton()` to open ModernShop
   - Auto-closes agent selection view

3. `/Assets/Scripts/ModernShopManager.cs`
   - Changed from `UIView` to `UICloseView`
   - Added close button support
   - Added `OnDeinitialize()` cleanup

4. `/Assets/Editor/CreateModernShop.cs`
   - Wires close button to ModernShopManager

## How It Works Now

### When You Click Shop Button:
1. ✅ Opens ModernShop view
2. ✅ Closes UIAgentSelectionView (if open)
3. ✅ Closes main menu
4. ✅ Shows shop with character cards

### When You Click a Card:
1. **If character is owned:**
   - Shows "SELECT" or "SELECTED" button
   - Clicking selects that character
   - Updates player's AgentID

2. **If character is NOT owned:**
   - Shows "BUY" button
   - Button is disabled if you don't have enough coins
   - Clicking purchases the character (deducts coins, unlocks character)
   - Automatically selects the newly purchased character
   - Updates all cards to reflect new state

### When You Click Close Button (X):
1. ✅ Closes the shop
2. ✅ Returns to main menu
3. ✅ Updates player preview with selected character

## Test It!

1. Press Play
2. Click **SHOP** button in main menu
3. You should see the modern shop open
4. Try purchasing a character (make sure you have coins!)
5. Click the X to close and return to menu

## Need Coins for Testing?

Add the `CloudCoinReward` component to any GameObject in your scene and call its method to give yourself coins, or use the debug helper script!

---

**Everything should work perfectly now!** 🎉
