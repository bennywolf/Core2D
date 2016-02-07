﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Core2D.UnitTests
{
    public class StylesTests
    {
        [Fact]
        [Trait("Core2D.Xaml", "Collections")]
        public void Inherits_From_ObservableResource()
        {
            var target = new Styles();
            Assert.True(target is ObservableResource);
        }

        [Fact]
        [Trait("Core2D.Xaml", "Collections")]
        public void Children_Not_Null()
        {
            var target = new Styles();
            Assert.NotNull(target.Children);
        }
    }
}