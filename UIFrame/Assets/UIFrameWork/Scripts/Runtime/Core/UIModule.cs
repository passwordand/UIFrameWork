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
    private List<WindowBase> mAllWindowList=new List<WindowBase>();//所有窗口的列表
    private List<WindowBase> mVisibleWindowList=new List<WindowBase>();//所有可见的窗口列表

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
        //实例化泛型类
        T t = new T();
        return null;
    }

    private WindowBase ShowWindow(string winName)
    {
        WindowBase window = null;
        if (mAllWindowDic.ContainsKey(winName))
        {
            window = mAllWindowDic[winName];
            //如果这个窗口存在并且没有显示就改为显示
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
            Debug.LogError(winName + "窗口不存在,请调用PopUpWindow弹出窗口");
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


