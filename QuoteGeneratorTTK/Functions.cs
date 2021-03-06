﻿using System;
using System.Data;
using Epicor;
using Utilities;
using System.Configuration;

namespace QuoteGeneratorTTK
{
    public class Functions
    {
        String collector = String.Empty;
        DBFunctions util = new DBFunctions();
        Statements query = new Statements();
        Logs log = new Logs();

        public void SearchQuotesToProcess()
        {
            Int32 customer = 0;
            Int32 index = 1;
            String newID = String.Empty;
            DataTable d = new DataTable();
            Adapters epic = new Adapters();
            log.createLog();
            log.writeOnLog(String.Format("[{0}] - Iniciando ejecución de la interfaz !!", DateTime.Now.ToString()));
            log.writeOnLog(String.Format("[{0}] - Obteniendo registros ...", DateTime.Now.ToString()));
            d = GetQuotesToProcess();

            if (d.Rows.Count > 0)
            {
                foreach (DataRow row in d.Rows)
                {

                    Console.WriteLine(String.Format("[Registro {0}] -> idCliente: {1} Folio: {2}", index, row[2].ToString(), row[19].ToString()));
                    log.writeOnLog(String.Format("[{0}] - Cliente obtenido -> {1} Folio: -> {2}", DateTime.Now.ToString(), row[2].ToString(), row[19].ToString()));

                    Int32 existe = epic.CustomerExists(row[2].ToString(), row[3].ToString(), out String CustName);

                    if (existe > 0)
                    {
                        if (CustName.ToLower().Contains(row[3].ToString().ToLower()))
                        {
                            Console.WriteLine(String.Format("Cliente: {0} Nombre: {1}", existe, CustName));
                            log.writeOnLog(String.Format("[{0}] - El cliente -> {1} ya existe en Epicor", DateTime.Now.ToString(), row[2].ToString()));

                            util.execOperation(String.Format(query.UPDSTATUSINTERFAZ, "99", row[18].ToString(), row[19].ToString(), row[2].ToString()));
                        }
                        else
                        {
                            Console.WriteLine(String.Format("El id o Nombre del cliente no existen, se agregará a Epicor!!", row[2].ToString()));
                            log.writeOnLog(String.Format("[{0}] - El cliente -> {1} no existe, se procede a crearlo en Epicor", DateTime.Now.ToString(), row[2].ToString()));
                            if (epic.ExceptionCollector.Equals(""))
                            {
                                newID = NextCustID();
                                customer = epic.CreateCustomer(row, newID);
                                Console.WriteLine(String.Format("Cliente creado: ", customer));
                                log.writeOnLog(String.Format("[{0}] - El cliente -> {1} ha sido creado exitosamente", DateTime.Now.ToString(), row[2].ToString()));
                            }
                            else
                            {
                                Console.WriteLine("Ocurrió un problema al intentar crear el cliente!!");
                                log.writeOnLog(String.Format("[{0}] - El cliente -> {1} no se pudo crear ... \n Exception: {2}", DateTime.Now.ToString(), row[2].ToString(), epic.ExceptionCollector));
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(String.Format("El cliente {0} no existe, se agregará a Epicor!!", row[2].ToString()));
                        log.writeOnLog(String.Format("[{0}] - El cliente -> {1} no existe, se procede a crearlo en Epicor", DateTime.Now.ToString(), row[2].ToString()));
                        if (epic.ExceptionCollector.Equals(""))
                        {
                            newID = NextCustID();
                            customer = epic.CreateCustomer(row, newID);
                            Console.WriteLine(String.Format("Cliente creado: ", customer));
                            log.writeOnLog(String.Format("[{0}] - El cliente -> {1} ha sido creado exitosamente", DateTime.Now.ToString(), row[2].ToString()));
                        }
                        else
                        {
                            Console.WriteLine("Ocurrió un problema al intentar crear el cliente!!");
                            log.writeOnLog(String.Format("[{0}] - El cliente -> {1} no se pudo crear ... \n Exception: {2}", DateTime.Now.ToString(), row[2].ToString(),epic.ExceptionCollector));
                        }
                    }

                    index++;
                }

                log.writeOnLog(String.Format("[{0}] - Hemos terminado !! \n\n\n", DateTime.Now.ToString()));
            }
            else
                log.writeOnLog(String.Format("[{0}] - Ningún registro encontrado, hemos terminado !!", DateTime.Now.ToString()));
        }

        private DataTable GetQuotesToProcess()
        {
            DataTable dt = new DataTable();

            try
            {
                dt = util.getRecords(String.Format(query.GETQUOTESTOPROCESS,ConfigurationManager.AppSettings["QuotesPerExecution"].ToString()));

                if (dt.Rows.Count > 0)
                {
                    foreach(DataRow i in dt.Rows)
                        util.execOperation(String.Format(query.UPDSTATUSINTERFAZ, "2", i[18].ToString(), i[19].ToString(), i[2].ToString()));
                }
            }
            catch (Exception e) {
                Console.WriteLine("[Error al obtener quotes] - Message: " + e.Message + "\n[Description] - " + e.StackTrace);
                log.writeOnLog(String.Format("[Error al obtener quotes] - Message: {0}\n[Description] - {1}", e.Message, e.StackTrace));
            }

            return dt;
        }

        private String NextCustID()
        {
            String nextID = String.Empty;
            try
            {
                DataTable x = util.getRecords(query.GETNEXTID);
                int parse = Convert.ToInt32(x.Rows[0].ItemArray[0]) + 1;
                nextID = parse.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error al obtener nuevo ID] - Message: " + e.Message + "\n[Description] - " + e.StackTrace);
                log.writeOnLog(String.Format("[Error al obtener quotes] - Message: {0}\n[Description] - {1}", e.Message, e.StackTrace));
            }
            return nextID;
        }
    }
}
