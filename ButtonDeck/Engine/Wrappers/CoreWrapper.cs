﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using NickAc.Backend.Objects.Implementation.DeckActions.General;
using ScribeBot.Engine.Containers;

namespace ScribeBot.Engine.Wrappers
{
    /// <summary>
    /// Wrapper containing all core functionality exposed to Lua environment.
    /// </summary>
    ///     [MoonSharpUserData]
    ///         [MoonSharpUserData]

   
       
   
    [MoonSharpUserData]
    static class CoreWrapper
    {

        public static void testando(string value)
           
        {
            ButtonDeck.Forms.MainForm.testando(value);
            


        }
        public static string Version => Core.Version;
      
        public static void ProcessConsoleInput() => Core.ProcessConsoleInput();

        public static void Write(string text) => Core.Write(text);

        public static void WriteLine(params object[] args)
        {
            if (args.Length > 1)
                Core.WriteLine(args);
            else
                Core.WriteLine(args[0].ToString());
        }

        public static void SetFocusWindow(string title) => Native.API.SetFocusWindow(title);

        public static string GetFocusWindow() => Native.API.GetFocusWindow();

        public static void SetWindowSize(string title, int w, int h) => Native.API.SetWindowSize(title, w, h);

        public static SizeContainer GetWindowSize(string title) => Native.API.GetWindowSize(title);

        public static void SetWindowPos(string title, int x, int y) => Native.API.SetWindowPos(title, x, y);

        public static PointContainer GetWindowPos(string title) => Native.API.GetWindowPos(title);

        public static bool IsWindowVisible(string title) => Native.API.IsWindowVisible(title);

        public static string[] GetWindowTitles() => Native.API.GetWindowTitles();

        public static void Close() => Core.Close();
    }
}
