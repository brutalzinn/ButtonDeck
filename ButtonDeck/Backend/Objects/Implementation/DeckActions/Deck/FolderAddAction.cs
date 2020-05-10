﻿using ButtonDeck.Backend.Utils;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ButtonDeck.Backend.Objects.Implementation.DeckActions.General
{
    public class FolderAddAction : AbstractDeckAction
    {
      

   

        public override bool IsPlugin()
        {
            return true;
        }
  

        [ActionPropertyInclude]
        [ActionPropertyDescription("name")]
          public static string name { get; set; } = "teste";

        public static MoonSharp.Interpreter.ScriptFunctionDelegate LuaExecuteButtonDown { get; set; } = null;
        public static string script { get; set; } = "";
        public static string DeckActionCategory_string { get; set; } = "Deck";
        public void ToExecuteHelper()
        {
        
        }

        public override AbstractDeckAction CloneAction()
        {
           return new FolderAddAction();
        }
      

        public override DeckActionCategory GetActionCategory()
        {

            DeckActionCategory animal = (DeckActionCategory)Enum.Parse(typeof(DeckActionCategory), DeckActionCategory_string);
            return animal;

        }

        public override string GetActionName()
        {
     
            return name;
        }

        public override void OnButtonDown(DeckDevice deckDevice)
        {

            ScribeBot.Scripter.Execute(script);
            DynValue luaFactFunction = ScribeBot.Scripter.Environment.Globals.Get("ButtonDown");

            ScribeBot.Scripter.Environment.Call(luaFactFunction);
            
        }

        public override void OnButtonUp(DeckDevice deckDevice)
        {
            ScribeBot.Scripter.Execute(script);
           DynValue luaFactFunction = ScribeBot.Scripter.Environment.Globals.Get("ButtonUP");

            ScribeBot.Scripter.Environment.Call(luaFactFunction);
        }
    }
}
