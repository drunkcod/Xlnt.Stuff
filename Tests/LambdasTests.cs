using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xlnt.Stuff;

namespace Xlnt.Tests
{
    [TestFixture]
    public class LambdasTests
    {
        [Test]
        public void Lazy_only_evaluates_once() {
            var count = 0;
            Func<int> fun = () => { ++count; return 1; };
            var lazy = Lambdas.Lazy(fun);

            lazy(); lazy();
            Assert.That(count, Is.EqualTo(1));
        }

        class RefCount
        {
            public static int LiveObjects = 0;


            public RefCount() { ++LiveObjects; }
            ~RefCount() { --LiveObjects; }

            public int Value() { return LiveObjects; }
        }

        [Test]
        public void Should_release_target_func_after_force() {
            var n = new RefCount().Value();
            var lazy = Lambdas.Lazy<int>(new RefCount().Value);
            lazy();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.That(RefCount.LiveObjects, Is.EqualTo(0));
        }
    }
}
