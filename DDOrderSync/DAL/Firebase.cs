/***
File: Firebase.cs
9/18/2019 1:10:10 PM
Author: DESKTOP-MTOGLKV\Mitchell.Street
***/

namespace DDOrderSync.DAL
{
    using DDOrderSync.Models;
    using Google.Cloud.Firestore;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="Firebase" />
    /// </summary>
    internal class Firebase : IDisposable
    {
        /// <summary>
        /// Gets or sets the _authFileName
        /// </summary>
        private string _authFileName { get; set; }

        /// <summary>
        /// Defines the _projectId
        /// </summary>
        private string _projectId = "order-integration-9400f";

        /// <summary>
        /// Initializes a new instance of the <see cref="Firebase"/> class.
        /// </summary>
        public Firebase()
        {
            SetupEnvironmentVariable();
        }

        /// <summary>
        /// The SetupEnvironmentVariable
        /// </summary>
        private void SetupEnvironmentVariable()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Setting up Firebase environment variable...");
            //Get file name/path in temp directory
            //_authFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            _authFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"order-integration-9400f-firebase-adminsdk-69dgr-08c8e973c9.json");
            //Create temp file from json in project embedded resources
            ResourceReader.CreateFileFromResource("LookupTableCopyTest.Resources.order-integration-9400f-firebase-adminsdk-69dgr-08c8e973c9.json", _authFileName);
            //Set Env variable for use by Google Cloud Firestore library
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _authFileName);
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Firebase environment variable set");
        }

        /// <summary>
        /// The GetApprovedOrders
        /// </summary>
        /// <returns>The <see cref="Task{List{Order}}"/></returns>
        public async Task<List<Order>> GetApprovedOrders()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Firebase.GetApprovedOrders...");
            List<Order> approvedOrders = new List<Order>();
            FirestoreDb db = FirestoreDb.Create(_projectId);
            CollectionReference collection = db.Collection("approvedOrders");
            QuerySnapshot documentSnapshots = await collection.GetSnapshotAsync(); //TODO:Problem is here
            foreach (DocumentSnapshot document in documentSnapshots.Documents)
            {
                // Do anything you'd normally do with a DocumentSnapshot
                //Order order = document.ConvertTo<Order>();

                //Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Document: " + JsonConvert.SerializeObject(document.ToDictionary()));

                //string json = JsonConvert.SerializeObject(document.ToString);
                Order order = JsonConvert.DeserializeObject<Order>(JsonConvert.SerializeObject(document.ToDictionary()));


                approvedOrders.Add(order);
            }

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Firebase.GetApprovedOrders complete");
            return approvedOrders;
        }

        /// <summary>
        /// The DeleteApprovedOrder
        /// </summary>
        /// <param name="orderId">The orderId<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task DeleteApprovedOrder(string orderId)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Firebase.DeleteApprovedOrder...");

            FirestoreDb db = FirestoreDb.Create(_projectId);
            CollectionReference collection = db.Collection("approvedOrders");
            DocumentReference docRef = collection.Document(orderId);
            await docRef.DeleteAsync();

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Firebase.DeleteApprovedOrder complete");

            return;
        }

        /// <summary>
        /// The CreateArchivedOrder
        /// </summary>
        /// <param name="order">The order<see cref="Order"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task CreateArchivedOrder(Order order)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Firebase.CreateArchivedOrder...");

            FirestoreDb db = FirestoreDb.Create(_projectId);
            CollectionReference collection = db.Collection("archivedOrders");
            DocumentReference docRef = collection.Document(order.OrderId);
            //Write to DB

            Debug.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Orders:" + JsonConvert.SerializeObject(order));

            await docRef.SetAsync(order);


            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Firebase.CreateArchivedOrder complete");

            return;
        }

        /// <summary>
        /// The SaveToFirebaseAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">The collectionName<see cref="string"/></param>
        /// <param name="items">The items<see cref="List{T}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SaveToFirebaseAsync<T>(string collectionName, List<T> items)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "In Firebase.SaveToFirebaseAsync...");
            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Collection: " + collectionName);

            FirestoreDb db = FirestoreDb.Create(_projectId);
            CollectionReference collection = db.Collection(collectionName);
            foreach (var item in items)
            {
                //Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + JsonConvert.SerializeObject(item));

                PropertyInfo propertyInfo = item.GetType().GetProperties()[1];
                DocumentReference docRef = collection.Document(propertyInfo.GetValue(item).ToString());

                //Write to DB
                try
                {
                    await docRef.SetAsync(item);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Error writing doc to db: " + JsonConvert.SerializeObject(item));
                    Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Error: " + ex);
                }
            }

            Console.WriteLine($"[{DateTime.Now.ToString("s")}] " + "Firebase.SaveToFirebaseAsync complete");
        }

        /// <summary>
        /// Defines the _disposedValue
        /// </summary>
        private bool _disposedValue = false;// To detect redundant calls

        /// <summary>
        /// The Dispose
        /// </summary>
        /// <param name="disposing">The disposing<see cref="bool"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                //Delete temp json file created for Firebase auth
                System.IO.File.Delete(_authFileName);

                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Firebase()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// The Dispose
        /// </summary>
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
