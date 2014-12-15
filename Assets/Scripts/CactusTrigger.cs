using UnityEngine;
using System.Collections;

public class CactusTrigger : CactusListener
{
    public float LastCactusLevel { get; set; }

    public override void NotifyCactusLevel (float level)
    {
        LastCactusLevel = level;
        print ("Trigger notified " + level);
    }
} 
