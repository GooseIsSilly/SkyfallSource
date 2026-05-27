# Connect Button References - Visual Guide

## 🎯 Current Issue

Your buttons are created, but not connected to the script! Let's fix that in 2 minutes.

## 📍 Where You Are Now

You have the `UIFortniteLobbyView` GameObject selected in the Hierarchy. Perfect!

## 🔧 Step-by-Step: Connect the Buttons

### 1. Make Sure Inspector is Visible

- Look at the right side of Unity Editor
- You should see the **Inspector** panel
- It should show the `UIFortniteLobbyView` component

### 2. Find the UIFortniteLobbyView Component

In the Inspector, scroll down until you see:

```
UIFortniteLobbyView (Script)
├─ Top Navigation Buttons
│  ├─ Shop Button         ⚠️ None (Button)
│  ├─ Quest Button        ⚠️ None (Button)
│  ├─ Locker Button       ⚠️ None (Button)
│  ├─ Battle Pass Button  ⚠️ None (Button)
│  └─ Settings Button     ⚠️ None (Button)
├─ Main Action Buttons
│  ├─ Play Button         ⚠️ None (Button)
│  └─ Play Button Text    ✅ (Already connected)
└─ Player Info
   ├─ Player Name Text    ✅ (Already connected)
   └─ Level Text          ✅ (Already connected)
```

### 3. Connect Each Button

For EACH button with ⚠️ None:

#### **Shop Button**
1. In **Hierarchy**, expand `UIFortniteLobbyView` → `TopNavigationBar`
2. Click and HOLD on `ShopButton`
3. Drag it to the **Shop Button** field in the Inspector
4. Release mouse - it should now show the button!

#### **Quest Button**
1. Same process: Drag `QuestButton` from Hierarchy
2. Drop it on **Quest Button** field in Inspector

#### **Locker Button**
1. Drag `LockerButton` from Hierarchy
2. Drop on **Locker Button** field

#### **Battle Pass Button**
1. Drag `BattlePassButton` from Hierarchy
2. Drop on **Battle Pass Button** field

#### **Settings Button**
1. Drag `SettingsButton` from Hierarchy
2. Drop on **Settings Button** field

#### **Play Button**
1. Drag `PlayButton` from Hierarchy (it's a direct child of UIFortniteLobbyView)
2. Drop on **Play Button** field

### 4. Verify All Connected

After connecting all buttons, your Inspector should look like:

```
UIFortniteLobbyView (Script)
├─ Top Navigation Buttons
│  ├─ Shop Button         ✅ ShopButton (Button)
│  ├─ Quest Button        ✅ QuestButton (Button)
│  ├─ Locker Button       ✅ LockerButton (Button)
│  ├─ Battle Pass Button  ✅ BattlePassButton (Button)
│  └─ Settings Button     ✅ SettingsButton (Button)
├─ Main Action Buttons
│  ├─ Play Button         ✅ PlayButton (Button)
│  └─ Play Button Text    ✅ Text (TextMeshProUGUI)
└─ Player Info
   ├─ Player Name Text    ✅ PlayerNameText (TextMeshProUGUI)
   └─ Level Text          ✅ LevelText (TextMeshProUGUI)
```

All ✅ = Ready to test!

## 🎮 Alternative Method: Use the Circle Icon

Instead of dragging from Hierarchy:

1. Click the small **circle icon** ⭕ next to each "None" field
2. A popup window appears showing all buttons in the scene
3. Double-click the correct button
4. It gets assigned!

Example:
```
Shop Button  [None (Button)] ⭕ ← Click this circle
```

Window shows:
```
Select Button
├─ ShopButton         ← Double-click this
├─ QuestButton
├─ LockerButton
└─ ...
```

## ✅ Quick Checklist

Connect these (in order):

- [ ] Shop Button → `TopNavigationBar/ShopButton`
- [ ] Quest Button → `TopNavigationBar/QuestButton`
- [ ] Locker Button → `TopNavigationBar/LockerButton`
- [ ] Battle Pass Button → `TopNavigationBar/BattlePassButton`
- [ ] Settings Button → `TopNavigationBar/SettingsButton`
- [ ] Play Button → `PlayButton`
- [ ] ✅ Play Button Text (already connected)
- [ ] ✅ Player Name Text (already connected)
- [ ] ✅ Level Text (already connected)

## 🎯 After Connecting

1. **Save** your scene (Ctrl+S or Cmd+S)
2. **Enter Play Mode** (press the Play button at the top of Unity)
3. **Test it!**
   - Click the PLAY button
   - Watch console for messages
   - Button should change to "SEARCHING..."

## 🐛 Troubleshooting

### "I can't find the buttons in Hierarchy"
- Make sure `UIFortniteLobbyView` is expanded (click the arrow ▶)
- Expand `TopNavigationBar` to see the 5 navigation buttons
- `PlayButton` is a direct child of `UIFortniteLobbyView`

### "Nothing happens when I drag"
- Make sure you're dragging a **Button** component (not a Text or Image)
- The field should highlight when you hover over it with the correct type
- Try using the circle icon method instead

### "The field won't accept the button"
- You might be dragging the wrong GameObject
- Make sure you're dragging the button itself (which has a Button component)
- Check that the button has a `Button` component attached

### "I accidentally assigned the wrong one"
- Click the field and press Delete/Backspace to clear it
- Or drag the correct button again to replace it

## 💡 Pro Tip

**Select Multiple in Inspector**: Hold Shift and click multiple fields to see if they're all properly assigned. Properly assigned fields will show the GameObject path.

## 🎓 Understanding What This Does

When you connect these references, you're telling the script:
- "When someone clicks THIS button, run THIS code"
- The script needs to know which button is which
- Without connections, clicks do nothing (no reference = can't listen for clicks)

## 🚀 Next Step

Once all buttons are connected → Go to **Play Mode** and test!

See `AFTER_SETUP_CHECKLIST.md` for what to do next.
