﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Orckestra.Composer.Configuration;
using Orckestra.Composer.Enums;
using Orckestra.Composer.Factory;
using Orckestra.Composer.Parameters;
using Orckestra.Composer.Providers.Dam;
using Orckestra.Composer.Repositories;
using Orckestra.Composer.Requests;
using Orckestra.Composer.Services.Lookup;
using Orckestra.Composer.Utils;
using Orckestra.Composer.ViewModels;
using Orckestra.Overture;
using Orckestra.Overture.ServiceModel.RecurringOrders;

namespace Orckestra.Composer.Services
{
    public class RecurringOrderTemplatesViewService : IRecurringOrderTemplatesViewService
    {
        protected IRecurringOrdersRepository RecurringOrderRepository { get; private set; }
        protected IViewModelMapper ViewModelMapper { get; private set; }
        protected IOvertureClient OvertureClient { get; private set; }
        protected IRecurringOrderTemplateViewModelFactory RecurringOrderTemplateViewModelFactory { get; private set; }
        protected ILookupService LookupService { get; private set; }
        protected IImageService ImageService { get; private set; }
        protected IRecurringOrdersSettings RecurringOrdersSettings { get; private set; }



        public RecurringOrderTemplatesViewService(IRecurringOrdersRepository recurringOrdersRepository, 
            IViewModelMapper viewModelMapper,
            IOvertureClient overtureClient, ILookupService lookupService,
            IRecurringOrderTemplateViewModelFactory recurringOrderTemplateViewModelFactory,
            IImageService imageService,
            IRecurringOrdersSettings recurringOrdersSettings)
        {
            RecurringOrderRepository = recurringOrdersRepository ?? throw new ArgumentNullException(nameof(recurringOrdersRepository));
            ViewModelMapper = viewModelMapper ?? throw new ArgumentNullException(nameof(viewModelMapper));
            OvertureClient = overtureClient ?? throw new ArgumentNullException(nameof(overtureClient));
            LookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
            RecurringOrderTemplateViewModelFactory = recurringOrderTemplateViewModelFactory ?? throw new ArgumentNullException(nameof(recurringOrderTemplateViewModelFactory));
            ImageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            RecurringOrdersSettings = recurringOrdersSettings ?? throw new ArgumentNullException(nameof(recurringOrdersSettings));
        }
        public virtual async Task<bool> GetIsPaymentMethodUsedInRecurringOrders(GetIsPaymentMethodUsedInRecurringOrdersRequest request)
        {
            if (!RecurringOrdersSettings.Enabled)
                return false;

            if (request.ScopeId == null) { throw new ArgumentNullException(nameof(request.ScopeId)); }
            if (request.CustomerId == null) { throw new ArgumentNullException(nameof(request.CustomerId)); }
            if (request.CultureInfo == null) { throw new ArgumentNullException(nameof(request.CultureInfo)); }
            if (request.PaymentMethodId == null) { throw new ArgumentNullException(nameof(request.PaymentMethodId)); }

            var listOfRecurringOrderLineItems = await RecurringOrderRepository.GetRecurringOrderTemplates(request.ScopeId, request.CustomerId).ConfigureAwait(false);

            if (listOfRecurringOrderLineItems != null)
            {
                foreach (var item in listOfRecurringOrderLineItems.RecurringOrderLineItems ?? Enumerable.Empty<RecurringOrderLineItem>())
                {
                    if (item.PaymentMethodId == request.PaymentMethodId.ToGuid())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual async Task<RecurringOrderTemplatesViewModel> GetRecurringOrderTemplatesViewModelAsync(GetRecurringOrderTemplatesParam param)
        {
            if (!RecurringOrdersSettings.Enabled)
                return new RecurringOrderTemplatesViewModel();

            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.CultureInfo == null) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param.CultureInfo)), nameof(param)); }
            if (param.Scope == null) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param.Scope)), nameof(param)); }
            if (param.CustomerId == null) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param.CustomerId)), nameof(param)); }

            var listOfRecurringOrderLineItems = await RecurringOrderRepository.GetRecurringOrderTemplates(param.Scope, param.CustomerId).ConfigureAwait(false);
                 
            var vm = await CreateTemplatesViewModelAsync(new CreateRecurringOrderTemplatesViewModelParam
            {
                ListOfRecurringOrderLineItems = listOfRecurringOrderLineItems,
                CultureInfo = param.CultureInfo,
                BaseUrl = param.BaseUrl,
                CustomerId = param.CustomerId,
                ScopeId = param.Scope,
            });

            return vm;
        }

        protected virtual async Task<RecurringOrderTemplatesViewModel> CreateTemplatesViewModelAsync(CreateRecurringOrderTemplatesViewModelParam param)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.ListOfRecurringOrderLineItems == null) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param.ListOfRecurringOrderLineItems)), nameof(param)); }

            param.ProductImageInfo = new ProductImageInfo
            {
                ImageUrls = await ImageService.GetImageUrlsAsync(param.ListOfRecurringOrderLineItems).ConfigureAwait(false),
            };

            var methodDisplayNames = await LookupService.GetLookupDisplayNamesAsync(new GetLookupDisplayNamesParam
            {
                CultureInfo = param.CultureInfo,
                LookupType = LookupType.Order,
                LookupName = "PaymentMethodType",
            });

            param.PaymentMethodDisplayNames = methodDisplayNames;

            var vm = await RecurringOrderTemplateViewModelFactory.CreateRecurringOrderTemplatesViewModel(param);

            return vm;
        }

        public virtual async Task<RecurringOrderTemplatesViewModel> UpdateRecurringOrderTemplateLineItemQuantityAsync(UpdateRecurringOrderTemplateLineItemQuantityParam param)
        {
            if (!RecurringOrdersSettings.Enabled)
                return new RecurringOrderTemplatesViewModel();

            if (param == null) throw new ArgumentNullException(nameof(param), ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param)));

            //TODO: To be determined if we update the carts

            var listOfRecurringOrderLineItems = await RecurringOrderRepository.UpdateRecurringOrderTemplateLineItemQuantityAsync(param).ConfigureAwait(false);

            return await CreateTemplatesViewModelAsync(new CreateRecurringOrderTemplatesViewModelParam
            {
                ListOfRecurringOrderLineItems = listOfRecurringOrderLineItems,
                CultureInfo = param.CultureInfo,
                BaseUrl = param.BaseUrl,
                ScopeId = param.ScopeId,
                CustomerId = param.CustomerId});
        }

        public virtual async Task<RecurringOrderTemplatesViewModel> RemoveRecurringOrderTemplateLineItemAsync(RemoveRecurringOrderTemplateLineItemParam param)
        {
            if (!RecurringOrdersSettings.Enabled)
                return new RecurringOrderTemplatesViewModel();

            if (param == null) throw new ArgumentNullException(nameof(param), ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param)));

            await RecurringOrderRepository.RemoveRecurringOrderTemplateLineItem(param).ConfigureAwait(false);

            return await GetRecurringOrderTemplatesViewModelAsync(new GetRecurringOrderTemplatesParam{
                Scope = param.ScopeId,
                CustomerId = param.CustomerId,
                CultureInfo =  param.Culture,
                BaseUrl = param.BaseUrl });
        }

        public virtual async Task<RecurringOrderTemplatesViewModel> RemoveRecurringOrderTemplatesLineItemsAsync(RemoveRecurringOrderTemplateLineItemsParam param)
        {
            if (!RecurringOrdersSettings.Enabled)
                return new RecurringOrderTemplatesViewModel();

            if (param == null) throw new ArgumentNullException(nameof(param), ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param)));

            await RecurringOrderRepository.RemoveRecurringOrderTemplateLineItems(param).ConfigureAwait(false);

            return await GetRecurringOrderTemplatesViewModelAsync(new GetRecurringOrderTemplatesParam
            {
                Scope = param.ScopeId,
                CustomerId = param.CustomerId,
                CultureInfo = param.Culture,
                BaseUrl = param.BaseUrl});
        }

        public virtual async Task<RecurringOrderTemplatesViewModel> UpdateRecurringOrderTemplateLineItemAsync(UpdateRecurringOrderTemplateLineItemParam param)
        {
            if (!RecurringOrdersSettings.Enabled)
                return new RecurringOrderTemplatesViewModel();

            if (param == null) throw new ArgumentNullException(nameof(param), ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param)));

            var listOfRecurringOrderLineItems = await RecurringOrderRepository.UpdateRecurringOrderTemplateLineItemAsync(param).ConfigureAwait(false);

            return await CreateTemplatesViewModelAsync(new CreateRecurringOrderTemplatesViewModelParam
            {
                ListOfRecurringOrderLineItems = listOfRecurringOrderLineItems,
                CultureInfo = param.CultureInfo,
                BaseUrl = param.BaseUrl,
                ScopeId = param.ScopeId,
                CustomerId = param.CustomerId,
            });
        }

        public virtual async Task<RecurringOrderTemplateViewModel> GetRecurringOrderTemplateDetailViewModelAsync(GetRecurringOrderTemplateDetailParam param)
        {
            if (!RecurringOrdersSettings.Enabled)
                return new RecurringOrderTemplateViewModel();

            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.CustomerId == null) { throw new ArgumentException(nameof(param.CustomerId)); }
            if (param.CultureInfo == null) { throw new ArgumentException(nameof(param.CultureInfo)); }
            if (param.RecurringOrderLineItemId == null) { throw new ArgumentException(nameof(param.RecurringOrderLineItemId)); }
            if (string.IsNullOrWhiteSpace(param.Scope)) { throw new ArgumentException(nameof(param.Scope)); }
            if (string.IsNullOrWhiteSpace(param.BaseUrl)) { throw new ArgumentException(nameof(param.BaseUrl)); }

            var template = await RecurringOrderRepository.GetRecurringOrderTemplateDetails(param).ConfigureAwait(false);

            //Check if template is one of the current customer.
            if (template == null || template.CustomerId != param.CustomerId)
            {
                return null;
            }

            var vm = await CreateTemplateDetailsViewModelAsync(new CreateRecurringOrderTemplateDetailsViewModelParam
            {
                RecurringOrderLineItem = template,
                CultureInfo = param.CultureInfo,
                BaseUrl = param.BaseUrl,
                CustomerId = param.CustomerId,
                ScopeId = param.Scope
            });

            return vm;
        }

        protected virtual async Task<RecurringOrderTemplateViewModel> CreateTemplateDetailsViewModelAsync(CreateRecurringOrderTemplateDetailsViewModelParam param)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.RecurringOrderLineItem == null) { throw new ArgumentException(ArgumentNullMessageFormatter.FormatErrorMessage(nameof(param.RecurringOrderLineItem)), nameof(param)); }

            param.ProductImageInfo = new ProductImageInfo
            {
                ImageUrls = await ImageService.GetImageUrlsAsync(param.RecurringOrderLineItem).ConfigureAwait(false)
            };

            var methodDisplayNames = await LookupService.GetLookupDisplayNamesAsync(new GetLookupDisplayNamesParam
            {
                CultureInfo = param.CultureInfo,
                LookupType = LookupType.Order,
                LookupName = "PaymentMethodType",
            });

            param.PaymentMethodDisplayNames = methodDisplayNames;

            var vm = await RecurringOrderTemplateViewModelFactory.CreateRecurringOrderTemplateDetailsViewModel(param);

            return vm;
        }
    }
}
