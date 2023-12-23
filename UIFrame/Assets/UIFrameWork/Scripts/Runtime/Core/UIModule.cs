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

    /// <summary>
    /// ����һ������
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
        //ʵ����������
        T t = new T();
        return InitializeWindow(t,widName) as T;
    }
    private WindowBase InitializeWindow(WindowBase windowBase,string widName)
    {
        //1.���ɶ�Ӧ�Ĵ���Ԥ����
        GameObject newwiddow = TempLoadWindow(widName);
        //2.��ʼ����Ӧ������
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
        Debug.LogError("����ʧ�� ������:"+widName);
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

    //�ҵ���ǰ�Ĵ���
    private WindowBase GetWindow(string winName)
    {
        if(mAllWindowDic.ContainsKey(winName))
        {
            return mAllWindowDic[winName];
        }
        return null;
    }


    /// <summary>
    /// ��ȡ�Ѿ������Ĵ���
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
        Debug.LogError("û�л�ȡ������:"+type.Name);
        return null;
    }

    //˽�е����ش��ڷ���
    private void HideWindow(string widName)
    {
        WindowBase window=GetWindow(widName);
        HideWindow(window);
    }
    /// <summary>
    /// �ṩ���ⲿ�ķ��͹رմ��ڷ���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void HideWindow<T>() where T:WindowBase
    {
        HideWindow(typeof(T).Name);
    }
    //ʵ���Ϲرյĺ���
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
            //ж��δʹ����Դ
            Resources.UnloadUnusedAssets();
        }
    }

    //��Դ����
    public GameObject TempLoadWindow(string widName)
    {
        //TODO:���ڴ��޸�
        var widdow= GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Window/" + widName));
        widdow.transform.SetParent(mUIRoot);
        widdow.transform.localScale = Vector3.one;
        widdow.transform.localPosition = Vector3.zero;
        widdow.transform.localRotation= Quaternion.identity;
        widdow.name = widName;
        return widdow;
    }
}


