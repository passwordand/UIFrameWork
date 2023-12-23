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

    /// <summary>
    /// 弹出一个弹窗
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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
        return InitializeWindow(t,widName) as T;
    }
    private WindowBase InitializeWindow(WindowBase windowBase,string widName)
    {
        //1.生成对应的窗口预制体
        GameObject newwiddow = TempLoadWindow(widName);
        //2.初始出对应管理类
        if(newwiddow!=null)
        {
            windowBase.gameObject=newwiddow;
            windowBase.Name = newwiddow.name;
            windowBase.transform=newwiddow.transform;
            windowBase.Canvas = newwiddow.GetComponent<Canvas>();
            windowBase.Canvas.worldCamera = mUICamera;
            windowBase.transform.SetAsLastSibling();
            windowBase.OnAwake();
            windowBase.SetVisible(true);
            windowBase.OnShow();
            RectTransform rectTrans = newwiddow.GetComponent<RectTransform>();
            rectTrans.anchorMax = Vector2.one;
            rectTrans.offsetMax = Vector2.zero;
            rectTrans.offsetMin = Vector2.zero;
            mAllWindowDic.Add(widName,windowBase);
            mAllWindowList.Add(windowBase);
            mVisibleWindowList.Add(windowBase);
            return windowBase;
        }
        Debug.LogError("加载失败 窗口名:"+widName);
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

    //找到当前的窗口
    private WindowBase GetWindow(string winName)
    {
        if(mAllWindowDic.ContainsKey(winName))
        {
            return mAllWindowDic[winName];
        }
        return null;
    }


    /// <summary>
    /// 获取已经弹出的窗口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetWindow<T>() where T:WindowBase
    {
        var type = typeof(T);
        foreach (var item in mVisibleWindowList)
        {
            if(item.Name==type.Name)
            {
                return (T)item;
            }
        }
        Debug.LogError("没有获取到窗口:"+type.Name);
        return null;
    }

    //私有的隐藏窗口方法
    private void HideWindow(string widName)
    {
        WindowBase window=GetWindow(widName);
        HideWindow(window);
    }
    /// <summary>
    /// 提供给外部的泛型关闭窗口方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void HideWindow<T>() where T:WindowBase
    {
        HideWindow(typeof(T).Name);
    }
    //实际上关闭的函数
    private void HideWindow(WindowBase window)
    {
        if(window!=null&&window.Visible)
        {
            mVisibleWindowList.Remove(window);
            window.SetVisible(false);
            window.OnHide();
        }
    }

    private void DestroyWindow(string widName)
    {
        WindowBase window=GetWindow(widName);
        DestroyWindow(window);
    }

    public void DestroyWindow<T>() where T:WindowBase
    {
        DestroyWindow(typeof(T).Name);
    }
    private void DestroyWindow(WindowBase window)
    {
        if(window!=null)
        {
            if(mAllWindowDic.ContainsKey(window.Name))
            {
                mAllWindowDic.Remove(window.Name);
                mAllWindowList.Remove(window);
                mVisibleWindowList.Remove(window);
            }
            window.SetVisible(false);
            window.OnHide();
            window.OnDestroy();
            GameObject.Destroy(window.gameObject);
            
        }
    }

    public void DestroyAllWindow(List<string> filterlist=null)
    {
        for(int i=mAllWindowList.Count-1;i>=0;i--)
        {
            WindowBase window = mAllWindowList[i];
            if(window==null||(filterlist!=null&&filterlist.Contains(window.Name)))
            {
                continue;
            }
            DestroyWindow(window.Name);
            //卸载未使用资源
            Resources.UnloadUnusedAssets();
        }
    }

    //资源加载
    public GameObject TempLoadWindow(string widName)
    {
        //TODO:后期待修改
        var widdow= GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Window/" + widName));
        widdow.transform.SetParent(mUIRoot);
        widdow.transform.localScale = Vector3.one;
        widdow.transform.localPosition = Vector3.zero;
        widdow.transform.localRotation= Quaternion.identity;
        widdow.name = widName;
        return widdow;
    }
}


