/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrossfireRPG.ServerInterface.Managers
{
    /// <summary>
    /// Base class of objects that a DataObjectManager can manage
    /// </summary>
    public abstract class DataObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property value, and raises a PropertyChanged event if the property changes
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="property">Property to change</param>
        /// <param name="value">New property value</param>
        /// <param name="propertyNames">Name of property and additional properties that reference this</param>
        /// <returns>true if the property changed, false if not</returns>
        protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = "",
            params string[] additionalPropertyNames)
        {
            //ignore change if the property didn't change
            if (EqualityComparer<T>.Default.Equals(property, value))
                return false;

            //set the property
            property = value;

            //raise event regardless of propertyName
            OnPropertyChanged(propertyName);

            //save changed properties if capturing all the changed properties
            if (_propertiesChanged != null)
                _propertiesChanged.Add(propertyName);

            if (additionalPropertyNames != null)
            {
                foreach (var additionalPropertyName in additionalPropertyNames)
                {
                    //only raise event and capture properties if a valid propertyName
                    if (!string.IsNullOrEmpty(additionalPropertyName))
                    {
                        OnPropertyChanged(additionalPropertyName); //raise event

                        //save changed properties if capturing all the changed properties
                        if (_propertiesChanged != null)
                            _propertiesChanged.Add(additionalPropertyName);
                    }
                }
            }

            return true;
        }

        private HashSet<string> _propertiesChanged = null;

        public void BeginPropertiesChanged()
        {
            //clear changed properties and start capture
            _propertiesChanged = new HashSet<string>();

        }

        public IReadOnlyCollection<string> EndPropertiesChanged()
        {
            //return null if no properties changed or BeginPropertiesChanged()
            //was not called
            if ((_propertiesChanged == null) || (_propertiesChanged.Count == 0))
                return null;

            //return changed properties after clearing internal list
            var rc = _propertiesChanged;
            _propertiesChanged = null;
            return rc;
        }
    }
}
