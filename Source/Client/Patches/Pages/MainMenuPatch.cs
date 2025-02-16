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
            [HarmonyPrefix]
            public static bool DoPre(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "")) DialogShortcuts.ShowConnectDialogs();
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
                }
            }
        }
    }
}