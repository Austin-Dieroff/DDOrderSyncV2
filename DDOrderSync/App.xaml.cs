/****
File: App.xaml.cs
9/3/2019 12:53:49 PM
Author: DESKTOP-MTOGLKV\Mitchell.Street
****/

namespace DDOrderSync
{
    using DDOrderSync.DAL;
    using DDOrderSync.Models;
    using DDOrderSync.ViewModels;
    using DDOrderSync.Views;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Defines the _notifyIcon
        /// </summary>
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        /// <summary>
        /// Defines the _isExit
        /// </summary>
        private bool _isExit;

        /// <summary>
        /// Defines the _backgroundWorker
        /// </summary>
        //private BackgroundWorker _backgroundWorker;

        /// <summary>
        /// Defines the textBoxOutputter
        /// </summary>
        public static TextBoxOutputter textBoxOutputter;

        /// <summary>
        /// The OnStartup
        /// </summary>
        /// <param name="e">The e<see cref="StartupEventArgs"/></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Something else");
            textBoxOutputter = new TextBoxOutputter();
            Console.WriteLine("Setting console output to TextBoxOutputter class");
            Console.SetOut(textBoxOutputter);
            Console.WriteLine("Started");

            try
            {
                base.OnStartup(e);
                MainWindow = new Settings();
                SettingsViewModel settingsViewModel = new SettingsViewModel();
                MainWindow.DataContext = settingsViewModel;
                MainWindow.Closing += MainWindow_Closing;

                _notifyIcon = new System.Windows.Forms.NotifyIcon();
                _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
                _notifyIcon.Icon = DDOrderSync.Properties.Resources.white_motor_logo_xOx_icon;
                _notifyIcon.Visible = true;

                CreateContextMenu();

                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Registering background worker...");
                //_backgroundWorker = new BackgroundWorker();
                //_backgroundWorker.WorkerSupportsCancellation = true;
                //_backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(_backgroundWorker_DoWorkAsync);
                //_backgroundWorker.RunWorkerAsync();
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Background worker registration complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// The CreateContextMenu
        /// </summary>
        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.Text = "D&D Order Sync";
            _notifyIcon.ContextMenuStrip.Items.Add("Settings").Click += (s, e) => ShowMainWindow();
            //_notifyIcon.ContextMenuStrip.Items.Add("Test").Click += (s, e) => Test();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        /// <summary>
        /// The ExitApplication
        /// </summary>
        private void ExitApplication()
        {
            //TODO: Add code to support bgworker cancellation
            // Cancel BackgroundWorker
            //if (!_backgroundWorker.IsBusy)
                //_backgroundWorker.CancelAsync();

            _isExit = true;
            MainWindow.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        /// <summary>
        /// The ShowMainWindow
        /// </summary>
        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        /// <summary>
        /// The MainWindow_Closing
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="CancelEventArgs"/></param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }

        /// <summary>
        /// The _backgroundWorker_DoWork
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="DoWorkEventArgs"/></param>
        private async void _backgroundWorker_DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Running background process...");
            try
            {
                //If sync interval is not set
                if (String.IsNullOrWhiteSpace(DDOrderSync.Properties.Settings.Default["SyncInterval"].ToString()))
                {
                    DDOrderSync.Properties.Settings.Default["SyncInterval"] = "1";
                    DDOrderSync.Properties.Settings.Default.Save();
                }

                while (true)
                {
                    Firebase firebase = new Firebase();
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Retrieving orders from Firebase...");

                    List<Order> orders = await firebase.GetApprovedOrders();

                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + $"Retrieved {orders.Count} orders");

                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Adding orders to Alliance...");
                    for(int x=0; x < orders.Count; x++ )
                    {
                        Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + 
                            $"Adding order #{x} to Alliance: " + JsonConvert.SerializeObject(orders[x]));
                        if (DDOrderSync.Properties.Settings.Default["Mode"].ToString() == "Production")
                        {
                            Alliance.AddSalesOrderToAlliance(orders[x]);
                            //Add code to insert order in archivedOrders collection
                            await firebase.CreateArchivedOrder(orders[x]);
                            //Add code to remove order from approvedOrders collection
                            await firebase.DeleteApprovedOrder(orders[x].OrderId);
                        }
                        else
                        {
                            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Skipping adding to Alliance because test mode");
                            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Skipping creating order in Firebase archivedOrders because test mode");
                            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Skipping removing order in Firebase approvedOrders because test mode");
                        }
                    }

                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Finished adding orders to Alliance");

                    //Console.WriteLine("Orders: " + JsonConvert.SerializeObject(orders));

                    //Contact contact = new Contact();
                    //contact.Name = "Mitchell";
                    //contact.Email = "test@inblackwell.com";
                    //contact.ContactId = "000002";

                    //Address address = new Address();
                    //address.AddressLine1 = "199 Lein Road";
                    //address.City = "Buffal";
                    //address.State = "NY";
                    //address.ZipCode = "14224";

                    //Order order = new Order();
                    //order.Date = "9/10/2019";
                    //CustomerDetails customerDetails = new CustomerDetails();
                    //order.CustomerDetails = customerDetails;
                    //OrderDetails orderDetails = new OrderDetails();
                    //order.OrderDetails = orderDetails;
                    //Contact contact = new Contact();
                    //contact.Name = "Mitchell Street";
                    //Address shipToAddress = new Address();
                    //shipToAddress.AddressLine1 = "123 West Main";
                    //shipToAddress.City = "Kzoo";
                    //shipToAddress.State = "MI";
                    //shipToAddress.ZipCode = "99999";
                    ////shipToAddress.AddressLine1 = "199 Lein Road";
                    //order.ShipToContact = contact;
                    //order.ShipToAddress = shipToAddress;

                    //OrderLine orderLine = new OrderLine();
                    //orderLine.CustomerLine = "1";
                    //orderLine.PartNumber = "testpn";
                    //orderLine.Qty = "1";
                    //orderLine.Price = "2";

                    //List<OrderLine> orderLines = new List<OrderLine>();
                    //orderLines.Add(orderLine);
                    //Alliance.InsertSalesOrderLines("SO14002256", orderLines);
                    //Console.WriteLine("Done");
                    //Console.WriteLine("Result: " + Alliance.InsertSalesOrderLines("SO14002256",orderLines));

                    //Models.Order order = new Models.Order();
                    //Models.Contact contact = new Models.Contact();
                    //contact.Name = "delavan Industries";
                    //Models.Address shipToAddress = new Models.Address();
                    //shipToAddress.AddressLine1 = "250 Lake Road";
                    //shipToAddress.City = "Blasdell";
                    //shipToAddress.State = "NY";
                    //shipToAddress.ZipCode = "14219";
                    ////shipToAddress.AddressLine1 = "199 Lein Road";
                    //order.ShipToContact = contact;
                    //order.ShipToAddress = shipToAddress;
                    ////bool result = Alliance.DoesAddressExist("1200",shipToAddress);
                    ////string result = Alliance.DoesCustomerExist(order);
                    //Console.WriteLine("Result: " + Alliance.HighestSONumber());


                    //Put Thread to sleep until duration of sync interval passes
                    Thread.Sleep(int.Parse(DDOrderSync.Properties.Settings.Default["SyncInterval"].ToString()) * 1000 * 60);

                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Background process execution cycle complete");
                }
                //ConsoleManager.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Error: " + ex.ToString());
                Thread.Sleep(1 * 1000 * 60);  //Default sleep set to 1 min
            }
        }

        private void Test()
        {
            //_backgroundWorker.CancelAsync();


            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);

            //List<string> customerIds = new List<string>() { "nothing", "test", "3nine", "5", "11" };
            List<string> soNumbers = new List<string>() { "SOS14002259", "SO14002258", "SO14002259" };

            Console.WriteLine("Highest SONumber: " + Alliance.HighestSONumber());
            
        }
    }
}
