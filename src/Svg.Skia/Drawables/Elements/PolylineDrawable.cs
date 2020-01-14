﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Parts of this source file are adapted from the https://github.com/vvvv/SVG
using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Svg.Skia
{
    public class PolylineDrawable : DrawablePath
    {
        public PolylineDrawable(SvgPolyline svgPolyline, SKRect skOwnerBounds, bool ignoreDisplay)
        {
            IgnoreDisplay = ignoreDisplay;
            IsDrawable = CanDraw(svgPolyline, IgnoreDisplay);

            if (!IsDrawable)
            {
                return;
            }

            Path = SKPathUtil.ToSKPath(svgPolyline.Points, svgPolyline.FillRule, false, skOwnerBounds, _disposable);
            if (Path == null || Path.IsEmpty)
            {
                IsDrawable = false;
                return;
            }

            IsAntialias = SKPaintUtil.IsAntialias(svgPolyline);

            TransformedBounds = Path.Bounds;

            Transform = SKMatrixUtil.GetSKMatrix(svgPolyline.Transforms);

            PathClip = SvgClipPathUtil.GetSvgVisualElementClipPath(svgPolyline, TransformedBounds, new HashSet<Uri>(), _disposable);
            PaintOpacity = SKPaintUtil.GetOpacitySKPaint(svgPolyline, _disposable);
            PaintFilter = SKPaintUtil.GetFilterSKPaint(svgPolyline, TransformedBounds, _disposable);

            if (SKPaintUtil.IsValidFill(svgPolyline))
            {
                PaintFill = SKPaintUtil.GetFillSKPaint(svgPolyline, TransformedBounds, _disposable);
                if (PaintFill == null)
                {
                    IsDrawable = false;
                    return;
                }
            }

            if (SKPaintUtil.IsValidStroke(svgPolyline, TransformedBounds))
            {
                PaintStroke = SKPaintUtil.GetStrokeSKPaint(svgPolyline, TransformedBounds, _disposable);
                if (PaintStroke == null)
                {
                    IsDrawable = false;
                    return;
                }
            }

            CreateMarkers(svgPolyline, Path, skOwnerBounds);

            // TODO: Transform _skBounds using _skMatrix.
            SKMatrix.MapRect(ref Transform, out TransformedBounds, ref TransformedBounds);
        }
    }
}
