﻿using System;
using Xml;

namespace Svg
{
    public abstract class SvgAnimationElement : SvgStylableElement
    {
        // ISvgAnimationAdditionAttributes

        [Attribute("additive", SvgElement.SvgNamespace)]
        public virtual string? Additive
        {
            get => this.GetAttribute("additive");
            set => this.SetAttribute("additive", value);
        }

        [Attribute("accumulate", SvgElement.SvgNamespace)]
        public virtual string? Accumulate
        {
            get => this.GetAttribute("accumulate");
            set => this.SetAttribute("accumulate", value);
        }

        // ISvgAnimationAttributeTargetAttributes

        [Attribute("attributeType", SvgElement.SvgNamespace)]
        public virtual string? AttributeType
        {
            get => this.GetAttribute("attributeType");
            set => this.SetAttribute("attributeType", value);
        }

        [Attribute("attributeName", SvgElement.SvgNamespace)]
        public virtual string? AttributeName
        {
            get => this.GetAttribute("attributeName");
            set => this.SetAttribute("attributeName", value);
        }

        // ISvgAnimationEventAttributes

        [Attribute("onbegin", SvgElement.SvgNamespace)]
        public virtual string? OnBegin
        {
            get => this.GetAttribute("onbegin");
            set => this.SetAttribute("onbegin", value);
        }

        [Attribute("onend", SvgElement.SvgNamespace)]
        public virtual string? OnEnd
        {
            get => this.GetAttribute("onend");
            set => this.SetAttribute("onend", value);
        }

        [Attribute("onrepeat", SvgElement.SvgNamespace)]
        public virtual string? OnRepeat
        {
            get => this.GetAttribute("onrepeat");
            set => this.SetAttribute("onrepeat", value);
        }

        [Attribute("onload", SvgElement.SvgNamespace)]
        public virtual string? OnLoad
        {
            get => this.GetAttribute("onload");
            set => this.SetAttribute("onload", value);
        }

        // ISvgAnimationTimingAttributes

        [Attribute("begin", SvgElement.SvgNamespace)]
        public virtual string? Begin
        {
            get => this.GetAttribute("begin");
            set => this.SetAttribute("begin", value);
        }

        [Attribute("dur", SvgElement.SvgNamespace)]
        public virtual string? Dur
        {
            get => this.GetAttribute("dur");
            set => this.SetAttribute("dur", value);
        }

        [Attribute("end", SvgElement.SvgNamespace)]
        public virtual string? End
        {
            get => this.GetAttribute("end");
            set => this.SetAttribute("end", value);
        }

        [Attribute("min", SvgElement.SvgNamespace)]
        public virtual string? Min
        {
            get => this.GetAttribute("min");
            set => this.SetAttribute("min", value);
        }

        [Attribute("max", SvgElement.SvgNamespace)]
        public virtual string? Max
        {
            get => this.GetAttribute("max");
            set => this.SetAttribute("max", value);
        }

        [Attribute("restart", SvgElement.SvgNamespace)]
        public virtual string? Restart
        {
            get => this.GetAttribute("restart");
            set => this.SetAttribute("restart", value);
        }

        [Attribute("repeatCount", SvgElement.SvgNamespace)]
        public virtual string? RepeatCount
        {
            get => this.GetAttribute("repeatCount");
            set => this.SetAttribute("repeatCount", value);
        }

        [Attribute("repeatDur", SvgElement.SvgNamespace)]
        public virtual string? RepeatDur
        {
            get => this.GetAttribute("repeatDur");
            set => this.SetAttribute("repeatDur", value);
        }

        [Attribute("fill", SvgElement.SvgNamespace)]
        public override string? Fill
        {
            get => this.GetAttribute("fill");
            set => this.SetAttribute("fill", value);
        }

        // ISvgAnimationValueAttributes

        [Attribute("calcMode", SvgElement.SvgNamespace)]
        public virtual string? CalcMode
        {
            get => this.GetAttribute("calcMode");
            set => this.SetAttribute("calcMode", value);
        }

        [Attribute("values", SvgElement.SvgNamespace)]
        public virtual string? Values
        {
            get => this.GetAttribute("values");
            set => this.SetAttribute("values", value);
        }

        [Attribute("keyTimes", SvgElement.SvgNamespace)]
        public virtual string? KeyTimes
        {
            get => this.GetAttribute("keyTimes");
            set => this.SetAttribute("keyTimes", value);
        }

        [Attribute("keySplines", SvgElement.SvgNamespace)]
        public virtual string? KeySplines
        {
            get => this.GetAttribute("keySplines");
            set => this.SetAttribute("keySplines", value);
        }

        [Attribute("from", SvgElement.SvgNamespace)]
        public virtual string? From
        {
            get => this.GetAttribute("from");
            set => this.SetAttribute("from", value);
        }

        [Attribute("to", SvgElement.SvgNamespace)]
        public virtual string? To
        {
            get => this.GetAttribute("to");
            set => this.SetAttribute("to", value);
        }

        [Attribute("by", SvgElement.SvgNamespace)]
        public virtual string? By
        {
            get => this.GetAttribute("by");
            set => this.SetAttribute("by", value);
        }

        public override void Print(Action<string> write, string indent)
        {
            base.Print(write, indent);

            if (this is ISvgAnimationAdditionAttributes svgAnimationAdditionAttributes)
            {
                PrintAnimationAdditionAttributes(svgAnimationAdditionAttributes, write, indent);
            }
            if (this is ISvgAnimationAttributeTargetAttributes svgAnimationAttributeTargetAttributes)
            {
                PrintAnimationAttributeTargetAttributes(svgAnimationAttributeTargetAttributes, write, indent);
            }
            if (this is ISvgAnimationEventAttributes svgAnimationEventAttributes)
            {
                PrintAnimationEventAttributes(svgAnimationEventAttributes, write, indent);
            }
            if (this is ISvgAnimationTimingAttributes svgAnimationTimingAttributes)
            {
                PrintAnimationTimingAttributes(svgAnimationTimingAttributes, write, indent);
            }
            if (this is ISvgAnimationValueAttributes svgAnimationValueAttributes)
            {
                PrintAnimationValueAttributes(svgAnimationValueAttributes, write, indent);
            }
        }
    }
}
