using System.Collections.Generic;
using System.Linq;

namespace ExampleProject
{
    public class ExampleClass
    {
        private readonly IQueryable<int> queryable = new[] {1, 3, 4}.AsQueryable();

        public void ImplicitlyConvertToEnumerable()
        {
            var enumerable = queryable.ToDictionary(i => i);
        }

        public IEnumerable<int> ReturnEnumerable()
        {
            return queryable;
        }

        public void CastToEnumerable()
        {
            var enumerable = (IEnumerable<int>) queryable;
        }

        public void ExplicitlyConvertToEnumerable()
        {
            var enumerable = queryable.Select(i => i*2).AsEnumerable();
        }

    }
}
