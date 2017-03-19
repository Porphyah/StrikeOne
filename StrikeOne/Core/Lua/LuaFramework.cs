using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Lua
{
    /// <summary>  
    /// Lua函数描述特性类  
    /// </summary>  
    public class LuaFunction : Attribute
    {
        public readonly string FunctionName;

        public LuaFunction(string strFuncName)
        {
            FunctionName = strFuncName;
        }
    }

    /// <summary>  
    /// Lua引擎  
    /// </summary>  
    public class LuaFramework
    {
        public readonly NLua.Lua LuaState = new NLua.Lua();//lua虚拟机  

        public object this[string Key]
        {
            set { LuaState[Key] = value; }
            get { return LuaState[Key]; }
        }

        public HashSet<string> ExecutedFiles { get; } = new HashSet<string>();

        /// <summary>  
        /// 注册lua函数  
        /// </summary>  
        /// <param name="LuaAPIClass">lua函数类</param>  
        public void BindLuaApiClass(Object LuaAPIClass)
        {
            foreach (MethodInfo Info in LuaAPIClass.GetType().GetMethods())
                foreach (string LuaFunctionName in Attribute.GetCustomAttributes(Info).OfType<LuaFunction>().Select(LuaFunction => LuaFunction.FunctionName))
                    LuaState.RegisterFunction(LuaFunctionName, LuaAPIClass, Info);
        }

        /// <summary>  
        /// 执行lua脚本文件  
        /// </summary>  
        /// <param name="luaFileName">脚本文件名</param>  
        public void ExecuteFile(string luaFileName)
        {
            if (ExecutedFiles.Contains(luaFileName)) return;
            StreamReader SReader = new StreamReader(luaFileName, Encoding.UTF8);
            try { LuaState.DoString(SReader.ReadToEnd()); }
            catch (Exception ex) { throw new Exception("在" + luaFileName + "中发现异常：" + ex.Message); }
            SReader.Close();
            ExecutedFiles.Add(luaFileName);
        }

        /// <summary>  
        /// 执行lua脚本文件中的某个函数。
        /// </summary>  
        /// <param name="FileName">脚本文件名</param>
        /// <param name="FunctionName">函数名</param>
        /// <param name="Parameters">函数参数</param>  
        public void ExecuteFunction(string FileName, string FunctionName, params object[] Parameters)
        {
            if (!ExecutedFiles.Contains(FileName))
            {
                StreamReader SReader = new StreamReader(FileName, Encoding.UTF8);
                LuaState.DoString(SReader.ReadToEnd());
                SReader.Close();
                ExecutedFiles.Add(FileName);
            }
            try { LuaState.GetFunction(FunctionName).Call(Parameters); }
            catch (Exception ex) { throw new Exception("在" + FileName + "中发现异常：" + ex.Message); }
        }

        /// <summary>  
        /// 执行lua脚本  
        /// </summary>  
        /// <param name="luaCommand">lua指令</param>  
        public object[] ExecuteString(string luaCommand)
        {
            return LuaState.DoString(luaCommand);
        }
    }
}
