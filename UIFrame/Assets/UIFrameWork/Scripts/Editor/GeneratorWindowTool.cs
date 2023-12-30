using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Text;

public class GeneratorWindowTool : Editor
{
    static Dictionary<string, string> methodDic = new Dictionary<string, string>();
    [MenuItem("GameObject/����Window�ű�", false, 0)]
    static void CreateFindComponentScripts()
    {
        GameObject obj = Selection.objects.First() as GameObject;//��ȡ����ǰѡ�������
        if (obj == null)
        {
            Debug.LogError("��Ҫѡ�� GameObject");
            return;
        }
        //���ýű�����·��
        if (!Directory.Exists(GeneratorConfig.WindowGeneratorPath))
        {
            Directory.CreateDirectory(GeneratorConfig.WindowGeneratorPath);
        }


        string csContent = CreateWindoCs(obj.name);
        Debug.Log("CsConent:\n" + csContent);

        string cspath = GeneratorConfig.WindowGeneratorPath + "/" + obj.name + ".cs";
        UIWindowEditor.ShowWindow(csContent,cspath,methodDic);
        //if (File.Exists(cspath))
        //{
        //    File.Delete(cspath);
        //}
        //StreamWriter writer = File.CreateText(cspath);
        //writer.Write(csContent);
        //writer.Close();
        //AssetDatabase.Refresh();

    }

    /// <summary>
    /// ����Window�ű�
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string CreateWindoCs(string name)
    {
        //��ȡjson
        var objDatalist = JsonMgr.Instance.LoadData<List<EditorObjectData>>(GeneratorConfig.OBJDATALIST_KEY);

        StringBuilder sb=new StringBuilder();
        //�������
        sb.AppendLine("/*---------------------------------");
        sb.AppendLine(" *Title:UI���ֲ�ű��Զ������ɹ���");
        sb.AppendLine(" *Date:" + System.DateTime.Now);
        sb.AppendLine(" *Description:UI ���ֲ㣬�ò�ֻ�������Ľ�����������صĸ��£��������д�κ�ҵ���߼�����");
        sb.AppendLine(" *ע��:�����ļ����Զ����ɵģ��ٴ����ɲ��Ḳ��ԭ�еĴ��룬����ԭ�еĴ����Ͻ����������ɷ���ʹ��");
        sb.AppendLine("---------------------------------*/");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UIFrameWork;");
        //sb.AppendLine("using UIFrameWork;");
        sb.AppendLine();

        //��������
        sb.AppendLine($"\tpublic class {name}:WindowBase");
        sb.AppendLine("\t{");
        sb.AppendLine("\t");

            //�����ֶ�
        sb.AppendLine($"\t\t public {name}UIComponent uiCompt=new {name}UIComponent();");

        //�����������ں��� Awake
        sb.AppendLine("\t");
        sb.AppendLine($"\t\t #region �������ں���");
        sb.AppendLine($"\t\t //���û�����Mono Awakeһ��");
        sb.AppendLine("\t\t public override void OnAwake()");
        sb.AppendLine("\t\t {");
        sb.AppendLine($"\t\t\t uiCompt.InitComponent(this);");
        sb.AppendLine("\t\t\t base.OnAwake();");
        sb.AppendLine("\t\t }");
        //OnShow
        sb.AppendLine($"\t\t //������ʾʱִ��");
        sb.AppendLine("\t\t public override void OnShow()");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t\t base.OnShow();");
        sb.AppendLine("\t\t }");
        //OnHide
        sb.AppendLine($"\t\t //��������ʱִ��");
        sb.AppendLine("\t\t public override void OnHide()");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t\t base.OnHide();");
        sb.AppendLine("\t\t }");

        //OnDestroy
        sb.AppendLine($"\t\t //��������ʱִ��");
        sb.AppendLine("\t\t public override void OnDestroy()");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t\t base.OnDestroy();");
        sb.AppendLine("\t\t }");

        sb.AppendLine($"\t\t #endregion");

        //API Function 
        sb.AppendLine($"\t\t #region API Function");
        sb.AppendLine($"\t\t    ");
        sb.AppendLine($"\t\t #endregion");
        //UI����¼�����
        sb.AppendLine($"\t\t #region UI����¼�");
        foreach (var item in objDatalist)
        {
            string type = item.fileType;
            string methodName = "On" + item.fileName;
            string suffix = "";
            if (type.Contains("Button"))
            {
                suffix = "ButtonClick";
                CreateMethod(sb, ref methodDic, methodName + suffix);
            }
            else if (type.Contains("InputField"))
            {
                suffix = "InputChange";
                CreateMethod(sb, ref methodDic, methodName + suffix, "string text");
                suffix = "InputEnd";
                CreateMethod(sb, ref methodDic, methodName + suffix, "string text");
            }
            else if (type.Contains("Toggle"))
            {
                suffix = "ToggleChange";
                CreateMethod(sb, ref methodDic, methodName + suffix, "bool state,Toggle toggle");
            }
        }
        sb.AppendLine($"\t\t #endregion");

        sb.AppendLine("\t}");
        return sb.ToString();
    }

    public static void CreateMethod(StringBuilder sb,ref Dictionary<string,string> methodDic,string methodName,string param="")
    {
        //����UI����¼�
        sb.AppendLine($"\t\t public void {methodName}({param})");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t");
        if (methodName == "OnCloseButtonClick")
        {
            sb.AppendLine("\t\t\tHideWindow();");
        }
        sb.AppendLine("\t\t }");

        //�洢UI����¼� �ṩ��������������ʹ��
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"\t\t public void {methodName}({param})");
        builder.AppendLine("\t\t {");
        builder.AppendLine("\t\t");
        builder.AppendLine("\t\t }");
        methodDic.Add(methodName, builder.ToString());
    }
}
