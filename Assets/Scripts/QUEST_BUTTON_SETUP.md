# Quest Button Setup Guide

## ✅ UIMainMenuView Script Updated!

I've added quest button support to the `UIMainMenuView.cs` script. Now you need to wire it up in the Unity Editor.

---

## 🔧 What Changed in UIMainMenuView.cs

### Added:
1. ✅ `_questsButton` field (SerializeField)
2. ✅ `OnQuestsButton()` method - Opens the quest view
3. ✅ Event listener registration in `OnInitialize()`
4. ✅ Event listener cleanup in `OnDeinitialize()`

---

## 📝 How to Connect the Button in Unity Editor

### **Step 1: Create or Find Your Quests Button**

You have two options:

#### **Option A: Use Auto-Generator (Recommended)**
1. Go to `TPSBR > Generate Quest UI`
2. Click "Generate"
3. The button will be created automatically! ✨
4. Skip to Step 3

#### **Option B: Manual Setup**
1. In the Hierarchy, find `MenuUI/UIMainMenuView`
2. Duplicate the `ShopButton` GameObject
3. Rename it to `QuestsButton`
4. Change the label text to "QUESTS"
5. Continue to Step 2

---

### **Step 2: Wire Up the Button Reference (Manual Only)**

If you created the button manually:

1. **Select** `UIMainMenuView` GameObject in Hierarchy
2. **Look at Inspector** - find the `UI Main Menu View` component
3. **Find the field** `Quests Button` (it should be empty)
4. **Drag** your `QuestsButton` GameObject into this field
5. **Save the scene** (Ctrl+S)

---

### **Step 3: Generate Quest UI (If Not Done Yet)**

1. Make sure **Menu scene is open**
2. Go to `TPSBR > Generate Quest UI`
3. Click "Generate"
4. Wait for success message
5. **Save the scene** (Ctrl+S)

---

## ✨ What the Button Does

When clicked, the Quests button will:

```csharp
private void OnQuestsButton()
{
    var questView = Open<UIQuestView>();
    
    if (questView == null)
    {
        Debug.LogWarning("[Quest System] UIQuestView not found. Please run: TPSBR → Generate Quest UI");
        return;
    }

    questView.Show();
}
```

1. ✅ Opens the `UIQuestView`
2. ✅ Calls `Show()` to display quests
3. ✅ Shows a warning if UIQuestView doesn't exist

---

## 🎯 Quick Test

1. **Enter Play Mode**
2. **Click "QUESTS" button** in main menu
3. **Quest window should open** showing your quests
4. **Click X** to close
5. **Done!** ✅

---

## 🐛 Troubleshooting

### Issue: "Quests Button field not showing in Inspector"
**Solution:** The UIMainMenuView script needs to recompile. Wait a few seconds and check again.

### Issue: "Button does nothing when clicked"
**Solution:** Make sure you've:
1. Assigned the button reference in Inspector
2. Generated the UIQuestView (TPSBR > Generate Quest UI)
3. Saved the scene

### Issue: "UIQuestView not found" warning
**Solution:** Run the UI generator: `TPSBR > Generate Quest UI`

### Issue: Button appears in wrong position
**Solution:** 
1. Select your QuestsButton
2. Adjust the position in RectTransform
3. Or re-run the auto-generator to get proper positioning

---

## 📊 Button Hierarchy Structure

After using the auto-generator, your hierarchy should look like:

```
MenuUI/
└── UIMainMenuView/
    ├── PlayButton
    ├── PartyButton
    ├── SettingsButton
    ├── ShopButton
    ├── QuestsButton       ← New!
    ├── ReplayButton
    ├── CreditsButton
    └── QuitButton
```

---

## 🚀 Next Steps

1. ✅ Script updated (already done!)
2. 🔲 Generate Quest UI or wire button manually
3. 🔲 Assign button reference in Inspector
4. 🔲 Save scene
5. 🔲 Test in Play Mode
6. 🔲 Enjoy your quest system! 🎉

---

## 💡 Pro Tips

- **Auto-generator is faster** - Let it create and wire everything
- **Button style** - The generator copies the ShopButton style automatically
- **Custom styling** - After generation, customize colors/fonts as needed
- **Multiple scenes** - The button works wherever UIMainMenuView exists

---

**The script is ready! Now just wire it up in the Editor!** 🎨
