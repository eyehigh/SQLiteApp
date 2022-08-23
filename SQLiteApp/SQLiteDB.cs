using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace SQLiteApp
{
    internal class SQLiteDB
    {
        private SQLiteConnectionStringBuilder? csb = null;
        private SQLiteConnection? conn = null;
        private SQLiteTransaction? transaction = null;
        private SQLiteCommand? cmd = null;

        public void asdf()
        {
            Console.WriteLine("Hello, World!");
        }
        public SQLiteDB()
        {
            Init();
        }
        public SQLiteDB(string path)
        {
            SetDB(path);
        }
        public void Init()
        {
            csb = new SQLiteConnectionStringBuilder();
            //csb.DataSource = "127.0.0.1"; //서버주소        
            //csb.Port = 3050; //사용포트    
            //csb.Add("DataSource", @".\SQLiteDB.db");  
            csb.Add("DataSource", @"C:\Users\ADMIN\source\repos\SQLiteApp\SQLiteApp\bin\Debug\net6.0\DB\SQLiteDB.db");
        }
        public void SetDB(string path)
        {
            csb = new SQLiteConnectionStringBuilder();
            csb.Add("DataSource", path);
        }
        public bool IsConnect()
        {
            if (conn == null)
            {
                return false;
            }
            if (conn.State != ConnectionState.Open)
            {
                return false;
            }
            return true;
        }
        public  bool ConnectDB()
        {
            if (csb == null)
            {
                Console.WriteLine("ConnectionString 누락, 연결 실패");
                return false;
            }
            string connectionString = csb.ToString();
            try
            {
                conn = new SQLiteConnection(connectionString);
                conn.Open();
                if (conn.State != ConnectionState.Open)
                {
                    Console.WriteLine("연결 실패");
                    return false;
                }
                else
                    return true;
            }
            catch (SQLiteException fbex)
            {
                Console.WriteLine(fbex.Message.ToString());
                return false;
            }
        }
        public  void DisconnectDB()
        {
            if (conn != null)
                conn.Close();
            conn = null;

            ClearTran();

            if (cmd != null)
            {
                cmd.Dispose();
            }

            if (csb != null)
            {
                csb.Clear();
                csb = null;
            }
        }
        public  bool beginTran()
        {
            if (IsConnect() == false)
            {
                Console.WriteLine("DB연결확인");
                return false;
            }
            if (transaction != null)
            {
                Console.WriteLine("트랜잭션 진행 중");
                return false;
            }

            transaction = conn.BeginTransaction();

            if (cmd == null)
            {
                cmd = new SQLiteCommand();
                cmd.Connection = conn;
            }
            cmd.Transaction = transaction;

            return true;
        }
        public  void ClearTran()
        {
            if (transaction != null)
            {
                transaction.Dispose();
                transaction = null;
            }

            if (cmd != null)
            {
                cmd.Transaction = null;
            }
        }
        public  void Commit()
        {
            if (transaction == null)
            {
                return;
            }
            transaction.Commit();
            ClearTran();
        }
        public  void Rollback()
        {
            if (transaction == null)
            {
                return;
            }
            transaction.Rollback();
            ClearTran();
        }
        public  DataTable SelectDT(string selectQuery)
        {
            if (selectQuery.Trim().Equals(""))
            {
                return null;
            }
            try
            {
                 //SqliteDataReader Reader = new SqliteDataReader(selectQuery, conn);
                DataTable dt = new DataTable();
                //adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public bool Query(string query)
        {
            bool result = false;

            beginTran();

            cmd.CommandText = query;
            try
            {
                int nRow = cmd.ExecuteNonQuery();
                if(nRow > 0)
                {
                    result = true;
                    Commit();
                }
                else
                {
                    result = false;
                    Rollback();
                }
            }
            catch(SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                ClearTran();
            }
            
            return result;
        }
        public bool MultiQuery(List<string> lst_query)
        {
            bool result = true;

            beginTran();
            for( int i = 0; i < lst_query.Count; i++)
            {
                cmd.CommandText = lst_query[i];
                try
                {
                    int nRow = cmd.ExecuteNonQuery();
                    if (nRow > 0)
                    {
                    }
                    else
                    {
                        result = false;
                        Console.WriteLine("Fail : "+ lst_query[i]);
                        break;
                    }
                }
                catch (SQLiteException ex)
                {
                    result = false;
                    Console.WriteLine(ex.Message);
                }
            }

            if (result)
            {
                Commit();
            }
            else
            {
                Rollback();
            }

            ClearTran();

            return result;
        }
        public void testFunc()
        {
            try
            {
                //Console.WriteLine("FDB Open Start :" + conn.State.ToString() + "\n");
                Init();
                ConnectDB();
                

                //Console.WriteLine("FDB Open Result :"+ conn.State.ToString()+"\n");

                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = conn;

                ////---------------------------------------------------------------------------
                //// INSERT DATA
                ///

                //Query("INSERT INTO TEST(NAME, DESC) VALUES('test3', 'desc')");
                List<string> strings = new List<string>();
                //strings.Add("INSERT INTO TEST(NAME, DESC) VALUES(null, 'desc')");
                strings.Add("DELETE FROM TEST WHERE id=1");
                MultiQuery(strings);

                //FbTransaction transaction = conn.BeginTransaction();
                //cmd.Transaction = transaction;
                //cmd.CommandText = "INSERT INTO T_POS(STR_ID_CODE, POS_NO, POS_NAME) VALUES('00', '11','" + DateTime.Now.ToString("HHmmtt") + "');";
                //int nRow = cmd.ExecuteNonQuery();

                //Console.WriteLine("INSERT row :" + nRow.ToString() + "\n");

                //transaction.Commit();
                //transaction.Dispose();
                //cmd.Transaction = null;
                ////-----------------------------------------------------------------------

                //SELECT DATA
                SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Test", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);


                DisconnectDB();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message.ToString());
            }
        }
    }
}
