// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Numerics;

namespace SixLabors.Fonts
{
    /// <summary>
    /// The combined set of properties that uniquely identify the glyph that is to be rendered
    /// at a particular size and dpi.
    /// </summary>
    [DebuggerDisplay("GlyphIndex = {GlyphIndex}, PointSize = {PointSize}, DpiX = {DpiX}, DpiY = {DpiY}")]
    public readonly struct GlyphRendererParameters : IEquatable<GlyphRendererParameters>
    {
        internal GlyphRendererParameters(GlyphMetrics glyph, float pointSize, float dpi)
        {
            this.Font = glyph.FontMetrics.Description.FontNameInvariantCulture?.ToUpper() ?? string.Empty;
            this.FontStyle = glyph.FontMetrics.Description.Style;
            this.GlyphIndex = glyph.GlyphId;
            this.PointSize = pointSize;
            this.Dpi = dpi;
            this.GlyphType = glyph.GlyphType;
            this.GlyphColor = glyph.GlyphColor ?? default;
        }

        /// <summary>
        /// Gets the name of the Font this glyph belongs to.
        /// </summary>
        public string Font { get; }

        /// <summary>
        /// Gets the type of this glyph.
        /// </summary>
        public GlyphColor GlyphColor { get; }

        /// <summary>
        /// Gets the type of this glyph.
        /// </summary>
        public GlyphType GlyphType { get; }

        /// <summary>
        /// Gets the style of the Font this glyph belongs to.
        /// </summary>
        public FontStyle FontStyle { get; }

        /// <summary>
        /// Gets the index of the glyph.
        /// </summary>
        public ushort GlyphIndex { get; }

        /// <summary>
        /// Gets the rendered point size.
        /// </summary>
        public float PointSize { get; }

        /// <summary>
        /// Gets the dpi along the X axis we are rendering at.
        /// </summary>
        public float Dpi { get; }

        /// <summary>
        /// Compares two <see cref="GlyphRendererParameters"/> objects for equality.
        /// </summary>
        /// <param name="left">
        /// The <see cref="GlyphRendererParameters"/> on the left side of the operand.
        /// </param>
        /// <param name="right">
        /// The <see cref="GlyphRendererParameters"/> on the right side of the operand.
        /// </param>
        /// <returns>
        /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        public static bool operator ==(GlyphRendererParameters left, GlyphRendererParameters right)
            => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="GlyphRendererParameters"/> objects for inequality.
        /// </summary>
        /// <param name="left">
        /// The <see cref="GlyphRendererParameters"/> on the left side of the operand.
        /// </param>
        /// <param name="right">
        /// The <see cref="GlyphRendererParameters"/> on the right side of the operand.
        /// </param>
        /// <returns>
        /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        public static bool operator !=(GlyphRendererParameters left, GlyphRendererParameters right)
            => !left.Equals(right);

        /// <inheritdoc/>
        public bool Equals(GlyphRendererParameters other)
            => other.PointSize == this.PointSize
            && other.FontStyle == this.FontStyle
            && other.Dpi == this.Dpi
            && other.GlyphIndex == this.GlyphIndex
            && other.GlyphType == this.GlyphType
            && other.GlyphColor.Equals(this.GlyphColor)
            && ((other.Font is null && this.Font is null)
            || (other.Font?.Equals(this.Font, StringComparison.OrdinalIgnoreCase) == true));

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is GlyphRendererParameters p && this.Equals(p);

        /// <inheritdoc/>
        public override int GetHashCode()
            => HashCode.Combine(
                this.Font,
                this.PointSize,
                this.GlyphIndex,
                this.GlyphType,
                this.GlyphColor,
                this.FontStyle,
                this.Dpi);
    }
}
