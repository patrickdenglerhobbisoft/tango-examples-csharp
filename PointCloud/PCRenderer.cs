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

namespace com.projecttango.pointcloudcsharp
{

    using Android.Content;
    using Android.Widget;
    using Android.Views;
    using Android.App;
    using Android.OS;
    using Com.Google.Atap.Tangoservice;
    using com.projecttango.tangoutils;
    using com.projecttango.tangoutils.renderables;
    using Android.Opengl;
    using System;

    /// <summary>
    /// OpenGL rendering class for the Motion Tracking API sample. This class
    /// managers the objects visible in the OpenGL view which are the
    /// <seealso cref="CameraFrustum"/>, <seealso cref="PointCloud"/> and the <seealso cref="Grid"/>. These objects
    /// are implemented in the TangoUtils library in the package
    /// <seealso cref="com.projecttango.tangoutils.renderables"/>.
    /// 
    /// This class receives <seealso cref="TangoPose"/> data from the <seealso cref="MotionTracking"/>
    /// class and updates the model and view matrices of the <seealso cref="Renderable"/>
    /// objects appropriately. It also handles the user-selected camera view, which
    /// can be 1st person, 3rd person, or top-down.
    /// 
    /// </summary>
    public class PCRenderer  :  Renderer,  GLSurfaceView.IRenderer
	{

		private PointCloud mPointCloud;
		private Grid mGrid;
		private CameraFrustumAndAxis mCameraFrustumAndAxis;
		private int mMaxDepthPoints;

		public PCRenderer(int maxDepthPoints)
		{
			mMaxDepthPoints = maxDepthPoints;
		}

        void GLSurfaceView.IRenderer.OnSurfaceCreated(Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
 
		{
			GLES20.GlClearColor(1f, 1f, 1f, 1.0f);
			GLES20.GlEnable(GLES20.GlDepthTest);
			mPointCloud = new PointCloud(mMaxDepthPoints);
			mGrid = new Grid();
			mCameraFrustumAndAxis = new CameraFrustumAndAxis();
			Matrix.SetIdentityM(mViewMatrix, 0);
			Matrix.SetLookAtM(mViewMatrix, 0, 5f, 5f, 5f, 0f, 0f, 0f, 0f, 1f, 0f);
			mCameraFrustumAndAxis.ModelMatrix = ModelMatCalculator.ModelMatrix;
		}

       void GLSurfaceView.IRenderer.OnSurfaceChanged(Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height)
 		{
			GLES20.GlViewport(0, 0, width, height);
			mCameraAspect = (float) width / height;
			Matrix.PerspectiveM(mProjectionMatrix, 0, CAMERA_FOV, mCameraAspect, CAMERA_NEAR, CAMERA_FAR);
		}

		    void GLSurfaceView.IRenderer.OnDrawFrame(Javax.Microedition.Khronos.Opengles.IGL10 gl)
		{
			GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
			mGrid.draw(mViewMatrix, mProjectionMatrix);
			mPointCloud.draw(mViewMatrix, mProjectionMatrix);
			mCameraFrustumAndAxis.draw(mViewMatrix, mProjectionMatrix);
		}

		public virtual PointCloud PointCloud
		{
			get
			{
				return mPointCloud;
			}
		}

        public IntPtr Handle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        void System.IDisposable.Dispose()
        {
           
        }

        
    }

}