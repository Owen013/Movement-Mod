﻿using HarmonyLib;
using UnityEngine;

namespace HikersMod.Components;

[HarmonyPatch]
public class AnimSpeedController : MonoBehaviour
{
    private Animator _animator;

    private PlayerCharacterController _characterController;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterController = Locator.GetPlayerController();
    }

    private void LateUpdate()
    {
        float groundSpeed = _characterController.GetRelativeGroundVelocity().magnitude;
        if (ModMain.Instance.SmolHatchlingAPI != null)
        {
            groundSpeed *= 1 / ModMain.Instance.SmolHatchlingAPI.GetPlayerScale();
        }
        float animSpeedMultiplier = Mathf.Sqrt(groundSpeed / 6f);
        float floatyPhysicsMultiplier = Mathf.Sqrt(_characterController._acceleration / ModMain.Instance.GroundAccel);
        float underwaterMultiplier = ModMain.Instance.ImmersionAPI != null ? ModMain.Instance.ImmersionAPI.GetAnimSpeed() : 1f;

        _animator.speed = _characterController.IsGrounded() ? Mathf.Max(animSpeedMultiplier * floatyPhysicsMultiplier, floatyPhysicsMultiplier) : underwaterMultiplier;
    }

    // add component to animator(s)
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.Start))]
    private static void AddToAnimController(PlayerAnimController __instance)
    {
        __instance.gameObject.AddComponent<AnimSpeedController>();
    }
}