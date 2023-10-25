//#define ENABLE_LOG
/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */

using System;
using System.Runtime.InteropServices;

/*
 * THETA S  vid:1482, pid:1001
 * THETA V  vid:1482, pid:1002
 * THETA Z1 vid:1482, pid:1005
 */

namespace Serenegiant.UVC
{

	[Serializable]
	public class UVCDevice
	{
		public readonly Int32 id;
		public readonly int vid;
		public readonly int pid;
		public readonly int deviceClass;
		public readonly int deviceSubClass;
		public readonly int deviceProtocol;

		public readonly string name;

		public UVCDevice(IntPtr devicePtr) {
			id = GetId(devicePtr);
			vid = GetVendorId(devicePtr);
			pid = GetProductId(devicePtr);
			name = GetName(devicePtr);
			deviceClass = GetDeviceClass(devicePtr);
			deviceSubClass = GetDeviceSubClass(devicePtr);
			deviceProtocol = GetDeviceProtocol(devicePtr);
		}

		public override string ToString()
		{
			return $"{base.ToString()}(id={id},vid={vid},pid={pid},name={name},deviceClass={deviceClass},deviceSubClass={deviceSubClass},deviceProtocol={deviceProtocol})";
		}


		/**
		 * Verifica se é um produto da Ricoh
		 * @param info
		 */
		public bool IsRicoh
		{
			get { return (vid == 1482); }
		}

        /**
		 * Verifica se é um THETA S, V ou Z1
		 */
        public bool IsTHETA
        {
            get { return IsTHETA_S || IsTHETA_V || IsTHETA_Z1; }
        }
   
		/**
		 * Verifica se é um THETA S
		 */
		public bool IsTHETA_S
		{
			get { return (vid == 1482) && (pid == 10001); }
		}

		/**
		 * Verifica se é um THETA V
		 */
		public bool IsTHETA_V
		{
			// O pid 872 do THETA V não é UVC e não funciona (o lado do THETA é para imagem estática/vídeo)
			get { return (vid == 1482) && (pid == 10002); }
		}

        /**
		 * Verifica se é um THETA Z1
		 * @param info
		 */
        public bool IsTHETA_Z1
        {
			// O pid 877 do THETA Z1 não é UVC e não funciona (o lado do THETA é para imagem estática/vídeo)
            get { return (vid == 1482) && (pid == 10005); }
        }

        //--------------------------------------------------------------------------------
        //  Funções de interface do plugin
        //--------------------------------------------------------------------------------
        /**
		 * Obtém o ID do dispositivo (esta é a única função pública)
		 */
        [DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_id")]
		public static extern Int32 GetId(IntPtr devicePtr);

		/**
			* Obtém a classe do dispositivo
			*/
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_device_class")]
		private static extern Byte GetDeviceClass(IntPtr devicePtr);

		/**
			* Obtém a subclasse do dispositivo
			*/
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_device_sub_class")]
		private static extern Byte GetDeviceSubClass(IntPtr devicePtr);

		/**
			* Obtém o protocolo do dispositivo
			*/
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_device_protocol")]
		private static extern Byte GetDeviceProtocol(IntPtr devicePtr);

		/**
			* Obtém o ID do fabricante
			*/
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_vendor_id")]
		private static extern UInt16 GetVendorId(IntPtr devicePtr);

		/**
			* Obtém o ID do produto
			*/
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_product_id")]
		private static extern UInt16 GetProductId(IntPtr devicePtr);

		/**
			* Obtém o nome do dispositivo
			*/
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_name")]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private static extern string GetName(IntPtr devicePtr);

	} // UVCDevice

} // namespace Serenegiant.UVC

