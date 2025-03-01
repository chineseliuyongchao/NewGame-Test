using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GenExcel2Json2Cs : EditorWindow
    {
        private string _excelPath;
        private string _jsonPath;

        [MenuItem("Tools/ExcelExport")]
        public static void Init()
        {
            GetWindow(typeof(GenExcel2Json2Cs));
        }

        private void OnEnable()
        {
            // 在 OnEnable 方法中初始化路径
            _excelPath = Application.dataPath + "/NumericalSimulation/Res/Excel2Json2Cs/Output/Excel/";
            _jsonPath = Application.dataPath + "/NumericalSimulation/Res/Excel2Json2Cs/Output/Json/";
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 50), "请选择Excel路径"))
            {
                _excelPath = EditorUtility.OpenFolderPanel("Choose Excel Path", "", "") + "/";
                _jsonPath = _excelPath.Replace("Output/Excel", "Output/Json");
            }

            GUI.Label(new Rect(120, 30, 800, 30), _excelPath);

            if (GUI.Button(new Rect(10, 70, 100, 50), "请选择Json路径"))
            {
                _jsonPath = EditorUtility.OpenFolderPanel("Choose Json Path", "", "");
            }

            GUI.Label(new Rect(120, 90, 800, 30), _jsonPath);
            if (!GUI.Button(new Rect(150, 250, 100, 50), "Excel2Json")) return;
            EditorUtility.DisplayDialog("", Generator.Excel2Json(_excelPath, _jsonPath) ? "完成" : "出错了", "确定");

            AssetDatabase.Refresh();
        }
    }
}