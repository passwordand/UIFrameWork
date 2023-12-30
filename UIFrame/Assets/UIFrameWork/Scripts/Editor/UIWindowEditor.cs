using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class UIWindowEditor : EditorWindow
{
    private string scriptContent;
    private string filePath;
    private Vector2 scroll=new Vector2 ();

    /// <summary>
    /// ����չʾWINDOW����
    /// </summary>
    /// <param name="content"></param>
    /// <param name="filePath"></param>
    /// <param name="insertDic">������ui�ķ����ֵ�</param>
    public static void ShowWindow(string content,string filePath,Dictionary<string,string> insertDic=null)
    {
        //��������չʾ����
        UIWindowEditor window = (UIWindowEditor)GetWindowWithRect(typeof(UIWindowEditor), new Rect(100, 50, 800, 700), true, "Window���ɽ���");
        window.scriptContent = content;
        window.filePath = filePath;
        //��������Ĵ���
        if(File.Exists(filePath)&&insertDic!=null)
        {
            //�Ȼ�ȡԭʼ�Ĵ���
            string originScript=File.ReadAllText(filePath);
            foreach (var item in insertDic)
            {
                //����ϴ���û�оͲ������
                if(!originScript.Contains(item.Key))
                {
                    int index = window.GetInsertIndex(item.Key);
                    originScript=window.scriptContent=originScript.Insert(index,item.Value+"\t\t");
                }
            }
        }
        window.Show();
    }
    public void OnGUI()
    {
        //����ScroView
        scroll= EditorGUILayout.BeginScrollView(scroll,GUILayout.Height(600),GUILayout.Width(800));
        EditorGUILayout.TextArea(scriptContent);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();

        //���ƽű�����·��
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextArea("�ű�����·����" + filePath);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //���ư�ť
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("���ɽű�",GUILayout.Height(30)))
        {
            //��ť�¼�
            ButtonClick();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ButtonClick()
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        StreamWriter writer = File.CreateText(filePath);
        writer.Write(scriptContent);
        writer.Close();
        AssetDatabase.Refresh();
        if (EditorUtility.DisplayDialog("�Զ������ɹ���", "���ɽű��ɹ�", "ȷ��"))
        {
            Close();
        }
    }

    /// <summary>
    /// ��ȡ���������±�
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public int GetInsertIndex(string content)
    {
        //�ҵ�UI����¼�����ĵ�һ��publicλ�ý��в���
        //������������ʽ
        Regex regex = new Regex("UI����¼�");
        Match match=regex.Match(content);

        Regex regex1 = new Regex("public");
        MatchCollection matchCollection=regex1.Matches(content);
        //�ҵ����е�public������,Ȼ�����ȥ�ҵ���match����������Ǹ�
        for(int i=0;i<matchCollection.Count;i++)
        {
            if (matchCollection[i].Index>match.Index)
            {
                return matchCollection[i].Index;
            }
        }
        return -1;
    }
}
