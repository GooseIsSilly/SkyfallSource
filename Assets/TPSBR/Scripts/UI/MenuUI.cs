using UnityEngine;

namespace TPSBR.UI
{
	public class MenuUI : SceneUI
	{
		public new T Open<T>() where T : UIView
		{
			T view = base.Open<T>();
			
			if (view == null)
			{
				view = TryLoadAndInstantiateView<T>();
				if (view != null)
				{
					base.Open(view);
				}
			}
			
			return view;
		}

		private T TryLoadAndInstantiateView<T>() where T : UIView
		{
			string viewName = typeof(T).Name;
			string prefabPath = $"Assets/TPSBR/UI/Prefabs/MenuViews/{viewName}.prefab";

#if UNITY_EDITOR
			T prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(prefabPath);
			if (prefab == null)
			{
				Debug.LogWarning($"[MenuUI] Could not find prefab for {viewName} at {prefabPath}. Please create it first.");
				return null;
			}

			T instance = Instantiate(prefab, transform);
			instance.name = viewName;

			instance.Initialize(this, null);
			instance.SetPriority(_views.Length);
			instance.gameObject.SetActive(false);

			var newViews = new UIView[_views.Length + 1];
			System.Array.Copy(_views, newViews, _views.Length);
			newViews[_views.Length] = instance;
			_views = newViews;

			Debug.Log($"[MenuUI] ✅ Auto-loaded {viewName} from prefab! You can now use it without manually adding it to the scene.");
			return instance;
#else
			Debug.LogWarning($"[MenuUI] Auto-loading views only works in Editor. Add {viewName} to MenuUI in scene for builds.");
			return null;
#endif
		}

		// SceneUI INTERFACE

		protected override void OnInitializeInternal()
		{
			base.OnInitializeInternal();

			Context.Input.RequestCursorVisibility(true, ECursorStateSource.Menu);

			if (Context.PlayerData.Nickname.HasValue() == false)
			{
				var changeNicknameView = Open<UIChangeNicknameView>();
				changeNicknameView.SetData("ENTER NICKNAME", true);
			}
		}

		protected override void OnDeinitializeInternal()
		{
			Context.Input.RequestCursorVisibility(false, ECursorStateSource.Menu);

			base.OnDeinitializeInternal();
		}

		protected override void OnActivate()
		{
			base.OnActivate();

			if (Global.Networking.ErrorStatus.HasValue() == true)
			{
				Open<UIMultiplayerView>();
				var errorDialog = Open<UIErrorDialogView>();

				errorDialog.Title.text = "Connection Issue";

				if (Global.Networking.ErrorStatus == Networking.STATUS_SERVER_CLOSED)
				{
					errorDialog.Description.text = $"Server was closed.";
				}
				else
				{
					errorDialog.Description.text = $"Failed to start network game\n\nReason:\n{Global.Networking.ErrorStatus}";
				}

				Global.Networking.ClearErrorStatus();
			}
		}
	}
}
