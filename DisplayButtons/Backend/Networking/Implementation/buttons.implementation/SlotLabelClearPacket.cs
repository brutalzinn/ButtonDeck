﻿using DisplayButtons.Backend.Networking.Attributes;
using DisplayButtons.Backend.Networking.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayButtons.Backend.Networking.Implementation
{
    [Architecture(PacketArchitecture.ClientToServer)]
    public class SlotLabelClearPacket : INetworkPacket
    {
        public SlotLabelClearPacket(int slotID)
        {
            SlotID = slotID;
        }

        public int SlotID { get; set; }
        public override void FromInputStream(DataInputStream reader)
        {
            
        }

        public override long GetPacketNumber() => 15;

        public override void ToOutputStream(DataOutputStream writer)
        {
            writer.WriteInt(SlotID);   
        }
    }
}
