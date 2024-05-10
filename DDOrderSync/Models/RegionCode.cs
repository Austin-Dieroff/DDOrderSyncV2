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
    class RegionCode
    {
        [FirestoreDocumentId]
        public DocumentReference Reference { get; set; }

        [FirestoreProperty]
        public string Code { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        public static List<RegionCode> GetValuesTyped()
        {
            List<RegionCode> regionCodes = new List<RegionCode>();

            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT `RegionCode`,`DescText` FROM `RegionCodes`";

                        cmd.Connection = connection;
                        connection.Open();
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Console.WriteLine(reader.GetString(0));
                                //Console.WriteLine(reader.GetString(1));
                                regionCodes.Add(new RegionCode { Code = reader.GetString(0), Description = reader.GetString(1) });
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

            return regionCodes;
        }
    }
}
