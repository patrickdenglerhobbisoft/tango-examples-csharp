using System;

/*
 * Copyright 2014 HobbiSoft. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.Apache.Org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace com.projecttango.tangoutils.renderables
{
    using Java.Nio;


    using GLES20 = Android.Opengl.GLES20;
	using Matrix = Android.Opengl.Matrix;
	using Log = Android.Util.Log;

	/// <summary>
	/// <seealso cref="Renderable"/> OpenGL object showing the Trajectory of the Project Tango
	/// device in 3D space. Points are added when the trajectory is updated by
	/// passing translation data obtained from Tango Pose Data.
	/// </summary>
	public class Trajectory : Renderable
	{

		private const int COORDS_PER_VERTEX = 3;
		private const float MIN_DISTANCE_CHECK = 0.025f;

		/// <summary>
		/// Note: due to resetPath() logic, keep this as a multiple of 9 * </summary>
		private const int MAX_VERTICES = 9000;
		private const int BYTES_PER_FLOAT = 4;
		private static int mTrajectoryCount = 0;

		private static readonly string TAG = typeof(Trajectory).Name;
		private string mVertexShaderCode = "uniform mat4 uMVPMatrix;" + "attribute vec4 vPosition;" + "uniform vec4 aColor;" + "varying vec4 vColor;" + "void main() {" + "gl_PointSize = 5.0;" + "vColor=aColor;" + "gl_Position = uMVPMatrix * vPosition;" + "}";
		private string mFragmentShaderCode = "precision mediump float;" + "varying vec4 vColor;" + "void main() {" + "gl_FragColor = vColor;" + "}";
        private FloatBuffer mVertexBuffer;
		private float[] mColor = new float[] {0.22f, 0.28f, 0.67f, 1.0f};
		private readonly int mProgram;
		private int mPosHandle;
		private int mMVPMatrixHandle;
		private int mColorHandle;
		private int mLineWidth;

		public Trajectory(int lineWidth)
		{
			mLineWidth = lineWidth;
			// Reset the model matrix to the identity
			Matrix.SetIdentityM(ModelMatrix, 0);

			// Allocate a vertex buffer
			ByteBuffer vertexByteBuffer = ByteBuffer.AllocateDirect(MAX_VERTICES * BYTES_PER_FLOAT);
			vertexByteBuffer.Order(ByteOrder.NativeOrder());
			mVertexBuffer = vertexByteBuffer.AsFloatBuffer();

			// Load the vertex and fragment shaders, then link the program
			int vertexShader = RenderUtils.loadShader(GLES20.GlVertexShader, mVertexShaderCode);
			int fragShader = RenderUtils.loadShader(GLES20.GlFragmentShader, mFragmentShaderCode);
			mProgram = GLES20.GlCreateProgram();
			GLES20.GlAttachShader(mProgram, vertexShader);
			GLES20.GlAttachShader(mProgram, fragShader);
			GLES20.GlLinkProgram(mProgram);
		}

		// float[] color should contain only 4 elements.
		public Trajectory(int lineWidth, float[] color)
		{
			mLineWidth = lineWidth;
			mColor = color;
			// Reset the model matrix to the identity
			Matrix.SetIdentityM(ModelMatrix, 0);

			// Allocate a vertex buffer
			ByteBuffer vertexByteBuffer = ByteBuffer.AllocateDirect(MAX_VERTICES * BYTES_PER_FLOAT);
			vertexByteBuffer.Order(ByteOrder.NativeOrder());
			mVertexBuffer = vertexByteBuffer.AsFloatBuffer();

			// Load the vertex and fragment shaders, then link the program
			int vertexShader = RenderUtils.loadShader(GLES20.GlVertexShader, mVertexShaderCode);
			int fragShader = RenderUtils.loadShader(GLES20.GlFragmentShader, mFragmentShaderCode);
			mProgram = GLES20.GlCreateProgram();
			GLES20.GlAttachShader(mProgram, vertexShader);
			GLES20.GlAttachShader(mProgram, fragShader);
			GLES20.GlLinkProgram(mProgram);
		}

		public virtual void updateTrajectory(float[] translation)
		{
			mVertexBuffer.Position(mTrajectoryCount * 3);
			if (((mTrajectoryCount + 1) * 3) >= MAX_VERTICES)
			{
				Log.Warn(TAG, "Clearing float buffer");
				resetPath();
			}
			float dx = 0, dy = 0, dz = 0;
			try
			{
				dx = mVertexBuffer.Get(mVertexBuffer.Position() - 3) - translation[0];
				dy = mVertexBuffer.Get(mVertexBuffer.Position() - 2) - translation[2];
				dz = mVertexBuffer.Get(mVertexBuffer.Position() - 1) - (-translation[1]);
			}
			catch (System.IndexOutOfRangeException)
			{
				mVertexBuffer.Put(new float[] {translation[0], translation[2], -translation[1]});
				mTrajectoryCount++;
			}
			float distance = (float) Math.Sqrt(dx * dx + dy * dy + dz * dz);
			if (distance > MIN_DISTANCE_CHECK)
			{
				mVertexBuffer.Put(new float[] {translation[0], translation[2], -translation[1]});
				mTrajectoryCount++;
			}
		}

		public virtual void resetPath()
		{
			int currentPosition = mVertexBuffer.Position();
			int pointsToGet = (MAX_VERTICES / 3);
			mVertexBuffer.Position(currentPosition - pointsToGet);

			float[] tail = new float[pointsToGet];
			mVertexBuffer.Get(tail, 0, pointsToGet);

			mVertexBuffer.Clear();
			mVertexBuffer.Put(tail);

			mTrajectoryCount = pointsToGet / 3;
		}

		public virtual void clearPath()
		{
			ByteBuffer vertexByteBuffer = ByteBuffer.AllocateDirect(MAX_VERTICES * BYTES_PER_FLOAT);
			vertexByteBuffer.Order(ByteOrder.NativeOrder());
			mVertexBuffer = vertexByteBuffer.AsFloatBuffer();
		}

		public override void draw(float[] viewMatrix, float[] projectionMatrix)
		{
			GLES20.GlUseProgram(mProgram);
			mVertexBuffer.Position(0);

			// Compose the model, view, and projection matrices into a single m-v-p
			// matrix
			updateMvpMatrix(viewMatrix, projectionMatrix);

			// Load vertex attribute data
			mPosHandle = GLES20.GlGetAttribLocation(mProgram, "vPosition");
			GLES20.GlVertexAttribPointer(mPosHandle, COORDS_PER_VERTEX, GLES20.GlFloat, false, 0, mVertexBuffer);
			GLES20.GlEnableVertexAttribArray(mPosHandle);

			mMVPMatrixHandle = GLES20.GlGetUniformLocation(mProgram, "uMVPMatrix");
			GLES20.GlUniformMatrix4fv(mMVPMatrixHandle, 1, false, MvpMatrix, 0);

			mColorHandle = GLES20.GlGetUniformLocation(mProgram, "aColor");
			GLES20.GlUniform4f(mColorHandle, mColor[0], mColor[1], mColor[2], mColor[3]);
			GLES20.GlLineWidth(mLineWidth);
			GLES20.GlDrawArrays(GLES20.GlLineStrip, 0, mTrajectoryCount);
		}

		public virtual float[] Color
		{
			set
			{
				mColor = value;
			}
		}
	}
}