using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System;
using System.Reflection;

public class GeneratorBindComponentTool : Editor
{
    public static List<EditorObjectData> objDataList;//���Ҷ��������
    [MenuItem("GameObject/����������ݽű�(Shift+B) #B", false, 0)]
    static void CreateFindComponentScripts()
    {
        GameObject obj = Selection.objects.First() as GameObject;//��ȡ����ǰѡ�������
        if (obj == null)
        {
            Debug.LogError("��Ҫѡ�� GameObject");
            return;
        }
        objDataList = new List<EditorObjectData>();
        //���ýű�����·��
        if (!Directory.Exists(GeneratorConfig.BindComponentGeneratorPath))
        {
            Directory.CreateDirectory(GeneratorConfig.BindComponentGeneratorPath);
        }
        //���������������
        if (GeneratorConfig.ParseType == ParseType.Tag)
            ParseWindowDataByTag(obj.transform, obj.name);
        else
            PresWindowNodeData(obj.transform, obj.name);

        JsonMgr.Instance.SaveData(objDataList, GeneratorConfig.OBJDATALIST_KEY);

        string csContent = CreatCS(obj.name);
        Debug.Log("CsConent:\n" + csContent);

        string cspath = GeneratorConfig.BindComponentGeneratorPath + "/" + obj.name + "DataComponent.cs";
        UIWindowEditor.ShowWindow(csContent, cspath);
        EditorPrefs.SetString("GeneratorClassName", obj.name + "DataComponent");
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="WinName"></param>
    public static void PresWindowNodeData(Transform trans, string WinName)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            GameObject obj = trans.GetChild(i).gameObject;
            string name = obj.name;
            if (name.Contains("[") && name.Contains("]"))
            {
                int index = name.IndexOf("]") + 1;
                string fileName = name.Substring(index, name.Length - index);//��ȡ�ֶ��ǳ�
                string fileType = name.Substring(1, index - 2);//��ȡ�ֶ�����
                objDataList.Add(new EditorObjectData { fileName = fileName, fileType = fileType, insID = obj.GetInstanceID() });
            }
            PresWindowNodeData(trans.GetChild(i), WinName);
        }
    }
    public static void ParseWindowDataByTag(Transform trans, string WinName)
    {
        for(int i = 0; i < trans.childCount; i++)
        {
            GameObject obj = trans.GetChild(i).gameObject;
            string tagName = obj.tag;
            if (GeneratorConfig.TAGArr.Contains(tagName))
            {
                string fileName = obj.name;//��ȡ�ֶ���
                string fileType = tagName;//��ȡ�ֶ�����
                objDataList.Add(new EditorObjectData { fileName = fileName, fileType = fileType, insID = obj.GetInstanceID() });
            }
            ParseWindowDataByTag(trans.GetChild(i), WinName);
        }    
    }

    /// <summary>
    /// �Զ������ɽű�
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string CreatCS(string name)
    {
        StringBuilder sb = new StringBuilder();
        string nameSpaceName = "UIFrameWork";
        //������ú�ע��
        sb.AppendLine("/*---------------------------------");
        sb.AppendLine(" *Title:UI�Զ���������ɴ��빤��");
        sb.AppendLine(" *Date:" + System.DateTime.Now);
        sb.AppendLine(" *Description:������Ҫ��[Text]���ż�������͵ĸ�ʽ����������Ȼ���Ҽ��������塪�� һ������UI����������ҽű�����");
        sb.AppendLine(" *ע��:�����ļ����Զ����ɵģ��κ��ֶ��޸Ķ��ᱻ�´����ɸ���,���ֶ��޸ĺ�,���������Զ�����");
        sb.AppendLine("---------------------------------*/");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using UnityEngine;");

        sb.AppendLine();
        //���������ռ�
        if (!string.IsNullOrEmpty(nameSpaceName))
        {
            sb.AppendLine($"namespace {nameSpaceName}");
            sb.AppendLine("{");
        }
        sb.AppendLine($"\tpublic class {name + "DataComponent:MonoBehaviour"}");
        sb.AppendLine("\t{");
        //�����ֶ������б� �����ֶ�
        foreach (var item in objDataList)
        {
            sb.AppendLine("\t\tpublic  " + item.fileType + "  " + item.fileName + item.fileType + ";\n");
        }
        //������ʼ������ӿ�
        sb.AppendLine("\t\tpublic  void InitComponent(WindowBase target)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t");
        sb.AppendLine("\t");
        sb.AppendLine("\t\t     //����¼���");
        //�õ��߼��� WindowBase => LoginWindow
        sb.AppendLine($"\t\t    {name} mWindow=({name})target;");

        //����UI�¼��󶨴���
        foreach (var item in objDataList)
        {
            string type = item.fileType;
            string methodName = item.fileName;
            string suffix = "";
            if (type.Contains("Button"))
            {
                suffix = "Click";
                sb.AppendLine($"\t\t     target.AddButtonClickListener({methodName}{type},mWindow.On{methodName}Button{suffix});");
            }
            if (type.Contains("InputField"))
            {
                sb.AppendLine($"\t\t     target.AddInputFieldListener({methodName}{type},mWindow.On{methodName}InputChange,mWindow.On{methodName}InputEnd);");
            }
            if (type.Contains("Toggle"))
            {
                suffix = "Change";
                sb.AppendLine($"\t\t     target.AddToggleClickListener({methodName}{type},mWindow.On{methodName}Toggle{suffix});");
            }
        }
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        if (!string.IsNullOrEmpty(nameSpaceName))
        {
            sb.AppendLine("}");
        }
        return sb.ToString();
    }

    public static EditorObjectData GetEditorObjectData(int insid)
    {
        foreach (var item in objDataList)
        {
            if (item.insID == insid)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// �������ϵͳ�Զ�����
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    public static void AddComponentToWindow()
    {
        //�����ǰ�����������ݽű��Ļص�,�Ͳ�����
        string className= EditorPrefs.GetString("GeneratorClassName");
        if (string.IsNullOrEmpty(className))
        {
            return;
        }
        //1.ͨ������ķ�ʽ���ӳ������ҵ�����ű����������ڵ���ǰ��������
        //��ȡ���еĳ���
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //�ҵ�C#����
        var cSharpAssembly = assemblies.First(assembly => assembly.GetName().Name == "Assembly-CSharp");
        //��ȡ�����ڵĳ���·��
        string relClassName = "UIFrameWork." + className;
        Type classType=cSharpAssembly.GetType(relClassName);
        if(classType == null)
        {
            return;
        }
        //��ȡҪ���ص��Ǹ�����
        string windowObjName = className.Replace("DataComponent", "");//ȥ��classname�ж���Ĳ��� 
        GameObject windowObj = GameObject.Find(windowObjName);
        if(windowObj == null)
        {
            windowObj = GameObject.Find("UIRoot/" + windowObjName);
            if(windowObj==null)
            {
                return;
            }
        }
        //�Ȼ�ȡ�ִ�������û�й��ظ��������,���û�����ٽ��й���
        Component compt = windowObj.GetComponent(classType);
        if(compt == null)
        {
            compt=windowObj.AddComponent(classType);
        }
        //2.ͨ������ķ�ʽ�����������б� �ҵ���Ӧ���ֶΣ���ֵ
        //��ȡ���������б�
        var objDataList = JsonMgr.Instance.LoadData<List<EditorObjectData>>(GeneratorConfig.OBJDATALIST_KEY);
        //��ȡ�ű������ֶ�
        FieldInfo[] fieldInfoList = classType.GetFields();

        foreach (var item in fieldInfoList)
        {
            foreach (var objData in objDataList)
            {
                if (item.Name == objData.fileName + objData.fileType)
                {
                    //����Insid�ҵ���Ӧ�Ķ���
                    GameObject uiObject = EditorUtility.InstanceIDToObject(objData.insID) as GameObject;
                    //���ø��ֶ�����Ӧ�Ķ���
                    if (string.Equals(objData.fileType, "GameObject"))
                    {
                        item.SetValue(compt, uiObject);
                    }
                    else
                    {
                        item.SetValue(compt, uiObject.GetComponent(objData.fileType));
                    }
                    break;
                }
            }
        }
        EditorPrefs.DeleteKey("GeneratorClassName");
    }
}
