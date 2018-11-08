using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;


namespace ADDMS.Data.DBUtility
{
    /// <summary>
    /// SQLiteHelper类
    /// </summary>
    public class SQLiteHelper
    {
        
        private static SQLiteConnection sqlCon;

        /// <summary>
        /// 连接对象
        /// </summary>
        ///更改为从配置文件中读取数据
        public static SQLiteConnection SqlCon
        {
            get
            {
                if (sqlCon == null)
                    sqlCon = new SQLiteConnection(System.Configuration.ConfigurationSettings.AppSettings["SQLiteConnectionString"]);
                if (sqlCon.State == ConnectionState.Closed)
                    sqlCon.Open();
                return SQLiteHelper.sqlCon;
            }
        }

        /// <summary>
        /// 执行查询的方法
        /// </summary>
        /// <param name="strSql">T-SQL语句</param>
        /// <param name="sqlPar">参数数组</param>
        /// <returns>返回一个DataReader对象</returns>
        public static SQLiteDataReader ExecuteReader(string strSql, SQLiteParameter [] sqlPar, CommandType type)
        {
            SQLiteCommand sqlLiteCommand = new SQLiteCommand(strSql, SqlCon);
            sqlLiteCommand.CommandType = type;
            if (sqlPar != null)
                sqlLiteCommand.Parameters.AddRange(sqlPar);
            return sqlLiteCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }
        /// <summary>
        ///  增删改查
        /// </summary>
        /// <param name="strSql">TSQl / Proc</param>
        /// <param name="sqlPar"></param>
        /// <param name="type">Text / Proc</param>
        /// <returns></returns>
        public static int Execute(string strSql, SQLiteParameter[] sqlPar, CommandType type)
        {
            SQLiteCommand sqLiteCommand = new SQLiteCommand(strSql, SqlCon);
            sqLiteCommand.CommandType = type;
            if (sqlPar != null)
                sqLiteCommand.Parameters.AddRange(sqlPar);
            return sqLiteCommand.ExecuteNonQuery();

        }
        /// <summary>
        /// 执行查询的方法
        /// </summary>
        /// <param name="strcon">连接字符串</param>
        /// <param name="cmdText">sql语句</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExcuteQuery(string cmdText, SQLiteParameter[] sqlpar, CommandType type)
        {
            using (SQLiteCommand sqlcom = new SQLiteCommand(cmdText, SqlCon))
            {

                sqlcom.CommandType = type;
                if (sqlpar != null)
                {
                    sqlcom.Parameters.AddRange(sqlpar);
                }
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(sqlcom);
                DataSet ds = new DataSet();
                sqlda.Fill(ds, "table");
                return ds;
            }
        }

        /// <summary>
        /// 查询第一行第一列的数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlpar"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ExcuteMyScalar(string sql, SQLiteParameter[] sqlpar, CommandType type)
        {
            SQLiteCommand SQLiteCommand = new SQLiteCommand(sql, SqlCon);
            SQLiteCommand.CommandType = type;
            if (sqlpar != null)
                SQLiteCommand.Parameters.AddRange(sqlpar);
            return SQLiteCommand.ExecuteScalar();
        }


        #region 公用方法

        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }
        //潘雪婷2013年0316修改，只要查询结果第一个单元格内容不为空，就表示存在
        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            //int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                //cmdresult = 0;
                return false;
            }
            else
            {
                //cmdresult = int.Parse(obj.ToString());
                return true;
            }
            //if (cmdresult == 0)
            //{
            //    return false;
            //}
            //else
            //{
            //    return true;
            //}
        }

        public static bool Exists(string strSql, params SQLiteParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(SQLString, SqlCon))
            {
                try
                {
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SQLite.SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
            }

        }

        /// <summary>
        /// 执行SQL语句，设置命令的执行等待时间
        /// </summary>
        /// <param name="SQLString"></param>
        /// <param name="Times"></param>
        /// <returns></returns>
        public static int ExecuteSqlByTime(string SQLString, int Times)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(SQLString, SqlCon))
            {
                try
                {
                    cmd.CommandTimeout = Times;
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SQLite.SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
            }

        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>        
        public static void ExecuteSqlTran(ArrayList SQLStringList)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = SqlCon;
            SQLiteTransaction tx = SqlCon.BeginTransaction();
            cmd.Transaction = tx;
            try
            {
                for (int n = 0; n < SQLStringList.Count; n++)
                {
                    string strsql = SQLStringList[n].ToString();
                    if (strsql.Trim().Length > 1)
                    {
                        cmd.CommandText = strsql;
                        cmd.ExecuteNonQuery();
                    }
                }
                tx.Commit();
            }
            catch (System.Data.SQLite.SQLiteException E)
            {
                tx.Rollback();
                throw new Exception(E.Message);
            }

        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            SQLiteCommand cmd = new SQLiteCommand(SQLString, SqlCon);
            SQLiteParameter myParameter = new SQLiteParameter("@content", DbType.String);
            myParameter.Value = content;
            cmd.Parameters.Add(myParameter);
            try
            {
                int rows = cmd.ExecuteNonQuery();
                return rows;
            }
            catch (System.Data.SQLite.SQLiteException E)
            {
                throw new Exception(E.Message);
            }
            finally
            {
                cmd.Dispose();
            }

        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static object ExecuteSqlGet(string SQLString, string content)
        {
            SQLiteCommand cmd = new SQLiteCommand(SQLString, SqlCon);
            SQLiteParameter myParameter = new SQLiteParameter("@content", DbType.String);
            myParameter.Value = content;
            cmd.Parameters.Add(myParameter);
            try
            {
                object obj = cmd.ExecuteScalar();
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    return null;
                }
                else
                {
                    return obj;
                }
            }
            catch (System.Data.SQLite.SQLiteException E)
            {
                throw new Exception(E.Message);
            }
            finally
            {
                cmd.Dispose();
            }

        }

        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            SQLiteCommand cmd = new SQLiteCommand(strSQL, SqlCon);
            SQLiteParameter myParameter = new SQLiteParameter("@fs", DbType.Binary);
            myParameter.Value = fs;
            cmd.Parameters.Add(myParameter);
            try
            {
                int rows = cmd.ExecuteNonQuery();
                return rows;
            }
            catch (System.Data.SQLite.SQLiteException E)
            {
                throw new Exception(E.Message);
            }
            finally
            {
                cmd.Dispose();
            }

        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(SQLString, SqlCon))
            {
                try
                {
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (System.Data.SQLite.SQLiteException e)
                {
                    throw new Exception(e.Message);
                }
            }

        }

        /// <summary>
        /// 执行查询语句，返回SQLiteDataReader(使用该方法切记要手工关闭SQLiteDataReader和连接)
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SQLiteDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string strSQL)
        {
            SQLiteCommand cmd = new SQLiteCommand(strSQL, SqlCon);
            try
            {
                SQLiteDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                throw new Exception(e.Message);
            }
   
        }

        /// <summary>
        /// 查询数据库结构中的表名
        /// </summary>
        /// <returns></returns>
        public static DataSet QueryDBContainTables()
        {
            //查询数据库结构
            string SQLString = "select * from sqlite_master WHERE type =\"table\"";
            DataSet ds = new DataSet();
            try
            {
                SQLiteDataAdapter command = new SQLiteDataAdapter(SQLString, SqlCon);
                command.Fill(ds, "ds");
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds;

        }

        /// <summary>
        /// 查询一张表中所有字段，返回DataSet
        /// </summary>
        /// <param name="SQLString">表名</param>
        /// <returns>DataSet</returns>
        public static DataSet QueryTable(string tableName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLiteDataAdapter command = new SQLiteDataAdapter("select * from " + tableName, SqlCon);
                command.Fill(ds, "ds");
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds;

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLiteDataAdapter command = new SQLiteDataAdapter(SQLString, SqlCon);
                command.Fill(ds, "ds");
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds;

        }

        public static DataSet Query(string SQLString, string TableName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLiteDataAdapter command = new SQLiteDataAdapter(SQLString, SqlCon);
                command.Fill(ds, TableName);
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds;

        }

        /// <summary>
        /// 执行查询语句，返回DataSet,设置命令的执行等待时间
        /// </summary>
        /// <param name="SQLString"></param>
        /// <param name="Times"></param>
        /// <returns></returns>
        public static DataSet Query(string SQLString, int Times)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLiteDataAdapter command = new SQLiteDataAdapter(SQLString, SqlCon);
                command.SelectCommand.CommandTimeout = Times;
                command.Fill(ds, "ds");
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds;

        }

        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params SQLiteParameter[] cmdParms)
        {
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                try
                {
                    PrepareCommand(cmd, SqlCon, null, SQLString, cmdParms);
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
                catch (System.Data.SQLite.SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
            }

        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SQLiteParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SQLiteTransaction trans = SqlCon.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand();
                try
                {
                    //循环
                    foreach (DictionaryEntry myDE in SQLStringList)
                    {
                        string cmdText = myDE.Key.ToString();
                        SQLiteParameter[] cmdParms = (SQLiteParameter[]) myDE.Value;
                        PrepareCommand(cmd, SqlCon, trans, cmdText, cmdParms);
                        int val = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        trans.Commit();
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params SQLiteParameter[] cmdParms)
        {
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                try
                {
                    PrepareCommand(cmd, SqlCon, null, SQLString, cmdParms);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (System.Data.SQLite.SQLiteException e)
                {
                    throw new Exception(e.Message);
                }
            }

        }

        /// <summary>
        /// 执行查询语句，返回SQLiteDataReader (使用该方法切记要手工关闭SQLiteDataReader和连接)
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SQLiteDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string SQLString, params SQLiteParameter[] cmdParms)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, SqlCon, null, SQLString, cmdParms);
                SQLiteDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                throw new Exception(e.Message);
            }
            //finally //不能在此关闭，否则，返回的对象将无法使用
            //{
            //    cmd.Dispose();
            //    connection.Close();
            //}   

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString, params SQLiteParameter[] cmdParms)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand(cmd, SqlCon, null, SQLString, cmdParms);
            using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (System.Data.SQLite.SQLiteException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }

        }

        public static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn,
                         SQLiteTransaction trans, string cmdText, SQLiteParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (SQLiteParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput
                            || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion

        #region 参数转换
        /// <summary>
        /// 放回一个SQLiteParameter
        /// </summary>
        /// <param name="name">参数名字</param>
        /// <param name="type">参数类型</param>
        /// <param name="size">参数大小</param>
        /// <param name="value">参数值</param>
        /// <returns>SQLiteParameter的值</returns>
        public static SQLiteParameter MakeSQLiteParameter(string name,
                   DbType type, int size, object value)
        {
            SQLiteParameter parm = new SQLiteParameter(name, type, size);
            parm.Value = value;
            return parm;
        }

        public static SQLiteParameter MakeSQLiteParameter(string name, DbType type, object value)
        {
            SQLiteParameter parm = new SQLiteParameter(name, type);
            parm.Value = value;
            return parm;
        }

        #endregion
   }

}
