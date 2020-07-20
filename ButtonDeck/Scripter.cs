﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using MoonSharp.Interpreter;
using System.Diagnostics;
using System.Threading.Tasks;
using ScribeBot.Engine.Containers;
using ScribeBot.Engine.Wrappers;
using ScribeBot.Engine.Proxies;
using ButtonDeck.Engine.Wrappers;
using static ButtonDeck.Backend.Objects.Implementation.DeckActions.General.FolderAddAction;

namespace ScribeBot
{
    /// <summary>
    /// Class creating and maintaining environment for Lua scripts.
    /// </summary>
    static class Scripter
    {
        /// <summary>
        /// Class instance containing MoonSharp scripting session.
        /// </summary>
        public static Script Environment { get; set; } = new Script();

        /// <summary>
        /// Thread scripts are executed on.
        /// </summary>
        public static Thread LuaThread { get; set; }

        /// <summary>
        /// Manual initializer.
        /// </summary>
        public static void Initialize() => Core.WriteLine(new ColorContainer(205, 205, 205), "-- SCRIPTER INITIALIZED");

        /// <summary>
        /// Static constructor initializing and sharing all vital functionality with Lua environment.
        /// </summary>
        static Scripter()
        {
            Script.WarmUp();
            Script.GlobalOptions.RethrowExceptionNested = true;

            UserData.RegisterAssembly();

            Environment.Options.DebugPrint = value => Core.Write(new ColorContainer(0, 131, 63), value + System.Environment.NewLine);
            Environment.Options.CheckThreadAccess = false;
            Environment.Options.UseLuaErrorLocations = true;
            Environment.PerformanceStats.Enabled = true;

            Directory.GetFiles($@"Data\Extensions\", "*.lua").ToList().ForEach(x => Environment.DoFile(x));

            //Libraries
            Environment.Globals["audio"] = typeof(AudioWrapper);
            Environment.Globals["core"] = typeof(CoreWrapper);
            Environment.Globals["database"] = typeof(DatabaseWrapper);
            Environment.Globals["input"] = typeof(InputWrapper);
            Environment.Globals["interface"] = typeof(InterfaceWrapper);
            Environment.Globals["screen"] = typeof(ScreenWrapper);
            Environment.Globals["webdriver"] = typeof(WebDriverProxy);
            Environment.Globals["bit"] = typeof(BitWrapper);
            Environment.Globals["teste"] = typeof(coreEvents);
            Environment.Globals["data"] = new PluginWrapper();
            Environment.Globals["minecraft"] = new MinecraftWrapper();
            Environment.Globals["socket"] = new SocketWrapper();
            //       Environment.Globals["teste"] = typeof(functio)
            //    Environment.Globals["teste"] = typeof(CoreWrapperVoid);
            //Types
            Environment.Globals["Color"] = new ColorContainer();
            Environment.Globals["Point"] = new PointContainer();
            Environment.Globals["Size"] = new SizeContainer();
            //Environment.Globals["formdesign"] = typeof(formcontrol);

            //Enums
            UserData.RegisterType<Native.VirtualKeyCode>();
            UserData.RegisterType<NoteFrequencies>();

            Environment.Globals["VirtualKey"] = UserData.CreateStatic<Native.VirtualKeyCode>();
            Environment.Globals["NoteFrequencies"] = UserData.CreateStatic<NoteFrequencies>();
        }

        /// <summary>
        /// Execute a string of code.
        /// </summary>
        /// <param name="code">String to execute.</param>
        /// <param name="silent">Defines whether console should hide code that's being executed.</param>
        public static void Execute(string code, bool silent = true)
        {
            Core.ConsoleInputQueue.Clear();

            if (!silent)
                Core.WriteLine(new ColorContainer(0, 131, 63), $"> {code}");

            if (LuaThread != null && LuaThread.IsAlive)
                LuaThread.Abort();

            LuaThread = new Thread(() =>
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();

                    Environment.DoString($"{code}");

                    stopWatch.Stop();

                    Core.WriteLine($"Script executed in: {stopWatch.ElapsedMilliseconds}ms");
                }
                catch (SyntaxErrorException exception)
                {
                    Core.WriteLine(new ColorContainer(177, 31, 41), $"Syntax Error: {exception}");
                }
                catch (IndexOutOfRangeException exception)
                {
                    Core.WriteLine(new ColorContainer(177, 31, 41), $"Engine Error: {exception}");
                }
                catch (ScriptRuntimeException exception)
                {
                    Core.WriteLine(new ColorContainer(177, 31, 41), $"Runtime Error: {exception}");
                }
				catch (Exception exception)
				{
					Core.WriteLine(new ColorContainer(177, 31, 41), $"Exception: {exception}");
				}
            })
            {
                Name = "Lua Thread",
                IsBackground = true
            };
            LuaThread.Start();
        }

        /// <summary>
        /// Adds a line to queue that can be processed via core.processConsoleInput.
        /// </summary>
        /// <param name="code">Code to process.</param>
        public static void InjectLine(string code)
        {
            if (LuaThread == null || !LuaThread.IsAlive)
            {
                Execute(code, false);
                return;
            }
            
            Core.ConsoleInputQueue.Add(code);
        }

        /// <summary>
        /// Stop Lua thread, effectively killing all running scripts.
        /// </summary>
        public static void Stop()
        {
            //Crude, but effective.
            if (LuaThread != null && LuaThread.IsAlive)
                LuaThread.Abort();
        }
    }
}
