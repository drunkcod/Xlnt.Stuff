using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;

namespace Xlnt.Data
{
    [Describe(typeof(ParameterCollection<>))]
    public class ParameterCollectionSpec
    {
        int next = 0;
        IEnumerable<int> GetSequence(int n) {
            for(var i = 0; i != n; ++i) yield return ++next;
        }

        public void should_be_lazy_when_composed_of_sequences() {
            var items = new ParameterCollection<int>();
            var sequence = GetSequence(1);
            
            items.AddRange(sequence);
            items.AddRange(sequence);

            var values = items.ToArray();
            Verify.That(() => values[0] == 1);
            Verify.That(() => values[1] == 2);
            values = items.ToArray();
            Verify.That(() => values[0] == 3);
            Verify.That(() => values[1] == 4);
        }
    }
}
