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
namespace com.projecttango.videooverlaysample
{

	using Activity = Android.App.Activity;
	using Bundle = Android.OS.Bundle;
	using Surface = Android.Views.Surface;
    using SurfaceHolder = Android.Views.ISurfaceHolder;
	using SurfaceView = Android.Views.SurfaceView;



    public class MainActivity : Activity, Android.Views.ISurfaceHolder, Android.Views.ISurfaceHolderCallback
	{

		private SurfaceView surfaceView;
		private SurfaceHolder surfaceHolder;
		private Tango mTango;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			//ContentView = Resource.Layout.activity_main; TODO: Content View
			surfaceView = (SurfaceView) FindViewById(Resource.Id.cameraView);
			surfaceHolder = surfaceView.Holder;
            surfaceHolder.AddCallback(this);
			mTango = new Tango(this);
		}

		public  void SurfaceCreated(SurfaceHolder holder)
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

		

        public void SurfaceChanged(SurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public void SurfaceDestroyed(SurfaceHolder holder)
        {
            throw new System.NotImplementedException();
        }

        public void AddCallback(Android.Views.ISurfaceHolderCallback callback)
        {
            throw new System.NotImplementedException();
        }

        public bool IsCreating
        {
            get { throw new System.NotImplementedException(); }
        }

        public Android.Graphics.Canvas LockCanvas(Android.Graphics.Rect dirty)
        {
            throw new System.NotImplementedException();
        }

        public Android.Graphics.Canvas LockCanvas()
        {
            throw new System.NotImplementedException();
        }

        public void RemoveCallback(Android.Views.ISurfaceHolderCallback callback)
        {
            throw new System.NotImplementedException();
        }

        public void SetFixedSize(int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public void SetFormat(Android.Graphics.Format format)
        {
            throw new System.NotImplementedException();
        }

        public void SetKeepScreenOn(bool screenOn)
        {
            throw new System.NotImplementedException();
        }

        public void SetSizeFromLayout()
        {
            throw new System.NotImplementedException();
        }

        public void SetType(Android.Views.SurfaceType type)
        {
            throw new System.NotImplementedException();
        }

        public Surface Surface
        {
            get { throw new System.NotImplementedException(); }
        }

        public Android.Graphics.Rect SurfaceFrame
        {
            get { throw new System.NotImplementedException(); }
        }

        public void UnlockCanvasAndPost(Android.Graphics.Canvas canvas)
        {
            throw new System.NotImplementedException();
        }
    }

}