﻿// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.DistributedComputeLib;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Allows storage of array data in sparse form, meaning that the indexes of the data stored are maintained while empty indexes are not. This allows
    /// savings in memory and computational efficiency because iterative algorithms need only query indexes containing valid data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// @author David Ray, Damir Dobric
    /// </remarks>
    public class SparseObjectMatrix<T> : AbstractSparseMatrix<T>, IEquatable<T> where T : class
    {

        //private IDictionary<int, T> sparseMap = new Dictionary<int, T>();
        private IDistributedDictionary<int, T> sparseMap;

        /// <summary>
        /// Returns true if sparse memory is remotely distributed. It means objects has to be synced with remote partitions.
        /// </summary>
        public bool IsRemotelyDistributed
        {
            get
            {
                return this.sparseMap is IHtmDistCalculus;
            }
        }

        /// <summary>
        /// Gets partitions (nodes) with assotiated indexes.
        /// </summary>
        /// <returns></returns>
        //public List<(int partId, int minKey, int maxKey)> GetPartitions()
        //{
        //    if (IsRemotelyDistributed)
        //    {
        //        IHtmDistCalculus map = this.sparseMap as IHtmDistCalculus;
        //        return map.GetPartitions();
        //    }
        //    else
        //        throw new InvalidOperationException("GetPartitions can only be ued for remotely distributed collections.");
        //}

        /**
         * Constructs a new {@code SparseObjectMatrix}
         * @param dimensions	the dimensions of this array
         */
        //public SparseObjectMatrix(int[] dimensions) : base(dimensions, false)
        //{

        //}

        /// <summary>
        /// Constructs a new <see cref="SparseObjectMatrix{T}"/>
        /// </summary>
        /// <param name="dimensions">the dimensions of this array</param>
        /// <param name="useColumnMajorOrdering">where inner index increments most frequently</param>
        /// <param name="dict"></param>
        public SparseObjectMatrix(int[] dimensions, bool useColumnMajorOrdering = false, IDistributedDictionary<int, T> dict = null) : base(dimensions, useColumnMajorOrdering)
        {
            if (dict == null)
                this.sparseMap = new InMemoryDistributedDictionary<int, T>(1);
            else
                this.sparseMap = dict;
        }


        /// <summary>
        /// Sets the object to occupy the specified index.
        /// </summary>
        /// <param name="index">The index the object will occupy</param>
        /// <param name="obj">the object to be indexed.</param>
        /// <returns></returns>
        public override AbstractFlatMatrix<T> set(int index, T obj)
        {
            //
            // If not distributed in cluster, we add element by element.
            if (!(this.sparseMap is IHtmDistCalculus))
            {
                if (!sparseMap.ContainsKey(index))
                    sparseMap.Add(index, (T)obj);
                else
                    sparseMap[index] = obj;
            }
            else
            {
                sparseMap[index] = obj;
            }

            return this;
        }

        public override AbstractFlatMatrix<T> set(List<KeyPair> updatingValues)
        {
            sparseMap.AddOrUpdate(updatingValues);
            return this;
        }

        /// <summary>
        /// Sets the specified object to be indexed at the index computed from the specified coordinates.
        /// </summary>
        /// <param name="coordinates">the row major coordinates [outer --> ,...,..., inner]</param>
        /// <param name="obj">the object to be indexed.</param>
        /// <returns></returns>
        public override AbstractFlatMatrix<T> set(int[] coordinates, T obj)
        {
            set(computeIndex(coordinates), obj);
            return this;
        }

        /**
         * Returns the T at the specified index.
         * 
         * @param index     the index of the T to return
         * @return  the T at the specified index.
         */
        // @Override
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override T getObject(int index)
        {
            return GetColumn(index);
        }



        /**
         * Returns the T at the index computed from the specified coordinates
         * @param coordinates   the coordinates from which to retrieve the indexed object
         * @return  the indexed object
         */
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="coordinates"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override T get(int[] coordinates)
        {
            return GetColumn(computeIndex(coordinates));
        }

        /// <summary>
        /// Returns the T at the specified index.
        /// </summary>
        /// <param name="index">the index of the T to return</param>
        /// <returns>the T at the specified index.</returns>
        public override T GetColumn(int index)
        {
            T val = null;

            this.sparseMap.TryGetValue(index, out val);

            return val;
            //return this.sparseMap[index];
        }

        /// <summary>
        /// Returns a sorted array of occupied indexes.
        /// </summary>
        /// <returns>a sorted array of occupied indexes.</returns>
        public override int[] getSparseIndices()
        {
            return Reverse(sparseMap.Keys.ToArray());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override String ToString()
        {
            return getDimensions().ToString();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + ((sparseMap == null) ? 0 : sparseMap.GetHashCode());
            return result;
        }

        public int MyProperty { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (!base.Equals(obj))
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            SparseObjectMatrix<T> other = obj as SparseObjectMatrix<T>;
            if (other == null)
                return false;

            if (sparseMap == null)
            {
                if (other.sparseMap != null)
                    return false;
            }
            else if (!sparseMap.Equals(other.sparseMap))
                return false;

            return true;
        }

        public bool Equals(T other)
        {
            return this.Equals((object)other);
        }

        public override ICollection<KeyPair> GetObjects(int[] indexes)
        {
            throw new NotImplementedException();
        }
    }
}
