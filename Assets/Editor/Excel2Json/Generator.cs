using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Excel;
using Newtonsoft.Json;
using UnityEngine;

namespace Editor
{
    public abstract class Generator
    {
        // private DataSet mResultSet;
        /// <summary>
        /// 转换为Json
        /// </summary>
        public static bool Excel2Json(string excelPath, string jsonPath)
        {
            Encoding encoding = Encoding.UTF8;

            DirectoryInfo folder = new DirectoryInfo(excelPath);
            FileSystemInfo[] files = folder.GetFileSystemInfos();
            int length = files.Length;

            for (int index = 0; index < length; index++)
            {
                if (files[index].Name.EndsWith(".xlsx"))
                {
                    string childPath = files[index].FullName;
                    Debug.Log("Excel2Json:" + childPath);
                    FileStream mStream = File.Open(childPath, FileMode.Open, FileAccess.Read);
                    IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
                    DataSet mResultSet = mExcelReader.AsDataSet();
                    ConvertToJson(childPath, mResultSet, jsonPath, encoding);
                    // ConvertToJsonEx(childPath, mResultSet, _jsonPath, encoding);
                }
            }

            return true;
        }

        /// <summary>
        /// 转换为Json
        /// string  int float  double  bool
        /// </summary>
        /// <param name="childPath"></param>
        /// <param name="mResultSet"></param>
        /// <param name="jsonPath">Json文件路径</param>
        /// <param name="encoding"></param>
        // public void ConvertToJson(DataSet mResultSet, string _jsonPath, Encoding encoding, string CSharpPath)
        private static void ConvertToJson(string childPath, DataSet mResultSet, string jsonPath, Encoding encoding)
        {
            List<string> dataName = new List<string>();
            List<string> dataType = new List<string>();
            if (dataType == null) throw new ArgumentNullException(nameof(dataType));
            //判断Excel文件中是否存在数据表
            if (mResultSet.Tables.Count < 1) return;

            Dictionary<string, object> allTable = new Dictionary<string, object>();
            for (int x = 0; x < mResultSet.Tables.Count; x++)
            {
                //默认读取第一个数据表
                DataTable mSheet = mResultSet.Tables[x];
                string jsonName = FirstCharToUpper(mSheet.TableName);
                if (jsonName.IndexOf('#') >= 0 && jsonName.LastIndexOf('#') != jsonName.IndexOf('#'))
                {
                    Debug.Log("无法导出名 " + jsonName + "  请确定#书写正确!");
                    continue;
                }

                //判断数据表内是否存在数据
                if (mSheet.Rows.Count < 1)
                    continue;
                //读取数据表行数和列数
                int rowCount = mSheet.Rows.Count;
                int colCount = mSheet.Columns.Count;
                //准备一个列表存储整个表的数据
                List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();
                List<object> tempValueStrList = null;
                string tempField = null;
                string tempTypeString = null;
                //读取数据
                for (int i = 3; i < rowCount; i++)
                {
                    //准备一个字典存储每一行的数据
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int j = 0; j < colCount; j++)
                    {
                        //读取第1行数据作为表头字段
                        string field = mSheet.Rows[1][j].ToString();
                        field = field.Trim();
                        if (field != "")
                        {
                            tempField = field;
                            if (!dataName.Contains(field))
                            {
                                dataName.Add(field);
                            }
                        }
                        else if (tempField != "" && field == "")
                        {
                            field = tempField;
                        }

                        string typeString = mSheet.Rows[2][j].ToString();
                        typeString = typeString.ToLower().Trim();
                        if (typeString != "")
                        {
                            tempTypeString = typeString;
                            if (tempValueStrList == null)
                            {
                                tempValueStrList = new List<object>();
                            }
                            else
                            {
                                tempValueStrList.Clear();
                            }

                            dataType.Add(typeString);
                        }
                        else if (typeString == "" && tempTypeString != "")
                        {
                            typeString = tempTypeString;
                        }

                        string valueStr = mSheet.Rows[i][j].ToString();
                        valueStr = valueStr.Trim();
                        //Key-Value对应 按类型存放
                        string stringReplace;
                        string[] tempValueStrArray;
                        switch (typeString)
                        {
                            case "int":
                                if (valueStr != "")
                                {
                                    row[field] = Convert.ToInt32(valueStr);
                                }
                                else
                                {
                                    row[field] = 0;
                                }

                                break;
                            case "float":
                                if (valueStr != "")
                                {
                                    row[field] = float.Parse(valueStr);
                                }
                                else
                                {
                                    row[field] = 0;
                                }

                                break;
                            case "double":
                                if (valueStr != "")
                                {
                                    row[field] = Convert.ToDouble(valueStr);
                                }
                                else
                                {
                                    row[field] = 0;
                                }

                                break;
                            case "long":
                                if (valueStr != "")
                                {
                                    row[field] = Convert.ToInt64(valueStr);
                                }
                                else
                                {
                                    row[field] = 0;
                                }

                                break;
                            case "bool":
                                if (valueStr == "0" || valueStr == "false" || valueStr == "")
                                {
                                    row[field] = false;
                                }
                                else
                                {
                                    row[field] = true;
                                }

                                break;
                            case "array<int>":
                                stringReplace = valueStr.Replace("[", "");
                                stringReplace = stringReplace.Replace("]", "");
                                //将字符串，转换成字符数组
                                tempValueStrArray = stringReplace.Split(',');

                                int[] tempIntArray = new int[tempValueStrArray.Length];
                                for (int index = 0; index < tempValueStrArray.Length; index++)
                                {
                                    if (tempValueStrArray.Length > 0)
                                    {
                                        if (tempValueStrArray[index] == "")
                                        {
                                            continue;
                                        }

                                        tempIntArray[index] = Convert.ToInt32(tempValueStrArray[index]);
                                    }
                                }

                                row[field] = tempIntArray;
                                break;
                            case "array<float>":
                                stringReplace = valueStr.Replace("[", "");
                                stringReplace = stringReplace.Replace("]", "");
                                //将字符串，转换成字符数组
                                tempValueStrArray = stringReplace.Split(',');

                                float[] tempFloatArray = new float[tempValueStrArray.Length];
                                for (int index = 0; index < tempValueStrArray.Length; index++)
                                {
                                    if (tempValueStrArray.Length > 0)
                                    {
                                        if (tempValueStrArray[index] == "")
                                        {
                                            continue;
                                        }

                                        tempFloatArray[index] = (float)Convert.ToDouble(tempValueStrArray[index]);
                                    }
                                }

                                row[field] = tempFloatArray;
                                break;
                            case "array<string>":
                                stringReplace = valueStr.Replace("[", "");
                                stringReplace = stringReplace.Replace("]", "");
                                //将字符串，转换成字符数组
                                tempValueStrArray = stringReplace.Split(',');

                                row[field] = tempValueStrArray;
                                break;
                            case "list<int>":
                                tempValueStrList.Add(valueStr);
                                List<int> tempIntList = new List<int>();
                                for (int index = 0; index < tempValueStrList.Count; index++)
                                {
                                    if (tempValueStrList.Count > 0)
                                    {
                                        if (tempValueStrList[index].ToString() == "")
                                        {
                                            continue;
                                        }

                                        tempIntList.Add(Convert.ToInt32(tempValueStrList[index]));
                                    }
                                }

                                row[field] = tempIntList;
                                break;
                            case "list<string>":
                                tempValueStrList.Add(valueStr);
                                List<object> tempStringList = new List<object>();
                                for (int index = 0; index < tempValueStrList.Count; index++)
                                {
                                    if (tempValueStrList.Count > 0)
                                    {
                                        if (tempValueStrList[index].ToString() == "")
                                        {
                                            continue;
                                        }

                                        tempStringList.Add(tempValueStrList[index]);
                                    }
                                }

                                row[field] = tempStringList;
                                break;
                            default:
                                row[field] = valueStr;
                                break;
                        }
                    }

                    //添加到表数据中
                    table.Add(row);
                }

                allTable.Add(jsonName, table);
            }

            string json = JsonConvert.SerializeObject(allTable, Formatting.None);
            json = ConvertJsonString(json);
            // _jsonPath + "/" + jsonName + ".json"
            // string JsonFilePath = _jsonPath + "/" + jsonName + ".json";
            string jsonFilePath = jsonPath + Path.GetFileNameWithoutExtension(childPath) + ".json";
            //写入文件
            using (FileStream fileStream = new FileStream(jsonFilePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
                {
                    textWriter.Write(json);
                }
            }

            dataName.Clear();
            dataType.Clear();
        }

        public static bool ConvertToJsonEx(string childPath, DataSet mResultSet, string jsonPath, Encoding encoding)
        {
            // 重新构建一个DataSet
            DataSet mNewDataSet = new DataSet();
            DataTable mNewTable = new DataTable();
            mNewDataSet.Tables.Add(mNewTable);

            //判断Excel文件中是否存在数据表
            if (mResultSet.Tables.Count < 1)
                return false;

            //默认读取第一个数据表
            DataTable mSheet = mResultSet.Tables[0];
            string jsonName = FirstCharToUpper(mSheet.TableName);

            //新构建的DataSet设置table名字
            mNewTable.TableName = jsonName;

            //判断数据表内是否存在数据
            if (mSheet.Rows.Count < 1)
                return false;

            //读取数据表行数和列数
            int rowCount = mSheet.Rows.Count;
            int colCount = mSheet.Columns.Count;

            for (int k = 0; k < colCount; k++)
            {
                //mSheet.Columns[k].ColumnName = mSheet.Rows[1][k].ToString();
                string temp = mSheet.Rows[1][k].ToString(); //属性名字_类型
                string[] tempArray = temp.Split('_');

                string pName = tempArray[0]; //属性名字
                string typeName = tempArray[1]; //类型

                mSheet.Columns[k].ColumnName = pName;

                //需要什么类型自己扩展
                switch (typeName)
                {
                    case "i":
                        mNewTable.Columns.Add(new DataColumn(pName, typeof(int)));
                        break;
                    case "s":
                        mNewTable.Columns.Add(new DataColumn(pName, typeof(string)));
                        break;
                    case "f":
                        mNewTable.Columns.Add(new DataColumn(pName, typeof(float)));
                        break;
                }
            }

            //思路来自：http://www.newtonsoft.com/json/help/html/SerializeDataSet.htm
            //读取数据
            for (int i = 2; i < rowCount; i++)
            {
                DataRow mNewRow = mNewTable.NewRow();
                for (int j = 0; j < colCount; j++)
                {
                    mNewRow[mSheet.Columns[j].ColumnName] = mSheet.Rows[i][j];
                }

                mNewTable.Rows.Add(mNewRow);
            }

            mNewDataSet.AcceptChanges();

            // 生成Json字符串
            string json = JsonConvert.SerializeObject(mNewDataSet, Formatting.Indented);

            //写入文件
            using FileStream fileStream =
                new FileStream(jsonPath + "/" + jsonName + ".json", FileMode.Create, FileAccess.Write);
            using TextWriter textWriter = new StreamWriter(fileStream, encoding);
            textWriter.Write(json);

            return true;
        }

        private static string FirstCharToUpper(string input)
        {
            char[] a = input.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        private static string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }
}