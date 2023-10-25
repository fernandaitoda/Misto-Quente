//#define ENABLE_LOG
/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Este código é responsável por renderizar o vídeo de dispositivos UVC 
// (Universal Video Class) em materiais, renderizadores ou elementos RawImage em 
// um ambiente Unity. Ele implementa uma interface IUVCDrawer para lidar com 
// eventos relacionados à conexão e desconexão de dispositivos UVC, bem como a 
// captura de vídeo.


namespace Serenegiant.UVC
{

	public class UVCDrawer : MonoBehaviour, IUVCDrawer
	{
		/**
		 * IUVCSelectorがセットされていないとき
		 * またはIUVCSelectorが解像度選択時にnullを
		 * 返したときのデフォルトの解像度(幅)
		 */
		public int DefaultWidth = 1280;
		/**
		 * IUVCSelectorがセットされていないとき
		 * またはIUVCSelectorが解像度選択時にnullを
		 * 返したときのデフォルトの解像度(高さ)
		 */
		public int DefaultHeight = 720;

		/**
		* O GameObject que mantém
		 * 接続時及び描画時のフィルタ用
		 */
		public UVCFilter[] UVCFilters;

		/**
		* O GameObject que mantém o Material de destino para renderizar a imagem do dispositivo UVC.
 		* Se não estiver configurado, o mesmo GameObject onde este script foi atribuído será usado.

		 */
		public List<GameObject> RenderTargets;

		//--------------------------------------------------------------------------------
		private const string TAG = "UVCDrawer#";

		/**
		 * UVC機器からの映像の描画先Material
		 * TargetGameObjectから取得する
		 * 優先順位：
		* Material de destino para renderizar a imagem do dispositivo UVC.
		* Obtido a partir do TargetGameObject.
		* Prioridade:
		*     Skybox do TargetGameObject
		*     > Renderer do TargetGameObject
		*     > RawImage do TargetGameObject
		*     > Material do TargetGameObject
		* Se nenhum desses métodos funcionar, uma UnityException será lançada em Start().
		 */
		private UnityEngine.Object[] TargetMaterials;
		/**
		 * Textura original.
		* O valor que estava configurado em GetComponent<Renderer>().material.mainTexture
		* antes de definir a textura para receber a imagem da câmera UVC.
		 */
		private Texture[] SavedTextures;

		private Quaternion[] quaternions;

		//================================================================================

		// Start is called before the first frame update
		void Start()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Start:");
#endif
			UpdateTarget();

		}

		//		// Update is called once per frame
		//		void Update()
		//		{
		//
		//		}

		//================================================================================

		/**
		* Um dispositivo UVC foi conectado.
		* Implementação de IOnUVCAttachHandler.
		* @param manager O UVCManager que fez a chamada.
		* @param device Informações sobre o dispositivo UVC alvo.
		* @return true: Usar o dispositivo UVC, false: Não usar o dispositivo UVC.
		*/
		public bool OnUVCAttachEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCAttachEvent:{device}");
#endif
		// XXX Na implementação atual, basicamente aceitamos todos os dispositivos UVC.
		// No entanto, omitimos o THETA S, THETA V e THETA Z1, pois eles têm interfaces que não podem capturar vídeo.
		// Da mesma forma que CanDraw, permitimos configurar um filtro de dispositivos UVC no Inspector.
			var result = !device.IsRicoh || device.IsTHETA;

			result &= UVCFilter.Match(device, UVCFilters);

			return true;
		}

		/**
		 * UVC機器が取り外された
		* Implementação do IOnUVCDetachEventHandler.
		 * @param manager O UVCManager que fez a chamada.
		 * @param device As informações do dispositivo UVC alvo.
		 */
		public void OnUVCDetachEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCDetachEvent:{device}");
#endif
		}

//		/**
//		 * 解像度選択
//		 * IOnUVCSelectSizeHandlerの実装
//		 * @param manager 呼び出し元のUVCManager
//		 * @param device 対象となるUVC機器の情報
//		 * @param formats 対応している解像度についての情報
//		 */
//		public SupportedFormats.Size OnUVCSelectSize(UVCManager manager, UVCDevice device, SupportedFormats formats)
//		{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//			Console.WriteLine($"{TAG}OnUVCSelectSize:{device}");
//#endif
//			if (device.IsTHETA_V || device.IsTHETA_Z1)
//			{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//				Console.WriteLine($"{TAG}OnUVCSelectSize:THETA V/Z1");
//#endif
//				return FindSize(formats, 3840, 1920);
//			}
//			else if (device.IsTHETA_S)
//			{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//				Console.WriteLine($"{TAG}OnUVCSelectSize:THETA S");
//#endif
//				return FindSize(formats, 1920, 1080);
//			}
//			else
//			{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//				Console.WriteLine($"{TAG}OnUVCSelectSize:other UVC device,{device}");
//#endif
//				return formats.Find(DefaultWidth, DefaultHeight);
//			}
//		}

		/**
		 * Obtém se IUVCDrawer é capaz de renderizar o vídeo do dispositivo UVC especificado. 
		 * Implementação de IUVCDrawer.
		 * @param manager O UVCManager que fez a chamada.
		 * @param device As informações do dispositivo UVC alvo.
		 */
		public bool CanDraw(UVCManager manager, UVCDevice device)
		{
			return UVCFilter.Match(device, UVCFilters);
		}

		/**
		 * Iniciou a captura de vídeo. 
		 * Implementação de IUVCDrawer. 
		 * @param manager O UVCManager que fez a chamada. 
		 * @param device As informações do dispositivo UVC alvo.
		 * @param tex Uma instância de Textura para receber o vídeo do dispositivo UVC."
		 */
		public void OnUVCStartEvent(UVCManager manager, UVCDevice device, Texture tex)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCStartEvent:{device}");
#endif
			HandleOnStartPreview(tex);
		}

		/**
		 * A captura de vídeo foi encerrada.
		 * Implementação de IUVCDrawer.
		 * @param manager O UVCManager que fez a chamada. 
		 * @param device As informações do dispositivo UVC alvo.
		 */
		public void OnUVCStopEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCStopEvent:{device}");
#endif
			HandleOnStopPreview();
		}

		//================================================================================
		/**
		 * Atualizar destino do desenho
		 */
		private void UpdateTarget()
		{
			bool found = false;
			if ((RenderTargets != null) && (RenderTargets.Count > 0))
			{
				TargetMaterials = new UnityEngine.Object[RenderTargets.Count];
				SavedTextures = new Texture[RenderTargets.Count];
				quaternions = new Quaternion[RenderTargets.Count];
				int i = 0;
				foreach (var target in RenderTargets)
				{
					if (target != null)
					{
						var material = TargetMaterials[i] = GetTargetMaterial(target);
						if (material != null)
						{
							found = true;
						}
#if (!NDEBUG && DEBUG && ENABLE_LOG)
						Console.WriteLine($"MQ:{TAG}UpdateTarget:material={material}");
#endif
					}
					i++;
				}
			}
			if (!found)
			{   // Se nenhum destino de desenho for encontrado, este script
				// Tenta obter do AddComponent GameObject
				// Definir gameObject como XXX RenderTargets?
				TargetMaterials = new UnityEngine.Object[1];
				SavedTextures = new Texture[1];
				quaternions = new Quaternion[1];
				TargetMaterials[0] = GetTargetMaterial(gameObject);
				found = TargetMaterials[0] != null;
			}

			if (!found)
			{
				throw new UnityException("no target material found.");
			}
		}

		/**
		* Obtenha o material para desenhar a imagem como textura
		* Se o GameObject especificado tiver Skybox/Renderer/RawImage/Material, obtenha o Material dele.
		* Se cada um for alocado várias vezes, retorne o primeiro disponível encontrado.
		* Prioridade: Skybox > Renderizador > RawImage > Material
		* @param target
		* @return Retorna nulo se não for encontrado
		 */
		UnityEngine.Object GetTargetMaterial(GameObject target/*NonNull*/)
		{
			// Tente obter o Skybox.
			// var skyboxs = target.GetComponents<Skybox>();
			// if (skyboxs != null)
			// {
			// 	foreach (var skybox in skyboxs)
			// 	{
			// 		if (skybox.isActiveAndEnabled && (skybox.material != null))
			// 		{
			// 			RenderSettings.skybox = skybox.material;
			// 			return skybox.material;
			// 		}
			// 	}
			// }
			// Se não for possível obter um Skybox, tente obter um Renderer.
			// var renderers = target.GetComponents<Renderer>();
			// if (renderers != null)
			// {
			// 	foreach (var renderer in renderers)
			// 	{
			// 		if (renderer.enabled && (renderer.material != null))
			// 		{
			// 			return renderer.material;
			// 		}

			// 	}
			// }
			// Se não for possível obter um Skybox ou um Renderer, tente obter um RawImage.
			var rawImages = target.GetComponents<RawImage>();
			if (rawImages != null)
			{
				foreach (var rawImage in rawImages)
				{
					if (rawImage.enabled && (rawImage.material != null))
					{
						return rawImage;
					}

				}
			}
			else {
				Debug.Log("MQ: RawImage nao encontrado");
			}
			// Se não for possível obter um Skybox, um Renderer ou um RawImage, tente obter um Material.
		// 	var material = target.GetComponent<Material>();
		// 	if (material != null)
		// 	{
		// 		return material;
		// 	}
			return null;
		}

		private void RestoreTexture()
		{
			for (int i = 0; i < TargetMaterials.Length; i++)
			{
				var target = TargetMaterials[i];
				try
				{
					if (target is Material)
					{
						(target as Material).mainTexture = SavedTextures[i];
					}
					else if (target is RawImage)
					{
						(target as RawImage).texture = SavedTextures[i];
					}
				}
				catch
				{
					Console.WriteLine($"{TAG}RestoreTexture:Exception cought");
				}
				SavedTextures[i] = null;
				quaternions[i] = Quaternion.identity;
			}
		}

		private void ClearTextures()
		{
			for (int i = 0; i < SavedTextures.Length; i++)
			{
				Debug.Log("MQ: Clear Textures");
				SavedTextures[i] = null;
			}
		}

		/**
		* Processamento no início da aquisição da imagem
		* @param tex Textura para receber a imagem
		*/
		private void HandleOnStartPreview(Texture tex)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStartPreview:({tex})");
#endif
			int i = 0;

			foreach (var target in TargetMaterials)
			{
				if (target is Material)
				{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"{TAG}HandleOnStartPreview:assign Texture to Material({target})");
#endif
					SavedTextures[i++] = (target as Material).mainTexture;
					(target as Material).mainTexture = tex;
				}
				else if (target is RawImage)
				{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"MQ:{TAG}HandleOnStartPreview:assign Texture to RawImage({target})");
#endif
					SavedTextures[i++] = (target as RawImage).texture;
					(target as RawImage).texture = tex;
				}
			}
		}

		/**
		 * Processamento no lado do Unity quando a aquisição da imagem for concluída
		 */
		private void HandleOnStopPreview()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"MQ:{TAG}HandleOnStopPreview:");
#endif
			// Restaurar a textura de destino do desenho
			RestoreTexture();
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"MQ: {TAG}HandleOnStopPreview:finished");
#endif
		}

	} // class UVCDrawer

} // namespace Serenegiant.UVC
