﻿using HarmonyLib;
using OWML.Common;
using UnityEngine;

namespace HikersMod
{
    public static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.Start))]
        public static void CharacterControllerStart()
        {
            HikersMod.Instance.OnCharacterStart();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(JetpackThrusterController), nameof(JetpackThrusterController.GetRawInput))]
        public static void GetJetpackInput(ref Vector3 __result, JetpackThrusterController __instance)
        {
            if (HikersMod.Instance.sprintButton == "Down Thrust" && HikersMod.Instance.disableUpDownThrust && __result.y < 0) __result.y = 0;
            else if (HikersMod.Instance.sprintButton == "Up Thrust" && HikersMod.Instance.disableUpDownThrust && __result.y > 0)
            {
                __result.y = 0;
                HikersMod.Instance.jetpackModel._boostActivated = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.Update))]
        public static bool CharacterControllerUpdate(PlayerCharacterController __instance)
        {
            if (!__instance._isAlignedToForce && !__instance._isZeroGMovementEnabled)
            {
                return false;
            }
            if ((OWInput.GetValue(InputLibrary.thrustUp, InputMode.All) == 0f) || (HikersMod.Instance.sprintButton == "Up Thrust" && HikersMod.Instance.disableUpDownThrust))
            {
                __instance.UpdateJumpInput();
            }
            else
            {
                __instance._jumpChargeTime = 0f;
                __instance._jumpNextFixedUpdate = false;
                __instance._jumpPressedInOtherMode = false;
            }
            if (__instance._isZeroGMovementEnabled)
            {
                __instance._pushPrompt.SetVisibility(OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam) && __instance._isPushable);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.IsBoosterAllowed))]
        public static bool IsBoosterAllowed(ref bool __result, PlayerResources __instance)
        {
            __result = !PlayerState.InZeroG() && !Locator.GetPlayerSuit().IsTrainingSuit() && !__instance._cameraFluidDetector.InFluidType(FluidVolume.Type.WATER) && __instance._currentFuel > 0f && !(HikersMod.Instance.sprintButton == "Up Thrust" && HikersMod.Instance.disableUpDownThrust);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.UpdateFocus))]
        public static void DreamLanternFocusChanged(DreamLanternItem __instance)
        {
            if (__instance._wasFocusing == __instance._focusing) return;
            HikersMod.Instance.dreamLanternFocused = __instance._focusing;
            HikersMod.Instance.dreamLanternFocusChanged = true;
            if (__instance._focusing) HikersMod.Instance.PrintLog("Focused Dream Lantern", MessageType.Info);
            else HikersMod.Instance.PrintLog("Unfocused Dream Lantern", MessageType.Info);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.UpdateAirControl))]
        public static bool CharacterUpdateAirControl(PlayerCharacterController __instance)
        {
            if (!HikersMod.Instance.enhancedAirControlEnabled) return true;
            if (__instance == null) return true;
            if (__instance._lastGroundBody != null)
            {
                Vector3 pointVelocity = __instance._transform.InverseTransformDirection(__instance._lastGroundBody.GetPointVelocity(__instance._transform.position));
                Vector3 localVelocity = __instance._transform.InverseTransformDirection(__instance._owRigidbody.GetVelocity()) - pointVelocity;
                localVelocity.y = 0f;
                float physicsTime = Time.fixedDeltaTime * 60f;
                float maxChange = __instance._airAcceleration * physicsTime;
                Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character | InputMode.NomaiRemoteCam);
                Vector3 localVelocityChange = new Vector3(maxChange * axisValue.x, 0f, maxChange * axisValue.y);
                Vector3 newLocalVelocity = localVelocity + localVelocityChange;
                if (newLocalVelocity.magnitude > __instance._airSpeed && newLocalVelocity.magnitude > localVelocity.magnitude)
                    __instance._owRigidbody.AddLocalVelocityChange(-localVelocity + Vector3.ClampMagnitude(newLocalVelocity, localVelocity.magnitude));
                else __instance._owRigidbody.AddLocalVelocityChange(localVelocityChange);
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.OnEnterDreamWorld))]
        public static void EnteredDreamWorld()
        {
            HikersMod.Instance.isDreaming = true;
            HikersMod.Instance.UpdateMoveSpeed();
            HikersMod.Instance.PrintLog("Entered Dream World", MessageType.Info);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.OnExitDreamWorld))]
        public static void ExitedDreamWorld()
        {
            HikersMod.Instance.isDreaming = false;
            HikersMod.Instance.UpdateMoveSpeed();
            HikersMod.Instance.PrintLog("Left Dream World", MessageType.Info);
        }
    }
}