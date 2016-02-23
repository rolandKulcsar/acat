////////////////////////////////////////////////////////////////////////////
// <copyright file="HungarianAccentManager.cs">
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ACAT.Lib.Core.Utility.Interfaces;

namespace ACAT.Lib.Core.Utility
{
    /// <summary>
    /// Represents a converter that converts words without accent to accented words 
    /// at hungarian culture.
    /// </summary>
    public sealed class HungarianAccentManager : IAccentManager
    {
        /// <summary>
        /// Encapsulates a stream interval.
        /// </summary>
        private sealed class Interval
        {
            public long BeginOffset { get; set; }

            public long EndOffset { get; set; }
        }

        private const int CodePage = 1250;

        private static readonly Dictionary<char, Interval> AccentDictionary = new Dictionary<char, Interval>();

        /// <summary>
        /// The hungarian culture called in .NET
        /// </summary>
        public const string CultureName = "hu-HU";

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public HungarianAccentManager()
        {
            using (Stream stream = GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1250)))
            {
                char startChar = 'a';
                string line;

                AccentDictionary.Add(startChar, new Interval { BeginOffset = 0 });

                while ((line = reader.ReadLine()) != null)
                {
                    char currentStartChar = line.FirstOrDefault();
                    if (startChar == currentStartChar)
                    {
                        continue;
                    }

                    Interval interval;
                    if (!AccentDictionary.TryGetValue(startChar, out interval))
                    {
                        return;
                    }

                    interval.EndOffset = reader.BaseStream.Position - 1;
                    startChar = currentStartChar;

                    AccentDictionary.Add(startChar, new Interval { BeginOffset = reader.BaseStream.Position });
                }
            }
        }

        /// <summary>
        /// Gets the supported culture.
        /// </summary>
        public CultureInfo SupportedCulture
        {
            get { return CultureInfo.GetCultureInfo(CultureName); }
        }

        /// <summary>
        /// Tries to get the accented version of the given word.
        /// </summary>
        /// <param name="word">The word</param>
        /// <param name="accentedWord">The accented version of the given word</param>
        /// <returns>true if the conversion was successful, otherwise false</returns>
        public bool TryGetAccentedWord(string word, out string accentedWord)
        {
            accentedWord = null;

            if (word == null)
            {
                return false;
            }

            char firstLetter = word.FirstOrDefault();

            Interval interval;
            if (!AccentDictionary.TryGetValue(firstLetter, out interval))
            {
                return false;
            }

            using (Stream stream = GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(CodePage)))
            {
                reader.BaseStream.Seek(interval.BeginOffset, SeekOrigin.Begin);

                while (reader.BaseStream.Position < interval.EndOffset)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        return false;
                    }

                    string[] entry = line.Split((char)Keys.Space);
                    if (entry.Length < 2)
                    {
                        continue;
                    }

                    if (entry[0] == word)
                    {
                        accentedWord = entry[1];

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the hungarian accent dictionary file stream.
        /// </summary>
        /// <returns>the file stream</returns>
        private static Stream GetStream()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            try
            {
                return executingAssembly.GetManifestResourceStream("ACAT.Lib.Core.Resources.AccentDictionary_hu.txt");
            }
            catch (Exception)
            {
                Log.Warn("Couldn't get the accent dictionary file!");

                throw;
            }
        }
    }
}
