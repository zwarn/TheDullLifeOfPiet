using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Prop
{
    public string name;
    public float value;
}

public class Rainbowify : CactusScaler
{
    public Material targetMaterial;		
    public float minAnimationSpeed = 0.2f;
    public float maxAnimationSpeed = 1;                   
    public float animationSpeed = 1;		
    public float time = 0;    
    private float getLowAt;
    private int timeID;
    private int intensityID;

    void Start ()
    {
        timeID = Shader.PropertyToID ("_TimeArg");
        intensityID = Shader.PropertyToID ("_Intensity");                
    }      

    void Update ()
    {
        time += animationSpeed * Time.deltaTime;
        targetMaterial.SetFloat (timeID, time);        
    }

    public override void OnIntensity (float scaledLevel)
    {        
        targetMaterial.SetFloat (intensityID, scaledLevel);
        animationSpeed = minAnimationSpeed + scaledLevel * (maxAnimationSpeed - minAnimationSpeed);
    }
    
    void OnDestroy ()
    {
        OnIntensity (0);
        targetMaterial.SetFloat (timeID, 0);        
    }
}
