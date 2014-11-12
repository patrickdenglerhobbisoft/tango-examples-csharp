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

namespace com.projecttango.areadescriptionjava
{
    using Com.Google.Atap.Tangoservice;
    using Android.Content;
	using Android.Views;
    using Android.App;
    using Android.OS;
    using Android.Widget;
    using R = Resource;
    using id = Resource.Id;
    using Layout = Resource.Layout;
    using _string = Resource.String;
	public class ADStartActivity : Activity, View.IOnClickListener
	{

		public static string USE_AREA_LEARNING = "com.projecttango.areadescriptionjava.usearealearning";
		public static string LOAD_ADF = "com.projecttango.areadescriptionjava.loadadf";
		public const string EXTRA_KEY_PERMISSIONTYPE = "PERMISSIONTYPE";
		public const string EXTRA_VALUE_MOTION_TRACKING = "MOTION_TRACKING_PERMISSION";
		public const string EXTRA_VALUE_ADF = "ADF_LOAD_SAVE_PERMISSION";
		public const string EXTRA_VALUE_IMPORT = "ADF_IMPORT_PERMISSION";
		public const string EXTRA_VALUE_EXPORT = "ADF_EXPORT_PERMISSION";
		private ToggleButton mLearningModeToggleButton;
		private ToggleButton mLoadADFToggleButton;
		private Button mStartButton;
		private bool mIsUseAreaLearning;
		private bool mIsLoadADF;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			this.SetContentView( Resource.Layout.start_activity);
            Title = "Area Defenition";
			mLearningModeToggleButton = (ToggleButton) FindViewById(Resource.Id.learningmode);
			mLoadADFToggleButton = (ToggleButton) FindViewById(Resource.Id.loadadf);
			mStartButton = (Button) FindViewById(Resource.Id.start);
            FindViewById(Resource.Id.ADFListView).Click += OnClick;
            mLearningModeToggleButton.Click += OnClick;
            mLoadADFToggleButton.Click += OnClick;
            mStartButton.Click += OnClick;
			mIsUseAreaLearning = mLearningModeToggleButton.Checked;
			mIsLoadADF = mLoadADFToggleButton.Checked;

			Intent permissionIntent = new Intent();
			permissionIntent.SetAction("android.intent.action.REQUEST_TANGO_PERMISSION");
			permissionIntent.PutExtra(EXTRA_KEY_PERMISSIONTYPE, EXTRA_VALUE_MOTION_TRACKING);
			StartActivityForResult(permissionIntent, 0);
			permissionIntent.PutExtra(EXTRA_KEY_PERMISSIONTYPE, EXTRA_VALUE_ADF);
			StartActivityForResult(permissionIntent, 1);

		}

		private void StartAreaDescriptionActivity()
		{
			Intent startADIntent = new Intent(this, typeof(AreaDescription));
			startADIntent.PutExtra(USE_AREA_LEARNING, mIsUseAreaLearning);
			startADIntent.PutExtra(LOAD_ADF, mIsLoadADF);
			StartActivity(startADIntent);
		}

		private void StartADFListView()
		{
			Intent startADFListViewIntent = new Intent(this, typeof(ADFUUIDListViewActivity));
			StartActivity(startADFListViewIntent);
		}
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            // Check which request we're responding to
            if (requestCode == 0)
            {
                // Make sure the request was successful
                if (resultCode == Result.Canceled)
                {
                    Toast.MakeText(this, R.String.motiontracking_permission, Android.Widget.ToastLength.Short).Show();
                    Finish();
                }
            }
            else if (requestCode == 1)
            {
                // Make sure the request was successful
                if (resultCode == Result.Canceled)
                {
                    Toast.MakeText(this, R.String.arealearning_permission, Android.Widget.ToastLength.Short).Show();
                    Finish();
                }
            }
        }


        void OnClick(object sender, System.EventArgs e)
        {
            // manage signature mismatch of interface (fix interface later)
            OnClick(sender as View);
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.loadadf:
                    mIsLoadADF = mLoadADFToggleButton.Checked;
                    break;
                case Resource.Id.learningmode:
                    mIsUseAreaLearning = mLearningModeToggleButton.Checked;
                    break;
                case Resource.Id.start:
                    StartAreaDescriptionActivity();
                    break;
                case Resource.Id.ADFListView:
                    StartADFListView();
                    break;
            }
        }
    }

}