using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Call_AzureFun_Plugin
{
    public class ParamsApi
    {
        public string title { get; set; }

        public string customerid { get; set; }

        public string customername { get; set; }

        public string customertype { get; set; }
    }
    public class Class1 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference.
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            //Entity cases = new Entity();
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName == "Create")
                    {

                        Entity caseTarget = context.InputParameters["Target"] as Entity;
                       
                        string title = (string)caseTarget.Attributes["title"];
                        string customertype = Convert.ToString(((EntityReference)(caseTarget.Attributes["customerid"])).LogicalName);
                        Guid customerid = ((EntityReference)(caseTarget.Attributes["customerid"])).Id;
                        Entity caseRet = service.Retrieve(customertype, customerid, new ColumnSet(true));
                       
                       
                        var customername = "";
                        if (customertype == "account" || customertype == "Account")
                        {
                            customername = (string)caseRet["name"];
                        }
                        else
                        {
                            customername = (string)caseRet["lastname"];
                        }



                       
                       
                        var _response = CallingAzureFunction(title, customername, customerid.ToString(), customertype, service, tracingService);
                        
                    }


                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error:" + ex.Message + " Inner Exception " + ex.InnerException);
            }
        }
        private object CallingAzureFunction(string title, string customername, string customerid, string customertype, IOrganizationService service, ITracingService tracingService)
        {

          
           using (WebClient client = new WebClient())
            {
                client.QueryString.Add("title", title);
                client.QueryString.Add("customername", customername);
                client.QueryString.Add("customerid", customerid);
                client.QueryString.Add("customertype", customertype);
               
                var data = client.UploadValues("https://crudazurefunction20211119142731.azurewebsites.net/api/Function1","POST", client.QueryString);
                var responseString = UnicodeEncoding.UTF8.GetString(data);
                return responseString.ToString();
            }


           







            //var paramsApi = new ParamsApi();
            //paramsApi.title = title;
            //paramsApi.customerid = customerid;
            //paramsApi.customertype = customertype;
            //paramsApi.customername = customername;
           
            //var webClient = new WebClient();
            
            //webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
          
            //var serviceUrl = "https://crudazurefunction20211119142731.azurewebsites.net/api/Function1";
            
            //var Requeststring = JsonConvert.SerializeObject(paramsApi);
            //string ResponseString = webClient.UploadString(serviceUrl, Requeststring);
            //var ResponseObject = JsonConvert.DeserializeObject(Requeststring);
            
           
           // return ResponseString;

        }

    }
}
