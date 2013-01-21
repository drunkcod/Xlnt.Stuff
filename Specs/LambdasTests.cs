using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;
using Xlnt.Stuff;

namespace Xlnt.Tests
{
	[Describe(typeof(Lambdas))]
	public class LambdasTests
    {
		public void Lazy_only_evaluates_once() {
            var count = 0;
            Func<int> fun = () => { ++count; return 1; };
            var lazy = Lambdas.Lazy(fun);

            lazy(); lazy();
            Verify.That(() => count == 1);
        }

		public void Box_should_not_wrap_already_boxed_object() {
            Func<int, object> fun = x => null;
            Verify.That(() => Object.ReferenceEquals(Lambdas.Box(fun), fun));
        }

        class RefCount
        {
            public static int LiveObjects = 0;


            public RefCount() { ++LiveObjects; }
            ~RefCount() { --LiveObjects; }

            public int Value() { return LiveObjects; }
        }

        public void Should_release_target_func_after_force() {
            var n = new RefCount().Value();
            var lazy = Lambdas.Lazy<int>(new RefCount().Value);
            lazy();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Verify.That(() => RefCount.LiveObjects == 0);
        }
    }
}
