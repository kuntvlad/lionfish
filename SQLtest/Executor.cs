﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SQLtest
{
    class Executor
    {

        public void Run (string sqlConnection, int dbType, string customSqlSetName, string myKey, string sqlDialect, List<string> failedDbs, bool debug)
        {
            
            // pripoj se do databaze
            SqlConnection connection = new SqlConnection(sqlConnection);
            connection.Open();
            ClassCompleteStructure querries = this.Run(connection, sqlDialect, debug);

            // send
            querries.dialect = sqlDialect;
            querries.userAccountId = myKey;
            querries.customSqlSetName = customSqlSetName;
            this.SendQuerries(querries);
        }

        private ClassCompleteStructure Run(SqlConnection connection, string sqlDialect, bool debug)
        {
            ClassCompleteStructure ret = new ClassCompleteStructure();
            List<string> dbNames = this.GetDbNames(connection, sqlDialect, debug);
            ret.queries = this.GetQuerries(connection, sqlDialect, dbNames, debug);

            return ret;
        }



        private List<string> GetDbNames (SqlConnection connection, string sqlDialect, bool debug)
        {
            List<string> sqls =  this.GetSQLCommands(sqlDialect, "databases", null);

            List<SQLResult> result = new List<SQLResult>();

            foreach (var item in sqls)
            {
                this.RunSql(connection, result, item);
            }

            int count = 0;
            List<string> ret = new List<string>();
            foreach (var item in result)
            {
                count++;
                ret.Add(item.Column0);

                if (debug && count > 2) break;
            }
            return ret;
        }

        private List<SQLQuerry> GetQuerries(SqlConnection connection, string sqlDialect, List<string> dbNames, bool debug)
        {
            List<SQLQuerry> ret = new List<SQLQuerry>();

            foreach (var dbName in dbNames)
            {
                try
                {
                    // sql commands
                    List<StrReplace> replaces = new List<StrReplace>();
                    StrReplace itemForReplace = new StrReplace()
                    {
                        SearchText = "##DBNAME##",
                        ReplaceText = dbName
                    };
                    replaces.Add(itemForReplace);

                    List<string> sqls = this.GetSQLCommands(sqlDialect, "queries", replaces);

                    List<SQLResult> result = new List<SQLResult>();
                    foreach (var item in sqls)
                    {
                        this.RunSql(connection, result, item);
                    }

                    foreach (var item in result)
                    {
                        SQLQuerry querryItem = new SQLQuerry()
                        {
                            sourceCode = item.Column0,
                            name = item.Column1,
                            groupName = item.Column2,
                            database = item.Column3,
                            schema = item.Column4
                        };

                        if(debug)
                        {
                            querryItem.sourceCode = querryItem.sourceCode.Substring(0, 25);
                        }

                        ret.Add(querryItem);
                    }
                }
                catch(Exception)
                {
                    ;
                }
            }

            return ret;
        }



















        private List<string> GetSQLCommands (string sqlDialect, string purpose, List<StrReplace> list)
        {

            string sqlCommands = System.IO.File.ReadAllText("./" + sqlDialect + "/" + purpose + "/cmd.sql");

            if (list != null)
            {
                foreach (var item in list)
                {
                    sqlCommands = sqlCommands.Replace(item.SearchText, item.ReplaceText);
                }
            }

            string[] separator = { "--split" };
            List<string> sqlCommandsList = sqlCommands.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();

            return sqlCommandsList;
        }











        private ClassCompleteStructure Run(SqlConnection connection, List<string> sqlCommandsList, List<string> failedDbs, bool debug)
        {

            // prvni prikaz zjistuje databaze
            List<string> dbNames = null; // this.RunSql(connection, sqlCommandsList.FirstOrDefault(), null, null, debug).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            ClassCompleteStructure querries = new ClassCompleteStructure();
            querries.queries = new List<SQLQuerry>();


            // The following SELECTS map to JSON (see example.json)

            // 2. SELECT
            // DDL ("queries" in JSON): procedures and views
            // - sourceCode: `select * from table1` (string, required)
            // - name (string, optional) - for visualization purposes - use your internal name if available
            // - groupName (string, optional) - for visualization purposes - use your internal name if available
            // - database (string, optional)
            // - schema (string, optional)
            //
            // Expect columns in this order: SourceCode, Name, GroupName, Database, Schema



            // 3. SELECT
            // DbDef: ("databaseModel" in JSON) details on table and view columns
            // ## Table (object)
            //
            // - schema: `ETL_SCHEMA` (string, required) - name of the schema for the table/view
            // - name: ACCOUNT (string, required) - name of the table/view, avoid using duplicate names (only the first occurrence may be processed)
            // - isView: false (boolean, optional, default) - true => view, otherwise => table
            // - columns (array[Column]) - columns are processed sequentially, provide them in the same order as in your table/view
            //
            // ## Column (object)
            //
            // - name: `ACC_ID` (string, required) - name of the column, avoid using duplicate names (only the first occurrence may be processed)
            // - dataType: `NUMBER(10)` (string, optional) - column data type
            // - comment: `Unique account identifier` (string, optional) - column comment if available
            //
            // Expect columns in this order: Database, Schema, TableName, IsView, ColumnName, DataType, Comment, ColOrder


            // 4. SELECT
            // synonyms
            // ## Synonym (object)
            //
            // - schema: `DW_SCHEMA` (string, required) - name of the synonym schema
            // - name: ACCOUNTS (string, required) - synonym name, avoid using duplicate names (only the first occurrence may be processed)
            // - sourceName: ACCOUNTS (string, required) - table/view name
            // - sourceSchema: `ETL_SCHEMA` (string, optional) - name of the schema for source table/view
            // - sourceDbLinkName (string, optional) - database link for source table/view
            //
            // Expect columns in this order: Database, Schema, Name, SoourceName, SourceSchema, SourceDbLinkName


            // 5. SELECT
            // ## DBLink (object)
            //
            // - owner : (string, required)
            // - name : (string, required)
            // - userName : (string, required)
            // - host : (string, required)
            //
            // Expect columns in this order: Owner, Name, UserName, Host

            foreach (var dbName in dbNames)
            {
                int iCmd = 0;
                try
                {
                    foreach (var item in sqlCommandsList)
                    {
                        if (iCmd > 0)
                        {
                            {
                                string sqlCmd = item.Replace("##DBNAME##", dbName);

                                List<SQLQuerry> sqldata = new List<SQLQuerry>();

                                string ret = null; // RunSql(connection, sqlCmd, sqldata, dbName, debug);


                                foreach (var item_data in sqldata)
                                {
                                    querries.queries.Add(item_data);

                                }

                            }
                        }

                        iCmd++;
                    }
                }
                catch(Exception)
                {
                    failedDbs.Add(dbName);
                }
                if (debug) break;
            }

            if (failedDbs.Count == dbNames.Count)
            {
                throw new Exception("None database was succesfully analyzed.");
            }

            return querries;
        }

        private void SendQuerries (ClassCompleteStructure querries)
        { 

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(querries);

            // post data
            //string URI = " https://dev-jessie.sqldep.com/api/rest/sqlset/create/";
            //string myParameters = json;
            //using (WebClient wc = new WebClient())
            //{
            //    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            //    string HtmlResult = wc.UploadString(URI, myParameters);
            //}

            return;

            var baseAddress = "https://sqldep.com/api/rest/sqlset/create/";

            var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";
            string parsedContent = json;
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();

        }

        private string RunSql(SqlConnection connection, List<SQLResult> result, string cmd)
        {
            string ret = string.Empty;

            SqlCommand toGo = connection.CreateCommand();
            toGo.CommandText = cmd;

            SqlDataReader reader = toGo.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int nCol = reader.VisibleFieldCount;

                    for (int ii = 0; ii < nCol; ii++)
                    {
                        if (ii > 0) ret += "\t";
                        ret += reader.GetString(ii);
                    }
                    ret += "\n";

                    SQLResult newItem = new SQLResult();
                        
                    if (nCol > 0)
                        newItem.Column0 = reader.GetString(0);
                    if (nCol > 1)
                        newItem.Column1 = reader.GetString(1);
                    if (nCol > 2)
                        newItem.Column2 = reader.GetString(2);
                    if (nCol > 3)
                        newItem.Column3 = reader.GetString(3);
                    if (nCol > 4)
                        newItem.Column4 = reader.GetString(4);
                    if (nCol > 5)
                        newItem.Column5 = reader.GetString(5);

                    result.Add(newItem);

                }
            }

            reader.Close();
            toGo.Dispose();

            return ret;
        }
    }
}
