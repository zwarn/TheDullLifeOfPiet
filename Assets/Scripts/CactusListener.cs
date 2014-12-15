﻿using UnityEngine;
using System.Collections;

public abstract class CactusListener : MonoBehaviour
{
    public abstract void NotifyCactusLevel (float level);
            
    void OnEnable ()
    {
        CactusController.Instance.Register (this);
    }
    
    void OnDisable ()
    {
        CactusController.Instance.Unregister (this);  
    } 
}
