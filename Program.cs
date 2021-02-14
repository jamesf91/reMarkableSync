using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync
{
    class Program
    {
        static void Main(string[] args)
        {
            RmCloud rmcloud = new RmCloud();
            List <RmItem> rootItems = rmcloud.GetItemHierarchy();

            RmItem testItem;
            foreach(var item in rootItems)
            {
                if (item.Type == RmItem.DocumentType)
                {
                    testItem = item;
                    break;
                }
            }
        }
    }
}
