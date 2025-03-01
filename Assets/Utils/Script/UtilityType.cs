namespace Utils.Script
{
    /// <summary>
    /// 所有的json文件对应的类，数组名字对应Sheet1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    class JsonWrapper<T>
    {
        // ReSharper disable once InconsistentNaming
        public T[] Sheet1;
    }

    /// <summary>
    /// 所有json数据对象的基类
    /// </summary>
    public class BaseJsonData
    {
        /// <summary>
        /// 编号
        /// </summary>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnassignedField.Global
        public int ID;
    }
}