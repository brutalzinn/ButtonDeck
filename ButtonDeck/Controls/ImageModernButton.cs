﻿using ButtonDeck.Forms;
using ButtonDeck.Backend.Networking;
using ButtonDeck.Backend.Networking.TcpLib;
using ButtonDeck.Backend.Objects;
using ButtonDeck.Backend.Utils;
using NickAc.ModernUIDoneRight.Controls;
using NickAc.ModernUIDoneRight.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ButtonDeck.Backend.Networking.Implementation;
using ButtonDeck.Backend.Objects.Implementation;


namespace ButtonDeck.Controls
{
    public class ImageModernButton : ModernButton
    {

        public int CurrentSlot {
            get {
                try {
                    return int.Parse(ExtractNumber(Name));
                } catch (Exception) {
                    return -1;
                }
            }
        }
        public ImageModernButton Origin { get; set; }


        public static Guid GetConnectionGuidFromDeckDevice(DeckDevice device)
        {
            if (Program.mode == 0)
            {

                var connections = Program.ServerThread.TcpServer?.Connections.OfType<ConnectionState>().Where(c => c.IsStillFunctioning());
                return DevicePersistManager.DeckDevicesFromConnection.Where(m => connections.Select(c => c.ConnectionGuid).Contains(m.Key)).FirstOrDefault(m => m.Value.DeviceGuid == device.DeviceGuid).Key;

            }
            else
            {
                var connections = Program.ClientThread.TcpClient?.Connections.OfType<ConnectionState>().Where(c => c.IsStillFunctioning());
                return DevicePersistManager.DeckDevicesFromConnection.Where(m => connections.Select(c => c.ConnectionGuid).Contains(m.Key)).FirstOrDefault(m => m.Value.DeviceGuid == device.DeviceGuid).Key;

            }

        }
      private string _text;
        private Font _font;
        private Brush _brush;

        private PointF _pointf;

        public void TextLabel(string text, Font font, Brush brush, PointF pointf)
        {

            _text = text;
            _font = font;
            _brush = brush;
            _pointf = pointf;

        }
        public string ExtractNumber(string original)
        {
            return new string(original.Where(Char.IsDigit).ToArray());
        }

        private Image _image;
      
        public Image NormalImage {
            get => Origin?._image ?? _image; set {
                if (Origin != null) {
                    Origin._image = value;
                    return;
                }
                _image = value;
                if (IsHandleCreated)
                    Invoke(new Action(Refresh));
            }
        }

        public new Image Image {
            get => Origin?.Image ?? _image;
            set {
                if (Origin != null) {
                    Origin.Image = value;
                    return;
                }
                _image = value;
                Refresh();

                if (Parent != null && Parent.Parent != null && Parent.Parent is MainForm frm) {
                    if (frm.CurrentDevice != null) {
                        int slot = int.Parse(ExtractNumber(Name));
                        IEnumerable<ConnectionState> connections;
                        if (Program.mode == 0)
                        {

                         connections = Program.ServerThread.TcpServer?.Connections.OfType<ConnectionState>().Where(c => c.IsStillFunctioning());


                        }
                        else
                        {
                             connections = Program.ClientThread.TcpClient?.Connections.OfType<ConnectionState>().Where(c => c.IsStillFunctioning());


                        }
                        var stateID = GetConnectionGuidFromDeckDevice(frm.CurrentDevice);
                        var state = connections.FirstOrDefault(m => m.ConnectionGuid == stateID);
                        if (value == null) {
                            //Send clear packet
                            state?.SendPacket(new SlotImageClearPacket(slot));
                            return;
                        }

                        Bitmap bmp = new Bitmap(value);
                        var deckImage = new DeckImage(bmp);
                        
                        if (Tag is DynamicDeckItem itemTag) {
                            itemTag.DeckImage = deckImage;

                        } else if (Tag is DynamicDeckFolder itemFolder) {
                            itemFolder.DeckImage = deckImage;
                        }
                        if (Tag is IDeckItem itemNew)
                        {
                            if (state != null)
                            {
                                state.SendPacket(new SingleUniversalChangePacket(deckImage)
                                {
                                    ImageSlot = slot,
                                    Color = itemNew.DeckColor,
                                    Font = " ",
                                    Text = itemNew.DeckName,
                                    Size = itemNew.DeckSize,
                                    Position = itemNew.DeckPosition

                                });
                            }
                        }
                        if (Tag is DynamicDeckItem item) {
                            var device = frm.CurrentDevice;
                            device.CheckCurrentFolder();
                            device.CurrentFolder.Add(slot, item);
                        }
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            if (Image != null) {
                pevent.Graphics.DrawImage(Image, DisplayRectangle);
            }
            if(_text != null){

                pevent.Graphics.DrawString(_text, _font, _brush, _pointf);

            }
        }
        

    }
}
