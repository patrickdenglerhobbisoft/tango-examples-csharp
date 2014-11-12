using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Com.Google.Atap.Tangoservice
{
    public class DecimalFormat
    {
        private string mFormatString;
        public DecimalFormat(string FormatString="")
        {
            if (FormatString == "")
                mFormatString = "0.000";
            else
                this.mFormatString = FormatString;

        }

        public string format(double number)
        {
            return string.Format(mFormatString, number);
       
        }
    }
}