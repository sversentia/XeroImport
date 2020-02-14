using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Model;
using XeroOAuth2Sample.Example;
using XeroOAuth2Sample.Extensions;
using XeroOAuth2Sample.Models;

namespace XeroOAuth2Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly MemoryTokenStore _tokenStore;
        private readonly IXeroClient _xeroClient;
        private readonly IAccountingApi _accountingApi;
        //private Guid? accid = new Guid();

        public HomeController(MemoryTokenStore tokenStore, IXeroClient xeroClient, IAccountingApi accountingApi)
        {
            _tokenStore = tokenStore;
            _xeroClient = xeroClient;
            _accountingApi = accountingApi;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("OutstandingInvoices");
            }
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OutstandingInvoices()
        {
            var token = await _tokenStore.GetAccessTokenAsync(User.XeroUserId());
            var connections = await _xeroClient.GetConnectionsAsync(token);
            if (!connections.Any())
            {
                return RedirectToAction("NoTenants");
            }
            var data = new Dictionary<string, int>();
            foreach (var connection in connections)
            {
                //Console.WriteLine("Connection ID :"+connection.id);
                var accessToken = token.AccessToken;
                var refreshToken = token.RefreshToken;
                var tenantId = connection.TenantId.ToString();
                var organisations = await _accountingApi.GetOrganisationsAsync(accessToken, tenantId);
                var accounts = await _accountingApi.GetAccountsAsync(accessToken, tenantId);
                for (int i = 0; i < organisations._Organisations.Count(); i++)
                {
                    await ImportContactDetails(accessToken, tenantId);
                    await FetchInvoicesFromDb(accessToken, tenantId);
                }
            }/*
                for(int i = 0; i < organisations._Organisations.Count(); i++)
                {                    
                    Console.WriteLine("\nOrganisation #"+i+" is "+organisations._Organisations[i].Name);
                    Console.WriteLine("\nImport for this organisation ? Press Y to confirm or any other button to continue..");
                    char input = Console.ReadKey().KeyChar;
                    if(input.ToString().ToUpper() == "Y" )
                    {
                        await ImportContactDetails(accessToken, tenantId);
                        await FetchInvoicesFromDb(accessToken, tenantId);
                        goto Foo;
                    }
                }                                
            }
            Foo:*/
            var model = new OutstandingInvoicesViewModel
            {
                Name = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}",
                Data = data
            };
            return View(model);
        }

        private async Task CreateItems(string accessToken, string tenantId)
        {
            string connectionString = "Data Source=data.versentia.com;Initial Catalog=Versentia;User ID=VersentiaUser;Password=toomanyhands";
            SqlConnection cnn = new SqlConnection(connectionString);
            cnn.Open();
            string sql = "SELECT TOP 1000 [FeeID], LEFT([Name],45) AS Name FROM [Versentia].[Billing].[Fees]";
            SqlCommand command = new SqlCommand(sql, cnn);
            SqlDataReader dataReader = command.ExecuteReader();
            var itemspresent = await _accountingApi.GetItemsAsync(accessToken, tenantId);
            HashSet<string> mySet = new HashSet<string>();
            foreach (var itemcode in itemspresent._Items)
            {
                mySet.Add(itemcode.Code);
            }

            while (dataReader.Read())
            {
                Item item = new Item()
                {
                    Name = dataReader.GetString(1),
                    Code = dataReader.GetInt32(0).ToString()
                };

                if (!mySet.Contains(dataReader.GetInt32(0).ToString()))
                {
                    try
                    {
                        //await _accountingApi.CreateItemAsync(accessToken, tenantId, item);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(item.Code + " " + item.Name);
                        Console.WriteLine(e.Message);
                    }
                }
            }
            dataReader.Close();
        }

        public static Guid ToGuid(int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        [HttpGet]
        [Authorize]
        public IActionResult NoTenants()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddConnection()
        {
            // Signing out of this client app allows the user to be taken through the Xero Identity connection flow again, allowing more organisations to be connected
            // The user will not need to log in again because they're only signed out of our app, not Xero.
            await HttpContext.SignOutAsync();
            return RedirectToAction("SignUp");
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "XeroSignUp")]
        public IActionResult SignUp()
        {
            return RedirectToAction("OutstandingInvoices");
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "XeroSignIn")]
        public IActionResult SignIn()
        {
            return RedirectToAction("OutstandingInvoices");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private async Task FetchInvoicesFromDb(string accessToken, string tenantId)
        {
            Console.WriteLine("Importing Invoices into Xero");
            string connectionString = "Data Source=data.versentia.com;Initial Catalog=Versentia;User ID=VersentiaUser;Password=toomanyhands";
            SqlConnection cnn = new SqlConnection(connectionString);
            cnn.Open();
            //string sql = "SELECT TOP 500 BI.InvoiceID,BP.Amount,BI.ClientID, BI.ProducedDate,BI.OverdueDate, AC.Name, AC.AddressLine1, BIP.Amount, VBI.Amount, VBI.Name  FROM [Versentia].[Billing].[Invoices] BI INNER JOIN [Versentia].[Billing].[InvoicePayments] BIP ON BI.InvoiceID = BIP.InvoiceID INNER JOIN [Versentia].[Billing].[Payments] BP ON BIP.PaymentID = BP.PaymentID INNER JOIN Versentia.Accounts.Clients AC ON BI.ClientID = AC.ClientID INNER JOIN [Versentia].[Billing].[InvoiceFees] VBI ON VBI.InvoiceID = BI.InvoiceID ORDER BY BI.InvoiceID";
            string sql = "SELECT VBI.InvoiceID, VBI.InvoiceSerialNumber, VBI.ClientID, VBI.ProducedOn, VBI.TotalFee, VAC.AddressLine1, VAC.Name FROM [Versentia].[Billing].[InvoiceDetails] VBI INNER JOIN [Versentia].[Accounts].[Clients] VAC ON VBI.ClientID = VAC.ClientID Where VBI.IsPaid = 0 AND VBI.ProducedDate >= CONVERT(datetime,'2019-04-01') ORDER BY VBI.ProducedOn";
            SqlCommand command = new SqlCommand(sql, cnn);
            SqlDataReader dataReader = command.ExecuteReader();
            int invoiceID = 0;
            var lineitems = new List<LineItem>();
            var invoice = new Invoice();
            Invoices batchInvoices = new Invoices();
            batchInvoices._Invoices = new List<Invoice>();
            LineItem lineitem = new LineItem();
            var invoicespresent = await _accountingApi.GetInvoicesAsync(accessToken, tenantId);
            while (dataReader.Read())
            {
                invoice = new Invoice
                {
                    Type = Invoice.TypeEnum.ACCREC,
                    Contact = new Contact
                    {
                        Name = dataReader.GetString(6),
                        ContactNumber = dataReader.GetInt32(2).ToString(),
                        Addresses = new List<Address>
                        {
                            new Address
                            {
                                AddressLine1 = dataReader.GetString(5)
                            }
                        }
                    },
                    Date = dataReader.GetDateTime(3),
                    DueDate = dataReader.GetDateTime(3).AddDays(30),
                    InvoiceNumber = "N" + dataReader.GetInt32(1).ToString(),
                    ExpectedPaymentDate = dataReader.GetDateTime(3).AddDays(30),
                    Status = Invoice.StatusEnum.DRAFT,
                    LineItems = new List<LineItem>()
                    {
                        new LineItem
                        {
                            Description = "Inspection Services",
                            Quantity = 1,                          
                            UnitAmount = (double)dataReader.GetDecimal(4),                            
                            AccountCode = "400",
                            TaxType = "NONE"
                        }
                    }
                };
                if(invoicespresent._Invoices.FindIndex(x => x.InvoiceNumber == "N"+dataReader.GetInt32(1).ToString()) > 0)
                {
                    Console.WriteLine("Invoice with invoice number N" + dataReader.GetInt32(1).ToString()+" already present in Xero\n");
                }
                else
                {
                    Console.WriteLine("\nImporting invoice N" + dataReader.GetInt32(1).ToString());
                    batchInvoices._Invoices.Add(invoice);
                }
                
            }
            Invoices partialInvoices = new Invoices();
            partialInvoices._Invoices = new List<Invoice>();
            for(int i = 0; i < batchInvoices._Invoices.Count(); i++)
            {
                partialInvoices._Invoices.Add(batchInvoices._Invoices[i]);
                if (i % 200 == 0)
                {
                    try 
                    { 
                        var createbatchinvoices = await _accountingApi.CreateInvoicesAsync(accessToken, tenantId, partialInvoices, false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message+"\n");
                    }
                    partialInvoices._Invoices.Clear();
                }
            }
            try
            {
                var createbatchinvoices = await _accountingApi.CreateInvoicesAsync(accessToken, tenantId, partialInvoices, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message+"\n");
            }
            dataReader.Close();
            Console.WriteLine("Import invoice function completed");
        }        


        private async Task ImportContactDetails(string accessToken, string tenantId)
        {
            Console.WriteLine("Imporing contacts into Xero");
            Contacts batchContacts = new Contacts();
            batchContacts._Contacts = new List<Contact>();
            string connectionString = "Data Source=data.versentia.com;Initial Catalog=Versentia;User ID=VersentiaUser;Password=toomanyhands";
            SqlConnection cnn = new SqlConnection(connectionString);
            cnn.Open();
            //string sql = "SELECT TOP 10 [ClientID],[Name],[PaperLessEmail],[AddressLine1],[City],[State],[Zip] FROM[Versentia].[Accounts].[Clients]";
            string sql = "select TOP 500 VAC.ClientID, VAC.Name, VAC1.FirstName, VAC1.LastName, VAC1.Email, VAC1.AddressLine1, VAC1.AddressLine2, VAC1.City, VAC1.State, VAC1.Zip from [Versentia].[Accounts].[Clients] VAC INNER JOIN [Versentia].[Accounts].[ClientContacts] VACC ON VAC.ClientID = VACC.ClientID INNER JOIN [Versentia].[Accounts].[Contacts] VAC1 ON VACC.ContactID = VAC1.ContactID ORDER BY VAC.ClientID";
            SqlCommand command = new SqlCommand(sql, cnn);
            SqlDataReader dataReader = command.ExecuteReader();

            var contactlist = await _accountingApi.GetContactsAsync(accessToken, tenantId);
            while (dataReader.Read())
            {
                if (contactlist._Contacts.FindIndex(x => x.ContactNumber == dataReader.GetInt32(0).ToString()) > 0)
                {
                    Console.WriteLine("Contact with contact number already present, not importing : \n"+ dataReader.GetInt32(0));
                    continue;
                }
                Contact contact = new Contact
                {                    
                    ContactNumber = dataReader.GetInt32(0).ToString(),
                    Name = dataReader.GetString(1),
                    FirstName = dataReader.IsDBNull(2) ? "" : dataReader.GetString(2),
                    LastName = dataReader.IsDBNull(3) ? "" : dataReader.GetString(3),
                    EmailAddress = dataReader.IsDBNull(4) ? "" : dataReader.GetString(4),
                    Addresses = new List<Address>
                    {
                        new Address
                        {
                            AddressType = Address.AddressTypeEnum.STREET,
                            AddressLine1 = dataReader.GetString(5),
                            AddressLine2 = dataReader.IsDBNull(6) ? "" : dataReader.GetString(6),
                            City = dataReader.IsDBNull(7) ? "" : dataReader.GetString(7),
                            Region = dataReader.IsDBNull(8) ? "" : dataReader.GetString(8),
                            PostalCode = dataReader.IsDBNull(9) ? "" : dataReader.GetString(9)
                        }
                    }

                };
                if (contactlist._Contacts.FindIndex(x => x.Name == dataReader.GetString(1)) > 0)
                {
                    Console.WriteLine("Contact already preent in Xero \n"+ dataReader.GetString(1));
                }
                else 
                {
                    Console.WriteLine("Importing contact \n"+ dataReader.GetInt32(0).ToString());
                    batchContacts._Contacts.Add(contact);
                }
            }
            dataReader.Close();            
            Contacts partialbatchcontacts = new Contacts();
            partialbatchcontacts._Contacts = new List<Contact>();
            for (int i = 0; i< batchContacts._Contacts.Count; i++) 
            {                    
                partialbatchcontacts._Contacts.Add(batchContacts._Contacts[i]);
                if (i % 200 == 0) 
                {
                    try
                    {
                        var contacts = await _accountingApi.CreateContactsAsync(accessToken, tenantId, partialbatchcontacts);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\n");
                    }
                    partialbatchcontacts._Contacts.Clear();
                }
            }
            try
            {
                var c = await _accountingApi.CreateContactsAsync(accessToken, tenantId, partialbatchcontacts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message+"\n");
            }            
            Console.WriteLine("Finsihed importing contacts");
        }

        private async Task InvoicePayment(string accessToken, string tenantId, Guid? accid)
        {
            var draftinvoicelist = new List<Invoice>();
            //var outstandingInvoices = await _accountingApi.GetInvoicesAsync(accessToken, tenantId, statuses: new List<string> { "DRAFT" }, where: "Type == \"ACCREC\"");
            var outstandingInvoices = await _accountingApi.GetInvoicesAsync(accessToken, tenantId, where: "Type == \"ACCREC\"");
            foreach (var o in outstandingInvoices._Invoices)
            {
                Console.WriteLine("Invoice Number :" + o.InvoiceNumber + " Amount Due :" + o.AmountDue + " STATUS :" + o.Status);
                if (o.InvoiceNumber.Contains("VER3_"))
                {
                    Payment paymentToChangeStatus = new Payment
                    {
                        Invoice = new Invoice
                        {
                            InvoiceID = o.InvoiceID,
                        },
                        Account = new Account
                        {
                            AccountID = accid
                        },
                        Amount = 1
                    };
                    try
                    {
                        var x = await _accountingApi.CreatePaymentAsync(accessToken, tenantId, paymentToChangeStatus);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
