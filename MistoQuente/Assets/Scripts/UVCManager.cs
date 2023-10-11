#define ENABLE_LOG
/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */

using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif

namespace MistoQuente.UVC
{
    [RequireComponent(typeof(AndroidUtils))]
	public class UVCManager : MonoBehaviour
	{
		private const string TAG = "UVCManager#";
		private const string FQCN_DETECTOR = "com.mistoquente.usb.DeviceDetectorFragment";
        private const int FRAME_TYPE_MJPEG = 0x000007;
        private const int FRAME_TYPE_H264 = 0x000014;
        private const int FRAME_TYPE_H264_FRAME = 0x030011;
	
		/**
		* Resolução padrão (largura) quando o IUVCSelector não está definido
		* ou quando o IUVCSelector retorna nulo durante a seleção de resolução.
		*/
		public Int32 DefaultWidth = 1280;
		/**
		* Resolução padrão (altura) quando o IUVCSelector não está definido
		* ou quando o IUVCSelector retorna nulo durante a seleção de resolução.
		*/
		public Int32 DefaultHeight = 720;
		/**
		* Priorizar negociação com H.264 durante a negociação com dispositivos UVC.
		* Válido apenas para dispositivos Android.
		* true: H.264 > MJPEG > YUV
		* false: MJPEG > H.264 > YUV
		*/
		public bool PreferH264 = false;
        /**
		* Renderizar a textura da imagem do dispositivo UVC antes da renderização da cena.
		*/
        public bool RenderBeforeSceneRendering = false;
   
		/**
		* Manipuladores de eventos relacionados a UVC.
		*/
		[SerializeField, ComponentRestriction(typeof(IUVCDrawer))]
		public Component[] UVCDrawers;

		/**
		 * Classe de contêiner para armazenar informações da câmera em uso.		
		*/
		public class CameraInfo
		{
			internal readonly UVCDevice device; // Dispositivo UVC associado a esta instância
			internal Texture previewTexture; // Textura usada para a visualização
			internal int frameType; // Tipo de quadro (frame)
			internal Int32 activeId; // ID ativo
			private Int32 currentWidth; // Largura atual da imagem
			private Int32 currentHeight; // Altura atual da imagem
			private bool isRenderBeforeSceneRendering; // Indica se o render ocorre antes da renderização da cena
			private bool isRendering; // Indica se o processo de renderização está ocorrendo

            internal CameraInfo(UVCDevice device)
			{
				this.device = device; // Construtor que associa o dispositivo UVC com a instância
			}

			/**
			 * Obtém o ID do dispositivo.
			 */
			public Int32 Id{
				get { return device.id;  }
			}
	
			/**
			 * Obtém o nome do dispositivo.
			 */
			public string DeviceName
			{
				get { return device.name; }
			}

			/**
			 * Obtém o ID do fabricante (Vendor ID).
			 */
			public int Vid
			{
				get { return device.vid; }
			}

			/**
			 * Obtém o ID do produto (Product ID).
			 */
			public int Pid
			{
				get { return device.pid; }
			}

			/**
			 * Verifica se a visualização da imagem está ocorrendo.
			 */
			public bool IsPreviewing
			{
				get { return (activeId != 0) && (previewTexture != null); }
			}

			/**
			 * Obtém a largura atual da imagem ou 0 se a 
			 * visualização não estiver ocorrendo.
			*/
			public Int32 CurrentWidth
			{
				get { return currentWidth; }
			}

			/**
			 * Obtém a altura atual da imagem ou 0 se a 
			 * visualização não estiver ocorrendo.
			 */
			public Int32 CurrentHeight
			{
				get { return currentHeight; }
			}

			/**
			 * Define a largura e altura atuais da imagem.
			 * @param width
			 * @param height
			 */
			internal void SetSize(Int32 width, Int32 height)
			{
				currentWidth = width;
				currentHeight = height;
			}

			/**
			* Retorna uma representação de texto do objeto.
			*/

			public override string ToString()
			{
				return $"{base.ToString()}({currentWidth}x{currentHeight},id={Id},activeId={activeId},IsPreviewing={IsPreviewing})";
			}

            /**
             * Inicia o processo de renderização da imagem do dispositivo UVC.
             * @param manager
             */
            internal Coroutine StartRender(UVCManager manager, bool renderBeforeSceneRendering)
            {
                StopRender(manager);
                isRenderBeforeSceneRendering = renderBeforeSceneRendering;
                isRendering = true;
                if (renderBeforeSceneRendering)
                {
                    return manager.StartCoroutine(OnRenderBeforeSceneRendering());
                } else
                {
                    return manager.StartCoroutine(OnRender());
                }
            }

            /**
             * Interrompe o processo de renderização da imagem do dispositivo UVC.
             * @param manager
             */
            internal void StopRender(UVCManager manager)
            {
                if (isRendering)
                {
                    isRendering = false;
                    if (isRenderBeforeSceneRendering)
                    {
                        manager.StopCoroutine(OnRenderBeforeSceneRendering());
                    }
                    else
                    {
                        manager.StopCoroutine(OnRender());
                    }
                }
            }

            /**
			* Para processamento de eventos de renderização
			* Executado como uma rotina
			* Solicita a renderização de imagens a partir de dispositivos UVC 
			* para uma textura antes da renderização da cena
			* (Executa a renderização da imagem antes da renderização da cena).
			*/
            private IEnumerator OnRenderBeforeSceneRendering()
			{
				var renderEventFunc = GetRenderEventFunc();
				for (; activeId != 0;)
				{
					yield return null;
					GL.IssuePluginEvent(renderEventFunc, activeId);
				}
				yield break;
			}

            /**
   			  * Executa a renderização da imagem após a renderização da cena.
             */
            private IEnumerator OnRender()
            {
                var renderEventFunc = GetRenderEventFunc();
                for (; activeId != 0;)
                {
                    yield return new WaitForEndOfFrame();
                    GL.IssuePluginEvent(renderEventFunc, activeId);
                }
                yield break;
            }

        }

        /**
		 * Instância do SynchronizationContext para executar em uma thread principal.
		 */
        private SynchronizationContext mainContext;
		/**
		 * Delegado para receber chamadas de retorno de eventos quando o estado dos dispositivos
		 * UVC conectados ao dispositivo muda.
		 */
		private OnDeviceChangedCallbackManager.OnDeviceChangedFunc callback;
		/**
		 * Lista de dispositivos UVC conectados ao dispositivo.
		 */
		private List<UVCDevice> attachedDevices = new List<UVCDevice>();
		/**
		* Mapeamento de dispositivos UVC que estão capturando imagens.
		* O mapeamento associa IDs de dispositivos a instâncias de CameraInfo.
		 */
		private Dictionary<Int32, CameraInfo> cameraInfos = new Dictionary<int, CameraInfo>();

        //--------------------------------------------------------------------------------
        // Chamadas UnityEngine
        //--------------------------------------------------------------------------------
        // Start é chamado antes do primeiro quadro (frame).
        IEnumerator Start()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Start:");
#endif
			mainContext = SynchronizationContext.Current;
            callback = OnDeviceChangedCallbackManager.Add(this);
	
			yield return Initialize();
		}

#if (!NDEBUG && DEBUG && ENABLE_LOG)
		void OnApplicationFocus()
		{
			Console.WriteLine($"{TAG}OnApplicationFocus:");
		}
#endif

#if (!NDEBUG && DEBUG && ENABLE_LOG)
		void OnApplicationPause(bool pauseStatus)
		{
			Console.WriteLine($"{TAG}OnApplicationPause:{pauseStatus}");
		}
#endif

#if (!NDEBUG && DEBUG && ENABLE_LOG)
		void OnApplicationQuits()
		{
			Console.WriteLine($"{TAG}OnApplicationQuits:");
		}
#endif

		void OnDestroy()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnDestroy:");
#endif
			StopAll();
            OnDeviceChangedCallbackManager.Remove(this);
		}

		//--------------------------------------------------------------------------------
		// Função de retorno de chamada do plugin quando o estado dos dispositivos UVC muda.
		//--------------------------------------------------------------------------------
        public void OnDeviceChanged(IntPtr devicePtr, bool attached)
        {
            var id = UVCDevice.GetId(devicePtr);
#if (!NDEBUG && DEBUG && ENABLE_LOG)
            Console.WriteLine($"{TAG}OnDeviceChangedInternal:id={id},attached={attached}");
#endif
            if (attached)
            {
                UVCDevice device = new UVCDevice(devicePtr);
#if (!NDEBUG && DEBUG && ENABLE_LOG)
                Console.WriteLine($"{TAG}OnDeviceChangedInternal:device={device.ToString()}");
#endif
                if (HandleOnAttachEvent(device))
                {
                    attachedDevices.Add(device);
                    StartPreview(device);
                }
            }
            else
            {
                var found = attachedDevices.Find(item =>
                {
                    return item != null && item.id == id;
                });
                if (found != null)
                {
                    HandleOnDetachEvent(found);
                    StopPreview(found);
                    attachedDevices.Remove(found);
                }
            }
        }
   
        //================================================================================
        /**
		* Obtém a lista de dispositivos UVC conectados.
		* @return Lista de dispositivos UVC conectados
		 */
        public List<CameraInfo> GetAttachedDevices()
		{
			var result = new List<CameraInfo>(cameraInfos.Count);

			foreach (var info in cameraInfos.Values)
			{
				result.Add(info);
			}

			return result;
		}

//		/**
//		 * 対応解像度を取得
//		 * @param camera 対応解像度を取得するUVC機器を指定
//		 * @return 対応解像度 既にカメラが取り外されている/closeしているのであればnull
//		 */
//		public SupportedFormats GetSupportedVideoSize(CameraInfo camera)
//		{
//			var info = (camera != null) ? Get(camera.DeviceName) : null;
//			if ((info != null) && info.IsOpen)
//			{
//				return GetSupportedVideoSize(info.DeviceName);
//			}
//			else
//			{
//				return null;
//			}
//		}

//		/**
//		 * 解像度を変更
//		 * @param 解像度を変更するUVC機器を指定
//		 * @param 変更する解像度を指定, nullならデフォルトに戻す
//		 * @param 解像度が変更されたかどうか
//		 */
//		public bool SetVideoSize(CameraInfo camera, SupportedFormats.Size size)
//		{
//			var info = (camera != null) ? Get(camera.DeviceName) : null;
//			var width = size != null ? size.Width : DefaultWidth;
//			var height = size != null ? size.Height : DefaultHeight;
//			if ((info != null) && info.IsPreviewing)
//			{
//				if ((width != info.CurrentWidth) || (height != info.CurrentHeight))
//				{   // 解像度が変更になるとき
//					StopPreview(info.DeviceName);
//					StartPreview(info.DeviceName, width, height);
//					return true;
//				}
//			}
//			return false;
//		}

		// Esta função inicia a visualização de um dispositivo UVC. 
		// Ela configura as configurações de resolução, cria uma textura para exibir a 
		// visualização, inicia a captura de vídeo e renderiza a imagem. A criação da 
		// textura é feita na thread principal usando mainContext.Post.

		private void StartPreview(UVCDevice device)
		{
			var info = CreateIfNotExist(device);
			if ((info != null) && !info.IsPreviewing) {

				int width = DefaultWidth;
				int height = DefaultHeight;

//				var supportedVideoSize = GetSupportedVideoSize(deviceName);
//				if (supportedVideoSize == null)
//				{
//					throw new ArgumentException("fauled to get supported video size");
//				}

//				// 解像度の選択処理
//				if ((UVCDrawers != null) && (UVCDrawers.Length > 0))
//				{
//					foreach (var drawer in UVCDrawers)
//					{
//						if ((drawer is IUVCDrawer) && ((drawer as IUVCDrawer).CanDraw(this, info.device)))
//						{
//							var size = (drawer as IUVCDrawer).OnUVCSelectSize(this, info.device, supportedVideoSize);
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//							Console.WriteLine($"{TAG}StartPreview:selected={size}");
//#endif
//							if (size != null)
//							{   // 一番最初に見つかった描画可能なIUVCDrawersがnull以外を返せばそれを使う
//								width = size.Width;
//								height = size.Height;
//								break;
//							}
//						}
//					}
//				}

				// FIXME Confirmação de resoluções compatíveis
#if (!NDEBUG && DEBUG && ENABLE_LOG)
				Console.WriteLine($"{TAG}StartPreview:({width}x{height}),id={device.id}");
#endif
                int[] frameTypes = {
                    PreferH264 ? FRAME_TYPE_H264 : FRAME_TYPE_MJPEG,
                    PreferH264 ? FRAME_TYPE_MJPEG : FRAME_TYPE_H264,
                };
                foreach (var frameType in frameTypes)
                {
                    if (Resize(device.id, frameType, width, height) == 0)
                    {
                        info.frameType = frameType;
                        break;
                    }
                }
                    
				info.SetSize(width, height);
				info.activeId = device.id;
				mainContext.Post(__ =>
				{   // A criação da textura deve ser feita na thread principal
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"{TAG}映像受け取り用テクスチャ生成:({width}x{height})");
#endif
					Texture2D tex = new Texture2D(
							width, height,
							TextureFormat.ARGB32,
							false, /* mipmap */
							true /* linear */);
					tex.filterMode = FilterMode.Point;
					tex.Apply();
					info.previewTexture = tex;
					var nativeTexPtr = info.previewTexture.GetNativeTexturePtr();
					Start(device.id, nativeTexPtr.ToInt32());
					HandleOnStartPreviewEvent(info);
					info.StartRender(this, RenderBeforeSceneRendering);
				}, null);
			}
		}
		//  Esta função interrompe a visualização de um dispositivo UVC. Ela para a captura de vídeo, libera a textura e 
		// realiza ações associadas à interrupção da visualização.
		private void StopPreview(UVCDevice device) {
			var info = Get(device);
			if ((info != null) && info.IsPreviewing)
			{
				mainContext.Post(__ =>
				{
					HandleOnStopPreviewEvent(info);
					Stop(device.id);
					info.StopRender(this);
					info.SetSize(0, 0);
					info.activeId = 0;
				}, null);
			}
		}

		// Esta função interrompe todas as visualizações em dispositivos UVC 
		// atualmente ativos, chamando StopPreview para cada um deles.
		private void StopAll() {
			List<CameraInfo> values = new List<CameraInfo>(cameraInfos.Values);
			foreach (var info in values)
			{
				StopPreview(info.device);
			}
		}

		//--------------------------------------------------------------------------------
		/**
		 * Lida com a ação quando um dispositivo UVC é conectado.
		 * @param device
 		* @return true: Usar o dispositivo UVC conectado, 
		  * false: Não usar o dispositivo UVC conectado
		 */
		private bool HandleOnAttachEvent(UVCDevice device/*NonNull*/)
		{
			if ((UVCDrawers == null) || (UVCDrawers.Length == 0))
			{   // Quando nenhum IUVCDrawer está atribuído, retorna true (usar o dispositivo UVC conectado).
				return true;
			}
			else
			{
				bool hasDrawer = false;
				foreach (var drawer in UVCDrawers)
				{
					if (drawer is IUVCDrawer)
					{
						hasDrawer = true;
						if ((drawer as IUVCDrawer).OnUVCAttachEvent(this, device))
						{   // Se pelo menos um IUVCDrawer retornar true, retorna true (usar o dispositivo UVC conectado).
							return true;
						}
					}
				}
				// Quando nenhum IUVCDrawer está atribuído, retorna true (usar o dispositivo UVC conectado).
				return !hasDrawer;
			}
		}

		/**
		 * Lida com a ação quando um dispositivo UVC é desconectado.
		 * @param info
		 */
		private void HandleOnDetachEvent(UVCDevice device/*NonNull*/)
		{
			if ((UVCDrawers != null) && (UVCDrawers.Length > 0))
			{
				foreach (var drawer in UVCDrawers)
				{
					if (drawer is IUVCDrawer)
					{
						(drawer as IUVCDrawer).OnUVCDetachEvent(this, device);
					}
				}
			}
		}

		/**
		 * Lida com a ação quando a captura de vídeo de um dispositivo UVC é iniciada.
		 * @param args Informações da câmera
		 */
		void HandleOnStartPreviewEvent(CameraInfo info)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStartPreviewEvent:({info})");
#endif
			if ((info != null) && info.IsPreviewing && (UVCDrawers != null))
			{
				foreach (var drawer in UVCDrawers)
				{
					if ((drawer is IUVCDrawer) && (drawer as IUVCDrawer).CanDraw(this, info.device))
					{
						(drawer as IUVCDrawer).OnUVCStartEvent(this, info.device, info.previewTexture);
					}
				}
			} else {
#if (!NDEBUG && DEBUG && ENABLE_LOG)
				Console.WriteLine($"{TAG}HandleOnStartPreviewEvent:No UVCDrawers");
#endif
			}
		}

		/**
		 * Lida com a ação quando a captura de vídeo de um dispositivo UVC é interrompida.
		 * @param args Informações da câmera
		 */
		void HandleOnStopPreviewEvent(CameraInfo info)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStopPreviewEvent:({info})");
#endif
			if (UVCDrawers != null)
			{
				foreach (var drawer in UVCDrawers)
				{
					if ((drawer is IUVCDrawer) && (drawer as IUVCDrawer).CanDraw(this, info.device))
					{
						(drawer as IUVCDrawer).OnUVCStopEvent(this, info.device);
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		/**
		* Obtém ou cria uma instância de CameraInfo correspondente à 
		string de identificação do dispositivo UVC especificada.
		* @param device Nome de identificação do dispositivo UVC
		* @return Retorna a instância de CameraInfo, criando uma nova 
		se ainda não estiver registrada.
		 * @param CameraInfo
		 */
		/*NonNull*/
		private CameraInfo CreateIfNotExist(UVCDevice device)
		{
			if (!cameraInfos.ContainsKey(device.id))
			{
				cameraInfos[device.id] = new CameraInfo(device);
			}
			return cameraInfos[device.id];
		}

		/**
		* Obtém uma instância de CameraInfo correspondente à string de identificação do 
		dispositivo UVC especificada, se já estiver registrada.
		* @param device Nome de identificação do dispositivo UVC
		* @return Retorna a instância de CameraInfo se estiver registrada, caso 
		contrário, retorna nulo.
		 */
		/*Nullable*/
		private CameraInfo Get(UVCDevice device)
		{
			return cameraInfos.ContainsKey(device.id) ? cameraInfos[device.id] : null;
		}


		//--------------------------------------------------------------------------------
		/**
		* Inicializa o plugin.
		* Verifica as permissões e, se forem concedidas, chama a função de inicialização real do 
		plugin, #InitPlugin.
		 */
		private IEnumerator Initialize()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Initialize:");
#endif
			if (AndroidUtils.CheckAndroidVersion(28))
			{
				yield return AndroidUtils.GrantCameraPermission((string permission, AndroidUtils.PermissionGrantResult result) =>
				{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"{TAG}OnPermission:{permission}={result}");
#endif
					switch (result)
					{
						case AndroidUtils.PermissionGrantResult.PERMISSION_GRANT:
							InitPlugin();
							break;
						case AndroidUtils.PermissionGrantResult.PERMISSION_DENY:
							if (AndroidUtils.ShouldShowRequestPermissionRationale(AndroidUtils.PERMISSION_CAMERA))
							{
								// A permissão foi negada
								// FIXME: Deve-se exibir um diálogo de explicação.
							}
							break;
						case AndroidUtils.PermissionGrantResult.PERMISSION_DENY_AND_NEVER_ASK_AGAIN:
							break;
					}
				});
			}
			else
			{
				InitPlugin();
			}

			yield break;
		}

		/**
		 * Inicializa o plugin.
		 * Envia solicitações de ação para o plugin uvc-plugin-unity.
		 */
		private void InitPlugin()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}InitPlugin:");
#endif
			// Verifica se algum objeto IUVCDrawer está atribuído
			var hasDrawer = false;
			if ((UVCDrawers != null) && (UVCDrawers.Length > 0))
			{
				foreach (var drawer in UVCDrawers)
				{
					if (drawer is IUVCDrawer)
					{
						hasDrawer = true;
						break;
					}
				}
			}
			if (!hasDrawer)
			{   // Se nenhum IUVCDrawer foi configurado no Inspector
				// Tentaremos obtê-lo a partir do objeto de jogo a que este script está anexado.
#if (!NDEBUG && DEBUG && ENABLE_LOG)
				Console.WriteLine($"{TAG}InitPlugin:has no IUVCDrawer, try to get from gameObject");
#endif
				var drawers = GetComponents(typeof(IUVCDrawer));
				if ((drawers != null) && (drawers.Length > 0))
				{
					UVCDrawers = new Component[drawers.Length];
					int i = 0;
					foreach (var drawer in drawers)
					{
						UVCDrawers[i++] = drawer;
					}
				}
			}
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}InitPlugin:num drawers={UVCDrawers.Length}");
#endif
			// aandusb: Carrega o DeviceDetector
			using (AndroidJavaClass clazz = new AndroidJavaClass(FQCN_DETECTOR))
			{
				clazz.CallStatic("initUVCDeviceDetector",
					AndroidUtils.GetCurrentActivity());
			}
		}

        //--------------------------------------------------------------------------------
        // Definições e declarações relacionadas a plugins nativos
        //--------------------------------------------------------------------------------
		/**
		 * Função nativa (C/C++) para obtenção de eventos de renderização no plugin
		 */
		[DllImport("unityuvcplugin")]
		private static extern IntPtr GetRenderEventFunc();
        /**
		 * Obtém uma função de evento de renderização para uso com plugins nativos.
		 */
        [DllImport("unityuvcplugin", EntryPoint = "Config")]
        private static extern Int32 Config(Int32 deviceId, Int32 enabled, Int32 useFirstConfig);
        /**
		 * Realiza configurações iniciais, possivelmente relacionadas às configurações de um dispositivo.
		 */
        [DllImport("unityuvcplugin", EntryPoint ="Start")]
		private static extern Int32 Start(Int32 deviceId, Int32 tex);
		/**
		 * Inicia a captura de vídeo de um dispositivo UVC e a exibe em uma textura. 
		 deviceId se refere ao identificador do dispositivo e tex à textura na qual a captura de 
		 vídeo é renderizada.
		 */
		[DllImport("unityuvcplugin", EntryPoint ="Stop")]
		private static extern Int32 Stop(Int32 deviceId);
		/**
		 * Para a captura de vídeo de um dispositivo UVC.
		 */
		[DllImport("unityuvcplugin")]
		private static extern Int32 Resize(Int32 deviceId, Int32 frameType, Int32 width, Int32 height);
	}   // Define o tamanho da imagem de vídeo a ser capturada pelo dispositivo UVC.

    /**
	* No IL2Cpp, não é possível realizar marshalling de delegados usados para callbacks de C/C++, 
	então é necessário processar em funções estáticas.
	* No entanto, dessa forma, não é possível chamar funções do objeto que fez a chamada original, 
	então um gerenciador de classes é criado.
	* Por enquanto, ele aceita apenas o UVCManager, então não foi implementada uma interface.
     */
    public static class OnDeviceChangedCallbackManager
    {
        //  Esta é uma classe que gerencia os callbacks de eventos de mudança de dispositivos UVC.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnDeviceChangedFunc(Int32 id, IntPtr devicePtr, bool attached);

        /**
		 * tipo de delegado usado para representar uma função de callback que recebe 
		 informações sobre a mudança de dispositivo UVC.
		 */
        [DllImport("unityuvcplugin")]
        private static extern IntPtr Register(Int32 id, OnDeviceChangedFunc callback);
        /**
		 * Registra um callback de evento de mudança de dispositivo na biblioteca nativa.
		 */
        [DllImport("unityuvcplugin")]
        private static extern IntPtr Unregister(Int32 id);

        private static Dictionary<Int32, UVCManager> sManagers = new Dictionary<Int32, UVCManager>();
  
        /**
         * Remove o registro de um callback de evento de mudança de dispositivo.
         */
        public static OnDeviceChangedFunc Add(UVCManager manager)
        {
            Int32 id = manager.GetHashCode();
            OnDeviceChangedFunc callback = new OnDeviceChangedFunc(OnDeviceChanged);
            sManagers.Add(id, manager);
            Register(id, callback);
            return callback;
        }

        /**
         * Adiciona um callback de evento de mudança de dispositivo gerenciado por UVCManager ao gerenciador. 
		 Isso permite que os callbacks sejam acionados quando ocorrem eventos de mudança de dispositivo UVC.
         */
        public static void Remove(UVCManager manager)
        {
            Int32 id = manager.GetHashCode();
            Unregister(id);
            sManagers.Remove(id);
        }

		// Remove um callback de evento de mudança de dispositivo do gerenciador.

        [MonoPInvokeCallback(typeof(OnDeviceChangedFunc))]
        public static void OnDeviceChanged(Int32 id, IntPtr devicePtr, bool attached)
        {
            var manager = sManagers.ContainsKey(id) ? sManagers[id] : null;
            if (manager != null)
            {
                manager.OnDeviceChanged(devicePtr, attached);
            }
        }
    } // Esta função é acionada quando um evento de mudança de dispositivo UVC ocorre. 
	// Ela chama a função OnDeviceChanged do UVCManager correspondente.


}   // namespace MistoQuente.UVC
