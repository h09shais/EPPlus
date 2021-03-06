/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  01/27/2020         EPPlus Software AB       Initial release EPPlus 5
 *************************************************************************************************/
using System;

namespace OfficeOpenXml.Core.CellStore
{
internal class ColumnIndex : IndexBase, IDisposable
{
    public ColumnIndex()
    {
        _pages = new PageIndex[CellStoreSettings.PagesPerColumnMin];
        PageCount = 0;
    }
    ~ColumnIndex()
    {
        _pages = null;
    }
    internal int GetPagePosition(int Row)
    {
        var page = (Row >> CellStoreSettings._pageBits);
        int pagePos;
        if (page >= 0 && page < PageCount && _pages[page].Index == page)
        {
            pagePos = page;
        }
        else
        {
            pagePos = ArrayUtil.OptimizedBinarySearch(_pages, page, PageCount);
        }

        if (pagePos >= 0)
        {
            GetPage(Row, ref pagePos);
            return pagePos;
        }
        else
        {
            var p = ~pagePos;

            if (GetPage(Row, ref p))
            {
                return p;
            }
            else
            {
                return pagePos;
            }
        }
    }

    private bool GetPage(int Row, ref int pagePos)
    {
        if (pagePos < PageCount && _pages[pagePos].MinIndex <= Row && (pagePos+1 == PageCount || _pages[pagePos+1].MinIndex > Row))
        {
            return true;
        }
        else
        {
            if (pagePos + 1 < PageCount && (_pages[pagePos + 1].MinIndex <= Row))
            {
                do
                {
                    pagePos++;
                }
                while (pagePos + 1 < PageCount && _pages[pagePos + 1].MinIndex <= Row);
                return true;
            }
            else if (pagePos - 1 >= 0 && _pages[pagePos - 1].MaxIndex >= Row)
            {
                do
                {
                    pagePos--;
                }
                while (pagePos - 1 > 0 && _pages[pagePos - 1].MaxIndex >= Row);
                return true;
            }
            return false;
        }
    }
    internal int GetNextRow(int row)
    {
        var p = GetPagePosition(row);
        if (p < 0)
        {
            p = ~p;
            if (p >= PageCount)
            {
                return -1;
            }
            else
            {

                if (_pages[p].IndexOffset + _pages[p].Rows[0].Index < row)
                {
                    if (p + 1 >= PageCount)
                    {
                        return -1;
                    }
                    else
                    {
                        return _pages[p + 1].IndexOffset + _pages[p].Rows[0].Index;
                    }
                }
                else
                {
                    return _pages[p].IndexOffset + _pages[p].Rows[0].Index;
                }
            }
        }
        else
        {
            if (p < PageCount)
            {
                var r = _pages[p].GetNextRow(row);
                if (r >= 0)
                {
                    return _pages[p].IndexOffset + _pages[p].Rows[r].Index;
                }
                else
                {
                    if (++p < PageCount)
                    {
                        return _pages[p].IndexOffset + _pages[p].Rows[0].Index;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            else
            {
                return -1;
            }
        }
    }
    //internal int FindNext(int Page)
    //{
    //    var p = GetPagePosition(Page);
    //    if (p < 0)
    //    {
    //        return ~p;
    //    }
    //    return p;
    //}
    internal PageIndex[] _pages;
    internal int PageCount;
    public void Dispose()
    {
        for (int p = 0; p < PageCount; p++)
        {
            ((IDisposable)_pages[p])?.Dispose();
        }
        _pages = null;
    }

}
}