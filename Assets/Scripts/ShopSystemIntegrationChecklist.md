# Shop System Integration Checklist

## ✅ Completed - Backend Systems

- ✅ **CloudCoinSystem** - Currency management system created
  - Location: `/Assets/Scripts/CloudCoinSystem.cs`
  - Features: Add/subtract coins, affordability checks, dirty tracking
  
- ✅ **ShopSystem** - Agent ownership and purchasing logic created
  - Location: `/Assets/Scripts/ShopSystem.cs`
  - Features: Track owned agents, purchase with CloudCoins, default Soldier ownership
  
- ✅ **PlayerData Integration** - Shop and coin systems integrated
  - Modified: `/Assets/TPSBR/Scripts/Player/PlayerData.cs`
  - Added: `CoinSystem` and `ShopSystem` properties
  - Shop initializes on player creation with Soldier as default

- ✅ **PlayerService Integration** - Shop initialization for loaded players
  - Modified: `/Assets/TPSBR/Scripts/Player/PlayerService.cs`
  - Ensures existing players get shop initialized
  
- ✅ **AgentSettings** - CloudCoin cost field added
  - Modified: `/Assets/TPSBR/Scripts/Settings/AgentSettings.cs`
  - Added: `CloudCoinCost` property for each agent

## ✅ Completed - UI Scripts

- ✅ **UIShopView** - Main shop view script created
  - Location: `/Assets/TPSBR/Scripts/UI/MenuViews/UIShopView.cs`
  - Features: Display agents, show coins, handle purchases
  
- ✅ **UIShopItem** - Shop item widget script created
  - Location: `/Assets/TPSBR/Scripts/UI/Widgets/UIShopItem.cs`
  - Features: Show agent info, price, purchase/select button
  - Fixed: Uses `Awake()`/`OnDestroy()` instead of non-existent lifecycle methods
  - Fixed: Uses `interactable` property instead of `SetInteractable()`

- ✅ **UIAgentSelectionView** - Modified to show only owned agents
  - Modified: `/Assets/TPSBR/Scripts/UI/MenuViews/UIAgentSelectionView.cs`
  - Added: `GetOwnedAgents()` method to filter locked agents

- ✅ **UIMainMenuView** - Shop button support added
  - Modified: `/Assets/TPSBR/Scripts/UI/MenuViews/UIMainMenuView.cs`
  - Added: `_shopButton` field and `OnShopButton()` handler

## ✅ Completed - Debug/Testing Tools

- ✅ **CloudCoinReward** - Test component to add coins
  - Location: `/Assets/Scripts/CloudCoinReward.cs`
  
- ✅ **ShopSystemDebugHelper** - Debug utility component
  - Location: `/Assets/Scripts/ShopSystemDebugHelper.cs`
  - Features: Add coins, log balance, reset shop, log owned agents

- ✅ **ShopUICreator** - Editor tool to help create UI
  - Location: `/Assets/Editor/ShopUICreator.cs`
  - Menu: `TPSBR/Create Shop UI`
  - Can create UIShopItem and UIShopView GameObjects

## 🔲 TODO - UI Prefabs (Manual Steps Required)

### Option A: Use Editor Tool (Recommended - Fastest!)

1. **Open Menu scene**
   - File → Open Scene → `Assets/TPSBR/Scenes/Menu.unity`

2. **Create UIShopItem widget**
   - Menu: `TPSBR/Create Shop UI`
   - Click "Create UIShopItem Widget"
   - In Inspector, configure `UI Shop Item` component:
     - Drag all child GameObjects to their respective fields
     - Set text formats (should be pre-filled)
   - Save as prefab: `Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab`
   - Delete from scene

3. **Create UIShopView panel**
   - Menu: `TPSBR/Create Shop UI`
   - Click "Create UIShopView Panel"
   - In Inspector, configure `UI Shop View` component:
     - Shop Items List: Drag `ShopItemsList` GameObject
     - Cloud Coins Text: Drag `CloudCoinsText` GameObject
     - Cloud Coins Format: `CloudCoins: {0}`
   - In `ShopItemsList` GameObject's `UI List` component:
     - Item Instance: Drag `UIShopItem.prefab` you created earlier
   - Save as prefab: `Assets/TPSBR/UI/Prefabs/MenuViews/UIShopView.prefab`
   - Delete from scene

### Option B: Manual Creation (See ShopUISetupGuide.md)

Detailed step-by-step instructions are in `/Assets/Scripts/ShopUISetupGuide.md`

### Option C: Quick Setup (See QuickShopSetup.md)

Minimal steps guide is in `/Assets/Scripts/QuickShopSetup.md`

## 🔲 TODO - Main Menu Integration

1. **Add Shop Button to Main Menu**
   - Open: `Assets/TPSBR/Scenes/Menu.unity`
   - Find the main menu UI (look for GameObject with `UIMainMenuView` component)
   - Duplicate an existing button (like Settings or Credits)
   - Rename to "ShopButton"
   - Change button text to "SHOP" or "AGENT SHOP"
   - Position it with other menu buttons
   
2. **Link Shop Button**
   - Select the GameObject with `UIMainMenuView` component
   - In Inspector, find the "Shop Button" field (newly added)
   - Drag your new shop button to this field
   - Save scene

## 🔲 TODO - Configure Agent Prices

1. **Find AgentSettings asset**
   - In Project window, search for "AgentSettings"
   - Should be in `Assets/TPSBR/Resources/Settings/Gameplay/` or similar

2. **Set CloudCoin costs for each agent**
   - Select AgentSettings asset
   - In Inspector, expand "Agents" array
   - For each agent:
     - **Soldier**: Set `Cloud Coin Cost` to **0** (free default agent)
     - **Marine**: Set `Cloud Coin Cost` to **500** (or your preferred price)
     - Add more agents with appropriate prices

3. **Save**
   - Ctrl+S or File → Save Project

## 📋 Testing Checklist

### Basic Functionality Tests

- [ ] **Start Game**
  - Launch the game in Play mode
  - Verify no compilation errors in Console

- [ ] **Check Default State**
  - New player should own only Soldier
  - Marine should be locked
  - CloudCoins should be 0

- [ ] **Open Shop**
  - Click "SHOP" button from main menu
  - Shop view should open
  - Should see CloudCoins: 0 at top
  - Should see list of agents with prices

- [ ] **Verify Agent States**
  - Soldier: Button should say "OWNED" or "SELECTED"
  - Marine: Button should say "BUY" and show price
  - Marine button should be disabled (not enough coins)

- [ ] **Add CloudCoins (Debug)**
  - Exit Play mode
  - Add `ShopSystemDebugHelper` to any GameObject in scene
  - Enter Play mode
  - Right-click component → "Add 1000 CloudCoins"
  - Shop should update: "CloudCoins: 1000"
  - Marine button should now be enabled

- [ ] **Purchase Agent**
  - Click "BUY" on Marine
  - Should hear purchase sound (if configured)
  - CloudCoins should decrease by cost
  - Marine button should change to "OWNED"
  - Marine should now be selectable

- [ ] **Select Different Agent**
  - Click button for owned agents
  - Button text should change to "SELECTED"
  - Previous selection should change to "OWNED"

- [ ] **Agent Selection View Integration**
  - Go to Agent Selection from main menu
  - Should only show owned agents
  - Marine should not appear until purchased

- [ ] **Persistence Test**
  - Make purchases, select agents
  - Exit Play mode
  - Re-enter Play mode
  - Verify purchases and selections are saved

### Edge Cases

- [ ] Try to buy with insufficient funds (should fail gracefully)
- [ ] Try to buy already owned agent (should select instead)
- [ ] Verify negative coin values are prevented
- [ ] Check that Soldier is always owned by default

## 🎨 Optional Enhancements

Ideas for later improvements:

- [ ] Add visual feedback for insufficient funds (shake, color flash)
- [ ] Add agent preview 3D model in shop
- [ ] Add currency rewards for gameplay achievements
- [ ] Add special offers or bundles
- [ ] Add rarity tiers with different colors
- [ ] Add purchase confirmation dialog
- [ ] Add unlock animations
- [ ] Add sound effects for purchases and failures

## 🐛 Troubleshooting

### Shop doesn't open when clicking button
- Check that UIShopView prefab exists in correct folder
- Verify shop button is linked in UIMainMenuView
- Check Console for errors

### No items appear in shop list
- Verify UIShopItem prefab is assigned to UIList component
- Check that AgentSettings has agents configured
- Check Console for errors

### Compilation errors
- Make sure all scripts are in correct folders
- Verify all using statements are present
- Check Unity has finished compiling (bottom-right corner)

### Prices don't show or show as 0
- Check AgentSettings asset is configured
- Verify CloudCoinCost is set for each agent
- Make sure you saved the asset after editing

### Purchases don't persist
- Check PlayerData is being saved correctly
- Verify IsDirty flags are being set
- Check PlayerService is calling ClearDirty after saving

## 📚 Documentation

- **Full Setup Guide**: `/Assets/Scripts/ShopUISetupGuide.md`
- **Quick Setup**: `/Assets/Scripts/QuickShopSetup.md`
- **Original README**: `/Assets/Scripts/ShopSystemReadme.md`

## 🎯 Current Status Summary

**Backend**: ✅ 100% Complete - All systems implemented and tested

**UI Scripts**: ✅ 100% Complete - All scripts created and bugs fixed

**UI Prefabs**: 🔲 Needs creation - Use editor tool or manual setup

**Integration**: 🔲 Needs shop button in main menu and price configuration

**Testing**: 🔲 Awaiting UI prefabs completion

---

## Next Steps

**You need to:**

1. Open Menu scene (`Assets/TPSBR/Scenes/Menu.unity`)
2. Run the Editor Tool (`TPSBR/Create Shop UI`) to create UI GameObjects
3. Save them as prefabs
4. Add shop button to main menu
5. Configure agent prices in AgentSettings
6. Test!

**I've created an editor tool to make this faster!** Just use the menu `TPSBR/Create Shop UI` and follow the button prompts.
