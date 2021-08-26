// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.Fonts.Tables.General.Glyphs
{
    /// <summary>
    /// Features provide information about how to use the glyphs in a font to render a script or language.
    /// For example, an Arabic font might have a feature for substituting initial glyph forms, and a Kanji font
    /// might have a feature for positioning glyphs vertically. All OpenType Layout features define data for
    /// glyph substitution, glyph positioning, or both.
    /// <see href="https://docs.microsoft.com/en-us/typography/opentype/spec/featurelist"/>
    /// <see href="https://docs.microsoft.com/en-us/typography/opentype/spec/chapter2#feature-list-table"/>
    /// </summary>
    internal class FeatureList
    {
        private FeatureList(FeatureListTable[] featureListTables)
            => this.FeatureListTables = featureListTables;

        public FeatureListTable[] FeatureListTables { get; }

        public static FeatureList Load(BigEndianBinaryReader reader, long offset)
        {
            // FeatureList
            // +---------------+------------------------------+-----------------------------------------------------------------------------------------------------------------+
            // | Type          | Name                         | Description                                                                                                     |
            // +===============+==============================+=================================================================================================================+
            // | uint16        | featureCount                 | Number of FeatureRecords in this table                                                                          |
            // +---------------+------------------------------+-----------------------------------------------------------------------------------------------------------------+
            // | FeatureRecord | featureRecords[featureCount] | Array of FeatureRecords — zero-based (first feature has FeatureIndex = 0), listed alphabetically by feature tag |
            // +---------------+------------------------------+-----------------------------------------------------------------------------------------------------------------+
            reader.Seek(offset, SeekOrigin.Begin);

            ushort featureCount = reader.ReadUInt16();
            var featureRecords = new FeatureRecord[featureCount];
            for (int i = 0; i < featureRecords.Length; i++)
            {
                // FeatureRecord
                // +----------+---------------+--------------------------------------------------------+
                // | Type     | Name          | Description                                            |
                // +==========+===============+========================================================+
                // | Tag      | featureTag    | 4-byte feature identification tag                      |
                // +----------+---------------+--------------------------------------------------------+
                // | Offset16 | featureOffset | Offset to Feature table, from beginning of FeatureList |
                // +----------+---------------+--------------------------------------------------------+
                uint featureTag = reader.ReadUInt32();
                ushort featureOffset = reader.ReadOffset16();
                featureRecords[i] = new FeatureRecord(featureTag, featureOffset);
            }

            // Load the other table features.
            // We do this last to avoid excessive seeking.
            var featureListTables = new FeatureListTable[featureCount];
            for (int i = 0; i < featureListTables.Length; i++)
            {
                FeatureRecord featureRecord = featureRecords[i];
                featureListTables[i] = FeatureListTable.Load(featureRecord.FeatureTag, reader, offset + featureRecord.FeatureOffset);
            }

            return new FeatureList(featureListTables);
        }

        private readonly struct FeatureRecord
        {
            public FeatureRecord(uint featureTag, ushort featureOffset)
            {
                this.FeatureTag = featureTag;
                this.FeatureOffset = featureOffset;
            }

            public uint FeatureTag { get; }

            public ushort FeatureOffset { get; }
        }
    }

    internal sealed class FeatureListTable
    {
        private FeatureListTable(uint featureTag, ushort[] lookupListIndices)
        {
            this.FeatureTag = featureTag;
            this.LookupListIndices = lookupListIndices;
        }

        public uint FeatureTag { get; }

        public ushort[] LookupListIndices { get; }

        public static FeatureListTable Load(uint featureTag, BigEndianBinaryReader reader, long offset)
        {
            // FeatureListTable
            // +----------+-------------------------------------+--------------------------------------------------------------------------------------------------------------+
            // | Type     | Name                                | Description                                                                                                  |
            // +==========+=====================================+==============================================================================================================+
            // | Offset16 | featureParamsOffset                 | Offset from start of Feature table to FeatureParams table, if defined for the feature and present, else NULL |
            // +----------+-------------------------------------+--------------------------------------------------------------------------------------------------------------+
            // | uint16   | lookupIndexCount                    | Number of LookupList indices for this feature                                                                |
            // +----------+-------------------------------------+--------------------------------------------------------------------------------------------------------------+
            // | uint16   | lookupListIndices[lookupIndexCount] | Array of indices into the LookupList — zero-based (first lookup is LookupListIndex = 0)                      |
            // +----------+-------------------------------------+--------------------------------------------------------------------------------------------------------------+
            reader.Seek(offset, SeekOrigin.Begin);

            // TODO: How do we use this?
            ushort featureParamsOffset = reader.ReadOffset16();
            ushort lookupIndexCount = reader.ReadUInt16();

            ushort[] lookupListIndices = reader.ReadUInt16Array(lookupIndexCount);
            return new FeatureListTable(featureTag, lookupListIndices);
        }
    }
}