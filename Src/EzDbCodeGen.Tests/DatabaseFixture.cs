using EzDbCodeGen.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Threading.Tasks;
using EzDbCodeGen.Internal;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace EzDbCodeGen.Tests
{
    public enum LocalDBAction {
        create,
        delete,
        start,
        stop
    }
    //https://www.dotnetcurry.com/visualstudio/1456/integration-testing-sqllocaldb Thanks! <3 
    //https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/sharing-databases
    public class DatabaseFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _databaseInitialized;

        public string LOCAL_SERVER = "logicbyter.lan";
        public string CI_SERVER = @"localhost\SQL2019";
        public string LOCAL_CONNECTION_STRING = @"";
        public string APPVOYER_CONNECTION_STRING = @"";
        private string connectionString = "";
        private SqlConnection connection = null;
        public DatabaseFixture()
        {
            LOCAL_CONNECTION_STRING = @"Server=" + LOCAL_SERVER + @";Database=master;User ID=sa;Password=Password12!;";
            APPVOYER_CONNECTION_STRING = @"Server=" + CI_SERVER + @";Database=master;User ID=sa;Password=Password12!;";
            //figure out which connection string we use
            WriteLine("Connection String=" + ConnectionString);
            EnsureDatabaseExists();
        }

        public SqlConnection Connection
        {
            get
            {
                if (connection == null) {
                    connection = new SqlConnection();
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    AppSettings.Instance.ConnectionString = connection.ConnectionString;
                }
                return connection;
            }
            set
            {
                connection = value;
            }
        }
        public void WriteLine(string lineToWrite)
        {
            Console.WriteLine(lineToWrite);
            System.Diagnostics.Debug.WriteLine(lineToWrite);
        }
        public bool IsLocalServer { get; set; } = false;
        public string ConnectionString {
            get
            {
                if (connectionString.Length==0)
                {
                    IsLocalServer = CanConnect(LOCAL_CONNECTION_STRING);
                    if (IsLocalServer) 
                        connectionString = LOCAL_CONNECTION_STRING;
                    else
                    {
                        var IsCIServer = CanConnect(APPVOYER_CONNECTION_STRING);
                        if (IsCIServer) connectionString = APPVOYER_CONNECTION_STRING;
                    }
                }
                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }

        public void Dispose()
        {
            if ((connection!= null) && (connection.State == System.Data.ConnectionState.Open))
            {
                connection.Close();
            }
            LocalDbActionExec(LocalDBAction.stop);
            LocalDbActionExec(LocalDBAction.delete);
        }

        public bool CanConnect(string connectionString)
        {
            return ServerName(connectionString) != "Unknown"; 
        }

        /// <summary>
        /// Action can 
        /// </summary>
        public void LocalDbActionExec(LocalDBAction action)
        {
            var dbcmd = "create localtestdb -s";
            if (action == LocalDBAction.delete) dbcmd = "delete localtestdb";
            if (action == LocalDBAction.start) dbcmd = "start localtestdb";
            if (action == LocalDBAction.stop) dbcmd = "stop localtestdb";
            // Use a ProcessStartInfo object to provide a simple solution to create a new LocalDbInstance
            var _processInfo =
            new ProcessStartInfo("cmd.exe", "/c " + string.Format("sqllocaldb.exe {0}", dbcmd))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var _process = Process.Start(_processInfo);
            _process.WaitForExit();

            string _output = _process.StandardOutput.ReadToEnd();
            string _error = _process.StandardError.ReadToEnd();

            var _exitCode = _process.ExitCode;

            WriteLine("output>>" + (String.IsNullOrEmpty(_output) ? "(none)" : _output).Trim());
            WriteLine("error>>" + (String.IsNullOrEmpty(_error) ? "(none)" : _error).Trim());
            WriteLine("ExitCode: " + _exitCode.ToString().Trim());
            connectionString = @"Data Source=(localdb)\localtestdb; Database=master; Trusted_Connection=True; MultipleActiveResultSets=true;";
            _process.Close();
        }


        public bool EnsureDatabaseExists()
        {

            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    LocalDbActionExec(LocalDBAction.create);
                    var database = "AdventureWorksLT2008";
                    string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar;
                    string bakFile = filePath + database + ".bak";
                    if (!File.Exists(bakFile)) throw new Exception(string.Format("Backup File {0} does not exist.", bakFile));
                    try
                    {
                        //WriteLine("Attempting to connect to {0}", connectionString);
                        using (var cn = new SqlConnection(ConnectionString + ""))
                        {
                            var SQL = string.Format(@"
DECLARE @dbname nvarchar(128);
SET @dbname = N'{0}';

IF (EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = @dbname OR name = @dbname)))
DROP DATABASE {0};

USE [master]
RESTORE DATABASE [{0}] 
FROM DISK = N'{2}{0}.bak' 
WITH  FILE = 1,  MOVE N'{0}_Data' 
TO N'{2}{0}_Data.mdf',  
MOVE N'{0}_Log' TO N'{2}{0}_log.ldf',  
NOUNLOAD, REPLACE, STATS = 5;
", database, bakFile, filePath);

                            WriteLine(string.Format("Restoring the database {0} from backup {1}", database, bakFile, filePath));
                            var cmd = cn.CreateCommand();
                            cn.InfoMessage += delegate (object sender, SqlInfoMessageEventArgs args)
                            {
                                WriteLine(string.Format("{0}", args.Message));
                                return;
                            };
                            cn.FireInfoMessageEventOnUserErrors = true;
                            cmd.CommandText = SQL;
                            cmd.CommandTimeout = 1800;
                            cn.Open();
                            cmd.ExecuteNonQuery();
                            WriteLine(string.Format("Database restored!", database, bakFile));
                            connectionString = connectionString.Replace("master", database);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLine(string.Format("Failure: {0}", ex.Message));
                    }

                    _databaseInitialized = true;
                    return true;
                }
            }
            return true;
        }

        public string ServerName()
        {
            return ServerName(this.ConnectionString);
        }

        public string ServerName(string connectionString)
        {
            var serverName = "Unknown";
            try
            {
                //WriteLine("Attempting to connect to {0}", connectionString);
                using (var connection = new SqlConnection(connectionString + ";Connect Timeout=5"))
                {
                    var query = "SELECT @@SERVERNAME";
                    var command = new SqlCommand(query, connection);
                    connection.Open();
                    serverName = command.ExecuteScalar()?.ToString();
                    //WriteLine(string.Format("SQL Query execution successful at {0}", serverName));
                }
            }
            catch (Exception ex)
            {
                WriteLine(string.Format("Failure: {0}", ex.Message));
            }
            return serverName ?? "Unknown";
        }

    }

    [CollectionDefinition("DatabaseTest")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
