﻿﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.TestPlatform.ObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    ///  Base class for test related classes.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors")]
    [DataContract]
    public abstract class TestObject
    {
        #region Fields

        /// <summary>
        /// The store for all the properties registered.
        /// </summary>
        private readonly Dictionary<TestProperty, object> store;

        /// <summary>
        /// Property used for Json (de)serialization of store dictionary. Serialization of dictionaries
        /// by default doesn't provide the required object representation. <c>List of KeyValuePair</c> on the
        /// other hand provides a clean Key, Value entries for <c>TestProperty</c> and it's value.
        /// </summary>
        [DataMember(Name = "Properties")]
        private List<KeyValuePair<TestProperty, object>> StoreKeyValuePairs
        {
            get
            {
                return this.store.ToList();
            }

            set
            {
                // Receive the <TestProperty, String> key value pairs from deserialized entity.
                // Store each property and value in the property data store.
                foreach (var property in value)
                {
                    TestProperty.Register(
                        property.Key.Id,
                        property.Key.Label,
                        property.Key.Category,
                        property.Key.Description,
                        property.Key.GetValueType(),
                        null,
                        property.Key.Attributes,
                        typeof(TestObject));
                    this.SetPropertyValue(property.Key, property.Value, CultureInfo.InvariantCulture);
                }
            }
        }

        #endregion Fields

        #region Constructors

        protected TestObject()
        {
            this.store = new Dictionary<TestProperty, object>();            
            TypeDescriptor.AddAttributes(typeof(Guid), new TypeConverterAttribute(typeof(CustomGuidConverter)));
            TypeDescriptor.AddAttributes(typeof(KeyValuePair<string, string>[]), new TypeConverterAttribute(typeof(CustomKeyValueConverter)));
            TypeDescriptor.AddAttributes(typeof(string[]), new TypeConverterAttribute(typeof(CustomStringArrayConverter)));
        }

        [OnSerializing]
#if FullCLR
        private void CacheLazyValuesOnSerializing(StreamingContext context)
#else
        public void CacheLazyValuesOnSerializing(StreamingContext context)
#endif
        {
            var lazyValues = this.store.Where(kvp => kvp.Value is ILazyPropertyValue).ToArray();

            foreach (var kvp in lazyValues)
            {
                var lazyValue = (ILazyPropertyValue)kvp.Value;
                var value = lazyValue.Value;
                this.store.Remove(kvp.Key);

                if (value != null)
                {
                    this.store.Add(kvp.Key, value);
                }
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Returns the TestProperties currently specified in this TestObject.
        /// </summary>
        public IEnumerable<TestProperty> Properties
        {
            get { return this.store.Keys; }
        }

        /// <summary>
        /// Returns property value of the specify TestProperty
        /// </summary>
        /// <param name="property">TestObject's TestProperty</param>
        /// <returns>property value</returns>
        public object GetPropertyValue(TestProperty property)
        {
            ValidateArg.NotNull(property, "property");

            object defaultValue = null;
            var valueType = property.GetValueType();

            if (valueType != null && valueType.GetTypeInfo().IsValueType)
            {
                defaultValue = Activator.CreateInstance(valueType);
            }

            return this.PrivateGetPropertyValue(property, defaultValue);
        }

        /// <summary>
        /// Returns property value of the specify TestProperty
        /// </summary>
        /// <typeparam name="T">Property value type</typeparam>
        /// <param name="property">TestObject's TestProperty</param>
        /// <param name="defaultValue">default property value if property is not present</param>
        /// <returns>property value</returns>
        public T GetPropertyValue<T>(TestProperty property, T defaultValue)
        {
            return this.GetPropertyValue<T>(property, defaultValue, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Set TestProperty's value
        /// </summary>
        /// <typeparam name="T">Property value type</typeparam>
        /// <param name="property">TestObject's TestProperty</param>
        /// <param name="value">value to be set</param>
        public void SetPropertyValue<T>(TestProperty property, T value)
        {
            this.SetPropertyValue<T>(property, value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Set TestProperty's value
        /// </summary>
        /// <typeparam name="T">Property value type</typeparam>
        /// <param name="property">TestObject's TestProperty</param>
        /// <param name="value">value to be set</param>
        public void SetPropertyValue<T>(TestProperty property, LazyPropertyValue<T> value)
        {
            this.SetPropertyValue<T>(property, value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Set TestProperty's value
        /// </summary>
        /// <param name="property">TestObject's TestProperty</param>
        /// <param name="value">value to be set</param>
        public void SetPropertyValue(TestProperty property, object value)
        {
            this.PrivateSetPropertyValue(property, value);
        }

        /// <summary>
        ///  Remove test property from the current TestObject.
        /// </summary>
        /// <param name="property"></param>
        public void RemovePropertyValue(TestProperty property)
        {
            ValidateArg.NotNull(property, "property");

            object value;
            if (this.store.TryGetValue(property, out value))
            {
                this.store.Remove(property);
            }
        }


        /// <summary>
        /// Returns TestProperty's value 
        /// </summary>
        /// <returns>property's value. default value is returned if the property is not present</returns>
        public T GetPropertyValue<T>(TestProperty property, T defaultValue, CultureInfo culture)
        {
            ValidateArg.NotNull(property, "property");
            ValidateArg.NotNull(culture, "culture");

            object objValue = this.PrivateGetPropertyValue(property, defaultValue);

            return ConvertPropertyTo<T>(property, culture, objValue);
        }

        /// <summary>
        /// Set TestProperty's value to the specified value T.
        /// </summary>
        public void SetPropertyValue<T>(TestProperty property, T value, CultureInfo culture)
        {
            ValidateArg.NotNull(property, "property");
            ValidateArg.NotNull(culture, "culture");

            object objValue = ConvertPropertyFrom<T>(property, culture, value);

            this.PrivateSetPropertyValue(property, objValue);
        }

        /// <summary>
        /// Set TestProperty's value to the specified value T.
        /// </summary>
        public void SetPropertyValue<T>(TestProperty property, LazyPropertyValue<T> value, CultureInfo culture)
        {
            ValidateArg.NotNull(property, "property");
            ValidateArg.NotNull(culture, "culture");

            object objValue = ConvertPropertyFrom<T>(property, culture, value);

            this.PrivateSetPropertyValue(property, objValue);
        }

        #endregion Property Values

        #region Helpers

        /// <summary>
        /// Return TestProperty's value
        /// </summary>
        /// <returns></returns>
        private object PrivateGetPropertyValue(TestProperty property, object defaultValue)
        {
            ValidateArg.NotNull(property, "property");

            object value;
            if (!this.store.TryGetValue(property, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Set TestProperty's value
        /// </summary>
        private void PrivateSetPropertyValue(TestProperty property, object value)
        {
            ValidateArg.NotNull(property, "property");

            if (property.ValidateValueCallback == null || property.ValidateValueCallback(value))
            {
                this.store[property] = value;
            }
            else
            {
                throw new ArgumentException(property.Label);
            }
        }

        /// <summary>
        /// Convert passed in value from TestProperty's specified value type.
        /// </summary>
        /// <returns>Converted object</returns>
        private static object ConvertPropertyFrom<T>(TestProperty property, CultureInfo culture, object value)
        {
            ValidateArg.NotNull(property, "property");
            ValidateArg.NotNull(culture, "culture");

            var valueType = property.GetValueType();

            if (valueType != null && valueType.GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()))
            {
                return value;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(valueType);
            if (converter == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.ConverterNotSupported, valueType.Name));
            }

            try
            {
                return converter.ConvertFrom(null, culture, value);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception e)
            {
                // some type converters throw strange exceptions (eg: System.Exception by Int32Converter)
                throw new FormatException(e.Message, e);
            }
        }

        /// <summary>
        /// Convert passed in value into the specified type when property is registered.
        /// </summary>
        /// <returns>Converted object</returns>
        private static T ConvertPropertyTo<T>(TestProperty property, CultureInfo culture, object value)
        {
            ValidateArg.NotNull(property, "property");
            ValidateArg.NotNull(culture, "culture");

            var lazyValue = value as LazyPropertyValue<T>;

            if (value == null)
            {
                return default(T);
            }
            else if (value is T)
            {
                return (T)value;
            }
            else if (lazyValue != null)
            {
                return lazyValue.Value;
            }

            var valueType = property.GetValueType();

            TypeConverter converter = TypeDescriptor.GetConverter(valueType);

            if (converter == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.ConverterNotSupported, valueType.Name));
            }

            try
            {
                return (T)converter.ConvertTo(null, culture, value, typeof(T));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception e)
            {
                // some type converters throw strange exceptions (eg: System.Exception by Int32Converter)
                throw new FormatException(e.Message, e);
            }
        }

        #endregion Helpers

        private TraitCollection traits;

        public TraitCollection Traits
        {
            get
            {
                if (this.traits == null)
                {
                    this.traits = new TraitCollection(this);
                }

                return this.traits;
            }
        }
    } 
}