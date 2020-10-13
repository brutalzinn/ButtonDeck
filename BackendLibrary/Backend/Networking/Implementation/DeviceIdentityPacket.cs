﻿using System;
using DisplayButtons.Backend.Networking.IO;
using DisplayButtons.Backend.Networking.Attributes;
using DisplayButtons.Backend.Networking.TcpLib;
using DisplayButtons.Backend.Objects;
using DisplayButtons.Backend.Utils;
using System.Diagnostics;

namespace DisplayButtons.Backend.Networking.Implementation
{
    [Architecture(PacketArchitecture.ClientToServer | PacketArchitecture.ServerToClient)]
    public class DeviceIdentityPacket : INetworkPacket
    {
        public DeviceIdentityPacket()
        {

        }
        private readonly bool hasDeviceGuid;

        public DeviceIdentityPacket(bool hasDeviceGuid)
        {
            this.hasDeviceGuid = hasDeviceGuid;
        }

       [ServerOnly]
        public Guid DeviceGuid { get; set; }

      [ClientOnly]
        public string DeviceName { get; set; }

        public override long GetPacketNumber() => 2;

        public override void FromInputStream(DataInputStream reader)
        {
            string receivedGuid = reader.ReadUTF().ToUpperInvariant();
 
           DeviceGuid = new Guid(receivedGuid);
              DeviceName = reader.ReadUTF();
        }

        public override void Execute(ConnectionState state)
        {

                

            DeckDevice deckDevice = new DeckDevice(DeviceGuid, DeviceName);

            DevicePersistManager.PersistDevice(deckDevice);
            DevicePersistManager.ChangeConnectedState(state, deckDevice);

    
            DevicePersistManager.OnDeviceConnected(this, deckDevice);
            Debug.WriteLine("MOSTRANDO GUID PARA: " + DeviceName);
            Debug.WriteLine("MOSTRANDO GUID PARA: " + DeviceGuid);


        }


        public override void ToOutputStream(DataOutputStream writer)
        {
            //From server to client
            //Tell the client if they are going to receive a device Guid
            writer.WriteBoolean(!hasDeviceGuid);
            if (!hasDeviceGuid) {
                DeviceGuid = Guid.NewGuid();
                writer.WriteUTF(DeviceGuid.ToString());
            }
        }

        public override object Clone()
        {
            return new DeviceIdentityPacket();
        }
    }
}