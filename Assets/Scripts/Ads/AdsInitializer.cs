using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
	[SerializeField] string _androidGameId;
	[SerializeField] string _iOSGameId;
	[SerializeField] bool _testMode = true;
	[SerializeField] private RewardedAdsHeart rewardedAdsHeart;
	private string _gameId;
	public static AdsInitializer instance;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);
		InitializeAds();
		DontDestroyOnLoad(gameObject);
	}

	public void InitializeAds()
	{
#if UNITY_IOS
            _gameId = _iOSGameId;
#elif UNITY_ANDROID
		_gameId = _androidGameId;
#elif UNITY_EDITOR
        _gameId = _androidGameId; // Testing
#endif
		if (!Advertisement.isInitialized && Advertisement.isSupported)
		{
			Advertisement.Initialize(_gameId, _testMode, this);
		}
	}

	public void OnInitializationComplete()
	{
		Debug.Log("Unity Ads initialization complete.");
		rewardedAdsHeart.LoadAd();
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message)
	{
		Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
	}
}