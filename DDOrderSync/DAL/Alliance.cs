/***
File: Alliance.cs
9/3/2019 11:48:10 AM
Author: Mitchell.Street
***/

namespace DDOrderSync.DAL
{
    using DDOrderSync.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Diagnostics;

    /// <summary>
    /// Defines the <see cref="Alliance" />
    /// </summary>
    internal class Alliance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Alliance"/> class.
        /// </summary>
        public Alliance()
        {
        }

        /// <summary>
        /// The AddSalesOrderToAlliance
        /// </summary>
        /// <param name="order">The order<see cref="Order"/></param>
        public static void AddSalesOrderToAlliance(Order order)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.AddSalesOrderToAlliance...");

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Checking...");

            string billToAddressID, shipToAddressID, billToContactID, shipToContactID;

            //Does Customer Exist
            //DEBUG
            string findcustomerresults = FindCustomer(order);
            Debug.WriteLine($"[{DateTime.Now.ToString("s")}] " + "findcustomerresults: " + findcustomerresults);


            int insertcustomerresults = 0;
            if (findcustomerresults == null)
            {
                insertcustomerresults = InsertCustomer(order);
                Debug.WriteLine($"[{DateTime.Now.ToString("s")}] " + "insertcustomerresults: " + insertcustomerresults);
            }

            //string customerID = FindCustomer(order) ?? InsertCustomer(order).ToString();

            string customerID = findcustomerresults ?? insertcustomerresults.ToString();
            Debug.WriteLine($"[{DateTime.Now.ToString("s")}] " + "customerID: " + customerID);


            //Find or Insert addresses and contacts
            shipToAddressID = FindAddress(customerID, order.ShipToAddress) ?? InsertAddress(customerID, order.ShipToAddress);
            Debug.WriteLine($"shipToAddressID {shipToAddressID}, billToContactID, shipToContactID");

            billToAddressID = FindAddress(customerID, order.BillToAddress) ?? InsertAddress(customerID, order.BillToAddress);
            Debug.WriteLine($"billToAddressID {billToAddressID}, shipToAddressID {shipToAddressID}, billToContactID, shipToContactID");

            shipToContactID = FindContact(customerID, shipToAddressID, order.ShipToContact) ?? InsertContact(customerID, shipToAddressID, order.ShipToContact);
            Debug.WriteLine($"billToAddressID {billToAddressID}, shipToAddressID {shipToAddressID}, billToContactID, shipToContactID {shipToContactID}");

            billToContactID = FindContact(customerID, billToAddressID, order.BillToContact) ?? InsertContact(customerID, billToAddressID, order.BillToContact);
            Debug.WriteLine($"billToAddressID {billToAddressID}, shipToAddressID {shipToAddressID}, billToContactID {billToContactID}, shipToContactID {shipToContactID}");

            //Insert Sales Order
            string soNumber = InsertSalesOrder(customerID, billToAddressID, shipToAddressID, billToContactID, shipToContactID, order);

            //Insert Sales Order Lines
            InsertSalesOrderLines(soNumber, order.Lines, DateTime.Parse(order.ShipDate).Date);

            Console.WriteLine("TEST VALUE" + DateTime.Parse(order.ShipDate).ToString());

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Successfully inserted order {0} into Alliance.", order.OrderId);

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.AddSalesOrderToAlliance complete");
        }

        /// <summary>
        /// The InsertSalesOrderLines
        /// </summary>
        /// <param name="soNumber">The soNumber<see cref="string"/></param>
        /// <param name="orderLines">The orderLines<see cref="List{OrderLine}"/></param>
        public static void InsertSalesOrderLines(string soNumber, List<OrderLine> orderLines, DateTime scheduledShipDate)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.InsertSalesOrderLines...");

            for (int x = 1; x <= orderLines.Count; x++)
            {
                InsertSalesOrderLine(soNumber, x, orderLines[x - 1], scheduledShipDate);
            }

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.InsertSalesOrderLines complete");
        }

        /// <summary>
        /// The InsertSalesOrderLine
        /// </summary>
        /// <param name="soNumber">The soNumber<see cref="string"/></param>
        /// <param name="lineNumber">The lineNumber<see cref="int"/></param>
        /// <param name="orderLine">The orderLine<see cref="OrderLine"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string InsertSalesOrderLine(string soNumber, int lineNumber, OrderLine orderLine, DateTime scheduledShipDate)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.InsertSalesOrderLine...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT into SODetail ([SONumber],[SOLine],[CustomerLine],[TaxableFlag],[PartNumber],[PartXReference]" +
                        ",[QuantityOrdered],[QuantityShipped],[QuantityReturned],[ScheduledShipDate],[CustomerPrice],[SalesUOM],[Notes],[UserDefined1]) values (?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    cmd.Parameters.AddWithValue("@SONumber", soNumber);
                    cmd.Parameters.AddWithValue("@SOLine", lineNumber.ToString("D3"));
                    cmd.Parameters.AddWithValue("@CustomerLine", DBNullCheck(orderLine.CustomerLine, 5));
                    //cmd.Parameters.AddWithValue("@TaxableFlag", orderLine.Taxable);
                    //cmd.Parameters.AddWithValue("@TaxableFlag", false);
                    cmd.Parameters.AddWithValue("@TaxableFlag", DBBoolNullCheck(orderLine.Taxable) ); 
                    cmd.Parameters.AddWithValue("@PartNumber", DBNullCheck(orderLine.PartNumber, 30));
                    cmd.Parameters.AddWithValue("@PartXReference", DBNullCheck(orderLine.PartXReference, 40));
                    cmd.Parameters.AddWithValue("@QuantityOrdered", float.Parse(orderLine.Qty));
                    cmd.Parameters.AddWithValue("@QuantityShipped", 0f);
                    cmd.Parameters.AddWithValue("@QuantityReturned", 0f);
                    cmd.Parameters.AddWithValue("@ScheduledShipDate", scheduledShipDate);
                    cmd.Parameters.AddWithValue("@CustomerPrice", float.Parse(orderLine.Total));
                    cmd.Parameters.AddWithValue("@SalesUOM", "EA");
                    cmd.Parameters.AddWithValue("@Notes", DBNullCheck(orderLine.Notes, 255));
                    cmd.Parameters.AddWithValue("@UserDefined1", DBNull.Value);

                    cmd.Connection = connection;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Record has been successfully added to SODetail table");
                }

                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.InsertSalesOrderLine complete");

                return lineNumber.ToString("D3");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Insert sales line error here:" + ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// The InsertSalesOrder
        /// </summary>
        /// <param name="customerID">The customerID<see cref="string"/></param>
        /// <param name="billToAddressID">The billToAddress<see cref="string"/></param>
        /// <param name="shipToAddressID">The shipToAddress<see cref="string"/></param>
        /// <param name="billToContactID">The billToContact<see cref="string"/></param>
        /// <param name="shipToContactID">The shipToContact<see cref="string"/></param>
        /// <param name="order">The order<see cref="Order"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string InsertSalesOrder(string customerID, string billToAddressID, string shipToAddressID, string billToContactID, string shipToContactID, Order order)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.InsertSalesOrder...");

            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    //TODO: Get Customer ID
                    //int customerId = CreateOrRetrieveCustomer(order);
                    string soNumber = "SO" + (Int32.Parse(HighestSONumber().Replace("SO", "")) + 1).ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        


                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "insert into SOHeader ([SONumber],[OrderDate],[CustomerID],[SalesPerson],[TermsCode]," +
                            "[ShipViaCode],[FOBCode],[RequiredDate],[EnteredBy],[Notes],[BillToAddress],[ShipToAddress],[OrderedBy]," +
                            "[CustomerPO],[ClosedFlag],[DepartmentCode],[CurrencyCode],[CurrencyRate],[JobNumber],[RegionCode],[Attention]," +
                            "[BillToContact],[ShipToContact],[Status],[DatePromised]) values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?);";
                            

                        



                        //cmd.CommandText = "insert into SOHeader ([SONumber],[OrderDate],[CustomerID],[SalesPerson],[TermsCode]," +
                        //    "[ShipViaCode],[FOBCode],[RequiredDate],[EnteredBy],[Notes],[BillToAddress],[ShipToAddress]) values (?,?,?,?,?,?,?,?,?,?,?,?)";
                        cmd.Parameters.AddWithValue("@SONumber", soNumber);
                        cmd.Parameters.AddWithValue("@OrderDate", DateTime.Parse(order.Date).Date);
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                        cmd.Parameters.AddWithValue("@SalesPerson", DBNullCheck(order.OrderDetails.Salesperson, 10));
                        cmd.Parameters.AddWithValue("@TermsCode", DBNullCheck(order.CustomerDetails.TermsCode, 10));
                        cmd.Parameters.AddWithValue("@ShipViaCode", DBNullCheck(order.CustomerDetails.ShipViaCode, 10));
                        cmd.Parameters.AddWithValue("@FOBCode", DBNullCheck(order.CustomerDetails.FobCode, 10));
                        cmd.Parameters.AddWithValue("@RequiredDate", DateTime.Parse(order.RequiredDate).Date);
                        cmd.Parameters.AddWithValue("@EnteredBy", "SUPERVISOR");  //Follow up on this, is this a lookup field?
                        cmd.Parameters.AddWithValue("@Notes", DBNullCheck(order.Notes, 255));
                        cmd.Parameters.AddWithValue("@BillToAddress", billToAddressID);
                        cmd.Parameters.AddWithValue("@ShipToAddress", shipToAddressID);
                        cmd.Parameters.AddWithValue("@OrderedBy", DBNullCheck(order.OrderDetails.OrderedBy, 30));
                        cmd.Parameters.AddWithValue("@CustomerPO", DBNullCheck(order.OrderDetails.CustomerPO, 18));
                        cmd.Parameters.AddWithValue("@ClosedFlag", false);
                        cmd.Parameters.AddWithValue("@DepartmentCode", DBNullCheck(order.OrderDetails.DepartmentCode, 10));
                        cmd.Parameters.AddWithValue("@CurrencyCode", DBNullCheck(order.CustomerDetails.CurrencyCode, 10));
                        cmd.Parameters.AddWithValue("@CurrencyRate", DBNullCheck(order.CustomerDetails.CurrencyRate, 10));
                        cmd.Parameters.AddWithValue("@JobNumber", DBNullCheck(order.OrderDetails.JobNumber, 15));
                        cmd.Parameters.AddWithValue("@RegionCode", DBNullCheck(order.CustomerDetails.RegionCode, 10));
                        cmd.Parameters.AddWithValue("@Attention", DBNullCheck(order.OrderDetails.Attention, 30));
                        cmd.Parameters.AddWithValue("@BillToContact", billToContactID);
                        cmd.Parameters.AddWithValue("@ShipToContact", shipToContactID);
                        cmd.Parameters.AddWithValue("@Status", 1);
                        cmd.Parameters.AddWithValue("@DatePromised", DateTime.Parse(order.DatePromised).Date);
                        

                        cmd.Connection = connection;
                        connection.Open();
                        cmd.ExecuteNonQuery();

                        Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "SOHeader record successfully added: " + soNumber);
                        return soNumber;
                    }

                    // Adding Using statement to update the Alliance order with the "shipDate" in Order tray

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
            }

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.InsertSalesOrder complete");

            return null;
        }

        /// <summary>
        /// The HighestSONumber
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public static string HighestSONumber()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.HighestSONumber...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection salesOrderConnection = new OleDbConnection(connectionString))
                {
                    string highestSONumber = null;

                    OleDbCommand salesOrderCommand = new OleDbCommand();
                    //salesOrderCommand.CommandText = "SELECT MAX(SONumber) FROM SOHeader";
                    //salesOrderCommand.Connection = salesOrderConnection;
                    //salesOrderConnection.Open();
                    //string highestSONumber = salesOrderCommand.ExecuteScalar().ToString();

                    salesOrderCommand.CommandText = "SELECT TOP 20 SONumber FROM SOHeader ORDER BY SOHeader_PKey DESC";
                    salesOrderCommand.Connection = salesOrderConnection;
                    salesOrderConnection.Open();
                    OleDbDataReader reader = salesOrderCommand.ExecuteReader();


                    List<string> soNumbers = new List<string>();
                    while (reader.Read())
                    {
                        soNumbers.Add(reader.GetValue(0).ToString());
                        //Console.WriteLine("Value: " + reader.GetValue(0).ToString());
                    }

                    highestSONumber = ParseSONumbers(soNumbers);

                    //Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Highest SONumber ID: " + highestSONumber);
                    return highestSONumber;
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.HighestSONumber complete");

            return null;
        }

        /// <summary>
        /// The ParseSONumbers
        /// </summary>
        /// <param name="soNumbers">The soNumbers<see cref="List{string}"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string ParseSONumbers(List<string> soNumbers)
        {
            int number;

            //Console.WriteLine(soNumbers[0]);
            //Console.WriteLine(soNumbers[0].Substring(2));

            if (soNumbers[0].ToUpper().Contains("SO") && int.TryParse(soNumbers[0].Substring(2), out number))
            {
                return "SO" + number.ToString();
            }
            else
            {
                soNumbers.RemoveAt(0);
                return ParseSONumbers(soNumbers);
            }
        }

        /// <summary>
        /// The UpsertCustomer
        /// </summary>
        /// <param name="order">The order<see cref="Order"/></param>
        /// <returns>The <see cref="int"/></returns>
        //public static int UpsertCustomer(Order order)
        //{
        //    return 0;
        //}

        /// <summary>
        /// The InsertCustomer
        /// </summary>
        /// <param name="order">The order<see cref="Order"/></param>
        /// <returns>The <see cref="int"/></returns>
        public static int InsertCustomer(Order order)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.InsertCustomer...");

            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    int customerID = HighestCustomerID() + 1;

                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "INSERT into Customers ([CustomerID],[CustomerName],[RegionCode],[TermsCode],[ShipViaCode],[FOBCode]" +
                            ",[CurrencyCode],[VATBranchID],[DateAdded],[ActiveFlag],[CreditLimit],[PriceDiscType],[UserDefined1],[SalespersonID]) values (?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                        cmd.Parameters.AddWithValue("@CustomerID", customerID.ToString());
                        cmd.Parameters.AddWithValue("@CustomerName", DBNullCheck(order.ShipToContact.Name, 40));
                        cmd.Parameters.AddWithValue("@RegionCode", DBNullCheck(order.CustomerDetails.RegionCode, 10));
                        cmd.Parameters.AddWithValue("@TermsCode", DBNullCheck(order.CustomerDetails.TermsCode, 10));
                        cmd.Parameters.AddWithValue("@ShipViaCode", DBNullCheck(order.CustomerDetails.ShipViaCode, 10));
                        cmd.Parameters.AddWithValue("@FOBCode", DBNullCheck(order.CustomerDetails.FobCode, 10));
                        cmd.Parameters.AddWithValue("@CurrencyCode", DBNullCheck(order.CustomerDetails.CurrencyCode, 10));
                        cmd.Parameters.AddWithValue("@VATBranchID", DBNullCheck(order.CustomerDetails.VATBranchID, 3));
                        cmd.Parameters.AddWithValue("@DateAdded", DateTime.Today.ToString("d"));
                        cmd.Parameters.AddWithValue("@ActiveFlag", true);
                        cmd.Parameters.AddWithValue("@CreditLimit", "0");
                        cmd.Parameters.AddWithValue("@PriceDiscType", "0");
                        cmd.Parameters.AddWithValue("@UserDefined1", DBNullCheck(order.CustomerDetails.UserDefined1, 10));
                        cmd.Parameters.AddWithValue("@SalespersonID", DBNullCheck(order.OrderDetails.Salesperson, 10));

                        cmd.Connection = connection;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Record has been successfully added to Customers table");
                    }

                    return customerID;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.InsertCustomer complete");

            return 0;
        }

        /// <summary>
        /// The HighestCustomerID
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public static int HighestCustomerID()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.HighestCustomerID...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection customerConnection = new OleDbConnection(connectionString))
                {
                    int highestCustomerID = 0;

                    OleDbCommand customerCommand = new OleDbCommand();
                    //customerCommand.CommandText = "SELECT MAX(CustomerID) FROM Customers WHERE Customers_PKey > 4000";
                    //customerCommand.Connection = customerConnection;
                    //customerConnection.Open();
                    //int highestCustomerID = Int32.Parse(customerCommand.ExecuteScalar().ToString());

                    customerCommand.CommandText = "SELECT TOP 20 CustomerID FROM Customers ORDER BY Customers_PKey DESC";
                    customerCommand.Connection = customerConnection;
                    customerConnection.Open();
                    OleDbDataReader reader = customerCommand.ExecuteReader();


                    List<string> customerIds = new List<string>();
                    while (reader.Read())
                    {
                        customerIds.Add(reader.GetValue(0).ToString());
                        //Console.WriteLine("Value: " + reader.GetValue(0).ToString());
                    }

                    highestCustomerID = ParseCustomerIDs(customerIds);


                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Highest Customer ID: " + highestCustomerID);
                    return highestCustomerID;
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.HighestCustomerID complete");

            return 0;
        }

        /// <summary>
        /// The ParseCustomerIDs
        /// </summary>
        /// <param name="customerIds">The customerIds<see cref="List{string}"/></param>
        /// <returns>The <see cref="int"/></returns>
        private static int ParseCustomerIDs(List<string> customerIds)
        {
            int result = -1;

            if (int.TryParse(customerIds[0], out result))
            {
                return result;
            }
            else
            {
                customerIds.RemoveAt(0);
                return ParseCustomerIDs(customerIds);
            }
        }

        
        



        /// <summary>
        /// The FindCustomer
        /// </summary>
        /// <param name="order">The order<see cref="Order"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string FindCustomer(Order order)
        {
            //TODO: Check this to fix for better customer search/address match

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.FindCustomer...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandText = "SELECT CustomerID, CustomerName FROM Customers WHERE CustomerID = ?";

                    cmd.Parameters.Add("?", OleDbType.BSTR).Value = order.CustomerId;
                    cmd.Connection = connection;
                    connection.Open();
                    OleDbDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + reader.GetString(0) + ", " + reader.GetString(1));
                        string customerID = reader.GetString(0);


                        return customerID;

                    }
                    reader.Close();

                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Error in DoesCustomerExist: " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.FindCustomer complete");

            return null;
        }




        /// <summary>
        /// The InsertAddress
        /// </summary>
        /// <param name="customerID">The customerID<see cref="string"/></param>
        /// <param name="address">The address<see cref="Address"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string InsertAddress(string customerID, Address address)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.InsertAddress...");

            try
            {
                int addressID = CountCustomerAddresses(customerID) + 1;
                //Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "New Address ID: " + addressID);

                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT into CustomerAddress ([CustomerID],[AddressID],[AddressLine1],[AddressLine2],[AddressLine3],[AddressLine4]" +
                        ",[City],[State],[ZIPCode],[Postal],[Country],[ShipToFlag],[BillToFlag]) values (?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);
                    cmd.Parameters.AddWithValue("@AddressID", addressID.ToString());
                    cmd.Parameters.AddWithValue("@AddressLine1", DBNullCheck(address.AddressLine1, 40));
                    cmd.Parameters.AddWithValue("@AddressLine2", DBNullCheck(address.AddressLine2, 40));
                    cmd.Parameters.AddWithValue("@AddressLine3", DBNullCheck(address.AddressLine3, 40));
                    cmd.Parameters.AddWithValue("@AddressLine4", DBNullCheck(address.AddressLine4, 40));
                    cmd.Parameters.AddWithValue("@City", DBNullCheck(address.City, 28));
                    cmd.Parameters.AddWithValue("@State", DBNullCheck(address.State, 3));
                    cmd.Parameters.AddWithValue("@ZIPCode", DBNullCheck(address.ZipCode, 9));
                    cmd.Parameters.AddWithValue("@Postal", DBNullCheck(address.Postal, 10));
                    cmd.Parameters.AddWithValue("@Country", DBNullCheck(address.Country, 10));
                    cmd.Parameters.AddWithValue("@ShipToFlag", true);
                    cmd.Parameters.AddWithValue("@BillToFlag", true);

                    cmd.Connection = connection;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Record has been successfully added to CustomerAddress table");
                }

                return addressID.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.InsertAddress complete");

            return null;
        }

        /// <summary>
        /// The FindAddress
        /// </summary>
        /// <param name="customerID">The customerID<see cref="string"/></param>
        /// <param name="address">The address<see cref="Address"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static string FindAddress(string customerID, Address address)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.FindAddress...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection addressConnection = new OleDbConnection(connectionString))
                {
                    OleDbCommand addressCommand = new OleDbCommand();
                    addressCommand.CommandText = "SELECT AddressID,AddressLine1,AddressLine2,AddressLine3,AddressLine4,City,State,ZIPCode FROM CustomerAddress WHERE CustomerID=?";
                    addressCommand.Parameters.Add("?", OleDbType.BSTR).Value = customerID;
                    addressCommand.Connection = addressConnection;
                    addressConnection.Open();
                    OleDbDataReader addressReader = addressCommand.ExecuteReader();
                    while (addressReader.Read())
                    {
                        //TODO:This doesn't seem to be matching properly
                        //if (address.AddressLine1 == (addressReader.GetValue(1) == DBNull.Value ? null : addressReader.GetString(1)))
                        //    if (address.AddressLine2 == (addressReader.GetValue(2) == DBNull.Value ? null : addressReader.GetString(2)))
                        //        if (address.AddressLine3 == (addressReader.GetValue(3) == DBNull.Value ? null : addressReader.GetString(3)))
                        //            if (address.AddressLine4 == (addressReader.GetValue(4) == DBNull.Value ? null : addressReader.GetString(4)))
                        //                if (address.City == (addressReader.GetValue(5) == DBNull.Value ? null : addressReader.GetString(5)))
                        //                    if (address.State == (addressReader.GetValue(6) == DBNull.Value ? null : addressReader.GetString(6)))
                        //                        if (address.ZipCode == (addressReader.GetValue(7) == DBNull.Value ? null : addressReader.GetString(7)))
                        //                            return addressReader.GetValue(0).ToString();
                        //Console.WriteLine("Alliance >> FindAddress >> addressReader.GetString(1).ToUpper() :" + addressReader.GetString(1).ToUpper());
                        if (address.AddressLine1.ToUpper() == (addressReader.GetValue(1) == DBNull.Value ? null : addressReader.GetString(1).ToUpper()))
                        {
                            Console.WriteLine("Address match found: " + addressReader.GetString(0));
                            return addressReader.GetValue(0).ToString();
                        }
                    }
                    addressReader.Close();
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.FindAddress complete");

            return null;
        }

        /// <summary>
        /// The CountCustomerAddresses
        /// </summary>
        /// <param name="customerID">The customerID<see cref="string"/></param>
        /// <returns>The <see cref="int"/></returns>
        public static int CountCustomerAddresses(string customerID)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.CountCustomerAddresses...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection addressConnection = new OleDbConnection(connectionString))
                {
                    OleDbCommand addressCommand = new OleDbCommand();
                    addressCommand.CommandText = "SELECT COUNT(AddressID) FROM CustomerAddress WHERE CustomerID=?";
                    addressCommand.Parameters.Add("?", OleDbType.BSTR).Value = customerID;
                    addressCommand.Connection = addressConnection;
                    addressConnection.Open();
                    //OleDbDataReader addressReader = (int)addressCommand.ExecuteScalar();
                    //Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Count: " + (int)addressCommand.ExecuteScalar());
                    return (int)addressCommand.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.CountCustomerAddresses complete");

            return 0;
        }

        /// <summary>
        /// The InsertContact
        /// </summary>
        /// <param name="customerID">The customerID<see cref="string"/></param>
        /// <param name="addressID">The addressID<see cref="string"/></param>
        /// <param name="contact">The contact<see cref="Contact"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string InsertContact(string customerID, string addressID, Contact contact)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.InsertContact...");

            try
            {
                string contactID = (CountCustomerContacts(customerID, addressID) + 1).ToString("D6");

                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT into CustomerContacts ([CustomerID],[AddressID],[ContactID],[Name],[Email],[Phone]" +
                        ",[PhoneFMT],[FAX],[FAXFMT]) values (?,?,?,?,?,?,?,?,?)";
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);
                    cmd.Parameters.AddWithValue("@AddressID", addressID.ToString());
                    cmd.Parameters.AddWithValue("@ContactID", contactID); //TODO: Is this fixed?
                    cmd.Parameters.AddWithValue("@Name", DBNullCheck(contact.Name, 40));
                    cmd.Parameters.AddWithValue("@Email", DBNullCheck(contact.Email, 50));
                    //cmd.Parameters.AddWithValue("@Notes", (object)contact.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", DBNullCheck(contact.Phone1, 30));
                    cmd.Parameters.AddWithValue("@PhoneFMT", DBNullCheck(contact.Phone2, 30));
                    cmd.Parameters.AddWithValue("@FAX", DBNullCheck(contact.Fax1, 30));
                    cmd.Parameters.AddWithValue("@FAXFMT", DBNullCheck(contact.Fax2, 30));

                    cmd.Connection = connection;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Record has been successfully added to SOHeader table");
                }

                return contactID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.InsertContact complete");

            return null;
        }

        /// <summary>
        /// The CountCustomerContacts
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public static int CountCustomerContacts(string customerID, string addressID)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.CountCustomerContacts...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection addressConnection = new OleDbConnection(connectionString))
                {
                    OleDbCommand addressCommand = new OleDbCommand();
                    addressCommand.CommandText = "SELECT COUNT(ContactID) FROM CustomerContacts WHERE CustomerID=? AND AddressID=?";
                    addressCommand.Parameters.Add("?", OleDbType.BSTR).Value = customerID;
                    addressCommand.Parameters.Add("?", OleDbType.BSTR).Value = addressID;
                    addressCommand.Connection = addressConnection;
                    addressConnection.Open();

                    return (int)addressCommand.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.CountCustomerContacts complete");

            return 0;
        }

        /// <summary>
        /// The FindContact
        /// </summary>
        /// <param name="customerID">The customerID<see cref="string"/></param>
        /// <param name="addressID">The addressID<see cref="string"/></param>
        /// <param name="contact">The contact<see cref="Contact"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static string FindContact(string customerID, string addressID, Contact contact)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.FindContact...");

            Console.WriteLine($"customerID: {customerID}, addressID: {addressID}, contact: " + JsonConvert.SerializeObject(contact));

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection contactConnection = new OleDbConnection(connectionString))
                {
                    OleDbCommand contactCommand = new OleDbCommand();
                    contactCommand.CommandText = "SELECT ContactID,Name,Email,Phone FROM CustomerContacts WHERE CustomerID=? AND AddressID=?";
                    contactCommand.Parameters.Add("?", OleDbType.BSTR).Value = customerID;
                    contactCommand.Parameters.Add("?", OleDbType.BSTR).Value = addressID;
                    contactCommand.Connection = contactConnection;
                    contactConnection.Open();
                    OleDbDataReader contactReader = contactCommand.ExecuteReader();
                    while (contactReader.Read())
                    {
                        //Match Name
                        if (contact.Name.ToUpper() == (contactReader.GetValue(1) == DBNull.Value ? null : contactReader.GetString(1).ToUpper()))
                        {
                            //Match Email
                            if (contact.Email.ToUpper() == (contactReader.GetValue(2) == DBNull.Value ? null : contactReader.GetString(2).ToUpper()))
                            {
                                //Match Phone
                                if (contact.Phone1.ToUpper() == (contactReader.GetValue(3) == DBNull.Value ? null : contactReader.GetString(3).ToUpper()))
                                {
                                    Console.WriteLine("Contact match found: " + contactReader.GetString(0));
                                    return contactReader.GetString(0);
                                }
                            }
                        }
                    }
                    contactReader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.FindContact complete");

            return null;
        }

        /// <summary>
        /// The CountCustomerContacts
        /// </summary>
        /// <param name="customerID">The customerID<see cref="string"/></param>
        /// <returns>The <see cref="int"/></returns>
        public static int CountCustomerContacts(string customerID)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Alliance.CountCustomerContacts...");

            try
            {
                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                using (OleDbConnection addressConnection = new OleDbConnection(connectionString))
                {
                    OleDbCommand addressCommand = new OleDbCommand();
                    addressCommand.CommandText = "SELECT COUNT(ContactID) FROM CustomerContacts WHERE CustomerID=?";
                    addressCommand.Parameters.Add("?", OleDbType.BSTR).Value = customerID;
                    addressCommand.Connection = addressConnection;
                    addressConnection.Open();
                    //OleDbDataReader addressReader = (int)addressCommand.ExecuteScalar();
                    //Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Count: " + (int)addressCommand.ExecuteScalar());
                    return (int)addressCommand.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Alliance.CountCustomerContacts complete");

            return 0;
        }

        /// <summary>
        /// The CreateTestWorkOrder
        /// </summary>
        public static void CreateTestWorkOrder()
        {
            if (Properties.Settings.Default["AllianceDBPath"] != null && Properties.Settings.Default["AllianceDBPath"].ToString() != "")
            {
                try
                {
                    string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Properties.Settings.Default["AllianceDBPath"].ToString();

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "insert into SOHeader ([SONumber],[OrderDate],[RequiredDate]) values (?,?,?)";
                        cmd.Parameters.AddWithValue("@SONumber", "Test" + DateTime.Now.ToString("hhmmss"));
                        cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@RequiredDate", DateTime.Now.ToString("MM/dd/yyyy"));

                        //cmd.Parameters.AddWithValue("@PartNumber", "Part Number Test");
                        cmd.Connection = connection;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Record has been successfully added to SOHeader table");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Alliance DB path first!");
            }
        }

        /// <summary>
        /// The DBNullCheck
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="maxLength">The maxLength<see cref="int"/></param>
        /// <returns>The <see cref="object"/></returns>
        private static object DBNullCheck(string value, int maxLength)
        {
            if (String.IsNullOrWhiteSpace(value))
                return DBNull.Value;
            if (value.Length < maxLength)
                return value;

            return value.Substring(0, maxLength);
        }
        private static object DBBoolNullCheck(bool? value)
        {
            if (value == null)
                return false;

            return value;
        }
    }
}
