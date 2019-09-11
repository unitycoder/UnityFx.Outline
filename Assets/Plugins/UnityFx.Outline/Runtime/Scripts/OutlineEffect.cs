﻿// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Renders outlines at specific camera. Should be attached to camera to function.
	/// </summary>
	/// <seealso cref="OutlineLayer"/>
	/// <seealso cref="OutlineBehaviour"/>
	/// <seealso cref="OutlineSettings"/>
	/// <seealso cref="https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/"/>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public sealed partial class OutlineEffect : MonoBehaviour
	{
		#region data

		[SerializeField]
		private OutlineResources _outlineResources;
		[SerializeField]
		private OutlineLayerCollection _outlineLayers;

		private CommandBuffer _commandBuffer;
		private bool _changed;

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets resources used by the effect implementation.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if setter argument is <see langword="null"/>.</exception>
		public OutlineResources OutlineResources
		{
			get
			{
				return _outlineResources;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("OutlineResources");
				}

				if (_outlineResources != value)
				{
					_outlineResources = value;
					_changed = true;
				}
			}
		}

		/// <summary>
		/// Gets or sets outline layers.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if setter argument is <see langword="null"/>.</exception>
		/// <seealso cref="ShareLayersWith(OutlineEffect)"/>
		public OutlineLayerCollection OutlineLayers
		{
			get
			{
				CreateLayersIfNeeded();
				return _outlineLayers;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("OutlineLayers");
				}

				if (_outlineLayers != value)
				{
					_outlineLayers = value;
					_changed = true;
				}
			}
		}

		/// <summary>
		/// Shares <see cref="OutlineLayers"/> with another <see cref="OutlineEffect"/> instance.
		/// </summary>
		/// <param name="other">Effect to share <see cref="OutlineLayers"/> with.</param>
		/// <seealso cref="OutlineLayers"/>
		public void ShareLayersWith(OutlineEffect other)
		{
			if (other)
			{
				CreateLayersIfNeeded();

				other._outlineLayers = _outlineLayers;
				other._changed = true;
			}
		}

		/// <summary>
		/// Detects changes in nested assets and updates outline if needed. The actual update might not be invoked until the next frame.
		/// </summary>
		public void UpdateChanged()
		{
			if (_outlineLayers)
			{
				_outlineLayers.UpdateChanged();
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
		}

		private void OnEnable()
		{
			var camera = GetComponent<Camera>();

			if (camera)
			{
				_commandBuffer = new CommandBuffer();
				_commandBuffer.name = string.Format("{0} - {1}", GetType().Name, name);
				_changed = true;

				camera.AddCommandBuffer(OutlineRenderer.RenderEvent, _commandBuffer);
			}
		}

		private void OnDisable()
		{
			var camera = GetComponent<Camera>();

			if (camera)
			{
				camera.RemoveCommandBuffer(OutlineRenderer.RenderEvent, _commandBuffer);
			}

			if (_commandBuffer != null)
			{
				_commandBuffer.Dispose();
				_commandBuffer = null;
			}
		}

		private void Update()
		{
#if UNITY_EDITOR

			UpdateChanged();

#endif

			if (_outlineLayers && (_changed || _outlineLayers.IsChanged))
			{
				FillCommandBuffer();
			}
		}

		private void LateUpdate()
		{
			// TODO: Find a way to do this once per OutlineLayerCollection instance.
			if (_outlineLayers)
			{
				_outlineLayers.AcceptChanges();
			}
		}

		private void OnDestroy()
		{
			if (_outlineLayers)
			{
				_outlineLayers.Reset();
			}
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			_changed = true;
		}

		private void Reset()
		{
			_outlineLayers = null;
			_changed = true;
		}

#endif

		#endregion

		#region implementation

		private void FillCommandBuffer()
		{
			if (_outlineResources && _outlineResources.IsValid)
			{
				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					_outlineLayers.Render(renderer, _outlineResources);
				}
			}
			else
			{
				_commandBuffer.Clear();
			}

			_changed = false;
		}

		private void CreateLayersIfNeeded()
		{
			if (ReferenceEquals(_outlineLayers, null))
			{
				_outlineLayers = ScriptableObject.CreateInstance<OutlineLayerCollection>();
				_outlineLayers.name = "OutlineLayers";
			}
		}

		#endregion
	}
}
