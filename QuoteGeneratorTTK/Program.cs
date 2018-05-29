using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Ice.Core;
using System.Configuration;
using System.Threading.Tasks;

namespace QuoteGeneratorTTK
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Session session = new Session(ConfigurationManager.AppSettings["DefaultUser"].ToString(), ConfigurationManager.AppSettings["DefaultPass"].ToString(), Session.LicenseType.Default, String.Format(ConfigurationManager.AppSettings["epiConnection"].ToString(), "Epicor10"));

                if (session != null)
                {
                    Functions process = new Functions();
                    process.SearchQuotesToProcess();
                }
                else
                    Console.WriteLine("No se encontraron cotizaciones a generar!!!");
            }
            catch (System.UnauthorizedAccessException x)
            {
                Console.WriteLine("[EpicorException] - " + x.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("[SystemException] - Message: {0} \nStackTrace: {1}", e.Message, e.StackTrace));
            }
            
        }
    }
}
