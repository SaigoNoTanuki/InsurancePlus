using BepInEx;
using HarmonyLib;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InsurancePlus
{
    [BepInAutoPlugin(id: "io.saigonotanuki.insurancePlus")]
    public partial class InsurancePlusPlugin : BaseUnityPlugin
    {
        private static readonly System.Random random = new System.Random();
        public event Action ?OnDeath;
        private HeroController ?hero;
        internal static bool insure;
        internal static int rosary;
        float time = 0f;

        private void Awake()
        {
            Harmony harmony = new Harmony(Id);
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        }

        private void Update()
        {
            time += Time.deltaTime;

            if (hero == null && HeroController.instance != null)
            {
                hero = HeroController.instance;
                hero.OnDeath += OnPlayerDeath;
            }

            if (time >= 300 && hero != null)
            {
                hero.TakeGeo(50);
                time = 0f;
            }
        }

        private void OnPlayerDeath()
        {
            if (hero == null) return;
            rosary = hero.GetCurrencyAmount(CurrencyType.Money);
            int chance = random.Next(0, 11);
            if (chance > 8) insure = true;
        }

        private void OnDestroy()
        {
            if (hero != null)
                hero.OnDeath -= OnPlayerDeath;
        }

        [HarmonyPatch(typeof(HeroController), "HeroRespawned")]
        public static class CitadelHealthPlus
        {
            [HarmonyPostfix]
            private static void ReturnShi(HeroController __instance)
            {
                if (!InsurancePlusPlugin.insure) return;

                __instance.AddGeo(InsurancePlusPlugin.rosary / 2);
                InsurancePlusPlugin.insure = false;
            }
        }
    }
}
