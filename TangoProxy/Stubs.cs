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
    public static class threeDec
    {
        private  const string mFormatString= "{0:0.00}";
       
        public static String format(double number)
        {
            return String.Format(mFormatString, number);
       
        }
    }
}