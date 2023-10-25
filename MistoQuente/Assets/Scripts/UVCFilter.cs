//#define ENABLE_LOG
/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Serenegiant.UVC
{
	/**
	 * Classe de definição de filtro para dispositivos UVC
	 */
	[Serializable]
	public class UVCFilter
	{
		private const string TAG = "UVCFilter#";

		/**
		 * Uma string para exibir comentários do filtro no inspector (não usada no script)
		 */
		public string Description;
		/**
		 * ID do fabricante correspondente
		 * 0 significa correspondência com todos os fabricantes
		 */
		public int Vid;
		/**
		 * ID do produto correspondente
		 * 0 significa correspondência com todos os produtos
		 */
		public int Pid;
		/**
		 * Nome do dispositivo correspondente
		 * null/vazio significa que não será verificado
		 */
		public string DeviceName;
		/**
		 * Se deve ser tratado como filtro de exclusão
		 */
		public bool IsExclude;

		//--------------------------------------------------------------------------------

		/**
		 * Obtém se corresponde a um dispositivo UVC dado
		 * @param device
		 */
		public bool Match(UVCDevice device)
		{
			bool result = device != null;

			if (result)
			{
				result &= ((Vid <= 0) || (Vid == device.vid))
					&& ((Pid <= 0) || (Pid == device.pid))
					&& (String.IsNullOrEmpty(DeviceName)
						|| DeviceName.Equals(device.name)
						|| DeviceName.Equals(device.name)
						|| (String.IsNullOrEmpty(device.name) || device.name.Contains(DeviceName))
						|| (String.IsNullOrEmpty(device.name) || device.name.Contains(DeviceName))
					);
			}

			return result;
		}

		//--------------------------------------------------------------------------------

		/**
		 * Processamento de filtro para dispositivos UVC
		 * Se os filtros forem nulos, considera-se que houve correspondência
		 * Se houver correspondência com um filtro de exclusão, a avaliação será encerrada 
		 nesse ponto e retornará falso
		 * Se não houver correspondência com um filtro de exclusão e houver correspondência 
		 com qualquer filtro regular, retornará verdadeiro
	
		 * @param device
		 * @param filters Nullable
		 */
		public static bool Match(UVCDevice device, List<UVCFilter> filters/*Nullable*/)
		{
			return Match(device, filters != null ? filters.ToArray() : (null as UVCFilter[]));
		}

		/**
		 * Processamento de filtro para dispositivos UVC
		 * Se os filtros forem nulos, considera-se que houve correspondência
		 * Se houver correspondência com um filtro de exclusão, a avaliação será encerrada 
		 nesse ponto e retornará falso
		 * Se não houver correspondência com um filtro de exclusão e houver correspondência com 
		 qualquer filtro regular, retornará verdadeiro
		 * @param device
		 * @param filters Nullable
		 */
		public static bool Match(UVCDevice device, UVCFilter[] filters/*Nullable*/)
		{
			var result = true;

			if ((filters != null) && (filters.Length > 0))
			{
				result = false;
				foreach (var filter in filters)
				{
					if (filter != null)
					{
						var b = filter.Match(device);
						if (b && filter.IsExclude)
						{   // Se houver correspondência com um filtro de exclusão, a avaliação será encerrada nesse ponto
							result = false;
							result = false;
							break;
						}
						else
						{   // Qualquer correspondência é suficiente
							result |= b;
						}
					}
					else
					{
						// Um filtro vazio é considerado uma correspondência
						result = true;
					}

				}
			}

#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Match({device}):result={result}");
#endif
			return result;
		}

	} // class UVCFilter

} // namespace Serenegiant.UVC
