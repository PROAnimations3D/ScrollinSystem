﻿using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UICashShopUtils
    {
        public static List<CashShopItem> GetFilteredList(List<CashShopItem> list, List<string> filterCategories)
        {
            // Prepare result
            List<CashShopItem> result = new List<CashShopItem>();
            // Trim filter categories
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
            CashShopItem entry;
            for (int i = 0; i < list.Count; ++i)
            {
                entry = list[i];
                if (entry == null)
                {
                    // Skip empty data
                    continue;
                }
                if (!string.IsNullOrEmpty(entry.Category) && !filterCategories.Contains(entry.Category.Trim().ToLower()))
                {
                    // Category filtering
                    continue;
                }
                result.Add(entry);
            }
            return result;
        }
    }
}
