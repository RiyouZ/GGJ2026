using UnityEngine;

namespace Frame.Audio
{
	public static class WwiseAudio
	{
		public static bool IsInitialized => AkUnitySoundEngine.IsInitialized();

		public static uint PlayEvent(string eventName, GameObject gameObject)
		{
			if (string.IsNullOrWhiteSpace(eventName))
			{
				LogWarning("PlayEvent failed: eventName is null or empty.");
				return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
			}

			if (!EnsureInitialized())
			{
				LogWarning($"PlayEvent skipped: Wwise not initialized. eventName={eventName}");
				return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
			}

			if (IsDestroyed(gameObject))
			{
				LogWarning($"PlayEvent fallback to global: gameObject is null or destroyed. eventName={eventName}");
				gameObject = null;
			}

			return AkUnitySoundEngine.PostEvent(eventName, gameObject);
		}


		public static void SeekOnEvent(string eventName, GameObject gameObject, float position, uint id)
		{
			var result = AkUnitySoundEngine.SeekOnEvent(eventName, gameObject, position, false, id);
			
			if (result != AKRESULT.AK_Success)
			{
				LogWarning($"SeekOnEvent failed: eventName={eventName}, position={position}, id={id}, result={result}");
			}
		}

		public static float GetRTPC(string rtpcName, GameObject gameObject)
		{
			if (string.IsNullOrWhiteSpace(rtpcName))
			{
				LogWarning("GetRTPC failed: rtpcName is null or empty.");
				return 0f;
			}

			if (!EnsureInitialized())
			{
				LogWarning($"GetRTPC skipped: Wwise not initialized. rtpcName={rtpcName}");
				return 0f;
			}

			if (IsDestroyed(gameObject))
			{
				LogWarning($"GetRTPC fallback to global: gameObject is null or destroyed. rtpcName={rtpcName}");
				gameObject = null;
			}

			var queryValue = (int)(gameObject ? AkQueryRTPCValue.RTPCValue_GameObject : AkQueryRTPCValue.RTPCValue_Global);
			var result = AkUnitySoundEngine.GetRTPCValue(rtpcName, gameObject, AkUnitySoundEngine.AK_INVALID_PLAYING_ID, out var value, ref queryValue);
			if (result != AKRESULT.AK_Success)
			{
				LogWarning($"GetRTPC failed: rtpcName={rtpcName}, result={result}");
				return 0f;
			}

			return value;
		}

		public static void SetRTPC(string rtpcName, float value, GameObject gameObject)
		{
			if (string.IsNullOrWhiteSpace(rtpcName))
			{
				LogWarning("SetRTPC failed: rtpcName is null or empty.");
				return;
			}

			if (!EnsureInitialized())
			{
				LogWarning($"SetRTPC skipped: Wwise not initialized. rtpcName={rtpcName}");
				return;
			}

			if (IsDestroyed(gameObject))
			{
				LogWarning($"SetRTPC fallback to global: gameObject is null or destroyed. rtpcName={rtpcName}");
				gameObject = null;
			}

			var result = gameObject == null
				? AkUnitySoundEngine.SetRTPCValue(rtpcName, value)
				: AkUnitySoundEngine.SetRTPCValue(rtpcName, value, gameObject);

			if (result != AKRESULT.AK_Success)
			{
				LogWarning($"SetRTPC failed: rtpcName={rtpcName}, value={value}, result={result}");
			}
		}

		public static bool LoadBank(string bankName)
		{
			if (string.IsNullOrWhiteSpace(bankName))
			{
				LogWarning("LoadBank failed: bankName is null or empty.");
				return false;
			}

			if (!EnsureInitialized())
			{
				LogWarning($"LoadBank skipped: Wwise not initialized. bankName={bankName}");
				return false;
			}

			var normalizedName = NormalizeBankName(bankName);
			var bankId = AkBankManager.LoadBank(normalizedName, false, false);

			var success = bankId == (uint)AKRESULT.AK_Success || bankId == AkUnitySoundEngine.AK_INVALID_UNIQUE_ID;
			if (!success)
			{
				LogWarning($"LoadBank failed: bankName={normalizedName}, result={bankId}");
			}

			return success;
		}

		private static bool EnsureInitialized()
		{
			if (AkUnitySoundEngine.IsInitialized())
			{
				return true;
			}

			var initializer = AkUnitySoundEngineInitialization.Instance;
			if (initializer == null)
			{
				LogWarning("Wwise initialization skipped: AkUnitySoundEngineInitialization.Instance is null.");
				return false;
			}

			return initializer.InitializeSoundEngine();
		}

		private static bool IsDestroyed(Object unityObject)
		{
			return unityObject == null;
		}

		private static string NormalizeBankName(string bankName)
		{
			if (bankName.EndsWith(".bnk"))
			{
				return bankName.Substring(0, bankName.Length - 4);
			}

			return bankName;
		}

		private static void LogWarning(string message)
		{
#if UNITY_EDITOR
			Debug.LogWarning($"[WwiseAudio] {message}");
#endif
		}
	}
}