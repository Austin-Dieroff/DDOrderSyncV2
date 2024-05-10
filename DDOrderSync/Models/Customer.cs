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
    class Customer
    {
        [FirestoreDocumentId]
        public DocumentReference Reference { get; set; }

        [FirestoreProperty]
        public string CustomerID { get; set; }

        [FirestoreProperty]
        public string CustomerName { get; set; }

        [FirestoreProperty]
        public string RegionCode { get; set; }

        [FirestoreProperty]
        public string TermsCode { get; set; }

        [FirestoreProperty]
        public string ShipViaCode { get; set; }

        [FirestoreProperty]
        public string FOBCode { get; set; }

        [FirestoreProperty]
        public string CurrencyCode { get; set; }

        [FirestoreProperty]
        public string VATRegNumber { get; set; }

        [FirestoreProperty]
        public string VATBranchID { get; set; }

        [FirestoreProperty]
        public string UserDefined1 { get; set; }

        [FirestoreProperty]
        public string SalespersonID { get; set; }

        [FirestoreProperty]
        public string Customers_PKey { get; set; }

        public static List<Customer> GetValuesTyped()
        {
            List<Customer> customers = new List<Customer>();

            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT `CustomerID`,`CustomerName`,`RegionCode`,`TermsCode`," +
                            "`ShipViaCode`,`FOBCode`,`CurrencyCode`,`VATRegNumber`,`VATBranchID`,`UserDefined1`,`SalespersonID`,`Customers_PKey` FROM `Customers`";

                        cmd.Connection = connection;
                        connection.Open();
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Console.WriteLine(reader.GetString(0));
                                //Console.WriteLine("Value: " + reader.GetValue(6));
                                customers.Add(new Customer {
                                    CustomerID = reader.GetString(0), 
                                    CustomerName = reader.GetString(1),
                                    RegionCode = reader.GetString(2),
                                    TermsCode = reader.GetString(3),
                                    ShipViaCode = reader.GetString(4),
                                    FOBCode = reader.GetString(5),
                                    CurrencyCode = !reader.IsDBNull(6) ? reader.GetString(6): null,
                                    VATRegNumber = !reader.IsDBNull(7) ? reader.GetString(7) : null,
                                    VATBranchID = !reader.IsDBNull(8) ? reader.GetString(8) : null,
                                    UserDefined1 = !reader.IsDBNull(9) ? reader.GetString(9) : null,
                                    SalespersonID = !reader.IsDBNull(10) ? reader.GetString(10) : null
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

            return customers;
        }
    }
}
