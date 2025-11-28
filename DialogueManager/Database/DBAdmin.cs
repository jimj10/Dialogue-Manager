/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace DialogueManager.Database
{
    static class DBAdmin
    {
        internal static readonly Object padlock = new Object();

        internal static string DBFileName { get; set; } = "DialogueManager.sqlite";

        internal static SQLiteConnection GetSQLConnection()
        {
            string connectionString = "Data Source=" + DBFileName + ";Version=3;FailIfMissing=True;PRAGMA journal_mode=WAL";
            return new SQLiteConnection(connectionString);
        }

        internal static bool TableExists(string tableName)
        {
            using (SQLiteConnection dbConnection = GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                    cmd.Parameters.Add(new SQLiteParameter("@tableName", DbType.String) { Value = tableName });
                    return cmd.ExecuteScalar() != null;
                }
            }
        }
        internal static string CreateDBFile()
        {
            string fname = Path.Combine(DirectoryMgr.AppDataDirectory, DBFileName);
            SQLiteConnection.CreateFile(fname);
            return fname;
        }

        internal static bool DefaultDatabaseExists()
        {
            DBFileName = Path.Combine(DirectoryMgr.AppDataDirectory, DBFileName);
            return File.Exists(DBFileName);
        }

        internal static bool LoadDatabaseFromDialog() // open dialog to select database file
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = ".sqlite",
                InitialDirectory = DirectoryMgr.AppDataDirectory,
                Title = "Select Database File",
                Filter = "SQLite files (*.sqlite)|*.sqlite|All files (*.*)|*.*"
            };
            bool result = (bool)dlg.ShowDialog();
            if (result)
            {
                if (!dlg.FileName.Equals(DBFileName))
                {
                    if (File.Exists(DBFileName))
                    {
                        BackupDatabase();
                    }

                    lock (padlock)
                    {
                        File.Copy(dlg.FileName, DBFileName, true);
                    }
                    return true;
                }
                else
                {
                    //var messageWin = new MessageWin("Load Database File", String.Format("Database file {0} is already loaded.", dlg.FileName));
                    // messageWin.Show();
                }
            }
            return false;
        }

        internal static void BackupDatabase(string snapshotName = null)
        {
            string backupFilename = snapshotName == null
                ? Path.Combine(DirectoryMgr.AppDataDirectory,
                    DateTime.Now.ToString("yyMMddHHmm") + "_"
                    + Path.GetFileNameWithoutExtension(DBFileName) + ".sqlite")
                : Path.Combine(DirectoryMgr.AppDataDirectory,
                    Path.GetFileNameWithoutExtension(DBFileName) + "-" + snapshotName + ".sqlite");
            lock (padlock)
            {
                File.Copy(DBFileName, backupFilename, true);
            }
        }

        internal static bool RestoreDatabase()
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = ".sqlite",
                InitialDirectory = DirectoryMgr.AppDataDirectory
            };
            bool? outcome = dlg.ShowDialog();
            if (outcome == true)
            {
                string restoreFilename = dlg.FileName;
                string backupFilename = Path.Combine(DirectoryMgr.AppDataDirectory, DateTime.Now.ToString("yyMMddHHmm") +
                    "_" + Path.GetFileNameWithoutExtension(DBFileName) + ".sqlite");
                lock (padlock)
                {
                    // backup current database file
                    File.Copy(DBFileName, backupFilename, true);
                    File.Copy(restoreFilename, DBFileName, true);
                    DBFileName = restoreFilename;
                }
                return true;
            }
            return false;
        }
    }
}