﻿
using BackendAPI.Utils.Native;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using BackendAPI.Properties;

namespace BackendAPI.Objects.Implementation.DeckActions.General
{
    public class MediaKeyAction : AbstractDeckAction
    {
        public enum MediaKeys
        {
            [Description("MISCMEDIAKEYSPREVIUS")]
            Back,
            [Description("MISCMEDIAKEYSNEXT")]
            Next,
            [Description("MISCMEDIAKEYSPLAYPAUSE")]
            PlayPause,
            [Description("MISMEDIAKEYSSTOP")]
            Stop,
            [Description("MISCMEDIAKEYSMUTE")]
            VolumeOff,
            [Description("MISCMEDIAKEYSVOLUMEDOWN")]
            VolumeMinus,
            [Description("MISCMEDIAKEYSVOLUMEUP")]
            VolumePlus
        }

        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONOFUNCTIONLIST")]
        [ActionPropertyUpdateImageOnChanged]
        public MediaKeys Key { get; set; } = MediaKeys.PlayPause;

        public override AbstractDeckAction CloneAction()
        {
            return new MediaKeyAction();
        }
    

        public override DeckActionCategory GetActionCategory()
        {
            return DeckActionCategory.General;
        }

        public override string GetActionName()
        {
            return Texts.rm.GetString("DECKGENERALMEDIAKEY", Texts.cultereinfo);
        }

        [Obsolete]
        public override bool OnButtonClick(DeckDevice deckDevice)
        {
            
            return false;
        }

        public Keys GetKeyFromMediaKey(MediaKeys mediaKey)
        {
            switch (mediaKey) {
                case MediaKeys.Back:
                    return Keys.MediaPreviousTrack;
                case MediaKeys.Next:
                    return Keys.MediaPreviousTrack;
                case MediaKeys.PlayPause:
                    return Keys.MediaPlayPause;
                case MediaKeys.Stop:
                    return Keys.MediaStop;
                case MediaKeys.VolumeOff:
                    return Keys.VolumeMute;
                case MediaKeys.VolumeMinus:
                    return Keys.VolumeDown;
                case MediaKeys.VolumePlus:
                    return Keys.VolumeUp;
            }
            return Keys.None;
        }

        public override void OnButtonDown(DeckDevice deckDevice)
        {

        }

        public override void OnButtonUp(DeckDevice deckDevice)
        {
            var key = GetKeyFromMediaKey(Key);
            if (key != Keys.None) {
                NativeKeyHandler.ClickKey(new[] { key });
            }
        }

        public override DeckImage GetDefaultItemImage()
        {
            var img = GetKey(Key);
            if (img != null)
                return new DeckImage(img);
            return base.GetDefaultItemImage();
        }

        private Bitmap GetKey(MediaKeys key)
        {
            switch (key) {
                case MediaKeys.Back:
                    return Resources.img_item_media_back;
                case MediaKeys.Next:
                    return Resources.img_item_media_next;
                case MediaKeys.PlayPause:
                    return Resources.img_item_media_playpause;
                case MediaKeys.Stop:
                    return Resources.img_item_media_stop;
                case MediaKeys.VolumeOff:
                    return Resources.img_item_media_volumeoff;
                case MediaKeys.VolumeMinus:
                    return Resources.img_item_media_volumedown;
                case MediaKeys.VolumePlus:
                    return Resources.img_item_media_volumeup;
                default:
                    return null;
            }
        }
    }
}
