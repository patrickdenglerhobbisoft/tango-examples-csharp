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

	/// <summary>
	/// <seealso cref="Renderable"/> OpenGL object showing the Camera Frustum in 3D. This shows
	/// the view of the Tango camera at the current translation and rotation.
	/// </summary>
	public class CameraFrustum : Renderable
	{

		private const int COORDS_PER_VERTEX = 3;

		private const string sVertexShaderCode = "uniform mat4 uMVPMatrix;" + "attribute vec4 vPosition;" + "attribute vec4 aColor;" + "varying vec4 vColor;" + "void main() {" + "  vColor=aColor;" + "gl_Position = uMVPMatrix * vPosition;" + "}";

		private const string sFragmentShaderCode = "precision mediump float;" + "varying vec4 vColor;" + "void main() {" + "gl_FragColor = vec4(0.8,0.5,0.8,1);" + "}";

		private FloatBuffer mVertexBuffer, mColorBuffer;

		private float[] mVertices = {};

		private float[] mColors = {};

		private readonly int mProgram;
		private int mPosHandle, mColorHandle;
		private int mMVPMatrixHandle;

		public CameraFrustum()
		{
			// Reset the model matrix to the identity
			Matrix.SetIdentityM(ModelMatrix, 0);

			// Load the vertices into a vertex buffer
			ByteBuffer byteBuf = ByteBuffer.AllocateDirect(mVertices.Length * 4);
			byteBuf.Order(ByteOrder.NativeOrder());
			mVertexBuffer = byteBuf.AsFloatBuffer();
			mVertexBuffer.Put(mVertices);
			mVertexBuffer.Position(0);

			// Load the colors into a color buffer
			ByteBuffer cByteBuff = ByteBuffer.AllocateDirect(mColors.Length * 4);
			cByteBuff.Order(ByteOrder.NativeOrder());
			mColorBuffer = cByteBuff.AsFloatBuffer();
			mColorBuffer.Put(mColors);
			mColorBuffer.Position(0);

			// Load the vertex and fragment shaders, then link the program
			int vertexShader = RenderUtils.loadShader(GLES20.GlVertexShader, sVertexShaderCode);
			int fragShader = RenderUtils.loadShader(GLES20.GlFragmentShader, sFragmentShaderCode);
			mProgram = GLES20.GlCreateProgram();
			GLES20.GlAttachShader(mProgram, vertexShader);
			GLES20.GlAttachShader(mProgram, fragShader);
			GLES20.GlLinkProgram(mProgram);
		}

		public override void draw(float[] viewMatrix, float[] projectionMatrix)
		{
			GLES20.GlUseProgram(mProgram);
			// updateViewMatrix(viewMatrix);

			// Compose the model, view, and projection matrices into a single mvp
			// matrix
			updateMvpMatrix(viewMatrix, projectionMatrix);

			// Load vertex attribute data
			mPosHandle = GLES20.GlGetAttribLocation(mProgram, "vPosition");
			GLES20.GlVertexAttribPointer(mPosHandle, COORDS_PER_VERTEX, GLES20.GlFloat, false, 0, mVertexBuffer);
			GLES20.GlEnableVertexAttribArray(mPosHandle);

			// Load color attribute data
			mColorHandle = GLES20.GlGetAttribLocation(mProgram, "aColor");
			GLES20.GlVertexAttribPointer(mColorHandle, 4, GLES20.GlFloat, false, 0, mColorBuffer);
			GLES20.GlEnableVertexAttribArray(mColorHandle);

			// Draw the CameraFrustum
			mMVPMatrixHandle = GLES20.GlGetUniformLocation(mProgram, "uMVPMatrix");
			GLES20.GlUniformMatrix4fv(mMVPMatrixHandle, 1, false, MvpMatrix, 0);
			GLES20.GlLineWidth(1);
			GLES20.GlDrawArrays(GLES20.GlLines, 0, 16);
		}
	}
}