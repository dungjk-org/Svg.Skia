﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Parts of this source file are adapted from the https://github.com/vvvv/SVG
using System;
using System.Reflection;
using SkiaSharp;
using Svg.DataTypes;
using Svg.Document_Structure;

namespace Svg.Skia
{
    public class SKSvgRenderer : ISvgRenderer
    {
        private readonly SKCanvas _skCanvas;
        private readonly SKSize _skSize;
        private readonly CompositeDisposable _disposable;

        public SKSvgRenderer(SKCanvas skCanvas, SKSize skSize)
        {
            _skCanvas = skCanvas;
            _skSize = skSize;
            _disposable = new CompositeDisposable();
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        internal void Draw(SvgElement svgElement)
        {
            switch (svgElement)
            {
                // TODO:
                //case SvgAnchor svgAnchor:
                //    DrawAnchor(svgAnchor);
                //    break;
                case SvgFragment svgFragment:
                    DrawFragment(svgFragment);
                    break;
                case SvgImage svgImage:
                    DrawImage(svgImage);
                    break;
                case SvgSwitch svgSwitch:
                    DrawSwitch(svgSwitch);
                    break;
                case SvgUse svgUse:
                    DrawUse(svgUse);
                    break;
                case SvgForeignObject svgForeignObject:
                    DrawForeignObject(svgForeignObject);
                    break;
                case SvgCircle svgCircle:
                    DrawCircle(svgCircle);
                    break;
                case SvgEllipse svgEllipse:
                    DrawEllipse(svgEllipse);
                    break;
                case SvgRectangle svgRectangle:
                    DrawRectangle(svgRectangle);
                    break;
                case SvgGlyph svgGlyph:
                    DrawGlyph(svgGlyph);
                    break;
                case SvgGroup svgGroup:
                    DrawGroup(svgGroup);
                    break;
                case SvgLine svgLine:
                    DrawLine(svgLine);
                    break;
                case SvgPath svgPath:
                    DrawPath(svgPath);
                    break;
                case SvgPolyline svgPolyline:
                    DrawPolyline(svgPolyline);
                    break;
                case SvgPolygon svgPolygon:
                    DrawPolygon(svgPolygon);
                    break;
                case SvgText svgText:
                    DrawText(svgText);
                    break;
                case SvgTextPath svgTextPath:
                    DrawTextPath(svgTextPath);
                    break;
                case SvgTextRef svgTextRef:
                    DrawTextRef(svgTextRef);
                    break;
                case SvgTextSpan svgTextSpan:
                    DrawTextSpan(svgTextSpan);
                    break;
                default:
                    break;
            }
        }

        internal void DrawMarker(SvgMarker svgMarker, SvgVisualElement pOwner, SKPoint pRefPoint, SKPoint pMarkerPoint1, SKPoint pMarkerPoint2, bool isStartMarker)
        {
            float fAngle1 = 0f;
            if (svgMarker.Orient.IsAuto)
            {
                float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
                float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
                fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
                if (isStartMarker && svgMarker.Orient.IsAutoStartReverse)
                {
                    fAngle1 += 180;
                }
            }
            RenderPart2(svgMarker, fAngle1, pOwner, pRefPoint);
        }

        internal void DrawMarker(SvgMarker svgMarker, SvgVisualElement pOwner, SKPoint pRefPoint, SKPoint pMarkerPoint1, SKPoint pMarkerPoint2, SKPoint pMarkerPoint3)
        {
            float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
            float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
            float fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
            xDiff = pMarkerPoint3.X - pMarkerPoint2.X;
            yDiff = pMarkerPoint3.Y - pMarkerPoint2.Y;
            float fAngle2 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
            RenderPart2(svgMarker, (fAngle1 + fAngle2) / 2, pOwner, pRefPoint);
        }

        internal SvgVisualElement? GetMarkerElement(SvgMarker svgMarker)
        {
            SvgVisualElement? markerElement = null;

            foreach (var child in svgMarker.Children)
            {
                if (child is SvgVisualElement svgVisualElement)
                {
                    markerElement = svgVisualElement;
                    break;
                }
            }

            return markerElement;
        }

        internal void RenderPart2(SvgMarker svgMarker, float fAngle, SvgVisualElement pOwner, SKPoint pMarkerPoint)
        {
            var markerElement = GetMarkerElement(svgMarker);
            if (markerElement == null)
            {
                return;
            }

            var skMarkerMatrix = SKMatrix.MakeIdentity();

            var skMatrixMarkerPoint = SKMatrix.MakeTranslation(pMarkerPoint.X, pMarkerPoint.Y);
            SKMatrix.Concat(ref skMarkerMatrix, ref skMarkerMatrix, ref skMatrixMarkerPoint);

            var skMatrixAngle = SKMatrix.MakeRotationDegrees(svgMarker.Orient.IsAuto ? fAngle : svgMarker.Orient.Angle);
            SKMatrix.Concat(ref skMarkerMatrix, ref skMarkerMatrix, ref skMatrixAngle);

            var strokeWidth = pOwner.StrokeWidth.ToDeviceValue(null, UnitRenderingType.Other, svgMarker);

            var refX = svgMarker.RefX.ToDeviceValue(null, UnitRenderingType.Horizontal, svgMarker);
            var refY = svgMarker.RefY.ToDeviceValue(null, UnitRenderingType.Horizontal, svgMarker);

            switch (svgMarker.MarkerUnits)
            {
                case SvgMarkerUnits.StrokeWidth:
                    {
                        var skMatrixStrokeWidth = SKMatrix.MakeScale(strokeWidth, strokeWidth);
                        SKMatrix.Concat(ref skMarkerMatrix, ref skMarkerMatrix, ref skMatrixStrokeWidth);

                        float viewBoxWidth = svgMarker.ViewBox.Width;
                        float viewBoxHeight = svgMarker.ViewBox.Height;
                        float markerWidth = svgMarker.MarkerWidth;
                        float markerHeight = svgMarker.MarkerHeight;

                        var scaleFactorWidth = (viewBoxWidth <= 0) ? 1 : (markerWidth / viewBoxWidth);
                        var scaleFactorHeight = (viewBoxHeight <= 0) ? 1 : (markerHeight / viewBoxHeight);

                        var viewBoxToMarkerUnitsScaleX = 1f;
                        var viewBoxToMarkerUnitsScaleY = 1f;

                        viewBoxToMarkerUnitsScaleX = Math.Min(scaleFactorWidth, scaleFactorHeight);
                        viewBoxToMarkerUnitsScaleY = Math.Min(scaleFactorWidth, scaleFactorHeight);

                        var skMatrixTranslateRefXY = SKMatrix.MakeTranslation(-refX * viewBoxToMarkerUnitsScaleX, -refY * viewBoxToMarkerUnitsScaleY);
                        SKMatrix.Concat(ref skMarkerMatrix, ref skMarkerMatrix, ref skMatrixTranslateRefXY);

                        var skMatrixScaleXY = SKMatrix.MakeScale(viewBoxToMarkerUnitsScaleX, viewBoxToMarkerUnitsScaleY);
                        SKMatrix.Concat(ref skMarkerMatrix, ref skMarkerMatrix, ref skMatrixScaleXY);
                    }
                    break;
                case SvgMarkerUnits.UserSpaceOnUse:
                    {
                        var skMatrixTranslateRefXY = SKMatrix.MakeTranslation(-refX, -refY);
                        SKMatrix.Concat(ref skMarkerMatrix, ref skMarkerMatrix, ref skMatrixTranslateRefXY);
                    }
                    break;
            }

            var originalParent = markerElement.Parent;
            var markerElementParent = markerElement.GetType().GetField("_parent", BindingFlags.NonPublic | BindingFlags.Instance);
            if (markerElementParent != null)
            {
                markerElementParent.SetValue(markerElement, svgMarker);
            }

            markerElement.InvalidateChildPaths();

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgMarker.Transforms);
            SKMatrix.Concat(ref skMatrix, ref skMatrix, ref skMarkerMatrix);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgMarker, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgMarker, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgMarker, _disposable);

            // TODO: _skCanvas.ClipRect(skRectClip, SKClipOperation.Intersect);

            Draw(markerElement);

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();

            if (markerElementParent != null)
            {
                markerElementParent.SetValue(markerElement, originalParent);
            }
        }

        internal void DrawMarkers(SvgMarkerElement svgMarkerElement, SKPath sKPath)
        {
            var pathTypes = SkiaUtil.GetPathTypes(sKPath);
            var pathLength = pathTypes.Count;

            if (svgMarkerElement.MarkerStart != null)
            {
                var refPoint1 = pathTypes[0].Point;
                var index = 1;
                while (index < pathLength && pathTypes[index].Point == refPoint1)
                {
                    ++index;
                }
                var refPoint2 = pathTypes[index].Point;
                var marker = svgMarkerElement.OwnerDocument.GetElementById<SvgMarker>(svgMarkerElement.MarkerStart.ToString());
                DrawMarker(marker, svgMarkerElement, refPoint1, refPoint1, refPoint2, true);
            }

            if (svgMarkerElement.MarkerMid != null)
            {
                var marker = svgMarkerElement.OwnerDocument.GetElementById<SvgMarker>(svgMarkerElement.MarkerMid.ToString());
                int bezierIndex = -1;
                for (int i = 1; i <= pathLength - 2; i++)
                {
                    // for Bezier curves, the marker shall only been shown at the last point
                    if ((pathTypes[i].Type & (byte)PathPointType.PathTypeMask) == (byte)PathPointType.Bezier)
                        bezierIndex = (bezierIndex + 1) % 3;
                    else
                        bezierIndex = -1;

                    if (bezierIndex == -1 || bezierIndex == 2)
                    {
                        DrawMarker(marker, svgMarkerElement, pathTypes[i].Point, pathTypes[i - 1].Point, pathTypes[i].Point, pathTypes[i + 1].Point);
                    }
                }
            }

            if (svgMarkerElement.MarkerEnd != null)
            {
                var marker = svgMarkerElement.OwnerDocument.GetElementById<SvgMarker>(svgMarkerElement.MarkerEnd.ToString());
                var index = pathLength - 1;
                var refPoint1 = pathTypes[index].Point;
                --index;
                while (index > 0 && pathTypes[index].Point == refPoint1)
                {
                    --index;
                }
                var refPoint2 = pathTypes[index].Point;
                DrawMarker(marker, svgMarkerElement, refPoint1, refPoint2, pathTypes[pathLength - 1].Point, false);
            }
        }

        internal void AddMarkers(SvgGroup svgGroup)
        {
            if (svgGroup.MarkerStart == null && svgGroup.MarkerMid == null && svgGroup.MarkerEnd == null)
            {
                return;
            }

            foreach (var svgElement in svgGroup.Children)
            {
                if (svgElement is SvgMarkerElement svgMarkerElement)
                {
                    if (svgGroup.MarkerStart != null && svgMarkerElement.MarkerStart == null)
                    {
                        svgMarkerElement.MarkerStart = svgGroup.MarkerStart;
                    }
                    if (svgGroup.MarkerMid != null && svgMarkerElement.MarkerMid == null)
                    {
                        svgMarkerElement.MarkerMid = svgGroup.MarkerMid;
                    }
                    if (svgGroup.MarkerEnd != null && svgMarkerElement.MarkerEnd == null)
                    {
                        svgMarkerElement.MarkerEnd = svgGroup.MarkerEnd;
                    }
                }
            }
        }

        internal bool CanDraw(SvgVisualElement svgVisualElement)
        {
            return svgVisualElement.Visible == true
                && !string.Equals(svgVisualElement.Display, "none", StringComparison.OrdinalIgnoreCase);
        }

        // TODO:
        //public void DrawAnchor(SvgAnchor svgAnchor)
        //{
        //    _skCanvas.Save();
        //
        //    var skMatrix = SkiaUtil.GetSKMatrix(svgAnchor.Transforms);
        //    SkiaUtil.SetTransform(_skCanvas, skMatrix);
        //
        //    var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgAnchor, _disposable);
        //
        //    foreach (var svgElement in svgAnchor.Children)
        //    {
        //        Draw(svgElement);
        //    }
        //
        //    if (skPaintOpacity != null)
        //    {
        //        _skCanvas.Restore();
        //    }
        //
        //    _skCanvas.Restore();
        //}

        public void DrawFragment(SvgFragment svgFragment)
        {
            float x = svgFragment.X.ToDeviceValue(null, UnitRenderingType.Horizontal, svgFragment);
            float y = svgFragment.Y.ToDeviceValue(null, UnitRenderingType.Vertical, svgFragment);
            var skSize = SkiaUtil.GetDimensions(svgFragment);

            _skCanvas.Save();

            switch (svgFragment.Overflow)
            {
                case SvgOverflow.Auto:
                case SvgOverflow.Visible:
                case SvgOverflow.Inherit:
                    break;
                default:
                    var skClipRect = SKRect.Create(x, y, skSize.Width, skSize.Height);
                    _skCanvas.ClipRect(skClipRect, SKClipOperation.Intersect);
                    break;
            }

            var skMatrixViewBox = SkiaUtil.GetSvgViewBoxTransform(svgFragment.ViewBox, svgFragment.AspectRatio, x, y, skSize.Width, skSize.Height);
            var skMatrix = SkiaUtil.GetSKMatrix(svgFragment.Transforms);
            SKMatrix.Concat(ref skMatrix, ref skMatrix, ref skMatrixViewBox);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgFragment, _disposable);

            foreach (var svgElement in svgFragment.Children)
            {
                Draw(svgElement);
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawImage(SvgImage svgImage)
        {
            if (!CanDraw(svgImage))
            {
                return;
            }

            float width = svgImage.Width.ToDeviceValue(null, UnitRenderingType.Horizontal, svgImage);
            float height = svgImage.Height.ToDeviceValue(null, UnitRenderingType.Vertical, svgImage);
            var location = svgImage.Location.ToDeviceValue(null, svgImage);

            if (width <= 0f || height <= 0f || svgImage.Href == null)
            {
                return;
            }

            var image = SkiaUtil.GetImage(svgImage, svgImage.Href);
            var skImage = image as SKImage;
            var svgFragment = image as SvgFragment;
            if (skImage == null && svgFragment == null)
            {
                return;
            }

            if (skImage != null)
            {
                _disposable.Add(skImage);
            }

            SKRect srcRect = default;

            if (skImage != null)
            {
                srcRect = SKRect.Create(0f, 0f, skImage.Width, skImage.Height);
            }

            if (svgFragment != null)
            {
                var skSize = SkiaUtil.GetDimensions(svgFragment);
                srcRect = SKRect.Create(0f, 0f, skSize.Width, skSize.Height);
            }

            var destClip = SKRect.Create(location.X, location.Y, width, height);
            var destRect = destClip;

            var aspectRatio = svgImage.AspectRatio;
            if (aspectRatio.Align != SvgPreserveAspectRatio.none)
            {
                var fScaleX = destClip.Width / srcRect.Width;
                var fScaleY = destClip.Height / srcRect.Height;
                var xOffset = 0f;
                var yOffset = 0f;

                if (aspectRatio.Slice)
                {
                    fScaleX = Math.Max(fScaleX, fScaleY);
                    fScaleY = Math.Max(fScaleX, fScaleY);
                }
                else
                {
                    fScaleX = Math.Min(fScaleX, fScaleY);
                    fScaleY = Math.Min(fScaleX, fScaleY);
                }

                switch (aspectRatio.Align)
                {
                    case SvgPreserveAspectRatio.xMinYMin:
                        break;
                    case SvgPreserveAspectRatio.xMidYMin:
                        xOffset = (destClip.Width - srcRect.Width * fScaleX) / 2;
                        break;
                    case SvgPreserveAspectRatio.xMaxYMin:
                        xOffset = (destClip.Width - srcRect.Width * fScaleX);
                        break;
                    case SvgPreserveAspectRatio.xMinYMid:
                        yOffset = (destClip.Height - srcRect.Height * fScaleY) / 2;
                        break;
                    case SvgPreserveAspectRatio.xMidYMid:
                        xOffset = (destClip.Width - srcRect.Width * fScaleX) / 2;
                        yOffset = (destClip.Height - srcRect.Height * fScaleY) / 2;
                        break;
                    case SvgPreserveAspectRatio.xMaxYMid:
                        xOffset = (destClip.Width - srcRect.Width * fScaleX);
                        yOffset = (destClip.Height - srcRect.Height * fScaleY) / 2;
                        break;
                    case SvgPreserveAspectRatio.xMinYMax:
                        yOffset = (destClip.Height - srcRect.Height * fScaleY);
                        break;
                    case SvgPreserveAspectRatio.xMidYMax:
                        xOffset = (destClip.Width - srcRect.Width * fScaleX) / 2;
                        yOffset = (destClip.Height - srcRect.Height * fScaleY);
                        break;
                    case SvgPreserveAspectRatio.xMaxYMax:
                        xOffset = (destClip.Width - srcRect.Width * fScaleX);
                        yOffset = (destClip.Height - srcRect.Height * fScaleY);
                        break;
                }

                destRect = SKRect.Create(
                    destClip.Left + xOffset, destClip.Top + yOffset,
                    srcRect.Width * fScaleX, srcRect.Height * fScaleY);
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgImage.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgImage, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgImage, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgImage, _disposable);

            _skCanvas.ClipRect(destClip, SKClipOperation.Intersect);

            if (skImage != null)
            {
                _skCanvas.DrawImage(skImage, srcRect, destRect);
            }

            if (svgFragment != null)
            {
                _skCanvas.Save();

                float dx = destRect.Left;
                float dy = destRect.Top;
                float sx = destRect.Width / srcRect.Width;
                float sy = destRect.Height / srcRect.Height;
                var skTranslationMatrix = SKMatrix.MakeTranslation(dx, dy);
                var skScaleMatrix = SKMatrix.MakeScale(sx, sy);
                SKMatrix.Concat(ref skTranslationMatrix, ref skTranslationMatrix, ref skScaleMatrix);
                SkiaUtil.SetTransform(_skCanvas, skTranslationMatrix);

                DrawFragment(svgFragment);

                _skCanvas.Restore();
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawSwitch(SvgSwitch svgSwitch)
        {
            if (!CanDraw(svgSwitch))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgSwitch.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgSwitch, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgSwitch, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgSwitch, _disposable);

            // TODO:

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawSymbol(SvgSymbol svgSymbol)
        {
            if (!CanDraw(svgSymbol))
            {
                return;
            }

            _skCanvas.Save();

            float x = 0f;
            float y = 0f;
            float width = svgSymbol.ViewBox.Width;
            float height = svgSymbol.ViewBox.Height;

            if (svgSymbol.CustomAttributes.TryGetValue("width", out string? _widthString))
            {
                if (new SvgUnitConverter().ConvertFrom(_widthString) is SvgUnit _width)
                {
                    width = _width.ToDeviceValue(null, UnitRenderingType.Horizontal, svgSymbol);
                }
            }

            if (svgSymbol.CustomAttributes.TryGetValue("height", out string? heightString))
            {
                if (new SvgUnitConverter().ConvertFrom(heightString) is SvgUnit _height)
                {
                    height = _height.ToDeviceValue(null, UnitRenderingType.Vertical, svgSymbol);
                }
            }

            var skRectBounds = SKRect.Create(x, y, width, height);

            var skMatrixViewBox = SkiaUtil.GetSvgViewBoxTransform(svgSymbol.ViewBox, svgSymbol.AspectRatio, x, y, width, height);
            var skMatrix = SkiaUtil.GetSKMatrix(svgSymbol.Transforms);
            SKMatrix.Concat(ref skMatrix, ref skMatrix, ref skMatrixViewBox);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgSymbol, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgSymbol, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgSymbol, _disposable);

            foreach (var svgElement in svgSymbol.Children)
            {
                Draw(svgElement);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawUse(SvgUse svgUse)
        {
            if (!CanDraw(svgUse))
            {
                return;
            }

            var svgVisualElement = SkiaUtil.GetReference<SvgVisualElement>(svgUse, svgUse.ReferencedElement);
            if (svgVisualElement == null || SkiaUtil.HasRecursiveReference(svgUse))
            {
                return;
            }

            float x = svgUse.X.ToDeviceValue(null, UnitRenderingType.Horizontal, svgUse);
            float y = svgUse.Y.ToDeviceValue(null, UnitRenderingType.Vertical, svgUse);
            var skMatrixTranslateXY = SKMatrix.MakeTranslation(x, y);
            var skMatrix = SkiaUtil.GetSKMatrix(svgUse.Transforms);
            SKMatrix.Concat(ref skMatrix, ref skMatrix, ref skMatrixTranslateXY);

            var ew = svgUse.Width.ToDeviceValue(null, UnitRenderingType.Horizontal, svgUse);
            var eh = svgUse.Height.ToDeviceValue(null, UnitRenderingType.Vertical, svgUse);
            if (ew > 0 && eh > 0)
            {
                var _attributes = svgVisualElement.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_attributes != null)
                {
                    var attributes = _attributes.GetValue(svgVisualElement) as SvgAttributeCollection;
                    if (attributes != null)
                    {
                        var viewBox = attributes.GetAttribute<SvgViewBox>("viewBox");
                        if (viewBox != SvgViewBox.Empty && Math.Abs(ew - viewBox.Width) > float.Epsilon && Math.Abs(eh - viewBox.Height) > float.Epsilon)
                        {
                            var sw = ew / viewBox.Width;
                            var sh = eh / viewBox.Height;

                            var skMatrixTranslateSWSH = SKMatrix.MakeTranslation(sw, sh);
                            SKMatrix.Concat(ref skMatrix, ref skMatrix, ref skMatrixTranslateSWSH);
                        }
                    }
                }
            }

            var originalParent = svgUse.Parent;
            var useParent = svgUse.GetType().GetField("_parent", BindingFlags.NonPublic | BindingFlags.Instance);
            if (useParent != null)
            {
                useParent.SetValue(svgVisualElement, svgUse);
            }

            svgVisualElement.InvalidateChildPaths();

            _skCanvas.Save();

            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgUse, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgUse, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgUse, _disposable);

            if (svgVisualElement is SvgSymbol svgSymbol)
            {
                DrawSymbol(svgSymbol);
            }
            else
            {
                Draw(svgVisualElement);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();

            if (useParent != null)
            {
                useParent.SetValue(svgVisualElement, originalParent);
            }
        }

        public void DrawForeignObject(SvgForeignObject svgForeignObject)
        {
            if (!CanDraw(svgForeignObject))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgForeignObject.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgForeignObject, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgForeignObject, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgForeignObject, _disposable);

            // TODO:

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawCircle(SvgCircle svgCircle)
        {
            if (!CanDraw(svgCircle))
            {
                return;
            }

            float cx = svgCircle.CenterX.ToDeviceValue(null, UnitRenderingType.Horizontal, svgCircle);
            float cy = svgCircle.CenterY.ToDeviceValue(null, UnitRenderingType.Vertical, svgCircle);
            float radius = svgCircle.Radius.ToDeviceValue(null, UnitRenderingType.Other, svgCircle);

            if (radius <= 0f)
            {
                return;
            }

            var skRectBounds = SKRect.Create(cx - radius, cy - radius, radius + radius, radius + radius);

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgCircle.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgCircle, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgCircle, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgCircle, _disposable);

            if (SkiaUtil.IsValidFill(svgCircle))
            {
                var skPaintFill = SkiaUtil.GetFillSKPaint(svgCircle, _skSize, skRectBounds, _disposable);
                _skCanvas.DrawCircle(cx, cy, radius, skPaintFill);
            }

            if (SkiaUtil.IsValidStroke(svgCircle))
            {
                var skPaintStroke = SkiaUtil.GetStrokeSKPaint(svgCircle, _skSize, skRectBounds, _disposable);
                _skCanvas.DrawCircle(cx, cy, radius, skPaintStroke);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawEllipse(SvgEllipse svgEllipse)
        {
            if (!CanDraw(svgEllipse))
            {
                return;
            }

            float cx = svgEllipse.CenterX.ToDeviceValue(null, UnitRenderingType.Horizontal, svgEllipse);
            float cy = svgEllipse.CenterY.ToDeviceValue(null, UnitRenderingType.Vertical, svgEllipse);
            float rx = svgEllipse.RadiusX.ToDeviceValue(null, UnitRenderingType.Other, svgEllipse);
            float ry = svgEllipse.RadiusY.ToDeviceValue(null, UnitRenderingType.Other, svgEllipse);

            if (rx <= 0f || ry <= 0f)
            {
                return;
            }

            var skRectBounds = SKRect.Create(cx - rx, cy - ry, rx + rx, ry + ry);

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgEllipse.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgEllipse, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgEllipse, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgEllipse, _disposable);

            if (SkiaUtil.IsValidFill(svgEllipse))
            {
                var skPaintFill = SkiaUtil.GetFillSKPaint(svgEllipse, _skSize, skRectBounds, _disposable);
                _skCanvas.DrawOval(cx, cy, rx, ry, skPaintFill);
            }

            if (SkiaUtil.IsValidStroke(svgEllipse))
            {
                var skPaintStroke = SkiaUtil.GetStrokeSKPaint(svgEllipse, _skSize, skRectBounds, _disposable);
                _skCanvas.DrawOval(cx, cy, rx, ry, skPaintStroke);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawRectangle(SvgRectangle svgRectangle)
        {
            if (!CanDraw(svgRectangle))
            {
                return;
            }

            float x = svgRectangle.X.ToDeviceValue(null, UnitRenderingType.Horizontal, svgRectangle);
            float y = svgRectangle.Y.ToDeviceValue(null, UnitRenderingType.Vertical, svgRectangle);
            float width = svgRectangle.Width.ToDeviceValue(null, UnitRenderingType.Horizontal, svgRectangle);
            float height = svgRectangle.Height.ToDeviceValue(null, UnitRenderingType.Vertical, svgRectangle);
            float rx = svgRectangle.CornerRadiusX.ToDeviceValue(null, UnitRenderingType.Horizontal, svgRectangle);
            float ry = svgRectangle.CornerRadiusY.ToDeviceValue(null, UnitRenderingType.Vertical, svgRectangle);

            if (width <= 0f || height <= 0f || rx < 0f || ry < 0f)
            {
                return;
            }

            if (rx > 0f)
            {
                float halfWidth = width / 2f;
                if (rx > halfWidth)
                {
                    rx = halfWidth;
                }
            }

            if (ry > 0f)
            {
                float halfHeight = height / 2f;
                if (ry > halfHeight)
                {
                    ry = halfHeight;
                }
            }

            bool isRound = rx > 0f && ry > 0f;
            var skRectBounds = SKRect.Create(x, y, width, height);

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgRectangle.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgRectangle, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgRectangle, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgRectangle, _disposable);

            if (SkiaUtil.IsValidFill(svgRectangle))
            {
                var skPaintFill = SkiaUtil.GetFillSKPaint(svgRectangle, _skSize, skRectBounds, _disposable);
                if (isRound)
                {
                    _skCanvas.DrawRoundRect(x, y, width, height, rx, ry, skPaintFill);
                }
                else
                {
                    _skCanvas.DrawRect(x, y, width, height, skPaintFill);
                }
            }

            if (SkiaUtil.IsValidStroke(svgRectangle))
            {
                var skPaintStroke = SkiaUtil.GetStrokeSKPaint(svgRectangle, _skSize, skRectBounds, _disposable);
                if (isRound)
                {
                    _skCanvas.DrawRoundRect(skRectBounds, rx, ry, skPaintStroke);
                }
                else
                {
                    _skCanvas.DrawRect(skRectBounds, skPaintStroke);
                }
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawGlyph(SvgGlyph svgGlyph)
        {
            if (!CanDraw(svgGlyph))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgGlyph.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgGlyph, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgGlyph, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgGlyph, _disposable);

            // TODO:

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawGroup(SvgGroup svgGroup)
        {
            if (!CanDraw(svgGroup))
            {
                return;
            }

            // TODO: Call AddMarkers only once.
            AddMarkers(svgGroup);

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgGroup.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgGroup, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgGroup, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgGroup, _disposable);

            foreach (var svgElement in svgGroup.Children)
            {
                Draw(svgElement);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawLine(SvgLine svgLine)
        {
            if (!CanDraw(svgLine))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgLine.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgLine, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgLine, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgLine, _disposable);

            var skPath = SkiaUtil.ToSKPath(svgLine, svgLine.FillRule, _disposable);
            if (skPath != null && !skPath.IsEmpty)
            {
                var skBounds = skPath.Bounds;

                if (SkiaUtil.IsValidStroke(svgLine))
                {
                    var skPaint = SkiaUtil.GetStrokeSKPaint(svgLine, _skSize, skBounds, _disposable);
                    _skCanvas.DrawPath(skPath, skPaint);
                }

                DrawMarkers(svgLine, skPath);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawPath(SvgPath svgPath)
        {
            if (!CanDraw(svgPath))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgPath.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgPath, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgPath, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgPath, _disposable);

            var skPath = SkiaUtil.ToSKPath(svgPath.PathData, svgPath.FillRule, _disposable);
            if (skPath != null && !skPath.IsEmpty)
            {
                var skBounds = skPath.Bounds;

                if (SkiaUtil.IsValidFill(svgPath))
                {
                    var skPaint = SkiaUtil.GetFillSKPaint(svgPath, _skSize, skBounds, _disposable);
                    _skCanvas.DrawPath(skPath, skPaint);
                }

                if (SkiaUtil.IsValidStroke(svgPath))
                {
                    var skPaint = SkiaUtil.GetStrokeSKPaint(svgPath, _skSize, skBounds, _disposable);
                    _skCanvas.DrawPath(skPath, skPaint);
                }

                DrawMarkers(svgPath, skPath);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawPolyline(SvgPolyline svgPolyline)
        {
            if (!CanDraw(svgPolyline))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgPolyline.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgPolyline, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgPolyline, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgPolyline, _disposable);

            var skPath = SkiaUtil.ToSKPath(svgPolyline.Points, svgPolyline.FillRule, false, _disposable);
            if (skPath != null && !skPath.IsEmpty)
            {
                var skBounds = skPath.Bounds;

                if (SkiaUtil.IsValidFill(svgPolyline))
                {
                    var skPaint = SkiaUtil.GetFillSKPaint(svgPolyline, _skSize, skBounds, _disposable);
                    _skCanvas.DrawPath(skPath, skPaint);
                }

                if (SkiaUtil.IsValidStroke(svgPolyline))
                {
                    var skPaint = SkiaUtil.GetStrokeSKPaint(svgPolyline, _skSize, skBounds, _disposable);
                    _skCanvas.DrawPath(skPath, skPaint);
                }

                DrawMarkers(svgPolyline, skPath);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawPolygon(SvgPolygon svgPolygon)
        {
            if (!CanDraw(svgPolygon))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgPolygon.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgPolygon, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgPolygon, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgPolygon, _disposable);

            var skPath = SkiaUtil.ToSKPath(svgPolygon.Points, svgPolygon.FillRule, true, _disposable);
            if (skPath != null && !skPath.IsEmpty)
            {
                var skBounds = skPath.Bounds;

                if (SkiaUtil.IsValidFill(svgPolygon))
                {
                    var skPaint = SkiaUtil.GetFillSKPaint(svgPolygon, _skSize, skBounds, _disposable);
                    _skCanvas.DrawPath(skPath, skPaint);
                }

                if (SkiaUtil.IsValidStroke(svgPolygon))
                {
                    var skPaint = SkiaUtil.GetStrokeSKPaint(svgPolygon, _skSize, skBounds, _disposable);
                    _skCanvas.DrawPath(skPath, skPaint);
                }

                DrawMarkers(svgPolygon, skPath);
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawText(SvgText svgText)
        {
            if (!CanDraw(svgText))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgText.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgText, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgText, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgText, _disposable);

            // TODO:
            bool isValidFill = SkiaUtil.IsValidFill(svgText);
            bool isValidStroke = SkiaUtil.IsValidStroke(svgText);

            if (isValidFill || isValidStroke)
            {
                var text = svgText.Text?.Trim();

                if (svgText.X.Count == 1 && svgText.Y.Count == 1 && !string.IsNullOrEmpty(text))
                {
                    // TODO:
                    float x0 = svgText.X[0].ToDeviceValue(null, UnitRenderingType.HorizontalOffset, svgText);
                    float y0 = svgText.Y[0].ToDeviceValue(null, UnitRenderingType.VerticalOffset, svgText);

                    // TODO:
                    var skBounds = SKRect.Create(0f, 0f, _skSize.Width, _skSize.Height);

                    if (SkiaUtil.IsValidFill(svgText))
                    {
                        var skPaint = SkiaUtil.GetFillSKPaint(svgText, _skSize, skBounds, _disposable);
                        SkiaUtil.SetSKPaintText(svgText, _skSize, skBounds, skPaint, _disposable);
                        _skCanvas.DrawText(text, x0, y0, skPaint);
                    }

                    if (SkiaUtil.IsValidStroke(svgText))
                    {
                        var skPaint = SkiaUtil.GetStrokeSKPaint(svgText, _skSize, skBounds, _disposable);
                        SkiaUtil.SetSKPaintText(svgText, _skSize, skBounds, skPaint, _disposable);
                        _skCanvas.DrawText(text, x0, y0, skPaint);
                    }
                }
            }

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawTextPath(SvgTextPath svgTextPath)
        {
            if (!CanDraw(svgTextPath))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgTextPath.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgTextPath, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgTextPath, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgTextPath, _disposable);

            // TODO:

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawTextRef(SvgTextRef svgTextRef)
        {
            if (!CanDraw(svgTextRef))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgTextRef.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgTextRef, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgTextRef, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgTextRef, _disposable);

            // TODO:

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }

        public void DrawTextSpan(SvgTextSpan svgTextSpan)
        {
            if (!CanDraw(svgTextSpan))
            {
                return;
            }

            _skCanvas.Save();

            var skMatrix = SkiaUtil.GetSKMatrix(svgTextSpan.Transforms);
            SkiaUtil.SetTransform(_skCanvas, skMatrix);
            SkiaUtil.SetClipPath(_skCanvas, svgTextSpan, _disposable);

            var skPaintOpacity = SkiaUtil.SetOpacity(_skCanvas, svgTextSpan, _disposable);

            var skPaintFilter = SkiaUtil.SetFilter(_skCanvas, svgTextSpan, _disposable);

            // TODO:

            if (skPaintFilter != null)
            {
                _skCanvas.Restore();
            }

            if (skPaintOpacity != null)
            {
                _skCanvas.Restore();
            }

            _skCanvas.Restore();
        }
    }
}
