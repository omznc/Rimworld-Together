﻿using System.Linq;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Patches.Pages
{
    public class MainMenuPatch
    {
        [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
        public static class PatchButton
        {
            private static void DefaultServer(string name, string password)
            {
                MainNetworkingUnit.client = new();
                MainNetworkingUnit.client.playerName = name;
                var loginDetails = new LoginDetailsJSON();
                loginDetails.username = name;
                loginDetails.password = Hasher.GetHash(password);
                Saver.SaveLoginDetails(loginDetails.username, loginDetails.password);
                Network.Network.ip = "127.0.0.1";
                Network.Network.port = "25555";
                Threader.GenerateThread(Threader.Mode.Start);
                Thread.Sleep(500);
                
                loginDetails.clientVersion = ClientValues.versionCode;
                loginDetails.runningMods = ModManager.GetRunningModList().ToList();
                ChatManager.username = loginDetails.username;

                var contents = new[] { Serializer.SerializeToString(loginDetails) };
                var packet = new Packet("LoginClientPacket", contents);
                Network.Network.SendData(packet);
            }

            [HarmonyPrefix]
            public static bool DoPre(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "")) DialogShortcuts.ShowConnectDialogs();
                    if (CommandLineParamsManager.instantConnect)
                    {
                        DefaultServer(CommandLineParamsManager.name, CommandLineParamsManager.name);
                        return true;
                    }

                    if (CommandLineParamsManager.fastConnect)
                        if (Widgets.ButtonText(new Rect(buttonLocation.x - 200, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                            DefaultServer(CommandLineParamsManager.name, CommandLineParamsManager.name);
                }

                return true;
            }

            [HarmonyPostfix]
            public static void DoPost(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "Play Together"))
                    {
                    }

                    if (CommandLineParamsManager.fastConnect)
                        Widgets.ButtonText(new Rect(buttonLocation.x - 200, buttonLocation.y, buttonSize.x, buttonSize.y), "FastConnect");
                }
            }
        }
    }
}