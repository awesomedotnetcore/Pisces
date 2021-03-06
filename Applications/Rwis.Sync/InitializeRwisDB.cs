﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Reclamation.TimeSeries;
using Reclamation.Core;
using System.Configuration;
using System.IO;
using Mono.Options;

namespace Rwis.Initialize
{
    class Program
    {
        static bool initializeSiteCatalog = true;
        static bool initializeParameterCatalog = true;
        static bool initializeSeriesCatalog = true;
        static private string path = ""; 

        public static void initializeMain(string[] argList)
        {

            var db = TimeSeriesDatabase.InitDatabase(new Arguments(argList));
            var svr = db.Server;
            if (initializeParameterCatalog)
            {
                // Get Parameter Catalog CSV
                var parCatCSV = ConvertCSVtoDataTable(path + "parametercatalog.csv");
                // Get DB Parameter Catalog
                var parCat = db.GetParameterCatalog();
                // Populate DB parametercatalog
                foreach (DataRow row in parCatCSV.Rows)
                {
                    parCat.AddparametercatalogRow(row["id"].ToString(), row["timeinterval"].ToString(), row["units"].ToString(),
                        row["statistic"].ToString(), row["name"].ToString());
                }
                // Save Table
                svr.SaveTable(parCat);
            }
            if (initializeSiteCatalog)
            {
                // Get Site Catalog CSV
                var steCatCSV = ConvertCSVtoDataTable(path + "sitecatalog.csv");
                // Get DB Site Catalog
                var steCat = db.GetSiteCatalog();
                // Populate DB sitecatalog
                foreach (DataRow row in steCatCSV.Rows)
                {
                    steCat.AddsitecatalogRow(row["siteid"].ToString(), row["description"].ToString(), row["state"].ToString(),
                        Convert.ToDouble(row["latitude"]), Convert.ToDouble(row["longitude"]), Convert.ToDouble(row["elevation"]), row["timezone"].ToString(),
                        row["install"].ToString(), row["horizontal_datum"].ToString(), row["vertical_datum"].ToString(),
                        Convert.ToDouble(row["vertical_accuracy"]), row["elevation_method"].ToString(), row["tz_offset"].ToString(),
                        row["active_flag"].ToString(), row["type"].ToString(), row["responsibility"].ToString(), row["agency_region"].ToString());
                }
                // Save Table
                svr.SaveTable(steCat);
            }
            if (initializeSeriesCatalog)
            {
                // Get Site Catalog CSV
                var serCatCSV = ConvertCSVtoDataTable(path + "seriescatalog.csv");
                // Get DB Site Catalog
                var serCat = db.GetSeriesCatalog();
                // Populate DB sitecatalog
                foreach (DataRow row in serCatCSV.Rows)
                {
                    serCat.AddSeriesCatalogRow(Convert.ToInt32(row["A_ID"]), Convert.ToInt32(row["parentid"]), Convert.ToInt16(row["isfolder"]),
                        Convert.ToInt32(row["sortorder"]), row["iconname"].ToString(), row["name"].ToString(), row["siteid"].ToString(),
                        row["units"].ToString(), row["timeinterval"].ToString(), row["parameter"].ToString(),
                        row["tablename"].ToString(), row["provider"].ToString(), row["connectionstring"].ToString(),
                        row["expression"].ToString(), row["notes"].ToString(), Convert.ToInt16(row["enabled"].ToString()));
                }
                // Save Table
                svr.SaveTable(serCat);
            }
        }


        /// <summary>
        /// Converts CSV file to C# DataTable
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                { dt.Columns.Add(header); }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    { dr[i] = rows[i]; }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
    }
}