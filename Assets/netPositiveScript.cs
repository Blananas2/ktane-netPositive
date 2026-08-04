using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class netPositiveScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMNeedyModule Needy;
    public KMAudio Audio;


    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = true;


    void Awake () {
        moduleId = moduleIdCounter++;
        Needy.OnNeedyActivation += OnNeedyActivation;
        Needy.OnNeedyDeactivation += OnNeedyDeactivation;
        Needy.OnTimerExpired += OnTimerExpired;

        /*
        foreach (KMSelectable stack in stacks) {
            KMSelectable pressedStack = stack;
            stack.OnInteract += delegate () { StackPress(pressedStack); return false; };
        }
        */
    }

    // Use this for initialization
    void Start () {
        moduleSolved = true;
	}

    void OnNeedyActivation()
    {
        moduleSolved = false;
        
    }

    void OnNeedyDeactivation ()
    {
        moduleSolved = true;
        
    }

    void OnTimerExpired()
    {
        Needy.HandleStrike();
        moduleSolved = true;
        
    }
}