//#define ENABLE_LOG
/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

#if UNITY_ANDROID
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif
#endif

namespace Serenegiant
{

	public class AndroidUtils : MonoBehaviour
	{
		public const string FQCN_UNITY_PLAYER = "com.unity3d.player.UnityPlayer";
		public const string PERMISSION_CAMERA = "android.permission.CAMERA";

		public enum PermissionGrantResult
		{
			PERMISSION_GRANT = 0,
			PERMISSION_DENY = -1,
			PERMISSION_DENY_AND_NEVER_ASK_AGAIN = -2
		}

		private const string TAG = "AndroidUtils#";
		private const string FQCN_PLUGIN = "com.serenegiant.androidutils.AndroidUtils";

		//--------------------------------------------------------------------------------
		/**
		 * Delegado para eventos de ciclo de vida
		 * @param resumed true: onResume, false: onPause
		 */
		public delegate void LifecycleEventHandler(bool resumed);

		/***
		 * Delegado para callback ao solicitar permissão
		 * @param permission
		 * @param grantResult 0:grant, -1:deny, -2:denyAndNeverAskAgain
		*/
		public delegate void OnPermission(string permission, PermissionGrantResult result);

		//--------------------------------------------------------------------------------
		/**
		 * Tempo limite ao solicitar permissão
		 */
		public static float PermissionTimeoutSecs = 30;
	
		public event LifecycleEventHandler LifecycleEvent;

		public static bool isPermissionRequesting;
		private static PermissionGrantResult grantResult;

		void Awake()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Awake:");
#endif
#if UNITY_ANDROID
			Input.backButtonLeavesApp = true;   // Permite encerrar o aplicativo com o botão "Voltar" do dispositivo
			Initialize();
#endif
		}

		//--------------------------------------------------------------------------------
		// Java event callback

		/**
		 * onStart evento
		 */
		public void OnStartEvent()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnStartEvent:");
#endif
		}

		/**
		 * onResume evento
		 */
		public void OnResumeEvent()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnResumeEvent:");
#endif
			LifecycleEvent?.Invoke(true);
		}

		/**
		 * onPause evento
		 */
		public void OnPauseEvent()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnPauseEvent:");
#endif
			LifecycleEvent?.Invoke(false);
		}

		/**
		 * onStop evento
		 */
		public void OnStopEvent()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnStopEvent:");
#endif
		}

		/**
		 * Permissão concedida com sucesso
		 */
		public void OnPermissionGrant()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnPermissionGrant:");
#endif
			grantResult = PermissionGrantResult.PERMISSION_GRANT;
			isPermissionRequesting = false;
		}

		/**
		 * Permissão negada
		 */
		public void OnPermissionDeny()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnPermissionDeny:");
#endif
			grantResult = PermissionGrantResult.PERMISSION_DENY;
			isPermissionRequesting = false;
		}

		/**
		 * Permissão negada e configurada para nunca mais perguntar
		 */
		public void OnPermissionDenyAndNeverAskAgain()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnPermissionDenyAndNeverAskAgain:");
#endif
			grantResult = PermissionGrantResult.PERMISSION_DENY_AND_NEVER_ASK_AGAIN;
			isPermissionRequesting = false;
		}

		//--------------------------------------------------------------------------------
#if UNITY_ANDROID
		/**
		 * Inicialização do plugin
		 */
		private void Initialize()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Initialize:{gameObject.name}");
#endif
			using (AndroidJavaClass clazz = new AndroidJavaClass(FQCN_PLUGIN))
			{
				clazz.CallStatic("initialize",
					AndroidUtils.GetCurrentActivity(), gameObject.name);
			}
		}

		/**
		 * Verifica se uma permissão específica está concedida
		 * @param permission
		 * @param Verdadeiro se a permissão especificada estiver concedida
		 */
		public static bool HasPermission(string permission)
		{
			using (AndroidJavaClass clazz = new AndroidJavaClass(FQCN_PLUGIN))
			{
				return clazz.CallStatic<bool>("hasPermission",
					AndroidUtils.GetCurrentActivity(), permission);
			}
		}

		/**
		 * Verifica se é necessário mostrar a explicação para uma permissão específica
		 * @param permission
		 * @param Verdadeiro se a explicação da permissão especificada deve ser mostrada
		 */
		public static bool ShouldShowRequestPermissionRationale(string permission)
		{
			using (AndroidJavaClass clazz = new AndroidJavaClass(FQCN_PLUGIN))
			{
				return clazz.CallStatic<bool>("shouldShowRequestPermissionRationale",
					AndroidUtils.GetCurrentActivity(), permission);
			}
		}

		/**
		* Solicitação de permissão
		* Não lida com a explicação da permissão no lado Java
		* @param permission
		* @param callback
		 */
		public static IEnumerator RequestPermission(string permission, OnPermission callback)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}GrantPermission:{permission}");
#endif
			if (!HasPermission(permission))
			{
				grantResult = PermissionGrantResult.PERMISSION_DENY;
				isPermissionRequesting = true;
				using (AndroidJavaClass clazz = new AndroidJavaClass(FQCN_PLUGIN))
				{
					clazz.CallStatic("requestPermission",
						AndroidUtils.GetCurrentActivity(), permission);
				}
				float timeElapsed = 0;
				while (isPermissionRequesting)
				{
					if ((PermissionTimeoutSecs > 0) && (timeElapsed > PermissionTimeoutSecs))
					{
						isPermissionRequesting = false;
						yield break;
					}
					timeElapsed += Time.deltaTime;
					yield return null;
				}
				callback(permission, grantResult);
			}
			else
			{
				callback(permission, PermissionGrantResult.PERMISSION_GRANT);
			}
	
			yield break;
		}

		/**
		* Solicitação de permissão
		* Lida com a explicação da permissão no lado Java
		* @param permission
		* @param callback
		 */
		public static IEnumerator GrantPermission(string permission, OnPermission callback)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}GrantPermission:{permission}");
#endif
			if (!HasPermission(permission))
			{
				grantResult = PermissionGrantResult.PERMISSION_DENY;
				isPermissionRequesting = true;
				using (AndroidJavaClass clazz = new AndroidJavaClass(FQCN_PLUGIN))
				{
					clazz.CallStatic("grantPermission",
						AndroidUtils.GetCurrentActivity(), permission);
				}
				float timeElapsed = 0;
				while (isPermissionRequesting)
				{
					if ((PermissionTimeoutSecs > 0) && (timeElapsed > PermissionTimeoutSecs))
					{
						isPermissionRequesting = false;
						yield break;
					}
					timeElapsed += Time.deltaTime;
						yield return null;
				}
				callback(permission, grantResult);
			}
			else
			{
				callback(permission, PermissionGrantResult.PERMISSION_GRANT);
			}
	
			yield break;
		}

		/**
		 * Solicitação de permissão da câmera
		 * @param callback
		 */
		public static IEnumerator GrantCameraPermission(OnPermission callback)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}GrantCameraPermission:");
#endif
			if (CheckAndroidVersion(23))
			{
				// A partir do Android 9, a permissão da câmera é necessária para acessar dispositivos UVC
				yield return GrantPermission(PERMISSION_CAMERA, callback);
			}
			else
			{
				// Em dispositivos Android com versão inferior a 6, a solicitação de permissão não é necessária
				callback(PERMISSION_CAMERA, PermissionGrantResult.PERMISSION_GRANT);
			}

			yield break;
		}


		//================================================================================

		/**
		 * Obtém a atividade UnityPlayer
		 */
		public static AndroidJavaObject GetCurrentActivity()
		{
			using (AndroidJavaClass playerClass = new AndroidJavaClass(FQCN_UNITY_PLAYER))
			{
				return playerClass.GetStatic<AndroidJavaObject>("currentActivity");
			}
		}

		/**
		 * Verifica se está rodando na versão especificada ou posterior
         * @param apiLevel
         * @return true: está sendo executado na versão especificada ou posterior, false: está sendo executado em um dispositivo com versão mais antiga
		 */
		public static bool CheckAndroidVersion(int apiLevel)
		{
			using (var VERSION = new AndroidJavaClass("android.os.Build$VERSION"))
			{
				return VERSION.GetStatic<int>("SDK_INT") >= apiLevel;
			}
		}

	} // class AndroidUtils

} // namespace Serenegiant

#endif // #if UNITY_ANDROID