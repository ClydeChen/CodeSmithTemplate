using System;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using System.Collections;

namespace ADDMS.Data.DBUtility
{
    public class MySQLHelper
    {
        private static MySqlConnection sqlCon;

        /// <summary>
        /// 连接对象
        /// </summary>
        public static MySqlConnection SqlCon
        {
            get
            {
                if (sqlCon == null)
                    sqlCon = new MySqlConnection(System.Configuration.ConfigurationSettings.AppSettings["MySqlConnectionString"]);
                if (sqlCon.State == ConnectionState.Closed)
                    sqlCon.Open();
                return MySQLHelper.sqlCon;
            }
        }

        /// <summary>
        /// 执行查询的方法
        /// </summary>
        /// <param name="strSql">T-SQL语句</param>
        /// <param name="sqlPar">参数数组</param>
        /// <returns>返回一个DataReader对象</returns>
        public static MySqlDataReader ExecuteReader(string strSql, MySqlParameter[] sqlPar, CommandType type)
        {
            MySqlCommand sqlCommand = new MySqlCommand(strSql, SqlCon);
            sqlCommand.CommandType = type;
            if (sqlPar != null)
                sqlCommand.Parameters.AddRange(sqlPar);
            return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }
        /// <summary>
        ///  增删改查
        /// </summary>
        /// <param name="strSql">TSQl / Proc</param>
        /// <param name="sqlPar"></param>
        /// <param name="type">Text / Proc</param>
        /// <returns></returns>
        public static int Execute(string strSql, MySqlParameter[] sqlPar, CommandType type)
        {
            MySqlCommand sqlCommand = new MySqlCommand(strSql, SqlCon);
            sqlCommand.CommandType = type;
            if (sqlPar != null)
                sqlCommand.Parameters.AddRange(sqlPar);
            return sqlCommand.ExecuteNonQuery();

        }
        /// <summary>
        /// 执行查询的方法
        /// </summary>
        /// <param name="strcon">连接字符串</param>
        /// <param name="cmdText">sql语句</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExcuteQuery(string cmdText, MySqlParameter[] sqlpar, CommandType type)
        {
            using (MySqlCommand sqlcom = new MySqlCommand(cmdText, SqlCon))
            {

                sqlcom.CommandType = type;
                if (sqlpar != null)
                {
                    sqlcom.Parameters.AddRange(sqlpar);
                }
                MySqlDataAdapter sqlda = new MySqlDataAdapter(sqlcom);
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
        public static object ExcuteMyScalar(string sql, MySqlParameter[] sqlpar, CommandType type)
        {
            MySqlCommand sqlCommand = new MySqlCommand(sql, SqlCon);
            sqlCommand.CommandType = type;
            if (sqlpar != null)
                sqlCommand.Parameters.AddRange(sqlpar);
            return sqlCommand.ExecuteScalar();
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

        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
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

        public static bool Exists(string strSql, params MySqlParameter[] cmdParms)
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
            using (MySqlCommand cmd = new MySqlCommand(SQLString, SqlCon))
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
            using (MySqlCommand cmd = new MySqlCommand(SQLString, SqlCon))
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
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SqlCon;
            MySqlTransaction tx = SqlCon.BeginTransaction();
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
            MySqlCommand cmd = new MySqlCommand(SQLString, SqlCon);
            MySqlParameter myParameter = new MySqlParameter("@content", DbType.String);
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
            MySqlCommand cmd = new MySqlCommand(SQLString, SqlCon);
            MySqlParameter myParameter = new MySqlParameter("@content", DbType.String);
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
            MySqlCommand cmd = new MySqlCommand(strSQL, SqlCon);
            MySqlParameter myParameter = new MySqlParameter("@fs", DbType.Binary);
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
            using (MySqlCommand cmd = new MySqlCommand(SQLString, SqlCon))
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
        /// 执行查询语句，返回MyMySqlDataReader(使用该方法切记要手工关闭MyMySqlDataReader和连接)
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MyMySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader(string strSQL)
        {
            MySqlCommand cmd = new MySqlCommand(strSQL, SqlCon);
            try
            {
                MySqlDataReader myReader = cmd.ExecuteReader();
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
                MySqlDataAdapter command = new MySqlDataAdapter(SQLString, SqlCon);
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
                MySqlDataAdapter command = new MySqlDataAdapter("select * from " + tableName, SqlCon);
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
                MySqlDataAdapter command = new MySqlDataAdapter(SQLString, SqlCon);
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
                MySqlDataAdapter command = new MySqlDataAdapter(SQLString, SqlCon);
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
                MySqlDataAdapter command = new MySqlDataAdapter(SQLString, SqlCon);
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
        public static int ExecuteSql(string SQLString, params MySqlParameter[] cmdParms)
        {
            using (MySqlCommand cmd = new MySqlCommand())
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
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MyMySqlParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (MySqlTransaction trans = SqlCon.BeginTransaction())
            {
                MySqlCommand cmd = new MySqlCommand();
                try
                {
                    //循环
                    foreach (DictionaryEntry myDE in SQLStringList)
                    {
                        string cmdText = myDE.Key.ToString();
                        MySqlParameter[] cmdParms = (MySqlParameter[]) myDE.Value;
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
        public static object GetSingle(string SQLString, params MySqlParameter[] cmdParms)
        {
            using (MySqlCommand cmd = new MySqlCommand())
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
        /// 执行查询语句，返回MyMySqlDataReader (使用该方法切记要手工关闭MyMySqlDataReader和连接)
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MyMySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader(string SQLString, params MySqlParameter[] cmdParms)
        {
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                PrepareCommand(cmd, SqlCon, null, SQLString, cmdParms);
                MySqlDataReader myReader = cmd.ExecuteReader();
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
        public static DataSet Query(string SQLString, params MySqlParameter[] cmdParms)
        {
            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, SqlCon, null, SQLString, cmdParms);
            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (MySqlException ex)
                {
                    throw new System.Exception(ex.Message);
                }
                return ds;
            }

        }

        public static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn,
                        MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
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


                foreach (MySqlParameter parameter in cmdParms)
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
        /// 放回一个MyMySqlParameter
        /// </summary>
        /// <param name="name">参数名字</param>
        /// <param name="type">参数类型</param>
        /// <param name="size">参数大小</param>
        /// <param name="value">参数值</param>
        /// <returns>MyMySqlParameter的值</returns>
        public static MySqlParameter MakeMyMySqlParameter(string name,
                   MySqlDbType type, int size, object value)
        {
            MySqlParameter parm = new MySqlParameter(name, type, size);
            parm.Value = value;
            return parm;
        }

        public static MySqlParameter MakeMyMySqlParameter(string name, DbType type, object value)
        {
            MySqlParameter parm = new MySqlParameter(name, type);
            parm.Value = value;
            return parm;
        }

        #endregion
    }
}




