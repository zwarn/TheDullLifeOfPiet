using UnityEngine;
using System.Collections.Generic;

public class CactusController : MonoBehaviour
{

    public float cactusLevel = 0;
    public float maxCactusLevel = 20;
    public float increaseDelta = 1f;
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
            //print ("get CactusLevel");
            return instance.cactusLevel;
        }
        set {
            instance.SetCactusLevel (value);
        }
    }
    
    void Awake ()
    {
        //print ("Awake");
        instance = this;
        listeners = new HashSet<CactusListener> ();
        timeBeforeDecrease = decreaseEverySeconds;       
    }
    
    void Start ()
    {
        //print ("Start");
        NotifyListeners ();
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
        foreach (CactusListener l in listeners) {
            l.OnCactusLevelChange (cactusLevel);
        } 
    }	   
    
    public void Register (CactusListener l)
    {
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
    
    public static void IncreaseCactusLevel ()
    {        
        CactusLevel = (Mathf.Min (CactusLevel + instance.increaseDelta, instance.maxCactusLevel));        
    }
    
    void DecreaseCactusLevel ()
    {        
        CactusLevel = (Mathf.Max (cactusLevel - decreaseDelta, 0));        
    }
}
