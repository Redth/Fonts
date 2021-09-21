// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.Fonts.Tables.AdvancedTypographic.GPos
{
    /// <summary>
    /// Cursive Attachment Positioning Subtable.
    /// Some cursive fonts are designed so that adjacent glyphs join when rendered with their default positioning.
    /// However, if positioning adjustments are needed to join the glyphs, a cursive attachment positioning (CursivePos) subtable can describe
    /// how to connect the glyphs by aligning two anchor points: the designated exit point of a glyph, and the designated entry point of the following glyph.
    /// <see href="https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#cursive-attachment-positioning-format1-cursive-attachment"/>
    /// </summary>
    internal sealed class LookupType3SubTable
    {
        private LookupType3SubTable()
        {
        }

        public static LookupSubTable Load(BigEndianBinaryReader reader, long offset)
        {
            reader.Seek(offset, SeekOrigin.Begin);
            ushort posFormat = reader.ReadUInt16();

            return posFormat switch
            {
                1 => LookupType3Format1SubTable.Load(reader, offset),
                _ => throw new InvalidFontFileException(
                    $"Invalid value for 'posFormat' {posFormat}. Should be '1'.")
            };
        }

        internal sealed class LookupType3Format1SubTable : LookupSubTable
        {
            private readonly CoverageTable coverageTable;
            private readonly EntryExitAnchors[] entryExitAnchors;

            public LookupType3Format1SubTable(CoverageTable coverageTable, EntryExitAnchors[] entryExitAnchors)
            {
                this.coverageTable = coverageTable;
                this.entryExitAnchors = entryExitAnchors;
            }

            public static LookupType3Format1SubTable Load(BigEndianBinaryReader reader, long offset)
            {
                // Cursive Attachment Positioning Format1.
                // +--------------------+---------------------------------+------------------------------------------------------+
                // | Type               |  Name                           | Description                                          |
                // +====================+=================================+======================================================+
                // | uint16             | posFormat                       | Format identifier: format = 1                        |
                // +--------------------+---------------------------------+------------------------------------------------------+
                // | Offset16           | coverageOffset                  | Offset to Coverage table,                            |
                // |                    |                                 | from beginning of CursivePos subtable.               |
                // +--------------------+---------------------------------+------------------------------------------------------+
                // | uint16             | entryExitCount                  | Number of EntryExit records.                         |
                // +--------------------+---------------------------------+------------------------------------------------------+
                // | EntryExitRecord    | entryExitRecord[entryExitCount] | Array of EntryExit records, in Coverage index order. |
                // +--------------------+---------------------------------+------------------------------------------------------+
                ushort coverageOffset = reader.ReadOffset16();
                ushort entryExitCount = reader.ReadUInt16();
                var entryExitRecords = new EntryExitRecord[entryExitCount];
                for (int i = 0; i < entryExitCount; i++)
                {
                    entryExitRecords[i] = new EntryExitRecord(reader, offset);
                }

                var entryExitAnchors = new EntryExitAnchors[entryExitCount];
                for (int i = 0; i < entryExitCount; i++)
                {
                    entryExitAnchors[i] = new EntryExitAnchors(reader, offset, entryExitRecords[i]);
                }

                var coverageTable = CoverageTable.Load(reader, offset + coverageOffset);

                return new LookupType3Format1SubTable(coverageTable, entryExitAnchors);
            }

            public override bool TryUpdatePosition(IFontMetrics fontMetrics, GPosTable table, GlyphPositioningCollection collection, ushort index, int count)
            {
                // Implements Cursive Attachment Positioning Subtable:
                // https://docs.microsoft.com/en-us/typography/opentype/spec/gpos#lookup-type-3-cursive-attachment-positioning-subtable
                for (ushort i = 0; i < count - 1; i++)
                {
                    int curIndex = i + index;
                    int glyphId = collection.GetGlyphIds(curIndex)[0];
                    if (glyphId < 0)
                    {
                        return false;
                    }

                    int nextIndex = i + index + 1;
                    int nextGlyphId = collection.GetGlyphIds(nextIndex)[0];
                    if (nextGlyphId < 0)
                    {
                        return false;
                    }

                    int coverageNext = this.coverageTable.CoverageIndexOf((ushort)nextGlyphId);
                    EntryExitAnchors nextRecord = this.entryExitAnchors[coverageNext];
                    AnchorTable? entry = nextRecord.EntryAnchor;
                    if (entry is null)
                    {
                        return false;
                    }

                    int coverage = this.coverageTable.CoverageIndexOf((ushort)glyphId);
                    EntryExitAnchors curRecord = this.entryExitAnchors[coverage];
                    AnchorTable? exit = curRecord.ExitAnchor;
                    if (exit is null)
                    {
                        return false;
                    }

                    // TODO: we need to know here if we are RTL or LTR. This assumes LTR.
                    int curOffset = 0; // TODO: get current offset.
                    int nextXOffset = 0; // TODO: get next offset.
                    int dx = entry.XCoordinate + nextXOffset;
                    int dy = exit.YCoordinate - entry.YCoordinate;
                    int curXAdvance = exit.XCoordinate + curOffset;
                    collection.SetAdvance(fontMetrics, (ushort)curIndex, (ushort)glyphId, (ushort)curXAdvance, (ushort)dy);
                    collection.Advance(fontMetrics, (ushort)nextIndex, (ushort)nextGlyphId, (short)-dx, 0);
                    collection.Offset(fontMetrics, (ushort)nextIndex, (ushort)nextGlyphId, (short)-dx, 0);
                }

                return true;
            }
        }
    }
}
