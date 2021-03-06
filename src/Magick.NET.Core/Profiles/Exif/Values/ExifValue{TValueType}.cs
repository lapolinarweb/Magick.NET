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

using System.Diagnostics.CodeAnalysis;

namespace ImageMagick
{
    internal abstract class ExifValue<TValueType> : ExifValue, IExifValue<TValueType>
    {
        public ExifValue(ExifTag<TValueType> tag)
            : base(tag)
        {
        }

        public ExifValue(ExifTagValue tag)
            : base(tag)
        {
        }

        public override bool IsArray => false;

        [SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "This value is typed.")]
        public TValueType Value { get; set; }

        protected abstract string StringValue { get; }

        public override object GetValue() => Value;

        public override bool SetValue(object value)
        {
            if (value == null)
            {
                Value = default;
                return true;
            }

            if (value is TValueType typeValue)
            {
                Value = typeValue;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            if (Value == null)
                return null;

            var description = GetDescription(Tag, Value);
            if (description != null)
                return description;

            return StringValue;
        }

        private static string GetDescription(ExifTag tag, object value)
        {
            var tagValue = (ExifTagValue)(ushort)tag;
            var attributes = TypeHelper.GetCustomAttributes<ExifTagDescriptionAttribute>(tagValue);

            if (attributes == null || attributes.Length == 0)
                return null;

            foreach (var attribute in attributes)
            {
                if (Equals(attribute.Value, value))
                    return attribute.Description;
            }

            return null;
        }
    }
}