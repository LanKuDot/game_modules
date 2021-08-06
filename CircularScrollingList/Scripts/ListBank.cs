/* Store the contents for ListBoxes to display.
 */
using UnityEngine;

namespace AirFishLab.ScrollingList
{
/* The base class of the list content container
 *
 * Create the individual ListBank by inheriting this class
 */
    public abstract class BaseListBank : MonoBehaviour
    {
        public abstract object GetListContent(int index);
        public abstract int GetListLength();
    }

/* The example of the ListBank
 */
    public class ListBank : BaseListBank
    {
        private int[] contents = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };

        public override object GetListContent(int index)
        {
            return contents[index].ToString();
        }

        public override int GetListLength()
        {
            return contents.Length;
        }
    }
}
