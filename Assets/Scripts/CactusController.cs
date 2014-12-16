using UnityEngine;
using System.Collections.Generic;

public class CactusController : MonoBehaviour
{

    public float cactusLevel = 0;    
    public float maxCactusLevel = 20;
    public float increaseDelta = 1f;
    public float decreasePerSecond = 0.1f;           
    public float smoothness = 3;
    public float smoothCactusLevel = 0;    
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
            instance.cactusLevel = value;
        }
    }
    
    void Awake ()
    {        
        instance = this;
        listeners = new HashSet<CactusListener> ();        
    }
    
    void Start ()
    {
        //print ("Start");
        NotifyListeners ();
    }
		
    void Update ()
    {   
        DecreaseCactusLevel (decreasePerSecond * Time.deltaTime);     
        smoothCactusLevel = Mathf.Lerp (smoothCactusLevel, cactusLevel, smoothness * Time.deltaTime);   
        NotifyListeners ();    
    }    
        
    public void NotifyListeners ()
    {
        foreach (CactusListener l in listeners) {
            l.OnCactusLevelChange (smoothCactusLevel);
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
                
    
    public static void IncreaseCactusLevel ()
    {        
        CactusLevel = (Mathf.Min (CactusLevel + instance.increaseDelta, instance.maxCactusLevel));        
    }
    
    void DecreaseCactusLevel (float amount)
    {        
        CactusLevel = (Mathf.Max (cactusLevel - amount, 0));        
    }
}
