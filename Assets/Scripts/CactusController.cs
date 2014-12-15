using UnityEngine;
using System.Collections.Generic;

public class CactusController : MonoBehaviour
{

    public float cactusLevel = 0;
    public float maxCactusLevel = 20;
    public float decreaseDelta = 0.05f;
    public float decreaseEverySeconds = 1;
   
    private float timeBeforeDecrease;
    private static CactusController instance;	
    private HashSet<CactusListener> listeners;         
    
    public static CactusController Instance {
        get {
            return instance;
        }
    }       
    
    public static float CactusLevel {
        get {
            return instance.cactusLevel;
        }
        set {
            instance.SetCactusLevel (value);
        }
    }
    
    void Awake ()
    {
        instance = this;
        listeners = new HashSet<CactusListener> ();
        timeBeforeDecrease = decreaseEverySeconds;       
    }
		
    void Update ()
    {
        timeBeforeDecrease -= Time.deltaTime;
        if (timeBeforeDecrease < 0) {
            timeBeforeDecrease = decreaseEverySeconds;
            DecreaseCactusLevel ();
        }
    }    
        
    public void NotifyListeners ()
    {
        print ("Notifying");
        foreach (CactusListener l in listeners) {
            l.NotifyCactusLevel (cactusLevel);
        } 
    }	   
    
    public void Register (CactusListener l)
    {
        print ("Registered " + l);
        listeners.Add (l); 
    }
    
    public void Unregister (CactusListener l)
    {    
        listeners.Remove (l);
    }
    
    void SetCactusLevel (float level)
    {
        if (cactusLevel != level) {
            cactusLevel = level;
            NotifyListeners ();
        }
    }
    
    public void IncreaseCactusLevel (float delta)
    {        
        SetCactusLevel (Mathf.Min (cactusLevel + delta, maxCactusLevel));        
    }
    
    void DecreaseCactusLevel ()
    {        
        SetCactusLevel (Mathf.Max (cactusLevel - decreaseDelta, 0));        
    }
}
