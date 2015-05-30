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
	/// <seealso cref="Renderable"/> OpenGL object showing the 'floor' of the current scene.
	/// This is a static grid placed in the scene to provide perspective in the
	/// various views.
	/// </summary>
	public class Grid : Renderable
	{

		private const int COORDS_PER_VERTEX = 3;
		private const int GRID_RANGE_M = 100;
		private const int BYTES_PER_FLOAT = 4;

		private const string sVertexShaderCode = "uniform mat4 uMVPMatrix;" + "attribute vec4 vPosition;" + "void main() {" + "gl_Position = uMVPMatrix * vPosition;" + "}";
		private const string sFragmentShaderCode = "precision mediump float;" + "uniform vec4 vColor;" + "void main() {" + " gl_FragColor = vec4(0.8,0.8,0.8,1.0);" + "}";

		private FloatBuffer mVertexBuffer;
		private readonly int mProgram;
		private int mPosHandle;
		private int mMVPMatrixHandle;

		public Grid()
		{
			// Reset the model matrix to the identity
			Matrix.SetIdentityM(ModelMatrix, 0);

			// Allocate a vertex buffer
			ByteBuffer vertexByteBuffer = ByteBuffer.AllocateDirect((GRID_RANGE_M * 2 + 1) * 4 * 3 * BYTES_PER_FLOAT);
			vertexByteBuffer.Order(ByteOrder.NativeOrder());
			mVertexBuffer = vertexByteBuffer.AsFloatBuffer();

			// Load the vertices for the z-axis grid lines into the vertex buffer
			for (int x = -GRID_RANGE_M; x <= GRID_RANGE_M; x++)
			{
				mVertexBuffer.Put(new float[] {x, -1.3f, (float) - GRID_RANGE_M});
				mVertexBuffer.Put(new float[] {x, -1.3f, (float) GRID_RANGE_M});
			}

			// Load the vertices for the x-axis grid lines into the vertex buffer
			for (int z = -GRID_RANGE_M; z <= GRID_RANGE_M; z++)
			{
				mVertexBuffer.Put(new float[] {(float) - GRID_RANGE_M, -1.3f, z});
				mVertexBuffer.Put(new float[] {(float) GRID_RANGE_M, -1.3f, z});
			}

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
			mVertexBuffer.Position(0);

			// Compose the model, view, and projection matrices into a single m-v-p
			// matrix
			updateMvpMatrix(viewMatrix, projectionMatrix);

			// Load vertex attribute data
			mPosHandle = GLES20.GlGetAttribLocation(mProgram, "vPosition");
			GLES20.GlVertexAttribPointer(mPosHandle, COORDS_PER_VERTEX, GLES20.GlFloat, false, 0, mVertexBuffer);
			GLES20.GlEnableVertexAttribArray(mPosHandle);

			// Draw the Grid
			mMVPMatrixHandle = GLES20.GlGetUniformLocation(mProgram, "uMVPMatrix");
			GLES20.GlUniformMatrix4fv(mMVPMatrixHandle, 1, false, MvpMatrix, 0);
			GLES20.GlLineWidth(1);
			GLES20.GlDrawArrays(GLES20.GlLines, 0, (GRID_RANGE_M * 2 + 1) * 4);
		}

	}
}