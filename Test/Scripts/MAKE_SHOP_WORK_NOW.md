# 🛒 Make Shop Button Work - 5 Minute Guide

Your shop button is fixed and won't crash, but it needs the UI prefabs to actually open. Here's how to create them:

## ⚠️ First: Delete the Broken GameObject

1. In **Hierarchy**, find: `MenuUI/UIChangeNicknameView/UIShopView`
2. **Delete it** (it's incomplete and in the wrong place)
3. **Save scene** (Ctrl+S)

---

## Step 1: Create UIShopItem Widget (2 minutes)

1. **Make sure you're in Menu scene** (`Assets/TPSBR/Scenes/Menu.unity`)

2. **Open the tool**: Unity menu → `TPSBR` → `Create Shop UI`

3. **Click**: `Create UIShopItem Widget` button

4. **In Inspector**, find the `UIShopItem` component and assign the fields:
   - **Agent Icon** → Drag the `AgentIcon` child GameObject
   - **Agent Name** → Drag the `AgentName` child GameObject  
   - **Cost Text** → Drag the `CostText` child GameObject
   - **Purchase Button** → Drag the `PurchaseButton` child GameObject
   - **Purchase Button Text** → Drag the `ButtonText` child (under PurchaseButton)

5. **Save as prefab**:
   - Drag `UIShopItem` from Hierarchy → Project window
   - Put it in: `Assets/TPSBR/UI/Prefabs/Widgets/`
   - Name it: `UIShopItem.prefab`

6. **Delete** `UIShopItem` from the Hierarchy (we only need the prefab)

---

## Step 2: Create UIShopView Panel (2 minutes)

1. **Still in the tool**: Click `Create UIShopView Panel` button

2. **In Inspector**, find the `UIShopView` component and assign:
   - **Shop Items List** → Drag the `ShopItemsList` child GameObject
   - **Cloud Coins Text** → Drag the `CloudCoinsText` child GameObject

3. **Still in ShopItemsList GameObject**, find the `UIList` component:
   - **Item Instance** → Drag the `UIShopItem.prefab` you created in Step 1

4. **Save as prefab**:
   - Drag `UIShopView` from Hierarchy → Project window
   - Put it in: `Assets/TPSBR/UI/Prefabs/MenuViews/`
   - Name it: `UIShopView.prefab`

5. **Delete** `UIShopView` from the Hierarchy

---

## Step 3: Add UIShopView to MenuUI (30 seconds)

**This is the critical step!**

1. In **Project** window, find: `Assets/TPSBR/UI/Prefabs/MenuViews/UIShopView.prefab`
2. **Drag** it onto `/MenuUI` in the **Hierarchy**
3. Make sure it's a **direct child** of `/MenuUI` (not nested under another view)
4. **Save scene** (Ctrl+S)

Your hierarchy should look like:
```
MenuUI
├── UIMainMenuView
├── UIAgentSelectionView
├── UIMultiplayerView
├── UISettingsView
├── UIChangeNicknameView
├── UIShopView  ← NEW! This should be here
└── ... other views
```

---

## Step 4: Configure Agent Prices (1 minute)

1. **Open the price tool**: Unity menu → `TPSBR` → `Shop System Setup Helper`

2. **Set prices**:
   - Soldier Cost: `0` (starts unlocked)
   - Marine Cost: `500` (must purchase)

3. **Click**: `Apply Agent Prices`

---

## ✅ Done! Test It

1. **Enter Play Mode**
2. **Click the Shop button**
3. **Shop should open!** 🎉

---

## Expected Result

- You'll see both agents (Soldier and Marine)
- Soldier shows "OWNED" or "SELECTED" (free)
- Marine shows "BUY" button with 500 CloudCoins cost
- You won't have enough coins yet (we'll add ways to earn them next)

---

## Troubleshooting

**Shop still won't open?**
- Check Console for the warning message
- Make sure prefabs are in the correct folders:
  - `UIShopItem.prefab` in `/Assets/TPSBR/UI/Prefabs/Widgets/`
  - `UIShopView.prefab` in `/Assets/TPSBR/UI/Prefabs/MenuViews/`
- **Most important**: Make sure you dragged `UIShopView.prefab` onto `/MenuUI` in the scene!

**Can't see shop items?**
- Make sure you linked `UIShopItem.prefab` to the UIList component

**Prices not showing?**
- Run the Shop System Setup Helper again

**NullReferenceException in UIList?**
- You forgot to link `UIShopItem.prefab` to the UIList's "Item Instance" field

---

## Understanding How It Works

The MenuUI system finds all views using `GetComponentsInChildren<UIView>()`, which means **all UI views must be children of `/MenuUI`** in the scene.

See `/Assets/Scripts/HOW_SHOP_UI_WORKS.md` for detailed explanation!

---

## Next Steps

After the shop works, you'll want to add ways to earn CloudCoins:

1. Use the `CloudCoinReward` component (already created)
2. Or use `ShopSystemDebugHelper` to add coins for testing

See `/Assets/Scripts/SHOP_SYSTEM_COMPLETE.md` for full details!
