using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WindowBase : WindowBehaviour
{
    private List<Button> mAllButtonList=new List<Button>();//����button���б�
    private List<Toggle> mToggleList=new List<Toggle>();//���е�toggle�б�
    private List<InputField> mInputList=new List<InputField>();//���е�������б�

    private CanvasGroup mUIMask;
    protected Transform mUIContent;
    private CanvasGroup mCanvasGroup;

    protected bool mDisableAnim = false;//���ö���
    private void InitializeBaseComponent()
    {
        mCanvasGroup = transform.Find("CanvasGroup").GetComponent<CanvasGroup>();
        mUIMask = transform.Find("UIMask").GetComponent<CanvasGroup>();
        mUIContent = transform.Find("UIContent").transform;
    }

    #region ��������
    public override void OnAwake()
    {
        base.OnAwake();
        InitializeBaseComponent();
    }
    public override void OnShow()
    {
        base.OnShow();
        //ShowAnimation();
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public override void OnHide()
    {
        base.OnHide();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        RemoveAllButtonListener();
        RemoveAllToggleListener();
        RemoveAllInputListener();
        mAllButtonList.Clear();
        mToggleList.Clear();
        mInputList.Clear();
    }
    #endregion

    #region ��������
    public void ShowAnimation()
    {
        //������������Ҫ����
        if (Canvas.sortingOrder>90&&mDisableAnim==false)
        {
            //Mask����
            mUIMask.alpha = 0;
            mUIMask.DOFade(1,0.2f);
            //���Ŷ���
            mUIContent.localScale = Vector3.one * 0.8f;
            mUIContent.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }
    public  void HideAnimation()
    {
        if (Canvas.sortingOrder > 90 && mDisableAnim == false)
        {
            mUIContent.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
           {
            //TODO:���޸�
            UIModule.Instance.HideWindow(Name);
           });
        }
        else
        {
            UIModule.Instance.HideWindow(Name);
        }
    }
    #endregion

    public void HideWindow()
    {
        UIModule.Instance.HideWindow(Name);
        //HideAnimation();
    }

    public override void SetVisible(bool isVisible)
    {
        mCanvasGroup.alpha = isVisible ? 1 : 0;
        mCanvasGroup.blocksRaycasts = isVisible;
        Visible = isVisible;
    }

    public void SetMaskVisible(bool isVisble)
    {
        //������ǵ�����ģʽ
        if(!UISetting.Instance.SINGMASK_SYSTEM)
        {
            return;
        }
        mUIMask.alpha = isVisble ? 1 : 0;
    }

    #region �¼�����
    public void AddButtonClickListener(Button btn,UnityAction action)
    {
        if(btn!=null)
        {
            //�����ǰ��list��û�о���ӽ�ȥ
            if(!mAllButtonList.Contains(btn))
            {
                mAllButtonList.Add(btn);
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }
    //boolֵ��ʾ��ǰ������Ƿ�ѡ��
    public void AddToggleClickListener(Toggle toggle,UnityAction<bool,Toggle> action)
    {
        if(toggle!=null)
        {
            if (!mToggleList.Contains(toggle))
            {
                mToggleList.Add(toggle);
            }
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((isOn) =>
            {
                action?.Invoke(isOn, toggle);
            });
        }
    }

    public void AddInputFieldListener(InputField input,UnityAction<string> onChangeAction,UnityAction<string> endAction)
    {
        if(input!=null)
        {
            if(!mInputList.Contains(input))
            {
                mInputList.Add(input);
            }
            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener(onChangeAction);
            input.onEndEdit.RemoveAllListeners();
            input.onEndEdit.AddListener(endAction);
        }
    }

    public void RemoveAllButtonListener()
    {
        foreach(var item in mAllButtonList)
        {
            item.onClick.RemoveAllListeners();
        }
    }
    public void RemoveAllToggleListener()
    {
        foreach(var item in mToggleList)
        {
            item.onValueChanged.RemoveAllListeners();
        }
    }
    public void RemoveAllInputListener()
    {
        foreach(var item in mInputList)
        {
            item.onValueChanged.RemoveAllListeners();
            item.onEndEdit.RemoveAllListeners();
        }
    }
    #endregion
}
