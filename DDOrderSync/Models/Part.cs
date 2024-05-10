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
    class Part
    {
        [FirestoreDocumentId]
        public DocumentReference Reference { get; set; }

        [FirestoreProperty]
        public string PartNumber { get; set; }

        [FirestoreProperty]
        public string Revision { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public string UOM { get; set; }

        public static List<Part> GetValuesTyped()
        {
            List<Part> parts = new List<Part>();

            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT `PartNumber`,`Revision`,`DescText`,`StockUOM` FROM `PartMaster`";

                        cmd.Connection = connection;
                        connection.Open();
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Console.WriteLine(reader.GetString(0));
                                //Console.WriteLine(reader.GetString(1));
                                parts.Add(new Part
                                {
                                    PartNumber = reader.GetString(0).Replace("/", "%2F"),
                                    Revision = !reader.IsDBNull(1)? reader.GetString(1): null,
                                    Description = reader.GetString(2),
                                    UOM = reader.GetString(3)
                                });
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

            return parts;
        }
    }
}
