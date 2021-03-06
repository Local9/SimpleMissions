﻿using GTA;
using GTA.Native;
using NativeUI;
using SimpleMissions.Attributes;
using System;
using System.Collections.Generic;

namespace SimpleMissions
{
    public abstract class Mission
    {
        internal static List<Type> missions = new List<Type>();
        internal static bool isOnMission = false;
        internal static Mission currentMission = null;
        internal static Type missionType = null;
        public int pay = 2500;

        /// <summary>
        /// This is called when a mission is started by the user. Typically this would be used to set up the mission by spawning things like peds and vehicles
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// This is called on every tick even from ScriptHookVDotNet
        /// </summary>
        public abstract void Tick();
        /// <summary>
        /// This is called as soon as the mission is ended. Typically this would be used for cleanup such as removing peds and vehicles
        /// </summary>
        public abstract void End();

        /// <summary>
        /// Stops a mission
        /// </summary>
        /// <param name="reason">The reason the mission was stopped</param>
        public void Stop(EndState reason)
        {
            isOnMission = false;
            missionType = null;
            int pay = currentMission.pay;
            currentMission.End();
            currentMission = null;

            if (reason == EndState.Pass)
            {
                Game.Player.Money += pay;
            }
            

            foreach (Blip blp in Main.blips)
            {
                blp.Remove();
            }
            Main.blips.Clear();
        }

        /// <summary>
        /// Fails the mission for the specified reason
        /// </summary>
        /// <param name="failReason">The reason the mission failed</param>
        public void Fail(string failReason)
        {
            MissionInfo info = Func.GetMissionInfo(missionType);

            Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "GENERIC_FAILED");
            while (!Function.Call<bool>(Hash.IS_MISSION_COMPLETE_PLAYING)) Script.Yield();
            if (info.type == MissionType.Heist) BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Heist Failed", failReason);
            else if (info.type == MissionType.HeistSetup) BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Heist Setup Failed", failReason);
            else BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Mission Failed", failReason);
            Stop(EndState.Fail);
        }

        /// <summary>
        /// Passes the mission, this is just to make it a bit more self explanatory for developers to pass missions
        /// </summary>
        public void Pass()
        {
            MissionInfo info = Func.GetMissionInfo(missionType);

            Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "FRANKLIN_BIG_01");
            while (!Function.Call<bool>(Hash.IS_MISSION_COMPLETE_PLAYING)) Script.Yield();
            if(info.type == MissionType.Heist) BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Heist Passed", info.displayName);
            else if(info.type == MissionType.HeistSetup) BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Heist Setup Passed", info.displayName);
            else BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Mission Passed", info.displayName);

            SaveManager.save.completedMissions.Add(info.id);
            SaveManager.Save();

            Stop(EndState.Pass);
        }
    }

    /// <summary>
    /// The type of mission, this determines what kind of blip and mission passed screen the mission has
    /// </summary>
    public enum MissionType
    {
        Mission, Stranger, Heist, HeistSetup
    }

    /// <summary>
    /// The reason a mission ended, this is exclusively used for the Stop function
    /// </summary>
    public enum EndState
    {
        Fail, Pass
    }
}
