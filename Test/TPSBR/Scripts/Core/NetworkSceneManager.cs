namespace TPSBR
{
	using System.Collections;
	using UnityEngine;
	using Fusion;

	using UnityScene = UnityEngine.SceneManagement.Scene;

	public class NetworkSceneManager : NetworkSceneManagerDefault
	{
		public Scene GameplayScene => _gameplayScene;

		private Scene _gameplayScene;

		private bool _isBusy;

		public override bool IsBusy => _isBusy | base.IsBusy;

		protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, UnityScene scene, NetworkLoadSceneParameters sceneParams)
		{
			_isBusy = true;
			_gameplayScene = scene.GetComponent<Scene>(true);

			float contextTimeout = 20.0f;
			while (_gameplayScene.ContextReady == false && contextTimeout > 0.0f)
			{
				yield return null;
				contextTimeout -= Time.unscaledDeltaTime;
			}

			var contextBehaviours = scene.GetComponents<IContextBehaviour>(true);
			foreach (var behaviour in contextBehaviours)
			{
				behaviour.Context = _gameplayScene.Context;
			}

			yield return base.OnSceneLoaded(sceneRef, scene, sceneParams);

			_isBusy = false;
		}
	}
}
