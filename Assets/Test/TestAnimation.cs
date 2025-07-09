using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims;
using UnityEngine;

public class TestAnimation : MonoBehaviour
{
    RPGCharacterController controller;

    void Start()
    {
        controller = GetComponent<RPGCharacterController>();
    }

    void Update()
    {
        bool inputJump = Input.GetKeyDown(KeyCode.Space);
        if (inputJump && controller.CanStartAction("Jump")) {
            controller.StartAction("Jump");
        }
    }
}
