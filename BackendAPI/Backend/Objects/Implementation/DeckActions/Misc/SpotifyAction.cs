﻿
using BackendAPI.Utils.Native;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;
using System.Linq;
using SpotifyAPI.Web;

using System.Threading.Tasks;

using BackendAPI.Bibliotecas.SpotifyWrapper;

using BackendAPI.Properties;

namespace BackendAPI.Objects.Implementation.DeckActions.General
{
    public class SpotifyAction : AbstractDeckAction, IDeckHelper
    {
 
        public enum SpotifyMediaKeys
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
            VolumePlus,
            [Description("MISCMEDIAKEYSPLAYLIST")]
            PlayList
        }
        int CurrentItem = 1;
        IDeckItem atual_item;
        public string PlayListId;
        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONVOLUMESTEPPER")]
        [ActionPropertyUpdateImageOnChanged]
        public int VolumeSensibility { get; set; } = 10;

        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONPLAYLISTCONFIG")]
        [ActionPropertyUpdateImageOnChanged]
         public string ConfigPlay { get; set; }


        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONOFUNCTIONLIST")]
        [ActionPropertyUpdateImageOnChanged]
        public SpotifyMediaKeys Key { get; set; } = SpotifyMediaKeys.PlayPause;
       
        public override AbstractDeckAction CloneAction()
        {
            return new SpotifyAction();
        }

        public override DeckActionCategory GetActionCategory()
        {
            return DeckActionCategory.Misc;
        }
        public override bool setLayer(int _current, IDeckItem item)
        {
            if (_current != -1)
            {
                CurrentItem = _current;
                atual_item = item;

            }
            return true;
        }
        public override bool getLayer()
        {
            switch (Key)
            {
                case SpotifyMediaKeys.VolumeOff:

                    return true;

                default:
                    return false;

            }
        }
        public override string GetActionName()
        {
            return Texts.rm.GetString("DECKMISCSPOTIFY", Texts.cultereinfo);
        }

        [Obsolete]
        public override bool OnButtonClick(DeckDevice deckDevice)
        {
            return false;
        }

    

        public override void OnButtonDown(DeckDevice deckDevice)
        {

        }

       
        public override void OnButtonUp(DeckDevice deckDevice)
        { 
            var auth_result = Task.FromResult( Spotify.Auth()).Result;
            if (!auth_result.Result)
            {
                return;

            }
            switch (Key)
            {
                case SpotifyMediaKeys.PlayPause:

                    
                    Task.FromResult(Spotify.ResumePlayBack());
                    break;
                case SpotifyMediaKeys.Stop:

                 
                    Task.FromResult(Spotify.PausePlayBack());
                    break;

                case SpotifyMediaKeys.VolumePlus:


                    Task.FromResult(Spotify.setVolume(VolumeSensibility));
                    break;
                case SpotifyMediaKeys.VolumeMinus:


                    Task.FromResult(Spotify.setVolume(-VolumeSensibility));
                    break;
                case SpotifyMediaKeys.PlayList:
                    var val = Task.Run(() => Spotify.getPlayListById(PlayListId));
                    Task.FromResult(Spotify.PlayPlaylist(val.Result));
                    break;
                case SpotifyMediaKeys.Next:
             
                    Task.FromResult(Spotify.SkipNext());
                    break;
                case SpotifyMediaKeys.Back:
                    Task.FromResult(Spotify.SkipPrevius());
                    break;
                case SpotifyMediaKeys.VolumeOff:
                  //  Task.FromResult(Spotify.MuteDesmute());
                    var result = Task.FromResult(Spotify.MuteDesmute().Result);
                    IDeckHelper.setVariable(result.Result, atual_item, deckDevice);
                    break;

            }
        }
        public void VolumeSensibilityHelper()
        {
            dynamic form = Activator.CreateInstance(FindType("Forms.ActionHelperForms.SpotifyForms.SpotifyAudioSensibility")) as Form;
            form.textBox1.Text = VolumeSensibility.ToString();
            if (form.ShowDialog() == DialogResult.OK)
            {
                int result = Convert.ToInt32(form.textBox1.Text);
                if( result <= 100)
                {
VolumeSensibility = result;

                }
                
            }
            else
            {
                form.Close();
            }


        }
   
        public void ConfigPlayHelper()
        {
      
            dynamic form = Activator.CreateInstance(FindType("Forms.ActionHelperForms.SpotifyForms.SpotifyPlaylists")) as Form;
            form.SelectPlaylist(PlayListId);
            if (form.ShowDialog() == DialogResult.OK)
            {


                PlayListId = ((GlobalControlSpotifyPlayList)form.comboBox1.SelectedItem).Value.Id;
            }
            else
            {
                form.Close();
            }
          
        }

        public override DeckImage GetDefaultItemImage()
        {
            var img = GetKey(Key);
            if (img != null)
                return new DeckImage(img);
            return base.GetDefaultItemImage();
        }

        private Bitmap GetKey(SpotifyMediaKeys key)
        {
            switch (key) {
                case SpotifyMediaKeys.Back:
                    return Resources.img_item_media_back;
                case SpotifyMediaKeys.Next:
                    return Resources.img_item_media_next;
                case SpotifyMediaKeys.PlayPause:
                    return Resources.img_item_media_playpause;
                case SpotifyMediaKeys.Stop:
                    return Resources.img_item_media_stop;
                case SpotifyMediaKeys.VolumeOff:
                    return Resources.img_item_media_volumeoff;
                case SpotifyMediaKeys.VolumeMinus:
                    return Resources.img_item_media_volumedown;
                case SpotifyMediaKeys.VolumePlus:
                    return Resources.img_item_media_volumeup;
                default:
                    return null;
            }
        }
    }
}
