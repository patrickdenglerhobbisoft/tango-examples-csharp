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

namespace com.projecttango.motiontrackingcsharp
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
    
    /// <summary>
    /// TangoListener class for use with com.projectango.net
    /// 
    /// This class is an input to the ConnectListener call for Tango initalization.  
    /// </summary>
    public class TangoListener : Java.Lang.Object, Tango.IOnTangoUpdateListener
    {
        #region Properties
        /// <summary>
        /// Holds Tango instance if access to Tango service is required
        /// </summary>
        private Tango mTango;
        public Tango TangoInstance
        {
            set { mTango = value; }
        }

        /// <summary>
        /// Handle to initializing activity for access to member fields
        /// </summary>
        private Activity mParentActivity;
        public Activity ParentActivity
        {
            set { mParentActivity = value; }
            get { return mParentActivity; }
        }

        readonly string _TAG;
        static int SECS_TO_MILLISECS = 1000;
        #endregion

        #region Constructers and Destructers

        public TangoListener()
        {
            _TAG = typeof(TangoListener).Name;
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
            ParentActivity.RunOnUiThread(() =>
            {
                MotionTracking parent = (mParentActivity as MotionTracking);
                // Log whenever Motion Tracking enters a n invalid state
                if (!parent.mIsAutoRecovery && (pose.StatusCode == TangoPoseData.PoseInvalid))
                {
                    Log.Wtf(mParentActivity.GetType().Name, "Invalid State");
                }
                parent.mDeltaTime = (float)(pose.Timestamp - parent.mPreviousTimeStamp) * SECS_TO_MILLISECS;
                parent.mPreviousTimeStamp = (float)pose.Timestamp;
                Log.Info(_TAG, "Delta Time is: " + parent.mDeltaTime);
                parent.count++;
                // Update the OpenGL renderable objects with the new Tango Pose
                // data
                float[] translation = pose.GetTranslationAsFloats();
                parent.mRenderer.Trajectory.updateTrajectory(translation);
                parent.mRenderer.ModelMatCalculator.updateModelMatrix(translation, pose.GetRotationAsFloats());
                parent.mRenderer.updateViewMatrix();
                parent.mGLView.RequestRender();

                DecimalFormat threeDec = new DecimalFormat("0.000");
                string translationString = "[" + threeDec.format(pose.Translation[0]) + ", " + threeDec.format(pose.Translation[1]) + ", " + threeDec.format(pose.Translation[2]) + "] ";
                string quaternionString = "[" + threeDec.format(pose.Rotation[0]) + ", " + threeDec.format(pose.Rotation[1]) + ", " + threeDec.format(pose.Rotation[2]) + ", " + threeDec.format(pose.Rotation[3]) + "] ";

                // Display pose data On screen in TextViews
                parent.mPoseTextView.Text = translationString;
                parent.mQuatTextView.Text = quaternionString;
                parent.mPoseCountTextView.Text = parent.count.ToString();
                parent.mDeltaTextView.Text = threeDec.format(parent.mDeltaTime);
                if (pose.StatusCode == TangoPoseData.PoseValid)
                {
                    parent.mPoseStatusTextView.Text = "Valid";
                }
                else if (pose.StatusCode == TangoPoseData.PoseInvalid)
                {
                    parent.mPoseStatusTextView.Text = "Invalid";
                }
                else if (pose.StatusCode == TangoPoseData.PoseInitializing)
                {
                    parent.mPoseStatusTextView.Text = "Initializing";
                }
                else if (pose.StatusCode == TangoPoseData.PoseUnknown)
                {
                    parent.mPoseStatusTextView.Text = "Unknown";
                }
            });
        }

        void Tango.IOnTangoUpdateListener.OnTangoEvent(TangoEvent args)
        {
            ParentActivity.RunOnUiThread(() =>
            {
                (mParentActivity as MotionTracking).mTangoEventTextView.Text = args.EventKey + ": " + args.EventValue;
            });

          
        }

        void Tango.IOnTangoUpdateListener.OnXyzIjAvailable(TangoXyzIjData args) { }
#endregion
    }

}

