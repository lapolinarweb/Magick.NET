﻿// Copyright 2013-2021 Dirk Lemstra <https://github.com/dlemstra/Magick.NET/>
//
// Licensed under the ImageMagick License (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//   https://www.imagemagick.org/script/license.php
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;

namespace ImageMagick
{
    /// <summary>
    /// Class that can be used to access an Exif profile.
    /// </summary>
    public sealed class ExifProfile : ImageProfile, IExifProfile
    {
        private List<IExifValue> _values;
        private List<ExifTag> _invalidTags = new List<ExifTag>();
        private int _thumbnailOffset;
        private int _thumbnailLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExifProfile"/> class.
        /// </summary>
        public ExifProfile()
          : base("exif")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExifProfile"/> class.
        /// </summary>
        /// <param name="data">The byte array to read the exif profile from.</param>
        public ExifProfile(byte[] data)
          : base("exif", data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExifProfile"/> class.
        /// </summary>
        /// <param name="fileName">The fully qualified name of the exif profile file, or the relative
        /// exif profile file name.</param>
        public ExifProfile(string fileName)
          : base("exif", fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExifProfile"/> class.
        /// </summary>
        /// <param name="stream">The stream to read the exif profile from.</param>
        public ExifProfile(Stream stream)
          : base("exif", stream)
        {
        }

        /// <summary>
        /// Gets or sets which parts will be written when the profile is added to an image.
        /// </summary>
        public ExifParts Parts { get; set; } = ExifParts.All;

        /// <summary>
        /// Gets the tags that where found but contained an invalid value.
        /// </summary>
        public IEnumerable<ExifTag> InvalidTags
        {
            get
            {
                InitializeValues();
                return _invalidTags;
            }
        }

        /// <summary>
        /// Gets the length of the thumbnail data in the <see cref="byte"/> array of the profile.
        /// </summary>
        public int ThumbnailLength
        {
            get
            {
                InitializeValues();
                return _thumbnailLength;
            }
        }

        /// <summary>
        /// Gets the offset of the thumbnail data in the <see cref="byte"/> array of the profile.
        /// </summary>
        public int ThumbnailOffset
        {
            get
            {
                InitializeValues();
                return _thumbnailOffset;
            }
        }

        /// <summary>
        /// Gets the values of this exif profile.
        /// </summary>
        public IEnumerable<IExifValue> Values
        {
            get
            {
                InitializeValues();
                return _values;
            }
        }

        /// <summary>
        /// Returns the value with the specified tag.
        /// </summary>
        /// <param name="tag">The tag of the exif value.</param>
        /// <returns>The value with the specified tag.</returns>
        /// <typeparam name="TValueType">The data type of the tag.</typeparam>
        public IExifValue<TValueType> GetValue<TValueType>(ExifTag<TValueType> tag)
        {
            foreach (var exifValue in Values)
            {
                if (exifValue.Tag == tag)
                    return (IExifValue<TValueType>)exifValue;
            }

            return null;
        }

        /// <summary>
        /// Removes the thumbnail in the exif profile.
        /// </summary>
        public void RemoveThumbnail()
        {
            // The values need to be initialized to make sure the thumbnail is not written.
            InitializeValues();

            _thumbnailLength = 0;
            _thumbnailOffset = 0;
        }

        /// <summary>
        /// Removes the value with the specified tag.
        /// </summary>
        /// <param name="tag">The tag of the exif value.</param>
        /// <returns>True when the value was fount and removed.</returns>
        public bool RemoveValue(ExifTag tag)
        {
            InitializeValues();

            for (int i = 0; i < _values.Count; i++)
            {
                if (_values[i].Tag == tag)
                {
                    _values.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Loads the data from the profile and rewrites the profile data. This can be used
        /// to fix an incorrect profile. It can also be used for products that require a
        /// specific exif structure.
        /// </summary>
        public void Rewrite()
        {
            InitializeValues();
            UpdateData();
        }

        /// <summary>
        /// Sets the value of the specified tag.
        /// </summary>
        /// <param name="tag">The tag of the exif value.</param>
        /// <param name="value">The value.</param>
        /// <typeparam name="TValueType">The data type of the tag.</typeparam>
        public void SetValue<TValueType>(ExifTag<TValueType> tag, TValueType value)
        {
            foreach (var exifValue in Values)
            {
                if (exifValue.Tag == tag)
                {
                    exifValue.SetValue(value);
                    return;
                }
            }

            var newExifValue = ExifValues.Create(tag);
            if (newExifValue == null)
                throw new NotSupportedException();

            newExifValue.SetValue(value);
            _values.Add(newExifValue);
        }

        /// <summary>
        /// Updates the data of the profile.
        /// </summary>
        protected override void UpdateData()
        {
            if (_values == null)
            {
                return;
            }

            if (_values.Count == 0)
            {
                SetData(null);
                return;
            }

            var writer = new ExifWriter(Parts);
            SetData(writer.Write(_values));
        }

        private void InitializeValues()
        {
            if (_values != null)
                return;

            var data = GetData();
            if (data == null)
            {
                _values = new List<IExifValue>();
                return;
            }

            var reader = new ExifReader();
            reader.Read(data);

            _values = reader.Values;
            _invalidTags = reader.InvalidTags;
            _thumbnailOffset = (int)reader.ThumbnailOffset;
            _thumbnailLength = (int)reader.ThumbnailLength;
        }
    }
}