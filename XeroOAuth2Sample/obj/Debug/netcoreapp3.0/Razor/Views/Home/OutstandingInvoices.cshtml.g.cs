#pragma checksum "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "a55c44bb42897149d7f8256ee9a2c2e421d2ad13"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_OutstandingInvoices), @"mvc.1.0.view", @"/Views/Home/OutstandingInvoices.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\_ViewImports.cshtml"
using XeroOAuth2Sample;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\_ViewImports.cshtml"
using XeroOAuth2Sample.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a55c44bb42897149d7f8256ee9a2c2e421d2ad13", @"/Views/Home/OutstandingInvoices.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0fc1d47aefaf6faf2b3bd1b7cd01f439d08d550f", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_OutstandingInvoices : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<OutstandingInvoicesViewModel>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\n");
#nullable restore
#line 3 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
  
    ViewData["Title"] = "Outstanding Invoices";

#line default
#line hidden
#nullable disable
            WriteLiteral("\n    <div class=\"text-center\">\r\n        <h1>");
#nullable restore
#line 8 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
       Write(ViewData["Title"]);

#line default
#line hidden
#nullable disable
            WriteLiteral("</h1>\r\n        <p><b>Hello ");
#nullable restore
#line 9 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
               Write(Model.Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("!</b> Thanks for signing in with Xero</p>\r\n        <hr />\r\n");
#nullable restore
#line 11 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
          
            foreach (var (key, value) in Model.Data)
            {

#line default
#line hidden
#nullable disable
            WriteLiteral("                <p><b>");
#nullable restore
#line 14 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
                 Write(key);

#line default
#line hidden
#nullable disable
            WriteLiteral("</b> has ");
#nullable restore
#line 14 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
                              Write(value);

#line default
#line hidden
#nullable disable
            WriteLiteral(" outstanding invoice(s)</p>\r\n");
#nullable restore
#line 15 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
            }
        

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n        ");
#nullable restore
#line 18 "F:\Xero\xero-netstandard-oauth2-samples-master\XeroOAuth2Sample\XeroOAuth2Sample\Views\Home\OutstandingInvoices.cshtml"
   Write(Html.ActionLink("Add another organisation", "AddConnection", "Home"));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n    </div>\n");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<OutstandingInvoicesViewModel> Html { get; private set; }
    }
}
#pragma warning restore 1591
