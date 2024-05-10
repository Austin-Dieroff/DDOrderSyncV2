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
    class TermsCode
    {
        [FirestoreDocumentId]
        public DocumentReference Reference { get; set; }

        [FirestoreProperty]
        public string Code { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        public static List<TermsCode> GetValuesTyped()
        {
            List<TermsCode> termsCodes = new List<TermsCode>();

            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT `TermsCode`,`DescText` FROM `TermsCodes`";

                        cmd.Connection = connection;
                        connection.Open();
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Console.WriteLine(reader.GetString(0));
                                //Console.WriteLine(reader.GetString(1));
                                string description = reader.GetString(1) ?? "";
                                termsCodes.Add(new TermsCode { Code = reader.GetString(0).Replace("/", "%2F"), Description = description });
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

            return termsCodes;
        }
    }
}
