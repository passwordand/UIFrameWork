using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WindowBehaviour
{
    public GameObject gameObject { get; set; }//��ǰ���ڵ�����
    public Transform transform { get; set; }//�����Լ�
    public Canvas Canvas { get; set; }
    public string Name { get; set; }
    public bool Visible { get; set; }

    //����Ƿ���ͨ����ջϵͳ�����ĵ���
    public bool isPopStack { get; set; }
    public Action<WindowBase> PopStackListener { get; set; }

    public virtual void OnAwake() { }//ֻ�������崴����ʱ��ִ��һ��
    public virtual void OnShow() { }
    public virtual void OnUpdate() { }
    public virtual void OnHide() { }
    public virtual void OnDestroy() { }
    public virtual void SetVisible(bool isVisible) { }
}
