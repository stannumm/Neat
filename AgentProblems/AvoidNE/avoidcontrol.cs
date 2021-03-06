﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class avoidcontrol : MonoBehaviour {

    /// <summary>
    /// Event subscriptions to notify controller when test is finished
    /// </summary>
    /// <param name="source">Source of the event (this)</param>
    /// <param name="args">Nothing</param>
    public delegate void TestFinishedEventHandler(object source, EventArgs args);
    public event TestFinishedEventHandler TestFinished;

    private bool isActive = false; // is this agent active
    private bool finished = false; // is this agent finished.  Making sure only 1 event is sent.

    private NEATNet net; //The brain

    private const string ACTION_ON_FINISHED = "OnFinished"; //On finished method

    private NEATGeneticControllerV2 controller; //Controller

    Rigidbody2D rBody;
    Vector2 rightdir = new Vector2(1, 1);
    Vector2 leftdir = new Vector2(-1, 1);

    private float[] sightHit = new float[3]; //where raycast hit

    /// <summary>
    /// Set Color to this agent. Looks visually pleasing and may help in debugging? 
    /// </summary>
    /// <param name="color"> color</param>
    public void SetColor(Color color)
    {
        Renderer[] childRend = transform.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < childRend.Length; i++)
            childRend[i].material.color = color;

        rBody = GetComponent<Rigidbody2D>();

    }

    /// <summary>
    /// Start up tasks for this agent game object.
    /// </summary>
    void Start()
    {
        transform.Translate(18,-52,0);
    }

    /// <summary>
    /// Tick
    /// </summary>
    public void UpdateNet()
    {
        UpdateOverTime();

        RaycastHit2D[] hit = new RaycastHit2D[3];
        hit[0] = Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y+10), transform.up);
        hit[1] = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 10), rightdir);
        hit[2] = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 10), leftdir);

        for (int i = 0; i < 3; i++)
        {
            if (hit[i].collider != null)
            {
                sightHit[i] = Mathf.Abs(hit[i].point.y - transform.position.y);
            }
        }

      

       //net.AddNetFitness(1f);
        rBody.velocity = transform.up * 2.5f;


        
    }
    public void UpdateOverTime()
    {

        float[] inputValues = {
            sightHit[0], sightHit[1], sightHit[2]
        };


        float[] output = net.FireNet(inputValues);

       // Debug.Log(output[0]+"   "+ output[1]);

     if(output[0]>output[1])
            rBody.AddForce(transform.right*output[0]*50f);
     else
            rBody.AddForce(-transform.right * output[1]*50f);


    }
    public void crashed(int type)
    {
        if (type == 0)
        {
            //net.SetNetFitness(net.GetNetFitness() * 0.5f);
            OnFinished();
        }
    }

    public void finishedtrack(int type)
    {

        if (type == 0)
        {
            //net.SetNetFitness(net.GetNetFitness() * 0.5f);
            net.AddNetFitness(net.GetNetFitness()+100f);
        }
    }
    /// <summary>
    /// Some fail condition for this agent
    /// </summary>
    /// <returns></returns>
    public bool FailCheck()
    {
        return false;
    }

    /// <summary>
    /// Fitness update per tick. Does not have to happen here! But good practice.
    /// </summary>
    public void CalculateFitnessOnUpdate()
    {

    }

    /// <summary>
    /// Final fitness calculation once this agent is finished or failed
    /// </summary>
    public void CalculateFitnessOnFinish()
    {
        net.AddNetFitness(-1000f);
    }

    /// <summary>
    /// No need to worry about this method! You just need to code in UpdateNet and CalculateFitnessOnUpdate :D
    /// </summary>
    void FixedUpdate()
    {
        if (isActive == true)
        {
            UpdateNet(); //update neural net
            CalculateFitnessOnUpdate(); //calculate fitness

            if (FailCheck() == true)
            {
                OnFinished();
            }
        }
    }



    /// <summary>
    /// OnFinished is called when we want to notify controller this agent is done. 
    /// Automatically handels notification.
    /// </summary>
    public void OnFinished()
    {
        if (TestFinished != null)
        {
            if (!finished)
            {
                finished = true;
                CalculateFitnessOnFinish();
                TestFinished(net.GetNetID(), EventArgs.Empty);
                TestFinished -= controller.OnFinished; //unsubscrive from the event notification
                Destroy(gameObject); //destroy this gameobject
            }
        }
    }

    /// <summary>
    /// Activated the agent when controller give it a brain. 
    /// </summary>
    /// <param name="net">The brain</param>
    public void Activate(NEATNet net)
    {
        this.net = net;
        Invoke(ACTION_ON_FINISHED, net.GetTestTime());
        isActive = true;
    }

    /// <summary>
    /// Getting net. 
    /// This could be used by some other objects that have reference to this game object 
    /// and want to see the brain.
    /// </summary>
    /// <returns> The brain</returns>
    public NEATNet GetNet()
    {
        return net;
    }

    /// <summary>
    /// Adds controller and subscribes to an event listener in controller
    /// </summary>
    /// <param name="controller">Controller</param>
    public void SubscriveToEvent(NEATGeneticControllerV2 controller)
    {
        this.controller = controller;
        TestFinished += controller.OnFinished; //subscrive to an event notification
    }

}
