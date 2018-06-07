using System;
using System.Configuration;
using System.ServiceModel.Channels;
using Epicor.ServiceModel.StandardBindings;
using Ice.Lib;
using Erp.BO;
using System.Data;
using Utilities;

namespace Epicor
{
    public class Adapters
    {
        DBFunctions sql;
        public String ExceptionCollector;
        Credentials cred;
        String currentCompany = ConfigurationManager.AppSettings["DefaultCompany"].ToString();
        String fileSys = String.Format(ConfigurationManager.AppSettings["epiConnection"].ToString(), "Epicor10");

        public Adapters()
        {
            cred = new Credentials();
            cred.username = ConfigurationManager.AppSettings["DefaultUser"].ToString();
            cred.password = ConfigurationManager.AppSettings["DefaultPass"].ToString();

            SetCompany();
        }

        private void SetCompany()
        {
            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Ice/BO/{1}.svc", appServerUrl, "UserFile"));

                using (Ice.Proxy.BO.UserFileImpl US = new Ice.Proxy.BO.UserFileImpl(wcfBinding, CustSvcUri))
                {
                    US.ClientCredentials.UserName.UserName = cred.username;
                    US.ClientCredentials.UserName.Password = cred.password;
                    US.SaveSettings(cred.username, true, currentCompany, true, false, true, true, true, true, true, true, true,
                                               false, false, -2, 0, 1456, 886, 2, "MAINMENU", "", "", 0, -1, 0, "", false);
                    US.Close();
                    US.Dispose();
                }
            }
            catch (System.UnauthorizedAccessException loginError)
            {
                ExceptionCollector += loginError.Message;
            }
            catch (Ice.Common.BusinessObjectException BOException)
            {
                ExceptionCollector += BOException.Message;
            }
            catch (Exception e)
            {
                ExceptionCollector += e.Message;
            }
        }

        public Int32 CustomerExists(String CustID, String Name, out String CustName)
        {
            CustName = String.Empty;
            Int32 cust = 0;
            Boolean MorePages = false;
            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "Customer"));

                using (Erp.Proxy.BO.CustomerImpl US = new Erp.Proxy.BO.CustomerImpl(wcfBinding, CustSvcUri))
                {
                    US.ClientCredentials.UserName.UserName = cred.username;
                    US.ClientCredentials.UserName.Password = cred.password;
                    CustomerListDataSet customer = new CustomerListDataSet();
                    customer.EnforceConstraints = true;

                    customer = US.GetList(String.Format("Company = 'TT' AND CustID = '{0}' OR Name = '{1}'",CustID, Name), 0, 1, out MorePages);

                    if (customer.Tables["CustomerList"].Rows.Count > 0)
                    {
                        CustName = customer.Tables["CustomerList"].Rows[0]["Name"].ToString();
                        cust = Convert.ToInt32(customer.Tables["CustomerList"].Rows[0]["CustNum"]);
                    }
                }
            }
            catch (System.UnauthorizedAccessException loginError)
            {
                ExceptionCollector = loginError.Message;
            }
            catch (Ice.Common.BusinessObjectException BOException)
            {
                ExceptionCollector = BOException.Message;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("[SystemException] - {0} \n [ExceptionMessage] - {1}", e.Message, e.StackTrace));
                ExceptionCollector += String.Format("[SystemException] Method: [CustomerExists] - {0} \nMessage: {1}", e.Message, e.StackTrace);
            }

            return cust;
        }

        public Int32 CreateCustomer(DataRow row, String newCustID)
        {
            sql = new DBFunctions();
            Statements stmt = new Statements();
            ExceptionCollector = String.Empty;
            Int32 CustNum = 0;
            String countryName = String.Empty;
            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "Customer"));

                using (Erp.Proxy.BO.CustomerImpl BO = new Erp.Proxy.BO.CustomerImpl(wcfBinding, CustSvcUri))
                {
                    BO.ClientCredentials.UserName.UserName = cred.username;
                    BO.ClientCredentials.UserName.Password = cred.password;
                    CustomerDataSet newCustomer = new CustomerDataSet();

                    BO.GetNewCustomer(newCustomer);

                    //Carga de infor del cliente
                    newCustomer.Tables["Customer"].Rows[0]["CustNum"] = 0;
                    newCustomer.Tables["Customer"].Rows[0]["CustID"] = /*row[2].ToString()*/ newCustID;
                    newCustomer.Tables["Customer"].Rows[0]["CustomerType"] = "PRO";

                    if (row[3].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["Name"] = row[3].ToString().Substring(0, 49).ToUpper(); else newCustomer.Tables["Customer"].Rows[0]["Name"] = row[3].ToString().ToUpper();
                    if (row[6].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["Address1"] = row[6].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["Address1"] = row[6].ToString();
                    if (row[7].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["Address2"] = row[7].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["Address2"] = row[7].ToString();
                    if (row[8].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["Address3"] = row[8].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["Address3"] = row[8].ToString();
                    if (row[9].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["City"] = row[9].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["City"] = row[9].ToString();
                    if (row[10].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["State"] = row[10].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["State"] = row[10].ToString();
                    if (!row[11].ToString().Equals("")) newCustomer.Tables["Customer"].Rows[0]["CountryNum"] = getCountryNum(row[11].ToString(), out countryName);
                    if (!row[11].ToString().Equals("")) newCustomer.Tables["Customer"].Rows[0]["Country"] = countryName;
                    if (!row[12].ToString().Equals("")) newCustomer.Tables["Customer"].Rows[0]["Zip"] = row[12].ToString();

                    //BillTo Information
                    if (row[3].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["BTName"] = row[3].ToString().Substring(0, 49).ToUpper(); else newCustomer.Tables["Customer"].Rows[0]["BTName"] = row[3].ToString().ToUpper();
                    if (row[6].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["BTAddress1"] = row[6].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["BTAddress1"] = row[6].ToString();
                    if (row[7].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["BTAddress2"] = row[7].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["BTAddress2"] = row[7].ToString();
                    if (row[8].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["BTAddress3"] = row[8].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["BTAddress3"] = row[8].ToString();
                    if (row[9].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["BTCity"] = row[9].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["BTCity"] = row[9].ToString();
                    if (row[10].ToString().Length > 50) newCustomer.Tables["Customer"].Rows[0]["BTState"] = row[10].ToString().Substring(0, 49); else newCustomer.Tables["Customer"].Rows[0]["BTState"] = row[10].ToString();
                    if (!row[11].ToString().Equals("")) newCustomer.Tables["Customer"].Rows[0]["BTCountryNum"] = getCountryNum(row[11].ToString(), out countryName);
                    if (!row[11].ToString().Equals("")) newCustomer.Tables["Customer"].Rows[0]["BTCountry"] = countryName;
                    if (!row[12].ToString().Equals("")) newCustomer.Tables["Customer"].Rows[0]["BTZip"] = row[12].ToString();

                    newCustomer.Tables["Customer"].Rows[0]["TermsCode"] = "1";
                    newCustomer.Tables["Customer"].Rows[0]["ShipViaCode"] = "CE";
                    //newCustomer.Tables["Customer"].Rows[0]["TaxRegionCode"] = row[15].ToString();
                    if (!row[17].ToString().Equals("")) newCustomer.Tables["Customer"].Rows[0]["DiscountPercent"] = Convert.ToDecimal(row[16]);
                    newCustomer.Tables["Customer"].Rows[0]["TerritoryID"] = getTerritory(row[4].ToString());
                    newCustomer.Tables["Customer"].Rows[0]["SalesRepCode"] = row[13].ToString();
                    
                    BO.Update(newCustomer);

                    CustNum = Convert.ToInt32(newCustomer.Tables["Customer"].Rows[0]["CustNum"]);
                    Console.WriteLine("Número de cliente generado: " + CustNum);

                    DataTable dt = sql.getRecords(stmt.GETPRICELIST);

                    if (dt.Rows.Count > 0)
                        foreach (DataRow fila in dt.Rows)
                        {
                            CreateCustomerPriceList(CustNum, "", fila[0].ToString());
                        }

                    sql.execOperation(String.Format(stmt.UPDLICLIENTES, row[2].ToString()));
                    sql.execOperation(String.Format(stmt.UPDSTATUSINTERFAZ, "99", row[18].ToString(), row[19].ToString(), row[2].ToString()));
                }
            }
            catch (System.UnauthorizedAccessException x)
            {
                ExceptionCollector += "[CreateCustomerException] -> " + x.Message;
                sql.execOperation(String.Format(stmt.UPDSTATUSINTERFAZ, "3", row[18].ToString(), row[19].ToString(), row[2].ToString()));
            }
            catch (Ice.Common.BusinessObjectException y)
            {
                ExceptionCollector += "[CreateCustomerException] -> " + y.Message;
                sql.execOperation(String.Format(stmt.UPDSTATUSINTERFAZ, "3", row[18].ToString(), row[19].ToString(), row[2].ToString()));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("[SystemException] - {0} \n [ExceptionMessage] - {1}", e.Message, e.StackTrace));
                ExceptionCollector += String.Format("Method: CreateCustomer \n Message: Error -> {0}", e.Message);
                sql.execOperation(String.Format(stmt.UPDSTATUSINTERFAZ, "3", row[18].ToString(), row[19].ToString(), row[2].ToString()));
            }

            return CustNum;
        }

        private void CreateCustomerPriceList(Int32 CustNum, String ShipToNum, String ListCode)
        {
            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "Customer"));

                using (Erp.Proxy.BO.CustomerImpl BO = new Erp.Proxy.BO.CustomerImpl(wcfBinding, CustSvcUri))
                {
                    BO.ClientCredentials.UserName.UserName = cred.username;
                    BO.ClientCredentials.UserName.Password = cred.password;
                    CustomerDataSet newCustomerPriceList = new CustomerDataSet();
                    newCustomerPriceList.EnforceConstraints = false;

                    BO.GetNewCustomerPriceLst(newCustomerPriceList, CustNum, ShipToNum);

                    newCustomerPriceList.Tables["CustomerPriceLst"].Rows[0]["CustNum"] = CustNum;
                    newCustomerPriceList.Tables["CustomerPriceLst"].Rows[0]["ShipToNum"] = ShipToNum;
                    newCustomerPriceList.Tables["CustomerPriceLst"].Rows[0]["ListCode"] = ListCode;

                    BO.Update(newCustomerPriceList);
                }
            }
            catch (System.UnauthorizedAccessException x)
            {
                ExceptionCollector += "[CreateCustomerPriceListException] -> " + x.Message;
            }
            catch (Ice.Common.BusinessObjectException y)
            {
                ExceptionCollector += "[CreateCustomerPriceListException] -> " + y.Message;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("[SystemException] - {0} \n [ExceptionMessage] - {1}", e.Message, e.StackTrace));
                ExceptionCollector += String.Format("Method: CreateCustomer \n Message: Error -> {0}", e.Message);
            }
        }

        private int getCountryNum(String Country, out String CountryName)
        {
            int CountryNum = 0;
            bool MorePages = false;
            CountryName = String.Empty;

            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "Country"));

                using (Erp.Proxy.BO.CountryImpl BO = new Erp.Proxy.BO.CountryImpl(wcfBinding, CustSvcUri))
                {
                    BO.ClientCredentials.UserName.UserName = cred.username;
                    BO.ClientCredentials.UserName.Password = cred.password;
                    CountryDataSet country = new CountryDataSet();
                    
                    country = BO.GetRows(String.Format("Company = 'TT' AND Description = '{0}'", Country), "", "", 0, 1, out MorePages);

                    if (country.Tables["Country"].Rows.Count > 0)
                    {
                        CountryNum = Convert.ToInt32(country.Tables["Country"].Rows[0]["CountryNum"]);
                        CountryName = country.Tables["Country"].Rows[0]["Country"].ToString();
                    }
                }
            }
            catch (System.UnauthorizedAccessException x)
            {
                ExceptionCollector += "[getCountryException] -> " + x.Message + "\n" + x.StackTrace;
            }
            catch (Ice.Common.BusinessObjectException y)
            {
                ExceptionCollector += "[getCountryException] -> " + y.Message + "\n" + y.StackTrace;
            }
            catch (Exception e)
            {
                ExceptionCollector += "[getCountryException] -> " + e.Message + "\n" + e.StackTrace;
            }

            return CountryNum;
        }

        private String getTerritory(String territory)
        {
            String territorio = String.Empty;
            bool MorePages = false;
            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "SalesTer"));

                using (Erp.Proxy.BO.SalesTerImpl BO = new Erp.Proxy.BO.SalesTerImpl(wcfBinding, CustSvcUri))
                {
                    BO.ClientCredentials.UserName.UserName = cred.username;
                    BO.ClientCredentials.UserName.Password = cred.password;
                    SalesTerDataSet x = new SalesTerDataSet();

                    x = BO.GetRows(String.Format("Company = 'TT' And (TerritoryID = '{0}' Or TerritoryDesc = '{1}')", territory.ToString(), territory.ToString()), "", "", 0, 1, out MorePages);

                    if (x.Tables["SalesTer"].Rows.Count > 0)
                        territorio = x.Tables["SalesTer"].Rows[0]["TerritoryID"].ToString();
                }
            }
            catch (System.UnauthorizedAccessException x)
            {
                ExceptionCollector += "[getTerritoryException] -> " + x.Message + "\n" + x.StackTrace;
            }
            catch (Ice.Common.BusinessObjectException y)
            {
                ExceptionCollector += "[getTerritoryException] -> " + y.Message + "\n" + y.StackTrace;
            }
            catch (Exception e)
            {
                ExceptionCollector += "[getTerritoryException] -> " + e.Message + "\n" + e.StackTrace;
            }

            return territorio;
        }

        public void CreateQuoteHead()
        {
            Int32 QuoteNum = 0;
            ExceptionCollector = String.Empty;
            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "Quote"));

                using (Erp.Proxy.BO.QuoteImpl US = new Erp.Proxy.BO.QuoteImpl(wcfBinding, CustSvcUri))
                {
                    US.ClientCredentials.UserName.UserName = cred.username;
                    US.ClientCredentials.UserName.Password = cred.password;
                    QuoteDataSet qHead = new QuoteDataSet();

                    US.GetNewQuoteHed(qHead);
                }

                ExceptionCollector += String.Format("QuoteNum: {0} Message: {1}", QuoteNum, "Done");
            }
            catch (System.UnauthorizedAccessException loginError)
            {
                ExceptionCollector = loginError.Message;
            }
            catch (Ice.Common.BusinessObjectException BOException)
            {
                ExceptionCollector = BOException.Message;
            }
            catch (Exception e) {
                Console.WriteLine(String.Format("[SystemException] - {0} \n [ExceptionMessage] - {1}", e.Message, e.StackTrace));
                ExceptionCollector += String.Format("QuoteNum: {0} Message: Error -> {1}", QuoteNum, e.Message);
            }
        }

        public void CreateQuoteDtl(Int32 QuoteNum)
        {
            ExceptionCollector = String.Empty;
            Int32 QuoteLine = 0;
            try
            {
                ExceptionCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "Quote"));

                using (Erp.Proxy.BO.QuoteImpl US = new Erp.Proxy.BO.QuoteImpl(wcfBinding, CustSvcUri))
                {
                    US.ClientCredentials.UserName.UserName = cred.username;
                    US.ClientCredentials.UserName.Password = cred.password;
                }
            }
            catch (System.UnauthorizedAccessException loginError)
            {
                ExceptionCollector = loginError.Message;
            }
            catch (Ice.Common.BusinessObjectException BOException)
            {
                ExceptionCollector = BOException.Message;
            }
            catch (Exception e) {
                Console.WriteLine(String.Format("[SystemException] - {0} \n [ExceptionMessage] - {1}", e.Message, e.StackTrace));
                ExceptionCollector += String.Format("QuoteNum: {0} QuoteLine: {1} Message: Error -> {2}", QuoteNum, QuoteLine, e.Message);
            }
        }
        
    }
}
