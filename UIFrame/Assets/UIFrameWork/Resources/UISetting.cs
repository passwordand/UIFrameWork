using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="UISetting",menuName ="UISetting",order =0)]
public class UISetting : ScriptableObject
{
    private static UISetting instance;
    public static UISetting Instance
    {
        get
        {
            if(instance is null)
            {
                instance = Resources.Load<UISetting>("UISetting");
            }
            return instance;
        }
    }
    public bool SINGMASK_SYSTEM;
}
