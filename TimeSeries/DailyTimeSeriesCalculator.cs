﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reclamation.Core;
using Reclamation.TimeSeries.Parser;
using Reclamation.TimeSeries.Hydromet;
using System.IO;

namespace Reclamation.TimeSeries
{
    /// <summary>
    /// DailyTimeSeriesCalculator manages calculation series. 
    /// Used to compute daily values in a batch type operation.
    /// 
    /// </summary>
    public class DailyTimeSeriesCalculator
    {
        TimeSeriesDatabase m_db;
        TimeInterval m_interval;
        string m_filter;
        string m_propertyFilter;
        List<CalculationSeries> m_dependencyList;
        public DailyTimeSeriesCalculator(TimeSeriesDatabase db, TimeInterval interval, string filter="",
            string propertyFilter="")
        {
            m_db = db;
            m_interval = interval;
            m_filter = filter;
            m_propertyFilter = propertyFilter;
            m_dependencyList = m_db.Factory.GetCalculationSeries(TimeInterval.Daily, m_filter, m_propertyFilter);
        }



        //public CalculationSeries[] GetDependentCalculations(string siteID, string pcode)
        //{

        //    TimeSeriesDependency td = new TimeSeriesDependency(m_dependencyList);
        //    TimeSeriesName tn = new TimeSeriesName(siteID + "_" + pcode, m_interval);
        //    var list = td.LookupCalculations(tn.GetTableName(), m_interval).ToArray();

        //    var cList = new List<CalculationSeries>();
        //    foreach (var item in list)
        //    {
        //        if (item is CalculationSeries)
        //            cList.Add(item as CalculationSeries);
        //    }
        //    return cList.ToArray();
        //}


        /// <summary>
        /// Calculates a group of daily values.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="propertyFilter">series property filter.  Example  program:agrimet </param>
        /// <param name="simulate">simulate calculations, don't actuually do it.</param>
        public CalculationSeries[] ComputeDailyValues(DateTime t1, DateTime t2, 
            string errorFileName="")
        {

            Performance p = new Performance();
            HydrometInstantSeries.Cache = new HydrometDataCache(); // clear out and make new cache.
            string dailyFileName = GetDailyFileName(m_propertyFilter);

            bool appendToFile = false; // for output file.
            
            Console.WriteLine("Computing daily values for  "+m_dependencyList.Count+" series" );
            TimeSeriesDependency td = new TimeSeriesDependency(m_dependencyList);
            
            var sorted = td.Sort();
            foreach (var s in sorted)
            {
                if (!s.Enabled)
                    continue; // calculations turned off

                string originalExpression = s.Expression;
                // compute  Values

                var t1a = s.AdjustStartingDateFromProperties(t1, t2);



                
                if( m_db.Parser.VariableResolver is HydrometVariableResolver)
                    CacheAllParametersForSite(s, t1a, t2); // 50% performance boost.

                Console.Write(s.Table.TableName + " = " + s.Expression);
              


                s.Calculate(t1a, t2); // Calculate() also saves to local time series database.

                
                if (s.Count == 0 || s.CountMissing() > 0)
                {
                    File.AppendAllText(errorFileName, "Error: " + s.Table.TableName + " = " + s.Expression + "\n");
                    string msg = "\nDetails: "+s.Table.TableName + " = " + s.Expression+"\n";
                    foreach (var x in s.Messages)
                    {
                        msg += "\n" + x;
                    }
                    Console.WriteLine(msg);
                }
                else
                {
                 //   File.AppendAllText(errorFileName, " OK. ");
                    Console.WriteLine(" OK. ");
                }

                s.Expression = originalExpression;

                TimeSeriesName n = new TimeSeriesName(s.Table.TableName);

                HydrometDailySeries.WriteToArcImportFile(s, n.siteid, n.pcode, dailyFileName, appendToFile);

                if (!appendToFile )
                    appendToFile = true; // append after the first time.
                
            }

            if( appendToFile) // might not have any results
            Console.WriteLine("Results Saved to "+dailyFileName);

          

            p.Report(); // 185 seconds   

            return sorted;
        }

        

        private static string GetDailyFileName(string propertyFilter)
        {

             string valueFilter = "";
            if (propertyFilter != "" && propertyFilter.IndexOf(":") >= 0)
            {
                valueFilter = propertyFilter.Split(':')[1];
            }

            return TimeSeriesRouting.GetOutgoingFileName("daily", valueFilter, "all");

        }



        static string debugFileName="";

        static HashSet<string> visitedSites = new HashSet<string>();
        

        private static void CacheAllParametersForSite(CalculationSeries s, DateTime t1, DateTime t2)
        {
            // if we need instant data for this site
            // get all instant parameters in memory for future use.

            string  tag = s.TimeInterval.ToString() + s.SiteID;

            if (visitedSites.Contains(tag))
                return;
            if (s.TimeInterval != TimeInterval.Daily)
                return;

            var t2a = t2.AddDays(1); // extra data when midnight next day is needed.


            if ( s.Expression.IndexOf("instant") >= 0)
            { // Instant to Daily Calculation

                var tbl = s.TimeSeriesDatabase.GetSeriesCatalog("siteid = '" + s.SiteID + "' and timeinterval = 'Irrregular'");

                var pcodes = DataTableUtility.Strings(tbl, "", "Parameter");

                var query = s.SiteID + " " + String.Join("," + s.SiteID + " ", pcodes);
                var pairs = query.Split(',');
               // 
                HydrometInstantSeries.Cache.Add(pairs, t1, t2a, HydrometHost.PN, TimeInterval.Irregular);

                visitedSites.Add(tag);
                Console.WriteLine(s.SiteID + " added to the cache");
            }
            else if ( s.Expression.IndexOf("instant") < 0)
            { // Daily to Daily type calculation..
                
                var tbl = s.TimeSeriesDatabase.GetSeriesCatalog("siteid = '" + s.SiteID + "' and timeinterval = 'Daily'");

                var pcodes = DataTableUtility.Strings(tbl, "", "Parameter");
                var query = s.SiteID + " " + String.Join("," + s.SiteID + " ", pcodes);
                var pairs = query.Split(',');

                //HydrometDailySeries.Cache = new HydrometDataCache(); // clear out and make new cache.
                HydrometDailySeries.Cache.Add(pairs, t1, t2a, HydrometHost.PN, TimeInterval.Daily);

                visitedSites.Add(tag);
                Console.WriteLine(s.SiteID + " added to the cache");
            }

        }


        

       


    }
}
