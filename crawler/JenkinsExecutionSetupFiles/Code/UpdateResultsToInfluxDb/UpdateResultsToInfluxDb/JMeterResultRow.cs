using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateResultsToInfluxDb
{
    class JMeterResultRow
    {
        public string sampler_label;
        public int aggregate_report_count;
        public int average;
        public int aggregate_report_median;
        public int aggregate_report_90_line;
        public int aggregate_report_95_line;
        public int aggregate_report_99_line;
        public int aggregate_report_min;
        public int aggregate_report_max;
        public double aggregate_report_error;
        public double aggregate_report_rate;
        public double aggregate_report_bandwidth;
        public double aggregate_report_stddev;
        // constructor to get the result row (string) from the csv to create a ResultRow object
        // assumes the csv file will be always have columns in the specific order
        public JMeterResultRow(string row)
        {
            string[] rowArr = row.Split(',');
            this.sampler_label = rowArr[0];
            this.aggregate_report_count = Convert.ToInt32(rowArr[1]);
            this.average = Convert.ToInt32(rowArr[2]);
            this.aggregate_report_median = Convert.ToInt32(rowArr[3]);
            this.aggregate_report_90_line = Convert.ToInt32(rowArr[4]);
            this.aggregate_report_95_line = Convert.ToInt32(rowArr[5]);
            this.aggregate_report_99_line = Convert.ToInt32(rowArr[6]);
            this.aggregate_report_min = Convert.ToInt32(rowArr[7]);
            this.aggregate_report_max = Convert.ToInt32(rowArr[8]);
            this.aggregate_report_error = Convert.ToDouble(rowArr[9].Replace("%", ""));
            this.aggregate_report_rate = Convert.ToDouble(rowArr[10]);
            this.aggregate_report_bandwidth = Convert.ToDouble(rowArr[11]);
            this.aggregate_report_stddev = Convert.ToDouble(rowArr[12]);
        }

        public JMeterResultRow() { }
    }
}
