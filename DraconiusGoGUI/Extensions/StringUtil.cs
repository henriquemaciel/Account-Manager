
using DracoProtos.Core.Base;
using DracoProtos.Core.Objects;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DraconiusGoGUI.Extensions
{
    public static class StringUtil
    {
        public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<FBaseLootItem> items)
        {
            var enumerable = items as IList<FBaseLootItem> ?? items;

            if (enumerable == null)
                return string.Empty;
            /*
 var enumerable = items as IList<ItemAward> ?? items.ToList();

 if (!enumerable.Any())
     return string.Empty;

 return
     enumerable.GroupBy(i => i.ItemId)
         .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
         .Select(y => String.Format("{0} x {1}", y.Amount, y.ItemName.Replace("Item", "")))
         .Aggregate((a, b) => String.Format("{0}, {1}", a, b));
         */

            string result = String.Empty;

            foreach (var i in enumerable)
                result += i.qty.ToString() + " x " + i.ToString();

            return result;
        }
    }
}
