﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Core2D.Shapes;
using Spatial;

namespace Core2D.Editor.Bounds.Shapes
{
    public class HitTestLine : HitTestBase
    {
        public override Type TargetType => typeof(ILineShape);

        public override IPointShape TryToGetPoint(IBaseShape shape, Point2 target, double radius, IDictionary<Type, HitTestBase> registered)
        {
            if (!(shape is ILineShape line))
                throw new ArgumentNullException(nameof(shape));

            var pointHitTest = registered[typeof(IPointShape)];

            if (pointHitTest.TryToGetPoint(line.Start, target, radius, registered) != null)
            {
                return line.Start;
            }

            if (pointHitTest.TryToGetPoint(line.End, target, radius, registered) != null)
            {
                return line.End;
            }

            return null;
        }

        public override bool Contains(IBaseShape shape, Point2 target, double radius, IDictionary<Type, HitTestBase> registered)
        {
            if (!(shape is ILineShape line))
                throw new ArgumentNullException(nameof(shape));

            var a = new Point2(line.Start.X, line.Start.Y);
            var b = new Point2(line.End.X, line.End.Y);
            var nearest = target.NearestOnLine(a, b);
            double distance = target.DistanceTo(nearest);
            return distance < radius;
        }

        public override bool Overlaps(IBaseShape shape, Rect2 target, double radius, IDictionary<Type, HitTestBase> registered)
        {
            if (!(shape is ILineShape line))
                throw new ArgumentNullException(nameof(shape));

            var a = new Point2(line.Start.X, line.Start.Y);
            var b = new Point2(line.End.X, line.End.Y);
            return Line2.LineIntersectsWithRect(a, b, target, out double x0clip, out double y0clip, out double x1clip, out double y1clip);
        }
    }
}
