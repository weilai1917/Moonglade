﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moonglade.Data.Entities;
using Moonglade.Data.Infrastructure;

namespace Moonglade.Data.Spec
{
    public sealed class PostSpec : BaseSpecification<PostEntity>
    {
        public PostSpec(Guid? categoryId, int? top = null) :
            base(p => !p.PostPublish.IsDeleted &&
                      p.PostPublish.IsPublished &&
                      p.PostPublish.IsFeedIncluded &&
                      (categoryId == null || p.PostCategory.Any(c => c.CategoryId == categoryId.Value)))
        {
            // AddInclude(p => p.PostPublish);
            ApplyOrderByDescending(p => p.PostPublish.PubDateUtc);

            if (top.HasValue)
            {
                ApplyPaging(0, top.Value);
            }
        }

        public PostSpec(int year, int month = 0) :
            base(p => p.PostPublish.PubDateUtc.Value.Year == year &&
                      (month == 0 || p.PostPublish.PubDateUtc.Value.Month == month))
        {
            // Fix #313: Filter out unpublished posts
            UseCriteria(p => p.PostPublish.IsPublished && !p.PostPublish.IsDeleted);

            AddInclude(post => post.Include(p => p.PostPublish));
            ApplyOrderByDescending(p => p.PostPublish.PubDateUtc);
        }

        public PostSpec(DateTime date, string slug)
            : base(p => p.Slug == slug &&
             p.PostPublish.IsPublished &&
             p.PostPublish.PubDateUtc.Value.Date == date &&
             !p.PostPublish.IsDeleted)
        {
            AddInclude(post => post
                .Include(p => p.PostPublish)
                .Include(p => p.PostExtension)
                .Include(p => p.Comment)
                .Include(p => p.PostTag).ThenInclude(pt => pt.Tag)
                .Include(p => p.PostCategory).ThenInclude(pc => pc.Category));
        }

        public PostSpec(int pageSize, int pageIndex, Guid? categoryId = null)
            : base(p => !p.PostPublish.IsDeleted &&
                        p.PostPublish.IsPublished &&
                        (categoryId == null || p.PostCategory.Select(c => c.CategoryId).Contains(categoryId.Value)))
        {
            var startRow = (pageIndex - 1) * pageSize;

            //AddInclude(post => post
            //    .Include(p => p.PostPublish)
            //    .Include(p => p.PostExtension)
            //    .Include(p => p.PostTag)
            //    .ThenInclude(pt => pt.Tag));
            ApplyPaging(startRow, pageSize);
            ApplyOrderByDescending(p => p.PostPublish.PubDateUtc);
        }

        public PostSpec(Guid id, bool includeRelatedData = true) : base(p => p.Id == id)
        {
            if (includeRelatedData)
            {
                AddInclude(post => post
                    .Include(p => p.PostPublish)
                    .Include(p => p.PostTag)
                    .ThenInclude(pt => pt.Tag)
                    .Include(p => p.PostCategory)
                    .ThenInclude(pc => pc.Category));
            }
        }

        public PostSpec(PostPublishStatus status)
        {
            switch (status)
            {
                case PostPublishStatus.Draft:
                    UseCriteria(p => !p.PostPublish.IsPublished && !p.PostPublish.IsDeleted);
                    break;
                case PostPublishStatus.Published:
                    UseCriteria(p => p.PostPublish.IsPublished && !p.PostPublish.IsDeleted);
                    break;
                case PostPublishStatus.Deleted:
                    UseCriteria(p => p.PostPublish.IsDeleted);
                    break;
                case PostPublishStatus.NotSet:
                    UseCriteria(p => true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public PostSpec(bool isDeleted) :
            base(p => p.PostPublish.IsDeleted == isDeleted)
        {

        }

        public PostSpec() :
            base(p => p.PostPublish.IsDeleted)
        {

        }
    }
}
