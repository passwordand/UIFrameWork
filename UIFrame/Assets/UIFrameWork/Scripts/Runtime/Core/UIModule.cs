  using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIModule 
{
    private static UIModule instance;
    public static UIModule Instance { get { if (instance == null) { instance = new UIModule(); } return instance; } }

    private Camera mUICamera;
    private Transform mUIRoot;
    private WindowConfig mWindowConfig;

    private Dictionary<string, WindowBase> mAllWindowDic = new Dictionary<string, WindowBase>();
    private List<WindowBase> mAllWindowList=new List<WindowBase>();//所有窗口的列表
    private List<WindowBase> mVisibleWindowList=new List<WindowBase>();//所有可见的窗口列表

    private Queue<WindowBase> mWindowStack=new Queue<WindowBase> ();//队列 用来管理弹窗的循环弹出
    //开始弹出堆栈的标志,可以用来处理多种情况,比如:正在出栈中有其他界面弹出,可以直接放到栈内弹出
    //队列先进先出,保证顺序
    private bool mStartPopStackWidStats = false;

    public void Initialize()
    {
        mUICamera = GameObject.Find("UICamera").GetComponent<Camera>();
        mUIRoot = GameObject.Find("UIRoot").transform;
        mWindowConfig = Resources.Load<WindowConfig>("WindowConfig");

        //手机上不会调用
#if UNITY_EDITOR
        mWindowConfig.GeneraWindowConfig();
#endif
    }



    #region 窗口管理
    public void PreLoadWindow<T>() where T:WindowBase,new()
    {
        var type = typeof(T);
        string widName = type.Name;
        T windowBase = new T();
        //实例化界面,初始化界面信息
        GameObject newwindow= TempLoadWindow(widName);
        if (newwindow != null)
        {
            windowBase.gameObject = newwindow;
            windowBase.Name = newwindow.name;
            windowBase.transform = newwindow.transform;
            windowBase.Canvas = newwindow.GetComponent<Canvas>();
            windowBase.Canvas.worldCamera = mUICamera; 
            windowBase.OnAwake();
            windowBase.SetVisible(false);
            RectTransform rectTrans = newwindow.GetComponent<RectTransform>();
            rectTrans.anchorMax = Vector2.one;
            rectTrans.offsetMax = Vector2.zero;
            rectTrans.offsetMin = Vector2.zero;
            mAllWindowDic.Add(widName, windowBase);
            mAllWindowList.Add(windowBase);
        }
        Debug.Log("预加载窗口 窗口名字:"+widName);
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

    /// <summary>
    /// 给堆栈管理系统使用
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    private WindowBase PopUpWindow(WindowBase window)
    {
        var type = window.GetType();
        string widName = type.Name;
        WindowBase wid = GetWindow(widName);
        if (wid != null)
        {
            return ShowWindow(widName);
        }
        //在压栈的时候new过了
        return InitializeWindow(window, widName) ;
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
            SetWindowMaskVisible();
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
                SetWindowMaskVisible();
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

   //提供给外部的接口
    public void HideWindow(string widName)
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
    //私有的隐藏窗口方法
    private void HideWindow(WindowBase window)
    {
        if(window!=null&&window.Visible)
        {
            mVisibleWindowList.Remove(window);
            window.SetVisible(false);
            SetWindowMaskVisible();
            window.OnHide();
        }
        //在出栈的情况下,上一个界面是否是队列中的,如果是,则隐藏的时候自动打开栈中的下一个界面
        PopNextStackWindow(window);
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
            SetWindowMaskVisible();
            window.OnHide();
            window.OnDestroy();
            GameObject.Destroy(window.gameObject);
            //在出栈的情况下,上一个界面是否是队列中的,如果是,则销毁的时候自动打开栈中的下一个界面
            PopNextStackWindow(window);
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

    /// <summary>
    /// 调整遮罩方法
    /// </summary>
    private void SetWindowMaskVisible()
    {
        if(!UISetting.Instance.SINGMASK_SYSTEM)
        {
            return;
        }
        WindowBase maxOrderWidBase = null;//最大渲染层级窗口
        int maxOrder = 0;//最大渲染层级
        int maxIndex = 0;//最大排序下标 在相同父节点位置下标
        //先关闭所有窗口的mask,设置为不可见
        //然后从所有的窗口中找到层级最大的窗口把mask设置为可见
        for(int i=0;i<mVisibleWindowList.Count;i++)
        {
            WindowBase window=mVisibleWindowList[i];
            if(window!=null&&window.gameObject!=null)
            {
                window.SetMaskVisible(false);
                if(maxOrderWidBase== null)
                {
                    maxOrderWidBase=window;
                    maxOrder = window.Canvas.sortingOrder;
                    maxIndex = window.transform.GetSiblingIndex();//获取同级索引
                }
                else
                {
                    //找到最大渲染层级的窗口,拿到它
                    if(maxOrder<window.Canvas.sortingOrder)
                    {
                        maxOrderWidBase = window;
                        maxOrder =window.Canvas.sortingOrder;
                    }
                    else if(maxOrder==window.Canvas.sortingOrder&& maxIndex < window.transform.GetSiblingIndex())
                    {
                        maxOrderWidBase = window;
                        maxIndex=window.transform.GetSiblingIndex();
                    }
                }
            }
        }
        if(maxOrderWidBase!=null)
        {
            maxOrderWidBase.SetMaskVisible(true);
        }
    }


    //资源加载
    public GameObject TempLoadWindow(string widName)
    {
        var widdow= GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(mWindowConfig.GetWindowPath(widName)), mUIRoot);
        //widdow.transform.SetParent(mUIRoot);
        widdow.transform.localScale = Vector3.one;
        widdow.transform.localPosition = Vector3.zero;
        widdow.transform.localRotation= Quaternion.identity;
        widdow.name = widName;
        return widdow;
    }
    #endregion
    /// <summary>
    /// 推入一个界面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="popCallBack"></param>
    #region 堆栈系统
    public void PushWindowToStack<T>(Action<WindowBase> popCallBack=null) where T:WindowBase,new()
    {
        T wndBase=new T();
        wndBase.PopStackListener = popCallBack;
        mWindowStack.Enqueue(wndBase);//压入队列
    }

    /// <summary>
    /// 弹出堆栈中第一个弹窗
    /// </summary>
    public void StartPopFirstStackWindow()
    {
        if (mStartPopStackWidStats) return;
        mStartPopStackWidStats = true;//开始进行堆栈弹出的流程
        PopStackWindow();
    }

    /// <summary>
    /// 压入并且弹出堆栈弹窗
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="popCallBack"></param>
    public void PushAndPopStackWindow<T>(Action<WindowBase> popCallBack = null) where T : WindowBase, new()
    {
        PushWindowToStack<T>(popCallBack);
        StartPopFirstStackWindow();
    }

    /// <summary>
    /// 弹出堆栈中下一个窗口
    /// </summary>
    /// <param name="window"></param>
    public void PopNextStackWindow(WindowBase window)
    {
        if(window!=null&&mStartPopStackWidStats&&window.isPopStack)
        {
            window.isPopStack = false;
            PopStackWindow();
        }
    }
    /// <summary>
    /// 弹出栈的方法
    /// </summary>
    /// <returns></returns>
    public bool PopStackWindow()
    {
        if(mWindowStack.Count > 0)
        {
            WindowBase window=mWindowStack.Dequeue();//窗口出栈
            WindowBase popWindow=PopUpWindow(window);//弹出窗口
            popWindow.PopStackListener = window.PopStackListener;
            popWindow.isPopStack = true;
            popWindow.PopStackListener?.Invoke(popWindow);
            popWindow.PopStackListener = null;
            return true; 
        }
        else
        {
            mStartPopStackWidStats = false;
            return false; 
        }
    }

    public void ClearStackWindows()
    {
        mWindowStack.Clear();
    }
    #endregion
}


