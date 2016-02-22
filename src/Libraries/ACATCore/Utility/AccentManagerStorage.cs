////////////////////////////////////////////////////////////////////////////
// <copyright file="AccentManagerStorage.cs">
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using ACAT.Lib.Core.Utility.Interfaces;

namespace ACAT.Lib.Core.Utility
{
    /// <summary>
    /// Contains managers and helps to get the accented version of a word.
    /// </summary>
    public class AccentManagerStorage
    {
        private readonly CultureInfo _currentCulture;

        private static readonly Dictionary<CultureInfo, IAccentManager> SupportedCultureManagerMap = 
            new Dictionary<CultureInfo, IAccentManager>()
        {
            { CultureInfo.GetCultureInfo(HungarianAccentManager.CultureName), new HungarianAccentManager() }
        };

        /// <summary>
        /// Indicates that the given culture is supported by any manager.
        /// </summary>
        public bool IsCurrentCultureSupported { get; }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="currentCulture"></param>
        public AccentManagerStorage(CultureInfo currentCulture)
        {
            if (currentCulture == null)
            {
                throw new ArgumentNullException("currentCulture");
            }

            _currentCulture = currentCulture;
            IsCurrentCultureSupported = SupportedCultureManagerMap.ContainsKey(_currentCulture);
        }

        /// <summary>
        /// Tries to get the accented version of the given word.
        /// </summary>
        /// <param name="word">The word</param>
        /// <param name="accentedWord">The accented version of the given word</param>
        /// <returns>true if the conversion was successful, otherwise false</returns>
        public bool TryGetAccentedText(string word, out string accentedWord)
        {
            accentedWord = null;

            if (word == null)
            {
                return false;
            }

            IAccentManager manager;
            if (!SupportedCultureManagerMap.TryGetValue(_currentCulture, out manager))
            {
                return false;
            }

            return manager.TryGetAccentedWord(word, out accentedWord);
        }
    }
}