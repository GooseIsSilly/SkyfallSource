# Shop System Implementation Guide

## Overview
The shop system allows players to purchase agent skins using CloudCoins currency. By default, players start with the Soldier skin unlocked and must purchase the Marine skin from the shop.

## Components Created

### 1. CloudCoinSystem.cs
Manages the CloudCoins currency for each player.
- **Location**: `/Assets/Scripts/CloudCoinSystem.cs`
- **Properties**:
  - `CloudCoins`: Current coin amount
  - `IsDirty`: Tracks if data needs saving
- **Methods**:
  - `CanAfford(int cost)`: Check if player can afford an item
  - `TryPurchase(int cost)`: Attempt to purchase (returns true/false)
  - `AddCoins(int amount)`: Add coins (for quest rewards)

### 2. ShopSystem.cs
Manages owned and unlocked agent skins.
- **Location**: `/Assets/Scripts/ShopSystem.cs`
- **Properties**:
  - `OwnedSkins`: HashSet of owned agent IDs
- **Methods**:
  - `OwnsAgent(string agentID)`: Check if player owns an agent
  - `TryUnlockAgent(string agentID, int cost, CloudCoinSystem)`: Purchase an agent

### 3. UIShopView.cs
The main shop UI view that displays all available agents.
- **Location**: `/Assets/TPSBR/Scripts/UI/MenuViews/UIShopView.cs`
- **Features**:
  - Displays all available agents with icons and prices
  - Shows current CloudCoin balance
  - Handles purchase transactions
  - Updates UI when coins change

### 4. UIShopItem.cs
Individual shop item widget for each agent.
- **Location**: `/Assets/TPSBR/Scripts/UI/Widgets/UIShopItem.cs`
- **Features**:
  - Shows agent icon and name
  - Displays CloudCoin cost
  - Shows "BUY", "OWNED", or "SELECTED" status
  - Allows equipping owned agents or purchasing locked ones

### 5. CloudCoinReward.cs
Helper script for rewarding CloudCoins (quest system foundation).
- **Location**: `/Assets/Scripts/CloudCoinReward.cs`
- **Methods**:
  - `AddCloudCoins(int amount)`: Add custom amount
  - `AddCloudCoins100()`: Add 100 coins
  - `AddCloudCoins500()`: Add 500 coins
  - `AddCloudCoins1000()`: Add 1000 coins

## Updated Files

### PlayerData.cs
Added shop and coin system integration:
```csharp
public CloudCoinSystem CoinSystem => _coinSystem;
public ShopSystem ShopSystem => _shopSystem;
```

### AgentSettings.cs
Added CloudCoin pricing to AgentSetup:
```csharp
public int CloudCoinCost => _cloudCoinCost;
```

### UIAgentSelectionView.cs
Updated to only show owned agents instead of all agents.

### PlayerService.cs
Updated to initialize shop system and ensure default agent (Soldier) is owned.

## Setup Instructions

### 1. Configure Agent Prices
1. Open the AgentSettings ScriptableObject (usually in `/Assets/TPSBR/Resources/Settings`)
2. For each agent in the Agents array:
   - Set "Soldier" CloudCoinCost to `0` (free/starter)
   - Set "Marine" CloudCoinCost to desired price (e.g., `500`)

### 2. Create Shop UI in Menu Scene
1. Open the Menu scene
2. Create a new UI panel for the shop
3. Add the `UIShopView` component
4. Configure the following in the Inspector:
   - **Shop Items List**: Reference a UIList component
   - **Cloud Coins Text**: Reference a TextMeshProUGUI for displaying coins
   - **Cloud Coins Format**: Set to "CloudCoins: {0}" or customize
   - **Purchase Sound**: Optional AudioSetup for purchase success
   - **Insufficient Funds Sound**: Optional AudioSetup for failed purchase

### 3. Create Shop Item Prefab
1. Create a prefab for shop items with:
   - Agent Icon (Image)
   - Agent Name (TextMeshProUGUI)
   - Cost Text (TextMeshProUGUI)
   - Purchase Button (UIButton)
   - Button Text (TextMeshProUGUI)
   - Owned Indicator (GameObject, shown when owned)
2. Add the `UIShopItem` component
3. Assign all serialized fields in the Inspector

### 4. Add Shop Button to Main Menu
1. Create a button in the main menu UI
2. Link it to open the `UIShopView`

### 5. Testing CloudCoins
To test the system, you can:
- Add the `CloudCoinReward` component to a GameObject
- Call `AddCloudCoins100()`, `AddCloudCoins500()`, etc. from buttons or code
- Later, integrate with your quest system

## Default Configuration
- **Starting CloudCoins**: 100 (set in CloudCoinSystem.cs)
- **Default Owned Agent**: Soldier (set in ShopSystem.cs Initialize())
- **Marine Cost**: Configure in AgentSettings

## Quest System Integration (Future)
When implementing quests, use the CloudCoinReward script or call:
```csharp
Global.PlayerService.PlayerData.CoinSystem.AddCoins(rewardAmount);
```

## Important Notes
- Player data is automatically saved when modified
- The shop system ensures players always have at least one owned agent (Soldier)
- If a player's selected agent becomes unavailable, they'll be switched to Soldier
- Marine will be locked by default and must be purchased in the shop
