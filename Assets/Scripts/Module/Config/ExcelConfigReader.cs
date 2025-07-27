using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using ExcelDataReader;
using Module.Battle;
using UnityEngine;

namespace Module.Config
{
    ///<summary>Excel配置表读取器</summary>
    public class ExcelConfigReader
    {

        private string _filePath;
        private string _sheetName;
        
        private Dictionary<string, AttackParameters> _moduleConfigs; // 使用模块类名作为键
        private Dictionary<string, int> _columnIndices; // 列名到索引的映射
        private string[] _requiredColumns; // 必需的列名数组
        private Dictionary<string, string> _fieldToColumnMapping; // 字段名到列名的映射
        private AttackParameters _defaultConfig; // 缓存默认配置以避免重复创建




        public ExcelConfigReader(string filepath, string sheetName)
        {
            this._filePath = filepath;
            _sheetName = sheetName;
        }

        

        ///<summary>根据模块类名获取配置</summary>
        ///<param name="moduleName">模块名称</param>
        ///<returns>模块配置文件</returns>
        public AttackParameters GetModuleConfig(string moduleName)
        {
            if (_moduleConfigs == null)
            {
                LoadConfigs();
            }

            if (_moduleConfigs != null && _moduleConfigs.TryGetValue(moduleName, out AttackParameters config))
                return config;
            
            Debug.LogWarning($"未找到模块配置: {moduleName ?? "null"}");
            return GetDefaultConfigs(); // 默认配置
        }
        
        ///<summary>根据模块对象获取配置</summary>
        ///<param name="module">模块</param>
        ///<returns>模块配置文件</returns>
        public AttackParameters GetModuleConfig(BaseModule module)
        {
            if (module == null)
            {
                Debug.LogWarning("模块对象为null");
                return GetDefaultConfigs();
            }
            
            string moduleName = module.GetType().Name; // 获取具体的类名
            return GetModuleConfig(moduleName);
        }
        
        ///<summary>从Excel文件加载配置</summary>
        public void LoadConfigs()
        {
            // 初始化必需的列名
            InitializeRequiredColumns();
            
            _moduleConfigs = new Dictionary<string, AttackParameters>();
            _columnIndices = new Dictionary<string, int>();
            
            // 使用Path.Combine确保路径分隔符正确
            string fullPath = Path.Combine(Application.dataPath, _filePath.TrimStart('/', '\\'));
            
            Debug.Log($"尝试加载Excel文件: {fullPath}");
            
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Excel配置文件不存在: {fullPath}");
                Debug.LogError($"Application.dataPath: {Application.dataPath}");
                Debug.LogError($"相对路径: {_filePath}");
                LoadDefaultConfigs();
                return;
            }
            
            try
            {
                using (FileStream stream = File.Open(fullPath, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataSet dataSet = reader.AsDataSet();
                        
                        // 查找指定的工作表
                        DataTable table = null;
                        if (dataSet.Tables.Contains(_sheetName))
                        {
                            table = dataSet.Tables[_sheetName];
                        }
                        else if (dataSet.Tables.Count > 0)
                        {
                            table = dataSet.Tables[0]; // 使用第一个工作表
                            Debug.LogWarning($"未找到工作表'{_sheetName}'，使用第一个工作表: {table.TableName}");
                        }
                        
                        if (table != null)
                        {
                            ParseModuleConfigs(table);
                        }
                    }
                }
                
                Debug.Log($"成功加载 {_moduleConfigs.Count} 个模块配置");
            }
            catch (Exception e)
            {
                Debug.LogError($"读取Excel配置文件失败: {e.Message}");
                LoadDefaultConfigs();
            }
        }
        
        ///<summary>解析模块配置数据</summary>
        private void ParseModuleConfigs(DataTable table)
        {
            if (table.Rows.Count < 2) // 至少需要标题行和一行数据
            {
                Debug.LogWarning("Excel表格数据行数不足，至少需要标题行和一行数据");
                return;
            }
            
            // 解析标题行，获取列索引
            _columnIndices = ParseColumnIndices(table.Rows[0]);
            
            // 验证必需的列是否存在
            if (!ValidateRequiredColumns())
            {
                Debug.LogError("Excel表格缺少必需的列，无法继续解析");
                return;
            }
            
            // 从第二行开始解析数据
            for (int i = 1; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                
                try
                {
                    AttackParameters config = ParseModuleConfig(row); // 解析单个模块配置
                    if (config != null && !string.IsNullOrEmpty(config.moduleName)) // 确保模块名称不为空
                    {
                        _moduleConfigs[config.moduleName] = config;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"解析第{i + 1}行数据失败: {e.Message}");
                }
            }
        }
        
        ///<summary>读取所有列索引</summary>
        private Dictionary<string, int> ParseColumnIndices(DataRow headerRow)
        {
            Dictionary<string, int> indices = new Dictionary<string, int>(); // 存储列名和索引
            
            for (int i = 0; i < headerRow.ItemArray.Length; i++)
            {
                string columnName = headerRow[i]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(columnName))
                {
                    indices[columnName] = i;
                }
            }
            
            return indices;
        }
        
        ///<summary>验证必需的列是否存在</summary>
        private bool ValidateRequiredColumns()
        {
            if (_columnIndices == null || _requiredColumns == null)
            {
                Debug.LogError("列索引或必需列定义为null");
                return false;
            }
            
            List<string> missingColumns = new List<string>();
            
            foreach (string requiredColumn in _requiredColumns)
            {
                if (!_columnIndices.ContainsKey(requiredColumn))
                {
                    missingColumns.Add(requiredColumn);
                }
            }
            
            if (missingColumns.Count > 0)
            {
                Debug.LogError($"Excel表格缺少以下必需列: {string.Join(", ", missingColumns)}");
                return false;
            }
            
            return true;
        }
        
        ///<summary>验证列索引是否有效</summary>
        private bool IsColumnIndexValid(string columnName, DataRow row)
        {
            return _columnIndices != null && _columnIndices.TryGetValue(columnName, out int index) && 
                   row != null && row.Table.Columns.Count > index;
        }
        
        ///<summary>安全获取列值</summary>
        private object GetColumnValue(string columnName, DataRow row)
        {
            if (!IsColumnIndexValid(columnName, row) || _columnIndices == null)
            {
                return null;
            }
            
            return row[_columnIndices[columnName]];
        }
        
        
        ///<summary>初始化必需的列名，使用反射找出所有必要的参数</summary>
        private void InitializeRequiredColumns()
        {
            var columns = new List<string>();
            _fieldToColumnMapping = new Dictionary<string, string>();

            // 获取所有公共字段
            FieldInfo[] fields = typeof(AttackParameters).GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                // 不读取 bulletPrefab 
                if (field.Name == "bulletPrefab")
                    continue;

                string columnName = ConvertFieldNameToColumnName(field.Name);
                columns.Add(columnName);
                _fieldToColumnMapping[field.Name] = columnName;
            }

            _requiredColumns = columns.ToArray();
        }

        ///<summary>将字段名转换为列名</summary>
        private string ConvertFieldNameToColumnName(string fieldName)
        {
            // 将camelCase转换为PascalCase
            if (string.IsNullOrEmpty(fieldName))
                return fieldName;

            return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        }
        
        ///<summary>使用反射动态设置字段值</summary>
        private void SetFieldValue(AttackParameters config, string fieldName, object value, Type fieldType)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return;

            try
            {
                // 判断字段类型并进行相应的转换
                if (fieldType == typeof(int))
                {
                    if (int.TryParse(value.ToString(), out int intValue))
                    {
                        typeof(AttackParameters).GetField(fieldName)?.SetValue(config, intValue);
                    }
                }
                else if (fieldType == typeof(float))
                {
                    if (float.TryParse(value.ToString(), out float floatValue))
                    {
                        typeof(AttackParameters).GetField(fieldName)?.SetValue(config, floatValue);
                    }
                }
                else if (fieldType == typeof(string))
                {
                    typeof(AttackParameters).GetField(fieldName)?.SetValue(config, value.ToString());
                }
                else if (fieldType.IsEnum)
                {
                    if (Enum.TryParse(fieldType, value.ToString(), out object enumValue))
                    {
                        typeof(AttackParameters).GetField(fieldName)?.SetValue(config, enumValue);
                    }
                }
                else
                {
                    Debug.LogWarning($"不支持的字段类型: {fieldType.Name} (字段名: {fieldName})");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"设置字段 {fieldName} 值失败: {e.Message}");
            }
        }

        ///<summary>解析单个模块配置</summary>
        private AttackParameters ParseModuleConfig(DataRow row)
        {
            AttackParameters config = new AttackParameters();
            Type moduleParametersType = typeof(AttackParameters);

            // 处理所有字段
            FieldInfo[] fields = moduleParametersType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.Name == "bulletPrefab") // 跳过GameObject字段
                    continue;

                if (_fieldToColumnMapping.TryGetValue(field.Name, out string columnName))
                {
                    object value = GetColumnValue(columnName, row);
                    SetFieldValue(config, field.Name, value, field.FieldType);
                }
            }
            
            // 确保模块名称不为空
            if (string.IsNullOrEmpty(config.moduleName))
            {
                return null;
            }
            
            return config;
        }

        ///<summary>加载默认配置</summary>
        private void LoadDefaultConfigs()
        {
            _defaultConfig = new AttackParameters();
            _moduleConfigs = new Dictionary<string, AttackParameters>
            {
                { _defaultConfig.moduleName, _defaultConfig }
            };
            Debug.LogWarning("加载默认配置");
        }
        
        ///<summary>获取默认配置</summary>
        private AttackParameters GetDefaultConfigs()
        {
            if (_defaultConfig == null)
            {
                _defaultConfig = new AttackParameters();
            }
            return _defaultConfig;
        }
    }
}
