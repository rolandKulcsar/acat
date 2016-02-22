////////////////////////////////////////////////////////////////////////////
// <copyright file="IAccentManager.cs">
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

using System.Globalization;

namespace ACAT.Lib.Core.Utility.Interfaces
{
    /// <summary>
    /// Represents a converter that converts words without accent to accented words.
    /// </summary>
    public interface IAccentManager
    {
        /// <summary>
        /// Gets the culture that the manager supports.
        /// </summary>
        CultureInfo SupportedCulture { get; }

        /// <summary>
        /// Tries to get the accented version of the given word.
        /// </summary>
        /// <param name="word">The word</param>
        /// <param name="accentedWord">The accented version of the given word</param>
        /// <returns>true if the conversion was successful, otherwise false</returns>
        bool TryGetAccentedWord(string word, out string accentedWord);
    }
}