﻿using BackendAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendAPI.Objects.Implementation.DeckActions.Misc
{
    public class IFTTTEventAction : AbstractDeckAction
    {
        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONEVENTNAME")]
        public string EventName { get; set; } = "";

        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONVALUEONE")]
        public string Value1 { get; set; } = "";

        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONVALUETWO")]
        public string Value2 { get; set; } = "";

        [ActionPropertyInclude]
        [ActionPropertyDescription("DESCRIPTIONVALUETHREE")]
        public string Value3 { get; set; } = "";


        public override AbstractDeckAction CloneAction()
        {
            return new IFTTTEventAction();
        }

        public override DeckActionCategory GetActionCategory()
        {
            return DeckActionCategory.Misc;
        }

        public override string GetActionName()
        {
            return Texts.rm.GetString("DECKMISCIFTTT", Texts.cultereinfo);
        }

        public override void OnButtonDown(DeckDevice deckDevice)
        {
        }

        public override void OnButtonUp(DeckDevice deckDevice)
        {
            if (CheckTokenAndWarnIfNeeded()) {
                IFTTTWebHookClient client = new IFTTTWebHookClient().WithToken(ApplicationSettingsManager.Settings.IFTTTAPIKey);

                client.FireEvent(EventName, new Utils.Misc.IFTTTWebhookProperties() { Value1 = Value1, Value2 = Value2, Value3 = Value3});
            }
        }

        private bool CheckTokenAndWarnIfNeeded()
        {
            if (!IFTTTWebHookClient.IsValid(ApplicationSettingsManager.Settings.IFTTTAPIKey)) {
                //Warn that the token is empty and not configured.
                return false;
            }
            return true;
        }
    }
}
