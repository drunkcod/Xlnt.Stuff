﻿using System.Collections.Generic;
using Cone;

namespace Xlnt.Stuff.Tests
{
    [Describe(typeof(CollectionExtensions))]
    public class CollectionExtensionsSpec
    {
        public void ForEach_enumerates_all_values() {
            var items = new List<int>();
            (new[]{ 1, 2, 3 }).ForEach(items.Add);
            Check.That(() => items[0] == 1 && items[1] == 2 && items[2] == 3);
        }
    }
}
