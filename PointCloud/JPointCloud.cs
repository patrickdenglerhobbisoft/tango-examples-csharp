using System;
using System.Collections.Generic;
using Com.Google.Atap.Tangoservice;
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

namespace com.projecttango.Pointcloudjava
{
    using PointCloud;
    using Com.Google.Atap.Tangoservice;

    using Android.Opengl;
    using Android.Util;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Views;
    using Android.OS;
    using Android.Widget;
    
    using Java.IO;

	using OnTangoUpdateListener = Tango.IOnTangoUpdateListener;

    using IOnClickListener = Android.Views.View.IOnClickListener;

	/// <summary>
	/// Main Activity class for the Point Cloud Sample. Handles the connection to the
	/// <seealso cref="Tango"/> service and propagation of Tango XyzIj data to OpenGL and
	/// Layout views. OpenGL rendering logic is delegated to the <seealso cref="PCrenderer"/>
	/// class.
	/// </summary>
	public class JPointCloud : Activity, IOnClickListener
	{

		public const string EXTRA_KEY_PERMISSIONTYPE = "PERMISSIONTYPE";
		public const string EXTRA_VALUE_MOTION_TRACKING = "MOTION_TRACKING_PERMISSION";
		private static readonly string TAG = typeof(JPointCloud).Name;
		private static int SECS_TO_MILLI = 1000;
		private Tango mTango;
		private TangoConfig mConfig;

		private PCRenderer mRenderer;
		private GLSurfaceView mGLView;

		private TextView mDeltaTextView;
		private TextView mPoseCountTextView;
		private TextView mPoseTextView;
		private TextView mQuatTextView;
		private TextView mPoseStatusTextView;
		private TextView mTangoEventTextView;
		private TextView mPointCountTextView;
		private TextView mTangoServiceVersionTextView;
		private TextView mApplicationVersionTextView;
		private TextView mAverageZTextView;
		private TextView mFrequencyTextView;

		private Button mFirstPersonButton;
		private Button mThirdPersonButton;
		private Button mTopDownButton;

		private int count;
		private float mDeltaTime;
		private float mPosePreviousTimeStamp;
		private float mXyIjPreviousTimeStamp;
		private float mCurrentTimeStamp;
		private string mServiceVersion;
		private bool mIsTangoServiceConnected;

		protected  override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			//ContentView = Resource.Layout.activity_jpoint_cloud;  // TODO CONTENTVIEW
			Title = GetString(Resource.String.app_name);

			mPoseTextView = (TextView) FindViewById(Resource.Id.pose);
			mQuatTextView = (TextView) FindViewById(Resource.Id.quat);
			mPoseCountTextView = (TextView) FindViewById(Resource.Id.posecount);
			mDeltaTextView = (TextView) FindViewById(Resource.Id.deltatime);
			mTangoEventTextView = (TextView) FindViewById(Resource.Id.tangoevent);
			mPoseStatusTextView = (TextView) FindViewById(Resource.Id.status);
			mPointCountTextView = (TextView) FindViewById(Resource.Id.pointCount);
			mTangoServiceVersionTextView = (TextView) FindViewById(Resource.Id.version);
			mApplicationVersionTextView = (TextView) FindViewById(Resource.Id.appversion);
			mAverageZTextView = (TextView) FindViewById(Resource.Id.averageZ);
			mFrequencyTextView = (TextView) FindViewById(Resource.Id.frameDelta);

			mFirstPersonButton = (Button) FindViewById(Resource.Id.first_person_button);
			mFirstPersonButton.SetOnClickListener(this);
			mThirdPersonButton = (Button) FindViewById(Resource.Id.third_person_button);
			mThirdPersonButton.SetOnClickListener(this);
			mTopDownButton = (Button) FindViewById(Resource.Id.top_down_button);
			mTopDownButton.SetOnClickListener(this);

			mTango = new Tango(this);
			mConfig = new TangoConfig();
			mConfig = mTango.GetConfig(TangoConfig.ConfigTypeCurrent);
			mConfig.PutBoolean(TangoConfig.KeyBooleanDepth, true);

			int maxDepthPoints = mConfig.GetInt("max_point_cloud_elements");
			mRenderer = new PCRenderer(maxDepthPoints);
			mGLView = (GLSurfaceView) FindViewById(Resource.Id.gl_surface_view);
            mGLView.SetEGLContextClientVersion(2);
            mGLView.SetRenderer(mRenderer);
			mGLView.RenderMode =  Rendermode.WhenDirty;

			PackageInfo packageInfo;
			try
			{
			packageInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
				mApplicationVersionTextView.Text = packageInfo.VersionName;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			// Display the version of Tango Service
			mServiceVersion = mConfig.GetString("tango_service_library_version");
			mTangoServiceVersionTextView.Text = mServiceVersion;
			mIsTangoServiceConnected = false;
		}

		protected  void OnPause()
		{
			base.OnPause();
			try
			{
				mTango.Disconnect();
				mIsTangoServiceConnected = false;
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
		}

		protected  override void OnResume()
		{
			base.OnResume();
			Intent permissionIntent = new Intent();
			permissionIntent.SetAction("android.intent.action.REQUEST_TANGO_PERMISSION");
			permissionIntent.PutExtra(EXTRA_KEY_PERMISSIONTYPE, EXTRA_VALUE_MOTION_TRACKING);
			if (!mIsTangoServiceConnected)
			{
				StartActivityForResult(permissionIntent, Tango.TangoIntentActivitycode);
			}
			Log.Info(TAG, "onResumed");
		}

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Check which request we're responding to
            if (requestCode == Tango.TangoIntentActivitycode)
            {
                Log.Info(TAG, "Triggered");
                // Make sure the request was successful
                if (resultCode == Result.Canceled)
                {
                    Toast.MakeText(this, Resource.String.motiontrackingpermission, ToastLength.Long).Show();
                    Finish();
                    return;
                }
                try
                {
                    SetTangoListeners();
                }
                catch (TangoErrorException)
                {
                    Toast.MakeText(this, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
                }
                try
                {
                    mTango.Connect(mConfig);
                    mIsTangoServiceConnected = true;
                }
                catch (TangoOutOfDateException)
                {
                    Toast.MakeText(ApplicationContext, Resource.String.TangoOutOfDateException, Android.Widget.ToastLength.Short).Show();
                }
                catch (TangoErrorException)
                {
                    Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
                }
                SetUpExtrinsics();

            }
        }

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		public void OnClick(View v)
		{
			switch (v.Id)
			{
			case Resource.Id.first_person_button:
				mRenderer.SetFirstPersonView();
				break;
			case Resource.Id.third_person_button:
				mRenderer.SetThirdPersonView();
				break;
			case Resource.Id.top_down_button:
				mRenderer.SetTopDownView();
				break;
			default:
				Log.Wtf(TAG, "Unrecognized button click.");
				return;
			}
		}

		public override bool OnTouchEvent(MotionEvent args)
		{
			return mRenderer.OnTouchEvent(args);
		}

		private void SetUpExtrinsics()
		{
			// Set device to imu matrix in Model Matrix Calculator.
			TangoPoseData device2IMUPose = new TangoPoseData();
			TangoCoordinateFramePair framePair = new TangoCoordinateFramePair();
			framePair.BaseFrame = TangoPoseData.CoordinateFrameImu;
			framePair.TargetFrame = TangoPoseData.CoordinateFrameDevice;
			try
			{
				device2IMUPose = mTango.GetPoseAtTime(0.0, framePair);
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
			mRenderer.ModelMatCalculator.SetDevice2IMUMatrix(device2IMUPose.GetTranslationAsFloats(), device2IMUPose.GetTranslationAsFloats());

			// Set color camera to imu matrix in Model Matrix Calculator.
			TangoPoseData color2IMUPose = new TangoPoseData();

			framePair.BaseFrame = TangoPoseData.CoordinateFrameImu;
			framePair.TargetFrame = TangoPoseData.CoordinateFrameCameraColor;
			try
			{
				color2IMUPose = mTango.GetPoseAtTime(0.0, framePair);
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
			mRenderer.ModelMatCalculator.SetColorCamera2IMUMatrix(color2IMUPose.GetTranslationAsFloats(), color2IMUPose.GetTranslationAsFloats());
		}

		private void SetTangoListeners()
		{
			// Configure the Tango coordinate frame pair

           // Original line in class:  final java.Util.ArrayList<com.google.atap.tangoservice.TangoCoordinateFramePair> framePairs = new java.Util.ArrayList<com.google.atap.tangoservice.TangoCoordinateFramePair>();
			List<TangoCoordinateFramePair> framePairs = new List<TangoCoordinateFramePair>();
			framePairs.Add(new TangoCoordinateFramePair(TangoPoseData.CoordinateFrameStartOfService, TangoPoseData.CoordinateFrameDevice));
			// Listen for new Tango data
			mTango.ConnectListener(framePairs, new OnTangoUpdateListenerAnonymousInnerClassHelper(this, framePairs));
		}

		private class OnTangoUpdateListenerAnonymousInnerClassHelper : Tango.IOnTangoUpdateListener
		{
			private readonly JPointCloud outerInstance;

			private List<TangoCoordinateFramePair> framePairs;

			public OnTangoUpdateListenerAnonymousInnerClassHelper(JPointCloud outerInstance, List<TangoCoordinateFramePair> framePairs)
			{
				this.outerInstance = outerInstance;
				this.framePairs = framePairs;
			}


//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
           // Original line in class:  @Override public void OnPoseAvailable(final com.google.atap.tangoservice.TangoPoseData pose)
			public void OnPoseAvailable(TangoPoseData pose)
			{
				outerInstance.mDeltaTime = (float)(pose.Timestamp - outerInstance.mPosePreviousTimeStamp) * SECS_TO_MILLI;
				outerInstance.mPosePreviousTimeStamp = (float) pose.Timestamp;
				outerInstance.count++;
				outerInstance.mRenderer.ModelMatCalculator.updateModelMatrix(pose.GetTranslationAsFloats(), pose.GetTranslationAsFloats());
				outerInstance.mRenderer.updateViewMatrix();
				outerInstance.mGLView.RequestRender();
				// Update the UI with TangoPose information
				System.Threading.Thread.CurrentThread.Start(new RunnableAnonymousInnerClassHelper(this, pose));
			}

			private class RunnableAnonymousInnerClassHelper 
			{
				private readonly OnTangoUpdateListenerAnonymousInnerClassHelper outerInstance;

				private TangoPoseData pose;

				public RunnableAnonymousInnerClassHelper(OnTangoUpdateListenerAnonymousInnerClassHelper outerInstance, TangoPoseData pose)
				{
					this.outerInstance = outerInstance;
					this.pose = pose;
				}

				public  void run()
				{
					DecimalFormat threeDec = new DecimalFormat("0.000");
					string translationString = "[" + threeDec.format(pose.Translation[0]) + ", " + threeDec.format(pose.Translation[1]) + ", " + threeDec.format(pose.Translation[2]) + "] ";
					string quaternionString = "[" + threeDec.format(pose.Rotation[0]) + ", " + threeDec.format(pose.Rotation[1]) + ", " + threeDec.format(pose.Rotation[2]) + ", " + threeDec.format(pose.Rotation[3]) + "] ";

					// Display pose data On screen in TextViews
					outerInstance.outerInstance.mPoseTextView.Text = translationString;
					outerInstance.outerInstance.mQuatTextView.Text = quaternionString;
					outerInstance.outerInstance.mPoseCountTextView.Text = Convert.ToString(outerInstance.outerInstance.count);
					outerInstance.outerInstance.mDeltaTextView.Text = threeDec.format(outerInstance.outerInstance.mDeltaTime);
					if (pose.StatusCode == TangoPoseData.PoseValid)
					{
						outerInstance.outerInstance.mPoseStatusTextView.Text = "Valid";
					}
					else if (pose.StatusCode == TangoPoseData.PoseInvalid)
					{
						outerInstance.outerInstance.mPoseStatusTextView.Text = "Invalid";
					}
					else if (pose.StatusCode == TangoPoseData.PoseInitializing)
					{
						outerInstance.outerInstance.mPoseStatusTextView.Text = "Initializing";
					}
					else if (pose.StatusCode == TangoPoseData.PoseUnknown)
					{
						outerInstance.outerInstance.mPoseStatusTextView.Text = "Unknown";
					}
				}
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
           // Original line in class:  @Override public void OnXyzIjAvailable(final com.google.atap.tangoservice.TangoXyzIjData xyzIj)
			public void  OnXyzIjAvailable(TangoXyzIjData xyzIj)
			{
				outerInstance.mCurrentTimeStamp = (float) xyzIj.Timestamp;

           // Original line in class:  final float frameDelta = (mCurrentTimeStamp - mXyIjPreviousTimeStamp) * SECS_TO_MILLI;
				float frameDelta = (outerInstance.mCurrentTimeStamp - outerInstance.mXyIjPreviousTimeStamp) * SECS_TO_MILLI;
				outerInstance.mXyIjPreviousTimeStamp = outerInstance.mCurrentTimeStamp;
				byte[] buffer = new byte[xyzIj.XyzCount * 3 * 4];
				FileInputStream fileStream = new FileInputStream(xyzIj.XyzParcelFileDescriptor.FileDescriptor);
				try
				{
					fileStream.Read(buffer, xyzIj.XyzParcelFileDescriptorOffset, buffer.Length);
					fileStream.Close();
				}
				catch (IOException e)
				{
					System.Diagnostics.Debug.WriteLine(e.ToString());
					System.Diagnostics.Debug.WriteLine(e.StackTrace);
				}
				try
				{
					TangoPoseData pointCloudPose = outerInstance.mTango.GetPoseAtTime(outerInstance.mCurrentTimeStamp, framePairs[0]);

					outerInstance.mRenderer.PointCloud.UpdatePoints(buffer, xyzIj.XyzCount);
					outerInstance.mRenderer.ModelMatCalculator.updatePointCloudModelMatrix(pointCloudPose.GetTranslationAsFloats(), pointCloudPose.GetTranslationAsFloats());
					outerInstance.mRenderer.PointCloud.ModelMatrix = outerInstance.mRenderer.ModelMatCalculator.PointCloudModelMatrixCopy;
				}
				catch (TangoErrorException)
				{
					Toast.MakeText(Android.App.Application.Context, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
				}
				catch (TangoInvalidException)
				{
                    Toast.MakeText(Android.App.Application.Context, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
				}

				// Must run UI changes On the UI thread. Running in the Tango
				// service thread
				// will result in an error.
				System.Threading.Thread.CurrentThread.Start(new RunnableAnonymousInnerClassHelper2(this, xyzIj, frameDelta));
			}

			private class RunnableAnonymousInnerClassHelper2 
			{
				private readonly OnTangoUpdateListenerAnonymousInnerClassHelper outerInstance;

				private TangoXyzIjData xyzIj;
				private float frameDelta;

				public RunnableAnonymousInnerClassHelper2(OnTangoUpdateListenerAnonymousInnerClassHelper outerInstance, TangoXyzIjData xyzIj, float frameDelta)
				{
					this.outerInstance = outerInstance;
					this.xyzIj = xyzIj;
					this.frameDelta = frameDelta;
					threeDec = new DecimalFormat("0.000");
				}

				internal DecimalFormat threeDec;

				public  void run()
				{
					// Display number of points in the point cloud
					outerInstance.outerInstance.mPointCountTextView.Text = Convert.ToString(xyzIj.XyzCount);
					outerInstance.outerInstance.mFrequencyTextView.Text = "" + threeDec.format(frameDelta);
					outerInstance.outerInstance.mAverageZTextView.Text = "" + threeDec.format(outerInstance.outerInstance.mRenderer.PointCloud.AverageZ);
				}
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
           // Original line in class:  @Override public void OnTangoEvent(final com.google.atap.tangoservice.TangoEvent event)
            public void OnTangoEvent(TangoEvent args)
            {
                System.Threading.Thread.CurrentThread.Start(new RunnableAnonymousInnerClassHelper3(this, args));
            }

			private class RunnableAnonymousInnerClassHelper3 
			{
				private readonly OnTangoUpdateListenerAnonymousInnerClassHelper outerInstance;

				private TangoEvent args;

				public RunnableAnonymousInnerClassHelper3(OnTangoUpdateListenerAnonymousInnerClassHelper outerInstance, TangoEvent args)
				{
					this.outerInstance = outerInstance;
					this.args = args;
				}

				public  void run()
				{
					outerInstance.outerInstance.mTangoEventTextView.Text = args.EventKey + ": " + args.EventValue;
				}
			}

            public IntPtr Handle
            {
                get { throw new NotImplementedException(); }
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
	}

}