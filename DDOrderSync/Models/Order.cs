using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOrderSync.Models
{
    [FirestoreData]
    public class Order
    {
        [FirestoreDocumentId]
        public DocumentReference Reference { get; set; }

        [FirestoreProperty("orderId")]
        public string OrderId { get; set; }

        [FirestoreProperty("salesChannel")]
        public string SalesChannel { get; set; }

        [FirestoreProperty("customerId")]
        public string CustomerId { get; set; }

        [FirestoreProperty("date")]
        public string Date { get; set; }

        [FirestoreProperty("shipDate")]
        public string ShipDate { get; set; }

        [FirestoreProperty("dateRequired")]
        public string DateRequired { get; set; }

        public string RequiredDate { get { return this.DateRequired; } }

        [FirestoreProperty("datePromised")]
        public string DatePromised { get; set; }

        [FirestoreProperty("notes")]
        public string Notes { get; set; }

        [FirestoreProperty("orderDetails")]
        //[FirestoreProperty]
        public OrderDetails OrderDetails { get; set; }

        [FirestoreProperty("customerDetails")]
        //[FirestoreProperty]
        public CustomerDetails CustomerDetails { get; set; }

        [FirestoreProperty("billToContact")]
        //[FirestoreProperty]
        public Contact BillToContact { get; set; }

        [FirestoreProperty("shipToContact")]
        //[FirestoreProperty]
        public Contact ShipToContact { get; set; }

        [FirestoreProperty("billToAddress")]
        //[FirestoreProperty]
        public Address BillToAddress { get; set; }

        [FirestoreProperty("shipToAddress")]
        //[FirestoreProperty]
        public Address ShipToAddress { get; set; }

        [FirestoreProperty("lines")]
        //[FirestoreProperty]
        public List<OrderLine> Lines { get; set; }
    }

    [FirestoreData]
    public class OrderDetails
    {
        [FirestoreProperty("departmentCode")]
        public string DepartmentCode { get; set; }

        [FirestoreProperty("salesperson")]
        public string Salesperson { get; set; }

        [FirestoreProperty("jobNumber")]
        public string JobNumber { get; set; }

        [FirestoreProperty("orderedBy")]
        public string OrderedBy { get; set; }

        [FirestoreProperty("customerPO")]
        public string CustomerPO { get; set; }

        [FirestoreProperty("attention")]
        public string Attention { get; set; }

        [FirestoreProperty("blanketOrderNumber")]
        public string BlanketOrderNumber { get; set; }

        [FirestoreProperty("quoteNumber")]
        public string QuoteNumber { get; set; }

        [FirestoreProperty("orderType")]
        public string OrderType { get; set; }

        [FirestoreProperty("orderTotal")]
        public string OrderTotal { get; set; }

    }

    [FirestoreData]
    public class CustomerDetails
    {
        [FirestoreProperty("termsCode")]
        public string TermsCode { get; set; }

        [FirestoreProperty("regionCode")]
        public string RegionCode { get; set; }

        [FirestoreProperty("fobCode")]
        public string FobCode { get; set; }

        [FirestoreProperty("shipViaCode")]
        public string ShipViaCode { get; set; }

        [FirestoreProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [FirestoreProperty("currencyRate")]
        public string CurrencyRate { get; set; }

        [FirestoreProperty("vatBranchID")]
        public string VATBranchID { get; set; }
        
        [FirestoreProperty("userDefined1")]
        public string UserDefined1 { get; set; }
    }

    [FirestoreData]
    public class Contact
    {
        [FirestoreProperty("addressId")]
        public string AddressId { get; set; }

        [FirestoreProperty("contactId")]
        public string ContactId { get; set; }

        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("phone1")]
        public string Phone1 { get; set; }

        [FirestoreProperty("phone2")]
        public string Phone2 { get; set; }

        [FirestoreProperty("fax1")]
        public string Fax1 { get; set; }

        [FirestoreProperty("fax2")]
        public string Fax2 { get; set; }

        [FirestoreProperty("email")]
        public string Email { get; set; }
    }


    [FirestoreData]
    public class Address
    {
        [FirestoreProperty("addressId")]
        public string AddressId { get; set; }

        [FirestoreProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [FirestoreProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [FirestoreProperty("addressLine3")]
        public string AddressLine3 { get; set; }

        [FirestoreProperty("addressLine4")]
        public string AddressLine4 { get; set; }

        [FirestoreProperty("city")]
        public string City { get; set; }

        [FirestoreProperty("state")]
        public string State { get; set; }

        [FirestoreProperty("country")]
        public string Country { get; set; }

        [FirestoreProperty("zipCode")]
        public string ZipCode { get; set; }

        [FirestoreProperty("postal")]
        public string Postal { get; set; }

        [FirestoreProperty("taxCode")]
        public string TaxCode { get; set; }
    }

    [FirestoreData]
    public class OrderLine
    {
        [FirestoreProperty("partXReference")]
        public string PartXReference { get; set; }

        [FirestoreProperty("partNumber")]
        public string PartNumber { get; set; }

        [FirestoreProperty("qty")]
        public string Qty { get; set; }

        [FirestoreProperty("price")]
        public string Price { get; set; }

        [FirestoreProperty("total")]
        public string Total { get; set; }

        [FirestoreProperty("tax")]
        public string Tax { get; set; }

        [FirestoreProperty("taxable")]
        public bool? Taxable { get; set; }

        [FirestoreProperty("uom")]
        public string UOM { get; set; }

        [FirestoreProperty("customerLine")]
        public string CustomerLine { get; set; }

        [FirestoreProperty("productClassCode")]
        public string ProductClassCode { get; set; }

        [FirestoreProperty("notes")]
        public string Notes { get; set; }
    }
}
