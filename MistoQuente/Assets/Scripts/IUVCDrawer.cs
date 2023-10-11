/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MistoQuente.UVC
{

	/**
	 * Interface de manipulação de eventos relacionados ao UVC
	 */
	public interface IUVCDrawer
	{
		/**
		 * Um dispositivo UVC foi conectado
		 * @param manager O UVCManager que fez a chamada
		 * @param device Informações sobre o dispositivo UVC conectado
		 * @return true: Usar o dispositivo UVC, false: Não usar o dispositivo UVC
		 */
		bool OnUVCAttachEvent(UVCManager manager, UVCDevice device);
		/**
		 * Um dispositivo UVC foi desconectado
		 * @param manager O UVCManager que fez a chamada
		 * @param device Informações sobre o dispositivo UVC desconectado
		 */
		void OnUVCDetachEvent(UVCManager manager, UVCDevice device);
		/**
		 * Verifica se o IUVCDrawer pode desenhar a imagem do dispositivo UVC especificado
		 * @param manager O UVCManager que fez a chamada
		 * @param device Informações sobre o dispositivo UVC conectado
		 */
		bool CanDraw(UVCManager manager, UVCDevice device);
		/**
		 * Iniciou a captura de vídeo do dispositivo UVC
		 * @param manager O UVCManager que fez a chamada
		 * @param device Informações sobre o dispositivo UVC conectado
		 * @param tex Objeto Texture para receber o vídeo do dispositivo UVC
		 */
		void OnUVCStartEvent(UVCManager manager, UVCDevice device, Texture tex);
		/**
		 * Encerrou a captura de vídeo do dispositivo UVC
		 * @param manager O UVCManager que fez a chamada
		 * @param device Informações sobre o dispositivo UVC conectado
		 */
		void OnUVCStopEvent(UVCManager manager, UVCDevice device);

	}   // interface IUVCDrawer

}	// namespace MistoQuente.UVC
