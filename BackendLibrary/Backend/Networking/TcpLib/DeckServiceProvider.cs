﻿using BackendLibrary;
using BackendLibrary.ObjectsHelpers.Events;
using DisplayButtons.Backend.Networking.Implementation;
using DisplayButtons.Backend.Objects;
using DisplayButtons.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DisplayButtons.Backend.Networking.TcpLib
{
    public class DeckServiceProvider : TcpServiceProvider
    {

        private static List<INetworkPacket> networkPackets = new List<INetworkPacket>();

        public static System.Timers.Timer aTimer = new System.Timers.Timer();

        public static bool isRetryconnectect = false;

        public static bool CanStart = false;
        static DeckServiceProvider()
        {
            RegisterNetworkPacket(new HelloPacket());
            RegisterNetworkPacket(new DeviceIdentityPacket());
            RegisterNetworkPacket(new HeartbeatPacket());
            RegisterNetworkPacket(new DesktopDisconnectPacket());
            RegisterNetworkPacket(new AlternativeHello());
            RegisterNetworkPacket(new ButtonInteractPacket());
            RegisterNetworkPacket(new MatrizPacket());
            //RegisterNetworkPacket(new SlotLabelButtonChangeChunkPacket());
            //RegisterNetworkPacket(new SlotLabelButtonClearChunkPacket());
            //    RegisterNetworkPacket(new UsbInteractPacket());
        }

        public static void RegisterNetworkPacket(INetworkPacket packet)
        {
            networkPackets.Add(packet);
        }

        public static INetworkPacket GetNewNetworkPacketById(long id)
        {
            try {
                return (INetworkPacket)networkPackets.FirstOrDefault(p => p.GetPacketNumber() == id).Clone();
            } catch (Exception) {
                throw new Exception($"NetworkPacket[ID: {id}] wasn't registered to the packet storage.");
            }
        }

        public override void OnAcceptConnection(ConnectionState state)
        {
          
        }
      

     
        public override void OnRetryConnect(ConnectionState state)
        {
            if(Config.mode == 1)
            {
                DeviceStatusInform teste = new DeviceStatusInform();
                teste.RaiseEvent(0);
            }
            

        }
      
        public override void OnDropConnection(ConnectionState state)
        {
            //isConnected = false;
           // state.SendPacket(new DesktopDisconnectPacket());
         DevicePersistManager.RemoveConnectionState(state);
        }

        public override void OnReceiveData(ConnectionState state)
        {
            List<byte> allData = new List<byte>();
            byte[] buffer;
            Debug.WriteLine("AvailiableData: " + state.AvailableData);
            
            while (state.AvailableData > 0) {
                buffer = new byte[1024];
                state.Read(buffer, 0, 1024);
                allData.AddRange(buffer);
            }
            var packet = state.ReadPacket(allData.ToArray());

        }
        public override object Clone()
        {
            return new DeckServiceProvider();
        }

    }
}