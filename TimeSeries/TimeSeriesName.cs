﻿using Reclamation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Reclamation.TimeSeries
{
    /// <summary>
    /// Manages a time series name in the form interval_siteid_pcode
    /// </summary>
    public class TimeSeriesName
    {
        string m_name="";
        public string siteid;
        public string pcode;
        public string interval;
        private string m_defaultInterval;

        public bool Valid = false;

        public TimeSeriesName(string siteId, string parameter, TimeInterval interval)
        {
            Init2(siteId + "_" + parameter, interval);
        }

        public TimeSeriesName(string name, TimeInterval interval)
        {
            Init2(name, interval);
        }

        private void Init2(string name, TimeInterval interval)
        {
            if (interval == TimeInterval.Irregular)
                Init(name, "instant");
            else if (interval == TimeInterval.Daily)
                Init(name, "daily");
            else if (interval == TimeInterval.Monthly)
                Init(name, "monthly");
            else throw new ArgumentException("TimeSeriesName: interval " + interval.ToString() + " not supported ");
        }
        public TimeSeriesName(string name, string defaultInterval = "")
        {
            Init(name, defaultInterval);
        }

        public static string GetTableName(string database, TimeInterval interval, string siteid, string parameter)
        {
            string rval = database;

            if (database.Trim() != "")
                rval += "_";

            rval += GetTimeIntervalForTableName(interval) + "_" + siteid;

            if (parameter != "")
                rval += "_" + parameter;

            return rval.ToLower().Trim();
        }

        public static string GetTimeIntervalForTableName(TimeInterval interval)
        {
            var rval = "instant";
            if (interval == TimeInterval.Irregular )
                rval = "instant";
            if (interval == TimeInterval.Hourly)
                rval = "hourly";
            if (interval == TimeInterval.Daily )
                rval = "daily";
            if (interval == TimeInterval.Monthly)
                rval = "monthly";

            return rval;
        }

        public TimeInterval GetTimeInterval()
        {
            TimeInterval rval = TimeInterval.Irregular;
            if (interval == "instant")
                rval = TimeInterval.Irregular;
            if (interval == "daily")
                rval = TimeInterval.Daily;
            if (interval == "monthly")
                rval = TimeInterval.Monthly;

            return rval;
        }

        private void Init(string name, string interval)
        {
            m_defaultInterval = interval;
            m_name = name;
            Parse();   
            if (!Valid)
            {
                Logger.WriteLine("Init(): Invalid table name '" + name + "'   [" +interval+"]" );
            }
        }




        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string GetTableName()
        {
            var tableName = string.Empty;
            if (Valid)
            {
                if (string.IsNullOrEmpty(pcode))
                    tableName = interval + "_" + m_name;
                else
                    tableName = interval + "_" + siteid + "_" + pcode;
            }
            else
                throw new Exception("GetTableName(): Invalid name : " + tableName);

            return tableName;
        }

         private void Parse()
        {

            string pattern = @"^(?<prefix>(instant|daily|monthly))?_?(?<cbtt>[a-zA-Z][a-zA-Z0-9]{1,7}\w)_(?<pcode>[a-zA-Z0-9_]{1,8}$)";
            var m = Regex.Match(m_name, pattern);

            siteid = "";
            pcode = "";
            interval = "";
            Valid = true;
            if (m.Success)
            {
                interval = m.Groups["prefix"].Value.ToLower();
                siteid = m.Groups["cbtt"].Value.ToLower();
                pcode = m.Groups["pcode"].Value.ToLower() ;
            }
            if (interval == "" && m_defaultInterval != "")
                interval = m_defaultInterval;
        }


        public bool HasInterval { 
            get{
                return interval != "";
            } 
        
        }
    }
}
