using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
public class Planeinput : MonoBehaviour
{
    private int frameCounter = 0;
    private int score = 0;
    private List<Tuple<int, string>> actionLog;
    private Rigidbody2D m_Rigidbody2D;
    InputAction tapAction;

    private bool inputEnabled = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        tapAction=InputSystem.actions.FindAction("Tap");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        score++;
        actionLog.Add(new Tuple<int, string>(frameCounter,"PassedGap"));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        col.collider.enabled = false;
        m_Rigidbody2D.angularVelocity = 180;
        m_Rigidbody2D.linearVelocityY = 4;
        m_Rigidbody2D.linearVelocityX = -2;
        GetComponent<Collider2D>().enabled = false;
        inputEnabled = false;
        actionLog.Add(new Tuple<int, string>(frameCounter,"Died"));

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        frameCounter++;
        if(!inputEnabled)
            return;
        if (tapAction.WasPressedThisFrame())
        {
            m_Rigidbody2D.linearVelocityY = 4;
            actionLog.Add(new Tuple<int, string>(frameCounter,"Tapped"));
        }
    }
}