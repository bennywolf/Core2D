﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Reflection;
using Autofac;
using Core2D.UI.Wpf.Dock.Factories;
using Core2D.UI.Wpf.Windows;
using Dock.Model;

namespace Core2D.UI.Wpf.Modules
{
    /// <summary>
    /// View components module.
    /// </summary>
    public class ViewModule : Autofac.Module
    {
        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(App).GetTypeInfo().Assembly).As<IDock>().InstancePerLifetimeScope();
            builder.RegisterType<EditorDockFactory>().As<IDockFactory>().InstancePerDependency();
            builder.RegisterType<MainWindow>().As<MainWindow>().InstancePerLifetimeScope();
        }
    }
}
