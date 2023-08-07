using UnityEngine;

// Note: we don't use standard gamelib singleton here because SceneLoader class is also used as a regular component (not only as singleton)
public class SceneLoaderSingleton : MonoBehaviour
{
	public SceneLoader SceneLoader;
	public static SceneLoader Instance { get; private set; }

	void Awake()
	{
		Instance = SceneLoader;
	}
}
