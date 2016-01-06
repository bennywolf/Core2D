﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Linq;

namespace Core2D
{
    /// <summary>
    /// The <see cref="IImageCache"/> implementation for <see cref="Project"/> class.
    /// </summary>
    public partial class Project : ObservableResource, IImageCache
    {
        private static string ProjectEntryName = "Project.json";
        private static string ImageEntryNamePrefix = "Images\\";
        private IDictionary<string, byte[]> _images = new Dictionary<string, byte[]>();

        /// <inheritdoc/>
        public IEnumerable<ImageKey> Keys
        {
            get
            {
                return _images.Select(i => new ImageKey() { Key = i.Key }).ToList();
            }
        }

        /// <inheritdoc/>
        public string AddImageFromFile(string path, byte[] bytes)
        {
            var name = System.IO.Path.GetFileName(path);
            var key = ImageEntryNamePrefix + name;

            if (_images.Keys.Contains(key))
                return key;

            _images.Add(key, bytes);
            Notify("Keys");
            return key;
        }

        /// <inheritdoc/>
        public void AddImage(string key, byte[] bytes)
        {
            if (_images.Keys.Contains(key))
                return;

            _images.Add(key, bytes);
            Notify("Keys");
        }

        /// <inheritdoc/>
        public byte[] GetImage(string key)
        {
            byte[] bytes;
            if (_images.TryGetValue(key, out bytes))
                return bytes;
            else
                return null;
        }

        /// <inheritdoc/>
        public void RemoveImage(string key)
        {
            _images.Remove(key);
            Notify("Keys");
        }

        /// <inheritdoc/>
        public void PurgeUnusedImages(ICollection<string> used)
        {
            foreach (var kvp in _images.ToList())
            {
                if (!used.Contains(kvp.Key))
                {
                    _images.Remove(kvp.Key);
                }
            }
            Notify("Keys");
        }
    }
}