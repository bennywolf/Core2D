﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Core2D
{
    /// <summary>
    /// <see cref="XPathGeometry"/> mini-language string representation parser.
    /// </summary>
    public static class XPathGeometryParser
    {
        /// <summary>
        /// Parse a mini-language string representation of the <see cref="XPathGeometry"/>.
        /// </summary>
        /// <remarks>
        /// The path geometry syntax may start with a "wsp*Fwsp*(0|1)" which indicate the winding mode (F0 is EvenOdd while F1 is NonZero).
        /// </remarks>
        /// <param name="source">The string with geometry data.</param>
        /// <returns>The new instance of the <see cref="XPathGeometry"/> class.</returns>
        public static XPathGeometry Parse(string source)
        {
            var fillRule = XFillRule.EvenOdd;
            var geometry = XPathGeometry.Create(new List<XPathFigure>(), fillRule);

            if (source != null)
            {
                int curIndex = 0;
                while ((curIndex < source.Length) && Char.IsWhiteSpace(source, curIndex))
                {
                    curIndex++;
                }

                if (curIndex < source.Length)
                {
                    if (source[curIndex] == 'F')
                    {
                        curIndex++;

                        while ((curIndex < source.Length) && Char.IsWhiteSpace(source, curIndex))
                        {
                            curIndex++;
                        }

                        if ((curIndex == source.Length) || ((source[curIndex] != '0') && (source[curIndex] != '1')))
                        {
                            throw new FormatException("Illegal token.");
                        }

                        fillRule = source[curIndex] == '0' ? XFillRule.EvenOdd : XFillRule.Nonzero;
                        curIndex++;
                    }
                }

                var parser = new SvgPathGeometryParser();

                parser.Parse(geometry, source, curIndex);
            }

            geometry.FillRule = fillRule;

            return geometry;
        }
    }

    /// <summary>
    /// Parser for SVG path geometry http://www.w3.org/TR/SVG11/paths.html.
    /// </summary>
    public class SvgPathGeometryParser
    {
        private const bool _allowSign = true;
        private const bool _allowComma = true;
        private const bool _isFilled = true;
        private const bool _isClosed = true;
        private const bool _isStroked = true;
        private const bool _isSmoothJoin = true;
        private string _pathString;
        private int _pathLength;
        private int _curIndex;
        private bool _figureStarted;
        private Point2 _lastStart;
        private Point2 _lastPoint;
        private Point2 _secondLastPoint;
        private char _token;
        private XPathGeometry _geometry;

        private void InvalidToken()
        {
            throw new FormatException($"Unexpected token {_pathString} at index {_curIndex - 1}.");
        }

        private bool HaveMore()
        {
            return _curIndex < _pathLength;
        }

        private bool SkipWhiteSpace(bool allowComma)
        {
            bool commaMet = false;

            while (HaveMore())
            {
                char ch = _pathString[_curIndex];

                switch (ch)
                {
                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t':
                        break;

                    case ',':
                        if (allowComma)
                        {
                            commaMet = true;
                            allowComma = false;
                        }
                        else
                        {
                            InvalidToken();
                        }
                        break;

                    default:
                        if (((ch > ' ') && (ch <= 'z')) || !Char.IsWhiteSpace(ch))
                        {
                            return commaMet;
                        }
                        break;
                }

                _curIndex++;
            }

            return commaMet;
        }

        private bool ReadToken()
        {
            SkipWhiteSpace(!_allowComma);

            if (HaveMore())
            {
                _token = _pathString[_curIndex++];
                return true;
            }
            return false;
        }

        private bool IsNumber(bool allowComma)
        {
            bool commaMet = SkipWhiteSpace(allowComma);

            if (HaveMore())
            {
                _token = _pathString[_curIndex];

                if ((_token == '.') || (_token == '-') || (_token == '+') || ((_token >= '0') && (_token <= '9'))
                    || (_token == 'I')
                    || (_token == 'N'))
                {
                    return true;
                }
            }

            if (commaMet)
            {
                InvalidToken();
            }

            return false;
        }

        private void SkipDigits(bool signAllowed)
        {
            if (signAllowed && HaveMore() && ((_pathString[_curIndex] == '-') || _pathString[_curIndex] == '+'))
            {
                _curIndex++;
            }

            while (HaveMore() && (_pathString[_curIndex] >= '0') && (_pathString[_curIndex] <= '9'))
            {
                _curIndex++;
            }
        }

        private double ReadNumber(bool allowComma)
        {
            if (!IsNumber(allowComma))
            {
                InvalidToken();
            }

            bool simple = true;
            int start = _curIndex;

            if (HaveMore() && ((_pathString[_curIndex] == '-') || _pathString[_curIndex] == '+'))
            {
                _curIndex++;
            }

            if (HaveMore() && (_pathString[_curIndex] == 'I'))
            {
                _curIndex = Math.Min(_curIndex + 8, _pathLength);
                simple = false;
            }
            else if (HaveMore() && (_pathString[_curIndex] == 'N'))
            {
                _curIndex = Math.Min(_curIndex + 3, _pathLength);
                simple = false;
            }
            else
            {
                SkipDigits(!_allowSign);

                if (HaveMore() && (_pathString[_curIndex] == '.'))
                {
                    simple = false;
                    _curIndex++;
                    SkipDigits(!_allowSign);
                }

                if (HaveMore() && ((_pathString[_curIndex] == 'E') || (_pathString[_curIndex] == 'e')))
                {
                    simple = false;
                    _curIndex++;
                    SkipDigits(_allowSign);
                }
            }

            if (simple && (_curIndex <= (start + 8)))
            {
                int sign = 1;

                if (_pathString[start] == '+')
                {
                    start++;
                }
                else if (_pathString[start] == '-')
                {
                    start++;
                    sign = -1;
                }

                int value = 0;

                while (start < _curIndex)
                {
                    value = value * 10 + (_pathString[start] - '0');
                    start++;
                }

                return value * sign;
            }
            else
            {
                string subString = _pathString.Substring(start, _curIndex - start);
                try
                {
                    return Convert.ToDouble(subString, CultureInfo.InvariantCulture);
                }
                catch (FormatException ex)
                {
                    throw new FormatException($"Unexpected token {_pathString} at index {start}.", ex);
                }
            }
        }

        private bool ReadBool()
        {
            SkipWhiteSpace(_allowComma);

            if (HaveMore())
            {
                _token = _pathString[_curIndex++];

                if (_token == '0')
                {
                    return false;
                }
                else if (_token == '1')
                {
                    return true;
                }
            }

            InvalidToken();

            return false;
        }

        private Point2 ReadPoint(char cmd, bool allowcomma)
        {
            double x = ReadNumber(allowcomma);
            double y = ReadNumber(_allowComma);

            if (cmd >= 'a')
            {
                x += _lastPoint.X;
                y += _lastPoint.Y;
            }

            return Point2.Create(x, y);
        }

        private Point2 Reflect()
        {
            return Point2.Create(2 * _lastPoint.X - _secondLastPoint.X, 2 * _lastPoint.Y - _secondLastPoint.Y);
        }

        private void EnsureFigure()
        {
            if (!_figureStarted)
            {
                _geometry.BeginFigure(XPoint.FromPoint2(_lastStart), _isFilled, !_isClosed);
                _figureStarted = true;
            }
        }

        /// <summary>
        /// Parse a SVG path geometry string.
        /// </summary>
        /// <param name="context">The path geometry.</param>
        /// <param name="pathString">The path geometry string</param>
        /// <param name="startIndex">The string start index.</param>
        public void Parse(XPathGeometry context, string pathString, int startIndex)
        {
            _geometry = context;
            _pathString = pathString;
            _pathLength = pathString.Length;
            _curIndex = startIndex;
            _secondLastPoint = Point2.Create(0, 0);
            _lastPoint = Point2.Create(0, 0);
            _lastStart = Point2.Create(0, 0);
            _figureStarted = false;
            bool first = true;
            char last_cmd = ' ';

            while (ReadToken())
            {
                char cmd = _token;

                if (first)
                {
                    if ((cmd != 'M') && (cmd != 'm'))
                    {
                        InvalidToken();
                    }

                    first = false;
                }

                switch (cmd)
                {
                    case 'm':
                    case 'M':
                        _lastPoint = ReadPoint(cmd, !_allowComma);

                        context.BeginFigure(XPoint.FromPoint2(_lastPoint), _isFilled, !_isClosed);
                        _figureStarted = true;
                        _lastStart = _lastPoint;
                        last_cmd = 'M';

                        while (IsNumber(_allowComma))
                        {
                            _lastPoint = ReadPoint(cmd, !_allowComma);
                            context.LineTo(XPoint.FromPoint2(_lastPoint), _isStroked, !_isSmoothJoin);
                            last_cmd = 'L';
                        }
                        break;

                    case 'l':
                    case 'L':
                    case 'h':
                    case 'H':
                    case 'v':
                    case 'V':
                        EnsureFigure();

                        do
                        {
                            switch (cmd)
                            {
                                case 'l':
                                    _lastPoint = ReadPoint(cmd, !_allowComma);
                                    break;
                                case 'L':
                                    _lastPoint = ReadPoint(cmd, !_allowComma);
                                    break;
                                case 'h':
                                    _lastPoint.X += ReadNumber(!_allowComma);
                                    break;
                                case 'H':
                                    _lastPoint.X = ReadNumber(!_allowComma);
                                    break;
                                case 'v':
                                    _lastPoint.Y += ReadNumber(!_allowComma);
                                    break;
                                case 'V':
                                    _lastPoint.Y = ReadNumber(!_allowComma);
                                    break;
                            }

                            context.LineTo(XPoint.FromPoint2(_lastPoint), _isStroked, !_isSmoothJoin);
                        }
                        while (IsNumber(_allowComma));

                        last_cmd = 'L';
                        break;

                    case 'c':
                    case 'C':
                    case 's':
                    case 'S':
                        EnsureFigure();

                        do
                        {
                            Point2 p;

                            if ((cmd == 's') || (cmd == 'S'))
                            {
                                if (last_cmd == 'C')
                                {
                                    p = Reflect();
                                }
                                else
                                {
                                    p = _lastPoint;
                                }

                                _secondLastPoint = ReadPoint(cmd, !_allowComma);
                            }
                            else
                            {
                                p = ReadPoint(cmd, !_allowComma);

                                _secondLastPoint = ReadPoint(cmd, _allowComma);
                            }

                            _lastPoint = ReadPoint(cmd, _allowComma);
                            context.BezierTo(
                                XPoint.FromPoint2(p),
                                XPoint.FromPoint2(_secondLastPoint),
                                XPoint.FromPoint2(_lastPoint),
                                _isStroked,
                                !_isSmoothJoin);

                            last_cmd = 'C';
                        }
                        while (IsNumber(_allowComma));

                        break;

                    case 'q':
                    case 'Q':
                    case 't':
                    case 'T':
                        EnsureFigure();

                        do
                        {
                            if ((cmd == 't') || (cmd == 'T'))
                            {
                                if (last_cmd == 'Q')
                                {
                                    _secondLastPoint = Reflect();
                                }
                                else
                                {
                                    _secondLastPoint = _lastPoint;
                                }

                                _lastPoint = ReadPoint(cmd, !_allowComma);
                            }
                            else
                            {
                                _secondLastPoint = ReadPoint(cmd, !_allowComma);
                                _lastPoint = ReadPoint(cmd, _allowComma);
                            }

                            context.QuadraticBezierTo(
                                XPoint.FromPoint2(_secondLastPoint),
                                XPoint.FromPoint2(_lastPoint),
                                _isStroked,
                                !_isSmoothJoin);

                            last_cmd = 'Q';
                        }
                        while (IsNumber(_allowComma));

                        break;

                    case 'a':
                    case 'A':
                        EnsureFigure();

                        do
                        {
                            double w = ReadNumber(!_allowComma);
                            double h = ReadNumber(_allowComma);
                            double rotation = ReadNumber(_allowComma);
                            bool large = ReadBool();
                            bool sweep = ReadBool();

                            _lastPoint = ReadPoint(cmd, _allowComma);

                            context.ArcTo(
                                XPoint.FromPoint2(_lastPoint),
                                XPathSize.Create(w, h),
                                rotation,
                                large,
                                sweep ? XSweepDirection.Clockwise : XSweepDirection.Counterclockwise,
                                _isStroked,
                                !_isSmoothJoin);
                        }
                        while (IsNumber(_allowComma));

                        last_cmd = 'A';
                        break;

                    case 'z':
                    case 'Z':
                        EnsureFigure();
                        context.SetClosedState(_isClosed);

                        _figureStarted = false;
                        last_cmd = 'Z';
                        _lastPoint = _lastStart;
                        break;

                    default:
                        InvalidToken();
                        break;
                }
            }
        }
    }
}
