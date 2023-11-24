using System;
using System.Collections.Generic;

namespace Pika.Web.Models;

public class PagedViewModel<T>
{
    public PagedViewModel(int currentPage, int totalCount, int postsPerPage)
    {
        if (totalCount < 0)
        {
            totalCount = 0;
        }

        TotalPages = Convert.ToInt32(Math.Ceiling(totalCount / (double)postsPerPage));

        if (currentPage <= 0)
        {
            currentPage = 1;
        }

        if (currentPage > TotalPages)
        {
            currentPage = TotalPages;
        }

        CurrentPage = currentPage;
        Items = [];
    }

    public int TotalPages { get; }

    public int CurrentPage { get; }

    public List<T> Items { get; }

    public string Url { get; set; }
}