﻿using CitizenFX.Core;
using System;

namespace Curiosity.Shared.Client.net.Classes.Environment
{
    public static class Experience
    {
        public static void Display(Vector3 dmgPos, string skill, int xp, int timeout, bool increase)
        {
            throw new NotImplementedException("No longer in use");
            //string message = $"{xp}xp";
            //if (increase)
            //{
            //    BaseScript.TriggerServerEvent("curiosity:Server:Skills:Increase", skill, xp);
            //}
            //else
            //{
            //    BaseScript.TriggerServerEvent("curiosity:Server:Skills:Decrease", skill, xp);
            //    message = $"-{xp}xp";
            //}
            //Draw3DTextTimeout(dmgPos.X, dmgPos.Y, dmgPos.Z, message, timeout, 40f, 60.0f);
        }
    }
}
