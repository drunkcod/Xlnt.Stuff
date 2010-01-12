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
    }
}
