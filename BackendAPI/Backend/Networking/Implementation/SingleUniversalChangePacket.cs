﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackendAPI.Bibliotecas;
using BackendAPI.Networking.Attributes;
using BackendAPI.Networking.IO;
using BackendAPI.Objects;

using Newtonsoft.Json;

namespace BackendAPI.Networking.Implementation
{
    [Architecture(PacketArchitecture.ClientToServer)]
    public class SingleUniversalChangePacket : INetworkPacket
    {
        public SingleUniversalChangePacket()
        { }
        public SingleUniversalChangePacket(DeckImage deckImage)
        {
            DeckImage = deckImage;
        }

        public DeckImage DeckImage { get; set; }
        public int ImageSlot { get; set; }
 

       public DeckItemMisc CurrentItem { get; set; }
        public override object Clone()
        {
            return new SingleUniversalChangePacket();
        }

        public override void FromInputStream(DataInputStream reader)
        {
            //Client to Server 
        }

        public override long GetPacketNumber() => 5;

        public override void ToOutputStream(DataOutputStream writer)
        {
            //Server to Client
            writer.WriteBoolean(DeckImage != null);
            if (DeckImage != null) {
                writer.WriteInt(ImageSlot);
                //Byte array lenght
                writer.WriteInt(DeckImage.InternalBitmap.Length);
                writer.Write(DeckImage.InternalBitmap);
                Json headerContent = new Json();

               
                headerContent.Font = " ";
                headerContent.Size = CurrentItem.Decksize;
                headerContent.Position = CurrentItem.Deckposition;
                headerContent.Text = CurrentItem.Deckname;
                headerContent.Color = CurrentItem.Deckcolor;
                headerContent.Stroke_color = CurrentItem.Stroke_color;
                headerContent.Stroke_dx = CurrentItem.Stroke_dxtext;
                headerContent.Stroke_radius = CurrentItem.Stroke_radius;
                headerContent.Stroke_dy = CurrentItem.Stroke_Dy;
                headerContent.IsStroke = CurrentItem.IsStroke;
                headerContent.Isboldtext = CurrentItem.Isboldtext;
                headerContent.Isnormaltext = CurrentItem.Isnormaltext;
                headerContent.Isitalictext = CurrentItem.Isitalictext;
                headerContent.Ishinttext = CurrentItem.Ishinttext;
                string jsonString = JsonConvert.SerializeObject(headerContent, Formatting.None);

                writer.WriteUTF(jsonString);
            }
        }
    }
}
