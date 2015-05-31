/*
 * Copyright 2014 HobbiSoft. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed On an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace TangoProxy
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Views;
    using Android.Content;
    using Android.Runtime;
    using Android.Widget;
    using System.Collections.Generic;
    using Android.Util;
    using Com.Google.Atap.Tangoservice;
    using System;


    /// <summary>
    /// TangoListener class for use with com.projectango.net
    /// 
    /// This class is an input to the ConnectListener call for Tango initalization.  
    /// </summary>
    public class TangoListener : Java.Lang.Object, Tango.IOnTangoUpdateListener
    {
        #region Properties

        /// <summary>
        /// Initalize the listener with the current actvity to receive
        /// callbacks.
        /// 
        /// var lisetner = new TangoListener(this)
        /// 
        /// then, before you connect this listener, set these call backs
        /// listener.OnPoseAvailableCallback = OnPoseAvailable;
        /// 
        /// then have an implementation OnPoseAvailable in your Activity.
        /// 
        /// 
        /// 
        /// public void OnPoseAvailable(TangoPoseData pose)
        ///	{
        ///	            ....
        /// }
        /// 
        /// If you do not need to receive the callbacks, simply don't set them
        /// 
        /// 
        /// </summary>
        private Activity mCurrentActivity;
    
       
        
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="pose"></param>
        public delegate void OnPoseAvailableCallbackFunc(TangoPoseData pose);
        public OnPoseAvailableCallbackFunc OnPoseAvailableCallback;
        
        public delegate void OnXyzIjAvailableCallbackFunc(TangoXyzIjData xyzIj);
        public OnXyzIjAvailableCallbackFunc OnXyzIjAvailableCallBack;

        public delegate void OnTangoEventCallBackFunc(TangoEvent args);
        public OnTangoEventCallBackFunc OnTangoEventCallBack;

        readonly string _TAG;
  

        #endregion

        #region Constructers and Destructers

        public TangoListener(Activity currentActivity)
        {
            _TAG = typeof(TangoListener).Name;
            mCurrentActivity = currentActivity;
            Log.Info(_TAG, _TAG + " Constructed");
        }
        ~TangoListener()
        {
            Log.Info(typeof(TangoListener).Name, typeof(TangoListener).Name + " Disposed");
        }

        #endregion

        #region Events

        void Tango.IOnTangoUpdateListener.OnPoseAvailable(TangoPoseData pose)
        {
            if (OnPoseAvailableCallback != null)
                mCurrentActivity.RunOnUiThread(() =>
                {

                    OnPoseAvailableCallback(pose);
                });
        }

        void Tango.IOnTangoUpdateListener.OnTangoEvent(TangoEvent args)
        {
            if (OnTangoEventCallBack != null)
                mCurrentActivity.RunOnUiThread(() =>
                {

                    OnTangoEventCallBack(args);
                });

          
        }

        void Tango.IOnTangoUpdateListener.OnXyzIjAvailable(TangoXyzIjData args) {

            if (OnXyzIjAvailableCallBack != null)
                mCurrentActivity.RunOnUiThread(() =>
                {

                    OnXyzIjAvailableCallBack(args);
                });
        }

        void Tango.IOnTangoUpdateListener.OnFrameAvailable(int p0)
        {
         //a   throw new Exception("Frame Available";
        }
        #endregion
    }

}

