﻿// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A serializable collection of outline layers.
	/// </summary>
	/// <seealso cref="OutlineLayer"/>
	/// <seealso cref="OutlineEffect"/>
	/// <seealso cref="OutlineSettings"/>
	[CreateAssetMenu(fileName = "OutlineLayerCollection", menuName = "UnityFx/Outline/Outline Layer Collection")]
	public sealed class OutlineLayerCollection : ScriptableObject, IList<OutlineLayer>, IChangeTracking
	{
		#region data

		[SerializeField, HideInInspector]
		private List<OutlineLayer> _layers = new List<OutlineLayer>();

		private bool _changed = true;

		#endregion

		#region interface

		internal void Reset()
		{
			foreach (var layer in _layers)
			{
				layer.Reset();
			}
		}

		internal void UpdateChanged()
		{
			foreach (var layer in _layers)
			{
				layer.UpdateChanged();
			}
		}

		internal void Render(OutlineRenderer renderer, OutlineResources resources)
		{
			// TODO: Render layers in order defined with layer priorities.
			for (var i = 0; i < _layers.Count; ++i)
			{
				_layers[i].Render(renderer, resources);
			}
		}

		#endregion

		#region ScriptableObject

		private void OnEnable()
		{
			foreach (var layer in _layers)
			{
				layer.SetCollection(this);
			}
		}

		#endregion

		#region IList

		/// <inheritdoc/>
		public OutlineLayer this[int layerIndex]
		{
			get
			{
				return _layers[layerIndex];
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("layer");
				}

				if (layerIndex < 0 || layerIndex >= _layers.Count)
				{
					throw new ArgumentOutOfRangeException("layerIndex");
				}

				if (_layers[layerIndex] != value)
				{
					value.SetCollection(this);

					_layers[layerIndex].SetCollection(null);
					_layers[layerIndex] = value;
					_changed = true;
				}
			}
		}

		/// <inheritdoc/>
		public int IndexOf(OutlineLayer layer)
		{
			if (layer != null)
			{
				return _layers.IndexOf(layer);
			}

			return -1;
		}

		/// <inheritdoc/>
		public void Insert(int index, OutlineLayer layer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}

			if (layer.ParentCollection != this)
			{
				layer.SetCollection(this);

				_layers.Insert(index, layer);
				_changed = true;
			}
		}

		/// <inheritdoc/>
		public void RemoveAt(int index)
		{
			if (index >= 0 && index < _layers.Count)
			{
				_layers[index].SetCollection(null);
				_layers.RemoveAt(index);
				_changed = true;
			}
		}

		#endregion

		#region ICollection

		/// <inheritdoc/>
		public int Count
		{
			get
			{
				return _layers.Count;
			}
		}

		/// <inheritdoc/>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public void Add(OutlineLayer layer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}

			if (layer.ParentCollection != this)
			{
				layer.SetCollection(this);

				_layers.Add(layer);
				_changed = true;
			}
		}

		/// <inheritdoc/>
		public bool Remove(OutlineLayer layer)
		{
			if (_layers.Remove(layer))
			{
				layer.SetCollection(null);

				_changed = true;
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void Clear()
		{
			if (_layers.Count > 0)
			{
				foreach (var layer in _layers)
				{
					layer.SetCollection(null);
				}

				_layers.Clear();
				_changed = true;
			}
		}

		/// <inheritdoc/>
		public bool Contains(OutlineLayer layer)
		{
			if (layer == null)
			{
				return false;
			}

			return _layers.Contains(layer);
		}

		/// <inheritdoc/>
		public void CopyTo(OutlineLayer[] array, int arrayIndex)
		{
			_layers.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		/// <inheritdoc/>
		public IEnumerator<OutlineLayer> GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		#endregion

		#region IChangeTracking

		/// <inheritdoc/>
		public bool IsChanged
		{
			get
			{
				if (_changed)
				{
					return true;
				}

				foreach (var layer in _layers)
				{
					if (layer.IsChanged)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <inheritdoc/>
		public void AcceptChanges()
		{
			foreach (var layer in _layers)
			{
				layer.AcceptChanges();
			}

			_changed = false;
		}

		#endregion

		#region implementation
		#endregion
	}
}
