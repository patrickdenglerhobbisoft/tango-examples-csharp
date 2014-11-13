
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



namespace com.projecttango.areadescriptioncsharp
{
    using System;
    using System.Collections.Generic;
    using Java.Lang;
    using Android.Opengl;
    using Android.Util;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Views;
    
    using Android.OS;
    using Android.Widget;
	using SetNameCommunicator = com.projecttango.areadescriptioncsharp.SetADFNameDialog.SetNameCommunicator;

    using Com.Google.Atap.Tangoservice;


	/// <summary>
	/// Main Activity class for the Area Learning API Sample. Handles the connection
	/// to the Tango service and propagation of Tango pose data to OpenGL and Layout
	/// views. OpenGL rendering logic is delegated to the <seealso cref="ADRenderer"/> class.
	/// </summary>
	public class AreaDescription : Activity, View.IOnClickListener, SetNameCommunicator
	{

		private static readonly string TAG = typeof(AreaDescription).Name;
		private const int SECONDS_TO_MILLI = 1000;
		private Tango mTango;
		private TangoConfig mConfig;
        private TextView mTangoEventTextView;

        public TextView TangoEventTextView
        {
            get { return mTangoEventTextView; }
        
        }
		private TextView mStart2DeviceTranslationTextView;
		private TextView mAdf2DeviceTranslationTextView;
		private TextView mAdf2StartTranslationTextView;
		private TextView mStart2DeviceQuatTextView;
		private TextView mAdf2DeviceQuatTextView;
		private TextView mAdf2StartQuatTextView;
		private TextView mTangoServiceVersionTextView;
		private TextView mApplicationVersionTextView;
		private TextView mUUIDTextView;
		private TextView mStart2DevicePoseStatusTextView;
		private TextView mAdf2DevicePoseStatusTextView;
		private TextView mAdf2StartPoseStatusTextView;
		private TextView mStart2DevicePoseCountTextView;
		private TextView mAdf2DevicePoseCountTextView;
		private TextView mAdf2StartPoseCountTextView;
		private TextView mStart2DevicePoseDeltaTextView;
		private TextView mAdf2DevicePoseDeltaTextView;
		private TextView mAdf2StartPoseDeltaTextView;

		private Button mSaveAdf;
		private Button mFirstPersonButton;
		private Button mThirdPersonButton;
		private Button mTopDownButton;

		private int mStart2DevicePoseCount;
		private int mAdf2DevicePoseCount;
		private int mAdf2StartPoseCount;

		private double mStart2DevicePoseDelta;
		private double mAdf2DevicePoseDelta;
		private double mAdf2StartPoseDelta;
		private double mStart2DevicePreviousPoseTimeStamp;
		private double mAdf2DevicePreviousPoseTimeStamp;
		private double mAdf2StartPreviousPoseTimeStamp;

		private bool mIsRelocalized;
		private bool mIsLearningMode;
		private bool mIsConstantSpaceRelocalize;
		private string mCurrentUUID;

		public ADRenderer mRenderer;
		private GLSurfaceView mGLView;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_area_learning);


            mTangoEventTextView = (TextView)FindViewById(Resource.Id.tangoevent);

			mAdf2DeviceTranslationTextView = (TextView) FindViewById(Resource.Id.adf2devicePose);
			mStart2DeviceTranslationTextView = (TextView) FindViewById(Resource.Id.start2devicePose);
			mAdf2StartTranslationTextView = (TextView) FindViewById(Resource.Id.adf2startPose);
			mAdf2DeviceQuatTextView = (TextView) FindViewById(Resource.Id.adf2deviceQuat);
			mStart2DeviceQuatTextView = (TextView) FindViewById(Resource.Id.start2deviceQuat);
			mAdf2StartQuatTextView = (TextView) FindViewById(Resource.Id.adf2startQuat);

			mAdf2DevicePoseStatusTextView = (TextView) FindViewById(Resource.Id.adf2deviceStatus);
			mStart2DevicePoseStatusTextView = (TextView) FindViewById(Resource.Id.start2deviceStatus);
			mAdf2StartPoseStatusTextView = (TextView) FindViewById(Resource.Id.adf2startStatus);

			mAdf2DevicePoseCountTextView = (TextView) FindViewById(Resource.Id.adf2devicePosecount);
			mStart2DevicePoseCountTextView = (TextView) FindViewById(Resource.Id.start2devicePosecount);
			mAdf2StartPoseCountTextView = (TextView) FindViewById(Resource.Id.adf2startPosecount);

			mAdf2DevicePoseDeltaTextView = (TextView) FindViewById(Resource.Id.adf2deviceDeltatime);
			mStart2DevicePoseDeltaTextView = (TextView) FindViewById(Resource.Id.start2deviceDeltatime);
			mAdf2StartPoseDeltaTextView = (TextView) FindViewById(Resource.Id.adf2startDeltatime);

			mFirstPersonButton = (Button) FindViewById(Resource.Id.first_person_button);
			mThirdPersonButton = (Button) FindViewById(Resource.Id.third_person_button);
			mTopDownButton = (Button) FindViewById(Resource.Id.top_down_button);

			mTangoServiceVersionTextView = (TextView) FindViewById(Resource.Id.version);
			mApplicationVersionTextView = (TextView) FindViewById(Resource.Id.appversion);
			mGLView = (GLSurfaceView) FindViewById(Resource.Id.gl_surface_view);

			mSaveAdf = (Button) FindViewById(Resource.Id.saveAdf);
			mUUIDTextView = (TextView) FindViewById(Resource.Id.uuid);

			mSaveAdf.Visibility =  Android.Views.ViewStates.Gone;
			// Set up button click listeners
			mFirstPersonButton.SetOnClickListener(this);
			mThirdPersonButton.SetOnClickListener(this);
			mTopDownButton.SetOnClickListener(this);

			PackageInfo packageInfo;
			try
			{
			packageInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
				mApplicationVersionTextView.Text = packageInfo.VersionName;
			}
			catch (Android.Content.PM.PackageManager.NameNotFoundException e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			// Configure OpenGL renderer
			mRenderer = new ADRenderer();
			mGLView.SetEGLContextClientVersion(2);
			mGLView.SetRenderer(mRenderer as Android.Opengl.GLSurfaceView.IRenderer);
            mGLView.RenderMode = Android.Opengl.Rendermode.WhenDirty;

			// Instantiate the Tango service
			mTango = new Tango(this);
			mIsRelocalized = false;

			Intent intent = Intent;
			mIsLearningMode = intent.GetBooleanExtra(ADStartActivity.USE_AREA_LEARNING, false);
			mIsConstantSpaceRelocalize = intent.GetBooleanExtra(ADStartActivity.LOAD_ADF, false);
			SetTangoConfig();
		}

		private void SetTangoConfig()
		{
			mConfig = new TangoConfig();
			mConfig = mTango.GetConfig(TangoConfig.ConfigTypeCurrent);
			// Check if learning mode
			if (mIsLearningMode)
			{
				// Set learning mode to config.
				mConfig.PutBoolean(TangoConfig.KeyBooleanLearningmode, true);
				// Set the ADF save button visible.
                mSaveAdf.Visibility = Android.Views.ViewStates.Visible; // TODO: Check to see if ViewStates is correct
				mSaveAdf.SetOnClickListener(this);
			}
			// Check for Load ADF/Constant Space relocalization mode
			if (mIsConstantSpaceRelocalize)
			{
				IList<string> fullUUIDList = new List<string>();
				// Returns a list of ADFs with their UUIDs
				fullUUIDList = mTango.ListAreaDescriptions();
				if (fullUUIDList.Count == 0)
				{
                    mUUIDTextView.Text = GetString(Resource.String.no_uuid);
				}

				// Load the latest ADF if ADFs are found.
				if (fullUUIDList.Count > 0)
				{
					mConfig.PutString(TangoConfig.KeyStringAreadescription, fullUUIDList[fullUUIDList.Count - 1]);
					mUUIDTextView.Text = GetString(Resource.String.number_of_adfs) + fullUUIDList.Count + GetString(Resource.String.latest_adf_is) + fullUUIDList[fullUUIDList.Count - 1];
				}
			}

			// Set the number of loop closures to zero at start.
			mStart2DevicePoseCount = 0;
			mAdf2DevicePoseCount = 0;
			mAdf2StartPoseCount = 0;
			mTangoServiceVersionTextView.Text = mConfig.GetString("tango_service_library_version");
		}

		private void SetUpTangoListeners()
		{

			// Set Tango Listeners for Poses Device wrt Start of Service, Device wrt
			// ADF and Start of Service wrt ADF
			List<TangoCoordinateFramePair> framePairs = new List<TangoCoordinateFramePair>();
			framePairs.Add(new TangoCoordinateFramePair(TangoPoseData.CoordinateFrameStartOfService, TangoPoseData.CoordinateFrameDevice));
			framePairs.Add(new TangoCoordinateFramePair(TangoPoseData.CoordinateFrameAreaDescription, TangoPoseData.CoordinateFrameDevice));
			framePairs.Add(new TangoCoordinateFramePair(TangoPoseData.CoordinateFrameAreaDescription, TangoPoseData.CoordinateFrameStartOfService));
            var listener = new TangoProxy.TangoListener(this);
            listener.OnPoseAvailableCallback = OnPoseAvailable;
            listener.OnTangoEventCallBack = OnTangoEvent;
     
            mTango.ConnectListener(framePairs, listener);
		}


        public void OnPoseAvailable(TangoPoseData pose)
        {

            // Update the text views with Pose info.
            updateTextViewWith(pose);
            bool updateRenderer = false;
            if (mIsRelocalized)
            {
                if (pose.BaseFrame == TangoPoseData.CoordinateFrameAreaDescription && pose.TargetFrame == TangoPoseData.CoordinateFrameDevice)
                {
                    updateRenderer = true;
                }
            }
            else
            {
                if (pose.BaseFrame == TangoPoseData.CoordinateFrameStartOfService && pose.TargetFrame == TangoPoseData.CoordinateFrameDevice)
                {
                    updateRenderer = true;
                }
            }

            // Update the trajectory, model matrix, and view matrix, then
            // render the scene again
            if (updateRenderer && (mRenderer.Trajectory != null))
            {
                float[] translation = pose.GetTranslationAsFloats();
                mRenderer.Trajectory.updateTrajectory(translation);
                mRenderer.ModelMatCalculator.updateModelMatrix(translation, pose.GetRotationAsFloats());
                mRenderer.updateViewMatrix();
                mGLView.RequestRender();
            }
        }

        public void OnTangoEvent(TangoEvent args)
        {
            RunOnUiThread(() =>
            {
                mTangoEventTextView.Text = args.EventKey + ": " + args.EventValue;
            });
        }
		private void saveAdf()
		{
			showSetNameDialog();
		}

		private void showSetNameDialog()
		{
			Bundle bundle = new Bundle();
			if (mCurrentUUID == null)
			{
				mCurrentUUID = mTango.SaveAreaDescription();
			}
			Log.Info("UUID", " uuid is: " + mCurrentUUID);
			TangoAreaDescriptionMetaData metaData = mTango.LoadAreaDescriptionMetaData(mCurrentUUID);
			byte[] adfNameBytes = metaData.Get("name");
			if (adfNameBytes != null)
			{
				string fillDialogName = StringHelper.NewString(adfNameBytes);
				bundle.PutString("name", fillDialogName);
			}
			bundle.PutString("id", mCurrentUUID);
			FragmentManager manager = FragmentManager;
			SetADFNameDialog SetADFNameDialog = new SetADFNameDialog();
			SetADFNameDialog.Arguments = bundle;
			SetADFNameDialog.Show(manager, "ADFNameDialog");
		}

		public virtual void SetName(string name, string uuids)
		{

			TangoAreaDescriptionMetaData metadata = new TangoAreaDescriptionMetaData();
			metadata = mTango.LoadAreaDescriptionMetaData(uuids);
			metadata.Set("name", (byte[])(System.Array)name.GetBytes());
			mTango.SaveAreaDescriptionMetadata(uuids, metadata);
			Toast.MakeText(ApplicationContext, GetString(Resource.String.adf_save) + uuids, Android.Widget.ToastLength.Short).Show();
		}

		/// <summary>
		/// Updates the text view in UI screen with the Pose. Each pose is associated
		/// with Target and Base Frame. We need to check for that pair ad update our
		/// views accordingly.
		/// </summary>
		/// <param name="pose"> </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
           // Original line in class:  private void updateTextViewWith(final com.google.atap.tangoservice.TangoPoseData pose)
        private void updateTextViewWith(TangoPoseData pose)
        {

          
            this.RunOnUiThread(() =>
                {
                    string translationString = "[" + threeDec.format(pose.Translation[0]) + "," + threeDec.format(pose.Translation[1]) + "," + threeDec.format(pose.Translation[2]) + "] ";

                    string quaternionString = "[" + threeDec.format(pose.Rotation[0]) + "," + threeDec.format(pose.Rotation[1]) + "," + threeDec.format(pose.Rotation[2]) + "," + threeDec.format(pose.Rotation[3]) + "] ";

                    if (pose.BaseFrame == TangoPoseData.CoordinateFrameAreaDescription && pose.TargetFrame == TangoPoseData.CoordinateFrameDevice)
                    {
                        mAdf2DevicePoseCount++;
                        mAdf2DevicePoseDelta = (pose.Timestamp - mAdf2DevicePreviousPoseTimeStamp) * SECONDS_TO_MILLI;
                        mAdf2DevicePreviousPoseTimeStamp = pose.Timestamp;
                        mAdf2DeviceTranslationTextView.Text = translationString;
                        mAdf2DeviceQuatTextView.Text = quaternionString;
                        mAdf2DevicePoseStatusTextView.Text = getPoseStatus(pose);
                        mAdf2DevicePoseCountTextView.Text = Convert.ToString(mAdf2DevicePoseCount);
                        mAdf2DevicePoseDeltaTextView.Text = threeDec.format(mAdf2DevicePoseDelta);
                    }

                    if (pose.BaseFrame == TangoPoseData.CoordinateFrameStartOfService && pose.TargetFrame == TangoPoseData.CoordinateFrameDevice)
                    {
                        mStart2DevicePoseCount++;
                        mStart2DevicePoseDelta = (pose.Timestamp - mStart2DevicePreviousPoseTimeStamp) * SECONDS_TO_MILLI;
                        mStart2DevicePreviousPoseTimeStamp = pose.Timestamp;
                        mStart2DeviceTranslationTextView.Text = translationString;
                        mStart2DeviceQuatTextView.Text = quaternionString;
                        mStart2DevicePoseStatusTextView.Text = getPoseStatus(pose);
                        mStart2DevicePoseCountTextView.Text = Convert.ToString(mStart2DevicePoseCount);
                        mStart2DevicePoseDeltaTextView.Text = threeDec.format(mStart2DevicePoseDelta);
                    }

                    if (pose.BaseFrame == TangoPoseData.CoordinateFrameAreaDescription && pose.TargetFrame == TangoPoseData.CoordinateFrameStartOfService)
                    {
                        mAdf2StartPoseCount++;
                        mAdf2StartPoseDelta = (pose.Timestamp - mAdf2StartPreviousPoseTimeStamp) * SECONDS_TO_MILLI;
                        mAdf2StartPreviousPoseTimeStamp = pose.Timestamp;
                        mAdf2StartTranslationTextView.Text = translationString;
                        mAdf2StartQuatTextView.Text = quaternionString;
                        mAdf2StartPoseStatusTextView.Text = getPoseStatus(pose);
                        mAdf2StartPoseCountTextView.Text = Convert.ToString(mAdf2StartPoseCount);
                        mAdf2StartPoseDeltaTextView.Text = threeDec.format(mAdf2StartPoseDelta);
                        if (pose.StatusCode == TangoPoseData.PoseValid)
                        {
                            mIsRelocalized = true;
                            // Set the color to green
                            mRenderer.Trajectory.Color = new float[] { 0.39f, 0.56f, 0.03f, 1.0f };
                        }
                        else
                        {
                            mIsRelocalized = false;
                            // Set the color blue
                            mRenderer.Trajectory.Color = new float[] { 0.22f, 0.28f, 0.67f, 1.0f };
                        }
                    }
                });
        }

		private string getPoseStatus(TangoPoseData pose)
		{
			switch (pose.StatusCode)
			{
			case TangoPoseData.PoseInitializing:
				return GetString(Resource.String.pose_initializing);
			case TangoPoseData.PoseInvalid:
				return GetString(Resource.String.pose_invalid);
			case TangoPoseData.PoseValid:
				return GetString(Resource.String.pose_valid);
			default:
				return GetString(Resource.String.pose_unknown);
			}
		}

		protected override void OnPause()
		{
            base.OnPause();
            try
            {
                mTango.Disconnect();
            }
            catch (TangoErrorException)
            {
                Toast.MakeText(ApplicationContext, Resource.String.tango_error, Android.Widget.ToastLength.Short).Show();
            }
		}

		protected override void OnResume()
		{
			base.OnResume();
			try
			{
				SetUpTangoListeners();
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.tango_error, Android.Widget.ToastLength.Short).Show();
			}
			try
			{
				mTango.Connect(mConfig);
			}
			catch (TangoOutOfDateException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.tango_out_of_date_exception, Android.Widget.ToastLength.Short).Show();
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.tango_error, Android.Widget.ToastLength.Short).Show();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		// OnClick Button Listener for all the buttons
		public void OnClick(View v)
		{
			switch (v.Id)
			{
			case Resource.Id.first_person_button:
				mRenderer.SetFirstPersonView();
				break;
			case Resource.Id.top_down_button:
				mRenderer.SetTopDownView();
				break;
			case Resource.Id.third_person_button:
				mRenderer.SetThirdPersonView();
				break;
			case Resource.Id.saveAdf:
				saveAdf();
				break;
			default:
				Log.Wtf(TAG, "Unknown button click");
				return;
			}
		}

		public override bool OnTouchEvent(MotionEvent args)
		{
			return mRenderer.OnTouchEvent(args);
		}
	}

}