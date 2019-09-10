﻿using System;
using System.Collections.Specialized;
using System.Web.Routing;
using Composite.Core;
using Composite.Core.Threading;
using Orckestra.Composer.CompositeC1.Services;
using Orckestra.Composer.Providers;
using Orckestra.Composer.Services;
using Orckestra.Composer.Store.Parameters;
using Orckestra.Composer.Store.Providers;
using Orckestra.Composer.Utils;
using Orckestra.ExperienceManagement.Configuration;

namespace Orckestra.Composer.CompositeC1.Providers
{
    public class StoreUrlProvider : IStoreUrlProvider
    {
        private const string UrlTemplate = "{0}/s-{1}/{2}";
        protected const string ResourceCategory = "Store";
        protected ILocalizationProvider LocalizationProvider { get; private set; }
        protected IPageService PageService { get; private set; }
// protected IComposerContext ComposerContext { get; private set; }
        protected IWebsiteContext WebsiteContext { get; private set; }

        public StoreUrlProvider(ILocalizationProvider localizationProvider, IPageService pageService, IWebsiteContext wbsiteContext)
        {
            if (localizationProvider == null) { throw new ArgumentNullException("localizationProvider"); }
            if (pageService == null) { throw new ArgumentNullException("pageService"); }

            LocalizationProvider = localizationProvider;
            PageService = pageService;
            WebsiteContext = wbsiteContext;
        }

        public void RegisterRoutes(RouteCollection routeCollection)
        {
            //
        }

        public virtual string GetStoreUrl(GetStoreUrlParam parameters)
        {
            Assert(parameters);
            // Because of ConfigureAwait(false), we lost context here.
            // Therefore we need to re-initialize C1 context because getting the Url.
            using (ThreadDataManager.EnsureInitialize())
            {
                var pagesConfiguration = SiteConfiguration.GetPagesConfiguration(parameters.CultureInfo, WebsiteContext.WebsiteId);
                var baseUrl = PageService.GetPageUrl(pagesConfiguration.StoreListPageId, parameters.CultureInfo);
                var url = string.Format(UrlTemplate, baseUrl, UrlFormatter.Format(parameters.StoreName), parameters.StoreNumber);
                var uri = new Uri(
                    new Uri(parameters.BaseUrl, UriKind.Absolute),
                    new Uri(url, UriKind.Relative));

                return uri.ToString();

            }
        }

        public virtual string GetStoreLocatorUrl(GetStoreLocatorUrlParam parameters)
        {
            using (ThreadDataManager.EnsureInitialize())
            {
                var pagesConfiguration = SiteConfiguration.GetPagesConfiguration(parameters.CultureInfo, WebsiteContext.WebsiteId);
                var url = PageService.GetPageUrl(pagesConfiguration.StoreListPageId, parameters.CultureInfo);
                if(string.IsNullOrEmpty(url)) {
                    Log.LogError("StoreUrlProvider", "StoreList PageId is not configured");
                    return string.Empty;
                }
                var urlBuilder = new UrlBuilder(url);
                return urlBuilder.ToString();
            }
        }

        public virtual string GetStoresDirectoryUrl(GetStoresDirectoryUrlParam parameters)
        {
            using (ThreadDataManager.EnsureInitialize())
            {
                var pagesConfiguration = SiteConfiguration.GetPagesConfiguration(parameters.CultureInfo, WebsiteContext.WebsiteId);
                var url = PageService.GetPageUrl(pagesConfiguration.StoreDirectoryPageId, parameters.CultureInfo);
                var urlBuilder = new UrlBuilder(url);
                var queryString = new NameValueCollection();
                if (parameters.Page != 1)
                    queryString.Add("page", parameters.Page.ToString());
                return UrlFormatter.AppendQueryString(urlBuilder.ToString(), queryString);
            }
        }

        private void Assert(GetStoreUrlParam parameters)
        {
            if (parameters == null) { throw new ArgumentNullException("parameters"); }
            if (string.IsNullOrWhiteSpace(parameters.BaseUrl)) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage("BaseUrl"), "parameters"); }
            if (string.IsNullOrWhiteSpace(parameters.StoreNumber)) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage("StoreNumber"), "parameters"); }
            if (parameters.CultureInfo == null) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage("CultureInfo"), "parameters"); }
        }
    }
}
