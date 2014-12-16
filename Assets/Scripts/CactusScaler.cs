using UnityEngine;
using System.Collections;

public abstract class CactusScaler : CactusListener
{    
    public float minLevel = 1;
    public float maxLevel = 5;   
    public float scaledLevel;
    public float maxScaled = 1; 
    
    public abstract void OnIntensity (float scaledLevel);    
    
    public override void OnCactusLevelChange (float level)
    {
        scaledLevel = (level - minLevel) * maxScaled / (maxLevel - minLevel);               
        if (scaledLevel > maxScaled) {
            scaledLevel = maxScaled;
        } else if (scaledLevel < 0) {
            scaledLevel = 0;
        }
        OnIntensity (scaledLevel);
    }
} 
