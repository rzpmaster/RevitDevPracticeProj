﻿using Log4NetDemo.Core.Interface;
using Log4NetDemo.Layout.PatternConverters;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.Globalization;

namespace Log4NetDemo.Layout.Data
{
    /// <summary>
    /// 此类承担了<see cref="PatternLayout"/>类的大部分工作
    /// </summary>
    public sealed class PatternParser
    {
        public PatternParser(string pattern)
        {
            m_pattern = pattern;
        }

        public Hashtable PatternConverters
        {
            get { return m_patternConverters; }
        }

        public PatternConverter Parse()
        {
            string[] converterNamesCache = BuildCache();

            ParseInternal(m_pattern, converterNamesCache);

            return m_head;
        }

        #region Private Instance Methods

        private string[] BuildCache()
        {
            string[] converterNamesCache = new string[m_patternConverters.Keys.Count];
            m_patternConverters.Keys.CopyTo(converterNamesCache, 0);

            // sort array so that longer strings come first
            Array.Sort(converterNamesCache, 0, converterNamesCache.Length, StringLengthComparer.Instance);

            return converterNamesCache;
        }

        #region StringLengthComparer

        /// <summary>
        /// Sort strings by length
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="IComparer" /> that orders strings by string length.
        /// The longest strings are placed first
        /// </para>
        /// </remarks>
        private sealed class StringLengthComparer : IComparer
        {
            public static readonly StringLengthComparer Instance = new StringLengthComparer();

            private StringLengthComparer()
            {
            }

            #region Implementation of IComparer

            public int Compare(object x, object y)
            {
                string s1 = x as string;
                string s2 = y as string;

                if (s1 == null && s2 == null)
                {
                    return 0;
                }
                if (s1 == null)
                {
                    return 1;
                }
                if (s2 == null)
                {
                    return -1;
                }

                return s2.Length.CompareTo(s1.Length);
            }

            #endregion
        }

        #endregion // StringLengthComparer

        private void ParseInternal(string pattern, string[] matches)
        {
            int offset = 0;
            while (offset < pattern.Length)
            {
                int i = pattern.IndexOf('%', offset);
                if (i < 0 || i == pattern.Length - 1)
                {
                    ProcessLiteral(pattern.Substring(offset));
                    offset = pattern.Length;
                }
                else
                {
                    if (pattern[i + 1] == '%')
                    {
                        // Escaped
                        ProcessLiteral(pattern.Substring(offset, i - offset + 1));
                        offset = i + 2;
                    }
                    else
                    {
                        ProcessLiteral(pattern.Substring(offset, i - offset));
                        offset = i + 1;

                        FormattingInfo formattingInfo = new FormattingInfo();

                        // Process formatting options

                        // Look for the align flag
                        if (offset < pattern.Length)
                        {
                            if (pattern[offset] == '-')
                            {
                                // Seen align flag
                                formattingInfo.LeftAlign = true;
                                offset++;
                            }
                        }
                        // Look for the minimum length
                        while (offset < pattern.Length && char.IsDigit(pattern[offset]))
                        {
                            // Seen digit
                            if (formattingInfo.Min < 0)
                            {
                                formattingInfo.Min = 0;
                            }

                            formattingInfo.Min = (formattingInfo.Min * 10) + int.Parse(pattern[offset].ToString(), NumberFormatInfo.InvariantInfo);

                            offset++;
                        }
                        // Look for the separator between min and max
                        if (offset < pattern.Length)
                        {
                            if (pattern[offset] == '.')
                            {
                                // Seen separator
                                offset++;
                            }
                        }
                        // Look for the maximum length
                        while (offset < pattern.Length && char.IsDigit(pattern[offset]))
                        {
                            // Seen digit
                            if (formattingInfo.Max == int.MaxValue)
                            {
                                formattingInfo.Max = 0;
                            }

                            formattingInfo.Max = (formattingInfo.Max * 10) + int.Parse(pattern[offset].ToString(), NumberFormatInfo.InvariantInfo);

                            offset++;
                        }

                        int remainingStringLength = pattern.Length - offset;

                        // Look for pattern
                        for (int m = 0; m < matches.Length; m++)
                        {
                            string key = matches[m];

                            if (key.Length <= remainingStringLength)
                            {
                                if (string.Compare(pattern, offset, key, 0, key.Length) == 0)
                                {
                                    // Found match
                                    offset = offset + matches[m].Length;

                                    string option = null;

                                    // Look for option
                                    if (offset < pattern.Length)
                                    {
                                        if (pattern[offset] == '{')
                                        {
                                            // Seen option start
                                            offset++;

                                            int optEnd = pattern.IndexOf('}', offset);
                                            if (optEnd < 0)
                                            {
                                                // error
                                            }
                                            else
                                            {
                                                option = pattern.Substring(offset, optEnd - offset);
                                                offset = optEnd + 1;
                                            }
                                        }
                                    }

                                    ProcessConverter(matches[m], option, formattingInfo);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ProcessLiteral(string text)
        {
            if (text.Length > 0)
            {
                // Convert into a pattern
                ProcessConverter("literal", text, new FormattingInfo());
            }
        }

        private void ProcessConverter(string converterName, string option, FormattingInfo formattingInfo)
        {
            LogLog.Debug(declaringType, "Converter [" + converterName + "] Option [" + option + "] Format [min=" + formattingInfo.Min + ",max=" + formattingInfo.Max + ",leftAlign=" + formattingInfo.LeftAlign + "]");

            // Lookup the converter type
            ConverterInfo converterInfo = (ConverterInfo)m_patternConverters[converterName];
            if (converterInfo == null)
            {
                LogLog.Error(declaringType, "Unknown converter name [" + converterName + "] in conversion pattern.");
            }
            else
            {
                // Create the pattern converter
                PatternConverter pc = null;
                try
                {
                    pc = (PatternConverter)Activator.CreateInstance(converterInfo.Type);
                }
                catch (Exception createInstanceEx)
                {
                    LogLog.Error(declaringType, "Failed to create instance of Type [" + converterInfo.Type.FullName + "] using default constructor. Exception: " + createInstanceEx.ToString());
                }

                // formattingInfo variable is an instance variable, occasionally reset 
                // and used over and over again
                pc.FormattingInfo = formattingInfo;
                pc.Option = option;
                pc.Properties = converterInfo.Properties;

                IOptionHandler optionHandler = pc as IOptionHandler;
                if (optionHandler != null)
                {
                    optionHandler.ActivateOptions();
                }

                AddConverter(pc);
            }
        }

        private void AddConverter(PatternConverter pc)
        {
            // Add the pattern converter to the list.

            if (m_head == null)
            {
                m_head = m_tail = pc;
            }
            else
            {
                // Set the next converter on the tail
                // Update the tail reference
                // note that a converter may combine the 'next' into itself
                // and therefore the tail would not change!
                m_tail = m_tail.SetNext(pc);
            }
        }

        #endregion

        private const char ESCAPE_CHAR = '%';

        /// <summary>
        /// The first pattern converter in the chain
        /// </summary>
        private PatternConverter m_head;

        /// <summary>
        ///  the last pattern converter in the chain
        /// </summary>
        private PatternConverter m_tail;

        /// <summary>
        /// The pattern
        /// </summary>
        private string m_pattern;

        /// <summary>
        /// Internal map of converter identifiers to converter types
        /// </summary>
        /// <remarks>
        /// <para>
        /// This map overrides the static s_globalRulesRegistry map.
        /// </para>
        /// </remarks>
        private Hashtable m_patternConverters = new Hashtable();

        private readonly static Type declaringType = typeof(PatternParser);
    }
}
