using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WindowBehaviour
{
    public GameObject gameObject { get; set; }//当前窗口的物体
    public Transform transform { get; set; }//代表自己
    public Canvas Canvas { get; set; }
    public string Name { get; set; }
    public bool Visible { get; set; }

    //检测是否是通过堆栈系统弹出的弹窗
    public bool isPopStack { get; set; }
    public Action<WindowBase> PopStackListener { get; set; }

    public virtual void OnAwake() { }//只会在物体创建的时候执行一次
    public virtual void OnShow() { }
    public virtual void OnUpdate() { }
    public virtual void OnHide() { }
    public virtual void OnDestroy() { }
    public virtual void SetVisible(bool isVisible) { }
}
