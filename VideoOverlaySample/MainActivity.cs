
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
namespace com.projecttango.videooverlaysample
{

    using Com.Google.Atap.Tangoservice;
    using Android.Graphics;
	using Android.OS;
	using Android.Views;
    using SurfaceHolder = Android.Views.ISurfaceHolder;
    using Android.App;

    [Activity(Label = "VideoOverlay",
       MainLauncher = true,
       Icon = "@drawable/icon")]
    public class MainActivity : Activity,  Android.Views.ISurfaceHolderCallback
	{

		private SurfaceView surfaceView;
		private SurfaceHolder surfaceHolder;
		private Tango mTango;
   

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_main);
			surfaceView = (SurfaceView) FindViewById(Resource.Id.cameraView);
			surfaceHolder = surfaceView.Holder;
            surfaceHolder.AddCallback(this);
			mTango = new Tango(this);
		}

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height) { }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Surface surface = holder.Surface;
            if (surface.IsValid)
            {
                TangoConfig config = new TangoConfig();
                config = mTango.GetConfig(TangoConfig.ConfigTypeCurrent);
                mTango.ConnectSurface(0, surface);
                mTango.Connect(config);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder){}
    }

}