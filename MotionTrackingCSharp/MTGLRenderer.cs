﻿/*
 * Copyright 2014 Google Inc. All Rights Reserved.
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

    using Renderer = com.projecttango.tangoutils.Renderer;
    using CameraFrustumAndAxis = com.projecttango.tangoutils.renderables.CameraFrustumAndAxis;
    using CameraFrustum = com.projecttango.tangoutils.renderables.CameraFrustum;
    using Grid = com.projecttango.tangoutils.renderables.Grid;
    using Trajectory = com.projecttango.tangoutils.renderables.Trajectory;

    using GLES20 = Android.Opengl.GLES20;
    using GLSurfaceView = Android.Opengl.GLSurfaceView;
    using Matrix = Android.Opengl.Matrix;
    using Android.Runtime;
    using Javax.Microedition.Khronos.Egl;
    using Javax.Microedition.Khronos.Opengles;
    using System;




    /// <summary>
    /// OpenGL rendering class for the Motion Tracking API sample. This class manages the objects
    /// visible in the OpenGL view which are the <seealso cref="CameraFrustum"/>, <seealso cref="CameraFrustumAndAxis"/>,
    /// <seealso cref="Trajectory"/>, and the <seealso cref="Grid"/>. These objects are implemented in the TangoUtils library
    /// in the package <seealso cref="com.projecttango.tangoutils.renderables"/>.
    /// 
    /// This class receives also handles the user-selected camera view, which can be 1st person, 3rd
    /// person, or top-down.
    /// </summary>
    public class MTGLRenderer : Renderer, GLSurfaceView.IRenderer
	{

		private Trajectory mTrajectory;
		private CameraFrustum mCameraFrustum;
		private CameraFrustumAndAxis mCameraFrustumAndAxis;
		private Grid mFloorGrid;
		private bool mIsValid = false;
		
  

        public void OnDrawFrame(IGL10 gl)
        {
            //  lock (MotionTrackingActivity.sharedLock) // TODO: How can I grab this shared lock?
            {
                GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
                mTrajectory.draw(ViewMatrix, mProjectionMatrix);
                mFloorGrid.draw(ViewMatrix, mProjectionMatrix);
                mCameraFrustumAndAxis.draw(ViewMatrix, mProjectionMatrix);
            }
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            GLES20.GlViewport(0, 0, width, height);
            mCameraAspect = (float)width / height;
            Matrix.PerspectiveM(mProjectionMatrix, 0, THIRD_PERSON_FOV, mCameraAspect, CAMERA_NEAR, CAMERA_FAR);
        }

        public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
        {
            // Set background color and enable depth testing
            GLES20.GlClearColor(1f, 1f, 1f, 1.0f);  // TODO: Review is this GLES20.Glcle?
            GLES20.GlEnable(GLES20.GlDepthTest);

            // resetModelMatCalculator();
            mCameraFrustum = new CameraFrustum();
            mFloorGrid = new Grid();
            mCameraFrustumAndAxis = new CameraFrustumAndAxis();
            mTrajectory = new Trajectory(3);

            // Construct the initial view matrix
            Matrix.SetIdentityM(mViewMatrix, 0);
            Matrix.SetLookAtM(mViewMatrix, 0, 5f, 5f, 5f, 0f, 0f, 0f, 0f, 1f, 0f);
            mCameraFrustumAndAxis.ModelMatrix = ModelMatCalculator.ModelMatrix;
            mIsValid = true;
        }

        void IDisposable.Dispose()
        {
          //  throw new NotImplementedException();
        }

        public virtual CameraFrustum CameraFrustum
		{
			get
			{
				return mCameraFrustum;
			}
		}

		public virtual CameraFrustumAndAxis CameraFrustumAndAxis
		{
			get
			{
				return mCameraFrustumAndAxis;
			}
		}

		public virtual Trajectory Trajectory
		{
			get
			{
				return mTrajectory;
			}
		}

		public virtual bool Valid
		{
			get
			{
				return mIsValid;
			}
		}

        IntPtr IJavaObject.Handle { get; }
    
        //IntPtr IJavaObject.Handle
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }

}