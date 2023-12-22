using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIModule 
{
    private static UIModule instance;
    public static UIModule Instance { get { if (instance == null) { instance = new UIModule(); } return instance; } }

    private Camera mUICamera;
    private Transform mUIRoot;

    private Dictionary<string, WindowBase> mAllWindowDic = new Dictionary<string, WindowBase>();
    private List<WindowBase> mAllWindowList=new List<WindowBase>();//���д��ڵ��б�
    private List<WindowBase> mVisibleWindowList=new List<WindowBase>();//���пɼ��Ĵ����б�

    public void Initialize()
    {
        mUICamera = GameObject.Find("UICamera").GetComponent<Camera>();
        mUIRoot = GameObject.Find("UIRoot").transform;
    }

    public T PopUpWindow<T>() where T:WindowBase,new()
    {
        var type = typeof(T);
        string widName = type.Name;
        WindowBase wid = GetWindow(widName);
        if(wid!=null)
        {
            return ShowWindow(widName) as T;
        }
        //ʵ����������
        T t = new T();
        return null;
    }

    private WindowBase ShowWindow(string winName)
    {
        WindowBase window = null;
        if (mAllWindowDic.ContainsKey(winName))
        {
            window = mAllWindowDic[winName];
            //���������ڴ��ڲ���û����ʾ�͸�Ϊ��ʾ
            if (window.gameObject != null && window.Visible == false)
            {
                mVisibleWindowList.Add(window);
                window.transform.SetAsLastSibling();
                window.SetVisible(true);
                window.OnShow();
            }
            return window;
        }
        else
            Debug.LogError(winName + "���ڲ�����,�����PopUpWindow��������");
        return null;
    }

    public WindowBase GetWindow(string winName)
    {
        if(mAllWindowDic.ContainsKey(winName))
        {
            return mAllWindowDic[winName];
        }
        return null;
    }
}


