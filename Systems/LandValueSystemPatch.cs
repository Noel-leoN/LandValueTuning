using Game;
using Game.Simulation;
using HarmonyLib;

namespace LandValueTuning.Systems
{
    [HarmonyPatch(typeof(LandValueSystem), "OnCreate")]
    public class LandValueSystem_OnCreatePatch
    {
        private static bool Prefix(LandValueSystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<LandValueSystemMod>();
            __instance.World.GetOrCreateSystemManaged<UpdateSystem>().UpdateAt<LandValueSystemMod>(SystemUpdatePhase.GameSimulation);
            return true;
        }
    }

    [HarmonyPatch(typeof(LandValueSystem), "OnCreateForCompiler")]
    public class LandValueSystem_OnCreateForCompilerPatch
    {
        private static bool Prefix(LandValueSystem __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(LandValueSystem), "OnUpdate")]
    public class LandValueSystem_OnUpdatePatch
    {
        private static bool Prefix(LandValueSystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<LandValueSystemMod>().Update();
            return false;
        }
    }
}