using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginWindow : WindowBase
{
    public override void OnAwake()
    {
        base.OnAwake();
        Debug.Log("login awake");
    }

    public override void OnShow()
    {
        base.OnShow();
        Debug.Log("login onshow");
    }

    public override void OnHide()
    {
        base.OnHide();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Debug.Log("login ondestroy");
    }

    public void Test()
    {
        Debug.Log("Test1");
    }
    public void Test2()
    {
        Debug.Log("µÃµ½´°¿Ú");
    }
}
