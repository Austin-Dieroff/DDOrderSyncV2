using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDOrderSync.ViewModels
{
    [FirestoreData]
    class Salesperson
    {
        [FirestoreDocumentId]
        public DocumentReference Reference { get; set; }

        [FirestoreProperty]
        public string EmployeeID { get; set; }

        [FirestoreProperty]
        public string FirstName { get; set; }

        [FirestoreProperty]
        public string LastName { get; set; }

        public static List<Salesperson> GetValuesTyped()
        {
            List<Salesperson> salespeople = new List<Salesperson>();

            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT `EmployeeID`,`FirstName`, `LastName` FROM `Employees` WHERE `IsSales`=true";

                        cmd.Connection = connection;
                        connection.Open();
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Console.WriteLine(reader.GetString(0));
                                //Console.WriteLine(reader.GetString(1));
                                salespeople.Add(new Salesperson { EmployeeID = reader.GetString(0), FirstName = reader.GetString(1), LastName = reader.GetString(2) });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
            }

            return salespeople;
        }
    }
}
