﻿using GTA;
using GTA.Native;
using NativeUI;
using SimpleMissions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace SimpleMissions
{
    public abstract class Mission
    {
        internal static List<Type> missions = new List<Type>();
        internal static bool isOnMission = false;
        internal static Mission currentMission = null;
        internal static Type missionType = null;

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
            currentMission.End();

            MissionInfo info = Func.GetMissionInfo(missionType);
            isOnMission = false;
            if(reason == EndState.Pass) Game.Player.Money += info.pay;
            currentMission = null;
            missionType = null;
        }

        /// <summary>
        /// Fails the mission for the specified reason
        /// </summary>
        /// <param name="failReason">The reason the mission failed</param>
        public void Fail(string failReason)
        {
            // TODO: IMPLEMENT
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
            if(info.type == MissionType.Heist) BigMessageThread.MessageInstance.ShowSimpleShard($"Heist Passed", info.displayName);
            else if(info.type == MissionType.HeistSetup) BigMessageThread.MessageInstance.ShowSimpleShard($"Heist Setup Passed", info.displayName);
            else BigMessageThread.MessageInstance.ShowSimpleShard($"Mission Passed", info.displayName);
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
