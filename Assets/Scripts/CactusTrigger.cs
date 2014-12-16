using UnityEngine;
using System.Collections;

public abstract class CactusTrigger : CactusListener
{    
    public float threshold = 1;
    public float maxLevel = 5;    
    
    private CactusState lastState = CactusState.Init;
    
    private enum CactusState
    {
        Init,
        Low,
        High
    }

    public abstract void OnHigh ();    
    public abstract void OnLow ();
    public abstract void OnIntensity (float scaledLevel);    
    
    public override void OnCactusLevelChange (float level)
    {
        float scaledLevel = (level - threshold) / (maxLevel - threshold);               
        if (scaledLevel > 1) {
            scaledLevel = 1;
        } else if (scaledLevel < 0) {
            scaledLevel = 0;
        }
        OnIntensity (scaledLevel);
        CactusState nextState = level < threshold ? CactusState.Low : CactusState.High;        
        if (nextState != lastState) {
            //print ("Cactus Level Change: " + nextState);
            lastState = nextState;
            switch (nextState) {
            case CactusState.Low:
                OnLow ();
                break;
            case CactusState.High:                
                OnHigh ();
                break;
            }
        }
    }
} 
