using UnityEngine; // Keep for SystemLanguage, may try to abstract away later
using System;
using System.IO;
using System.Collections.Generic;
using Beats2;
using Beats2.Common;

/*
 * DONE
 */
namespace Beats2.Common {
	
	/// <summary>
	/// System info. Wraps Unity's SystemInfo class.
	/// </summary>
	public static class SysInfo {
		private const string TAG = "SysInfo";
		private const string BEATS2_DIR = "Beats2";
		private const string FILE_PROTOCOL = "file://";
		
		public enum DeviceTypes {
			DESKTOP,
			CONSOLE,
			WEB,
			TABLET,
			PHONE
		}

		public enum Platforms {
			DESKTOP_WINDOWS,
			DESKTOP_OSX,
			DESKTOP_LINUX,
			CONSOLE_WII,
			CONSOLE_XBOX,
			CONSOLE_PS3,
			WEB_BROWSER,
			WEB_GOOGLE,
			WEB_FLASH,
			DEVICE_ANDROID,
			DEVICE_IOS,
			DEVICE_WINPHONE
		}

		public static string appName			{ get; private set; }
		public static string appVersionNum		{ get; private set; }
		public static string appVersionName		{ get; private set; }

		public static string deviceName			{ get; private set; }
		public static string deviceModel		{ get; private set; }
		public static string deviceId			{ get; private set; }

		public static Platforms platform		{ get; private set; }
		public static DeviceTypes deviceType	{ get; private set; }
		public static string operatingSystem	{ get; private set; }
		public static string unityVersion		{ get; private set; }

		public static bool vibrationSupport		{ get; private set; }
		public static bool touchSupport			{ get; private set; }

		public static SystemLanguage language	{ get; private set; }

		public static string rootPath			{ get; private set; }
		public static string dataPath			{ get; private set; }
		
		public static void Init() {
			appName = Constants.APP_NAME;
			appVersionNum = Constants.APP_VERSION_NUM;
			appVersionName = Constants.APP_VERSION_NAME;

			deviceName = SystemInfo.deviceName;
			deviceModel = SystemInfo.deviceModel;
			// FIXME: deviceId = SystemInfo.deviceUniqueIdentifier;

			SetPlatformAndDeviceType();
			operatingSystem = SystemInfo.operatingSystem;
			unityVersion = Application.unityVersion;

			vibrationSupport = SystemInfo.supportsVibration;
			touchSupport = Input.multiTouchEnabled;

			if ((rootPath = GetRootPath()) == null) {
				Logger.Error(TAG, "Unable to find root path");
				throw new BeatsException("Unable to find root path");
			}
			if ((dataPath = GetDataPath()) == null) {
				// TODO - extract resources
			}

			Reset();
			Logger.Debug(TAG, "Initialized...");
			Logger.Debug(TAG, InfoString());
		}
		
		public static void Reset() {
			language = Application.systemLanguage;
			Logger.Debug(TAG, "Reset...");
		}

		private static void SetPlatformAndDeviceType() {
			switch (Application.platform) {
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.OSXDashboardPlayer:
					platform = Platforms.DESKTOP_OSX;
					deviceType = DeviceTypes.DESKTOP;
					break;
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
					platform = Platforms.DESKTOP_WINDOWS;
					deviceType = DeviceTypes.DESKTOP;
					break;
				case RuntimePlatform.LinuxPlayer:
					platform = Platforms.DESKTOP_LINUX;
					deviceType = DeviceTypes.DESKTOP;
					break;
				case RuntimePlatform.OSXWebPlayer:
				case RuntimePlatform.WindowsWebPlayer:
					platform = Platforms.WEB_BROWSER;
					deviceType = DeviceTypes.WEB;
					break;
				case RuntimePlatform.FlashPlayer:
					platform = Platforms.WEB_FLASH;
					deviceType = DeviceTypes.WEB;
					break;
				case RuntimePlatform.NaCl:
					platform = Platforms.WEB_GOOGLE;
					deviceType = DeviceTypes.WEB;
					break;
				case RuntimePlatform.WiiPlayer:
					platform = Platforms.CONSOLE_WII;
					deviceType = DeviceTypes.CONSOLE;
					break;
				case RuntimePlatform.XBOX360:
					platform = Platforms.CONSOLE_XBOX;
					deviceType = DeviceTypes.CONSOLE;
					break;
				case RuntimePlatform.PS3:
					platform = Platforms.CONSOLE_PS3;
					deviceType = DeviceTypes.CONSOLE;
					break;
				case RuntimePlatform.IPhonePlayer:
					platform = Platforms.DEVICE_IOS;
					deviceType = (IsPhone()) ? DeviceTypes.PHONE : DeviceTypes.TABLET;
					break;
				case RuntimePlatform.Android:
					platform = Platforms.DEVICE_ANDROID;
					deviceType = (IsPhone()) ? DeviceTypes.PHONE : DeviceTypes.TABLET;
					break;
				// Not implemented yet
				/*
				case RuntimePlatform.WindowsPhone:
					platform = Platforms.DEVICE_WINPHONE;
					deviceType = DeviceTypes.PHONE;
					break;
				*/
				default:
					platform = Platforms.DESKTOP_WINDOWS;
					deviceType = DeviceTypes.DESKTOP;
					break;
			}
		}
		
		private static bool IsPhone() {
			float phoneScreenWidth = SettingsManager.GetValueFloat(Settings.SYSTEM_PHONE_SCREEN_WIDTH);
			return Screens.minPhysical < phoneScreenWidth;
		}

#if UNITY_ANDROID
		private static string[] sdcardPaths = {
			//"/mnt/sdcard/external_sd",
			//"/mnt/sdcard/sd",
			//"/sdcard/external_sd",
			//"/sdcard/sd",
			//"/mnt/external_sd",
			//"/mnt/sdcard-ext",
			"/mnt/sdcard",
			"/mnt/sd",
			//"/mnt/external_sd",
			//"/sdcard-ext",
			"/sdcard",
			"/sd"
		};
#endif

		private static string GetRootPath() {
#if UNITY_ANDROID
			foreach (string path in sdcardPaths) {
				if (Directory.Exists(path)) {
					return path;
				}
			}
#else
			string path = Application.dataPath;
			if (path.IndexOf('/') != -1) {
				path = path.Substring(0, path.LastIndexOf('/'));
			}
			if (Directory.Exists(path)) {
				return path;
			}
#endif
			return null;
		}

		private static string GetDataPath() {
			return String.Format("{0}{1}{2}", rootPath, Path.AltDirectorySeparatorChar, BEATS2_DIR);
		}

		public static string GetPath(string fileName) {
			string filePath = String.Format("{0}{1}{2}", dataPath, Path.AltDirectorySeparatorChar, fileName);
			if (File.Exists(filePath)) {
				return FILE_PROTOCOL + filePath;
			} else {
				Logger.Error(TAG, String.Format("Unable to find file \"{0}\"", filePath));
				return null;
			}
		}

		public static string InfoString() {
			return
				"appName: " + appName +
				"\nappVersionNum: " + appVersionNum +
				"\nappVersionName: " + appVersionName +
				"\ndeviceName: " + deviceName +
				"\ndeviceModel: " + deviceModel +
				"\ndeviceId: " + deviceModel +
				"\nplatform: " + platform +
				"\ndeviceType: " + deviceType +
				"\noperatingSystem: " + operatingSystem +
				"\nunityVersion: " + unityVersion +
				"\nvibrationSupport: " + vibrationSupport +
				"\ntouchSupport: " + touchSupport +
				"\nsystemLanguage: " + language +
				"\ndataPath: " + dataPath +
				"\ndataPath: " + dataPath
			;
		}
	}
}