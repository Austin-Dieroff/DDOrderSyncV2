/***
File: SettingsViewModel.cs
9/18/2019 1:23:20 PM
Author: DESKTOP-MTOGLKV\Mitchell.Street
***/

namespace DDOrderSync.ViewModels
{
    using DDOrderSync.DAL;
    using DDOrderSync.Models;
    using DDOrderSync.Views;
    using Newtonsoft.Json;
    using Prism.Commands;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;
    using System.Deployment.Application;
    using System.Reflection;


    /// <summary>
    /// Defines the <see cref="SettingsViewModel" />
    /// </summary>
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        //private string _allianceDBPath;
        /// <summary>
        /// Defines the _modes
        /// </summary>
        private List<string> _modes;

        /// <summary>
        /// Defines the _syncIntervals
        /// </summary>
        private List<string> _syncIntervals;

        private string _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel()
        {
            Mode = Properties.Settings.Default["Mode"].ToString();
            AllianceDBPath = Properties.Settings.Default["AllianceDBPath"].ToString();
            SyncInterval = Properties.Settings.Default["SyncInterval"].ToString();

            Modes = new List<string>()
            {
                "Test",
                "Production"
            };

            SyncIntervals = new List<string>()
            {
                "1",
                "2",
                "5",
                "10",
                "15",
                "30",
                "60"
            };

            Version = "test";
            _version = "V " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        /// <summary>
        /// Gets or sets the Version
        /// </summary>
        public string Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }

        /// <summary>
        /// Gets or sets the Mode
        /// </summary>
        public string Mode
        {
            get
            {
                return Properties.Settings.Default["Mode"].ToString();
            }
            set
            {
                Properties.Settings.Default["Mode"] = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets the Modes
        /// </summary>
        public List<string> Modes
        {
            get { return _modes; }
            set { _modes = value; }
        }

        /// <summary>
        /// Gets or sets the SyncIntervals
        /// </summary>
        public List<string> SyncIntervals
        {
            get { return _syncIntervals; }
            set { _syncIntervals = value; }
        }

        /// <summary>
        /// Gets or sets the SyncInterval
        /// </summary>
        public string SyncInterval
        {
            get
            {
                return Properties.Settings.Default["SyncInterval"].ToString();
            }
            set
            {
                Properties.Settings.Default["SyncInterval"] = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets the AllianceDBPath
        /// </summary>
        public string AllianceDBPath
        {
            get
            {
                return Properties.Settings.Default["AllianceDBPath"].ToString();
            }
            set
            {
                Properties.Settings.Default["AllianceDBPath"] = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets the SyncCodesCommand
        /// </summary>
        public ICommand SyncCodesCommand
        {
            get { return new DelegateCommand<object>(SyncCodes, CanSyncCodes); }
        }

        /// <summary>
        /// The SyncCodes
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        private async void SyncCodes(object context)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Sync Codes Clicked!");

            Firebase firebase = new Firebase();

            try
            {
                await firebase.SaveToFirebaseAsync("currencyCodes", CurrencyCode.GetValuesTyped());
                await firebase.SaveToFirebaseAsync("departmentCodes", DepartmentCode.GetValuesTyped());
                await firebase.SaveToFirebaseAsync("fobCodes", FOBCode.GetValuesTyped());
                await firebase.SaveToFirebaseAsync("regionCodes", RegionCode.GetValuesTyped());
                await firebase.SaveToFirebaseAsync("salespeople", Salesperson.GetValuesTyped());
                await firebase.SaveToFirebaseAsync("shipViaCodes", ShipViaCode.GetValuesTyped());
                await firebase.SaveToFirebaseAsync("termsCodes", TermsCode.GetValuesTyped());
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error: " + ex.ToString());
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Error: " + ex.ToString());
            }
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Sync Codes Complete");
        }

        /// <summary>
        /// The CanSyncCodes
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool CanSyncCodes(object context)
        {
            return true;
        }

        /// <summary>
        /// Gets the ManualSyncCommand
        /// </summary>
        public ICommand ManualSyncCommand
        {
            get { return new DelegateCommand<object>(ManualSync, CanManualSync); }
        }

        /// <summary>
        /// The ManualSync
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        private async void ManualSync(object context)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Manual Sync Clicked: " + context);

            Firebase firebase = new Firebase();

            switch (context)
            {
                case "currencyCodes":
                    await firebase.SaveToFirebaseAsync("currencyCodes", CurrencyCode.GetValuesTyped());
                    break;
                case "departmentCodes":
                    await firebase.SaveToFirebaseAsync("departmentCodes", DepartmentCode.GetValuesTyped());
                    break;
                case "fobCodes":
                    await firebase.SaveToFirebaseAsync("fobCodes", FOBCode.GetValuesTyped());
                    break;
                case "parts":
                    await firebase.SaveToFirebaseAsync("parts", Part.GetValuesTyped());
                    break;
                case "regionCodes":
                    await firebase.SaveToFirebaseAsync("regionCodes", RegionCode.GetValuesTyped());
                    break;
                case "salespeople":
                    await firebase.SaveToFirebaseAsync("salespeople", Salesperson.GetValuesTyped());
                    break;
                case "shipViaCodes":
                    await firebase.SaveToFirebaseAsync("shipViaCodes", ShipViaCode.GetValuesTyped());
                    break;
                case "termsCodes":
                    await firebase.SaveToFirebaseAsync("termsCodes", TermsCode.GetValuesTyped());
                    break;
                default:
                    return;
            }

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "context: " + context);

            return;
        }

        /// <summary>
        /// The CanManualSync
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool CanManualSync(object context)
        {
            return true;
        }

        /// <summary>
        /// Gets the RetrieveOrdersCommand
        /// </summary>
        public ICommand RetrieveOrdersCommand
        {
            get { return new DelegateCommand<object>(RetrieveOrdersAsync, CanRetrieveOrders); }
        }

        /// <summary>
        /// The RetrieveOrdersAsync
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        private async void RetrieveOrdersAsync(object context)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Retrieve Orders Clicked!");

            Firebase firebase = new Firebase();
            List<Order> orders = await firebase.GetApprovedOrders();

            foreach (Order order in orders)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Order: " + JsonConvert.SerializeObject(order));
                Alliance.AddSalesOrderToAlliance(order);



                //Add code to insert order in archivedOrders collection
                await firebase.CreateArchivedOrder(order);
                //Add code to remove order from approvedOrders collection
                await firebase.DeleteApprovedOrder(order.OrderId);
            }
        }

        /// <summary>
        /// The CanRetrieveOrders
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool CanRetrieveOrders(object context)
        {
            return true;
        }

        /// <summary>
        /// Gets the TestOrderCommand
        /// </summary>
        public ICommand TestOrderCommand
        {
            get { return new DelegateCommand<object>(CreateTestOrder, CanCreateTestOrder); }
        }

        /// <summary>
        /// The CreateTestOrder
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        private void CreateTestOrder(object context)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Create Test Sales Order Clicked!");
        }

        /// <summary>
        /// The CanCreateTestOrder
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool CanCreateTestOrder(object context)
        {
            return true;
        }

        /// <summary>
        /// Gets the ViewLogCommand
        /// </summary>
        public ICommand ViewLogCommand
        {
            get { return new DelegateCommand<object>(ViewLog, CanViewLog); }
        }

        /// <summary>
        /// The ViewLog
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        private void ViewLog(object context)
        {
            //Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "View Log Clicked!");
            Log log = new Log();
            //log.LogTextBox = App.textBoxOutputter.GetTxtBox();
            log.LogTextBox.Text = App.textBoxOutputter.GetTxtBox().Text;
            log.ShowDialog();
        }

        /// <summary>
        /// The CanViewLog
        /// </summary>
        /// <param name="context">The context<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool CanViewLog(object context)
        {
            return true;
        }

        /// <summary>
        /// Defines the PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The RaisePropertyChanged
        /// </summary>
        /// <returns>The <see cref="Action{PropertyChangedEventArgs}"/></returns>
        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }
}
