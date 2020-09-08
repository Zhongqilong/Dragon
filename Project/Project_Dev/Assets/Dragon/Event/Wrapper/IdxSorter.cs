using System.Collections.Generic;

namespace Dragon.Events
{
    public class IdxSorter : Singleton<IdxSorter>, IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x > y)
            {
                return -1;
            }
            else
            {
                if (x == y)
                {
                    return 0;
                }
                return 1;
            }
        }
    }

}