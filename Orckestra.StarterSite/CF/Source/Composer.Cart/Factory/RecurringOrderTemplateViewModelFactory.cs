﻿using Orckestra.Composer.Cart.Helper;
using Orckestra.Composer.Cart.Parameters;
using Orckestra.Composer.Cart.Repositories.Order;
using Orckestra.Composer.Cart.ViewModels;
using Orckestra.Composer.Country;
using Orckestra.Composer.Parameters;
using Orckestra.Composer.Product.Factory;
using Orckestra.Composer.Product.Parameters;
using Orckestra.Composer.Product.Requests;
using Orckestra.Composer.Product.Services;
using Orckestra.Composer.Providers;
using Orckestra.Composer.Providers.Dam;
using Orckestra.Composer.Providers.Localization;
using Orckestra.Composer.Repositories;
using Orckestra.Composer.Services;
using Orckestra.Composer.Utils;
using Orckestra.Composer.ViewModels;
using Orckestra.Overture;
using Orckestra.Overture.ServiceModel;
using Orckestra.Overture.ServiceModel.Customers;
using Orckestra.Overture.ServiceModel.Metadata;
using Orckestra.Overture.ServiceModel.Orders;
using Orckestra.Overture.ServiceModel.Products;
using Orckestra.Overture.ServiceModel.RecurringOrders;
using Orckestra.Overture.ServiceModel.Requests.Customers;
using Orckestra.Overture.ServiceModel.Requests.Products;
using Orckestra.Overture.ServiceModel.Requests.RecurringOrders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orckestra.Composer.Cart.Factory
{
    public class RecurringOrderTemplateViewModelFactory : IRecurringOrderTemplateViewModelFactory
    {

        private RetrieveCountryParam _countryParam;
        protected RetrieveCountryParam CountryParam
        {
            get
            {
                return _countryParam ?? (_countryParam = new RetrieveCountryParam
                {
                    CultureInfo = ComposerContext.CultureInfo,
                    IsoCode = ComposerContext.CountryCode
                });
            }
        }

        protected ILocalizationProvider LocalizationProvider { get; private set; }
        protected IViewModelMapper ViewModelMapper { get; private set; }
        protected ICountryService CountryService { get; private set; }
        protected IComposerContext ComposerContext { get; private set; }
        protected IRecurringOrdersRepository RecurringOrdersRepository { get; private set; }
        protected IAddressRepository AddressRepository { get; private set; }
        protected IProductViewModelFactory ProductViewModelFactory { get; private set; }
        protected IProductUrlProvider ProductUrlProvider { get; private set; }
        protected IProductPriceViewService ProductPriceViewService { get; private set; }
        protected IOvertureClient OvertureClient { get; private set; }
        protected ICartViewModelFactory CartViewModelFactory { get; private set; }
        
        public RecurringOrderTemplateViewModelFactory(
            IOvertureClient overtureClient,
            ILocalizationProvider localizationProvider,
            IViewModelMapper viewModelMapper,
            ICountryService countryService,
            IComposerContext composerContext,
            IRecurringOrdersRepository recurringOrdersRepository,
            IAddressRepository addressRepository,
            IProductViewModelFactory productViewModelFactory,
            IProductUrlProvider productUrlProvider,
            IProductPriceViewService productPriceViewService,
            ICartViewModelFactory cartViewModelFactory)
        {
            if (overtureClient == null) { throw new ArgumentNullException(nameof(overtureClient)); }
            if (localizationProvider == null) { throw new ArgumentNullException(nameof(localizationProvider)); }
            if (viewModelMapper == null) { throw new ArgumentNullException(nameof(viewModelMapper)); }
            if (countryService == null) { throw new ArgumentNullException(nameof(countryService)); }
            if (recurringOrdersRepository == null) { throw new ArgumentNullException(nameof(recurringOrdersRepository)); }
            if (addressRepository == null) { throw new ArgumentNullException(nameof(addressRepository)); }
            if (productViewModelFactory == null) { throw new ArgumentNullException(nameof(productViewModelFactory)); }
            if (productUrlProvider == null) { throw new ArgumentNullException(nameof(productUrlProvider)); }
            if (productPriceViewService == null) { throw new ArgumentNullException(nameof(productPriceViewService)); }
            if (cartViewModelFactory == null) { throw new ArgumentNullException(nameof(cartViewModelFactory)); }

            LocalizationProvider = localizationProvider;
            ViewModelMapper = viewModelMapper;
            CountryService = countryService;
            ComposerContext = composerContext;
            RecurringOrdersRepository = recurringOrdersRepository;
            AddressRepository = addressRepository;
            ProductViewModelFactory = productViewModelFactory;
            ProductUrlProvider = productUrlProvider;
            ProductPriceViewService = productPriceViewService;
            OvertureClient = overtureClient;
            CartViewModelFactory = cartViewModelFactory;
        }

        public async Task<RecurringOrderTemplatesViewModel> CreateRecurringOrderTemplatesViewModel(CreateRecurringOrderTemplatesViewModelParam param)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.CultureInfo == null) { throw new ArgumentNullException(nameof(param.CultureInfo)); }
            if (param.ProductImageInfo == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo)); }
            if (param.ProductImageInfo.ImageUrls == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo.ImageUrls)); }
            if (param.ScopeId == null) { throw new ArgumentNullException(nameof(param.ScopeId)); }

            var vm = new RecurringOrderTemplatesViewModel();

            vm.RecurringOrderTemplateViewModelList = await CreateTemplateGroupedShippingAddress(new CreateTemplateGroupedShippingAddressParam { 
                ListOfRecurringOrderLineItems = param.ListOfRecurringOrderLineItems,
                CultureInfo = param.CultureInfo,
                ProductImageInfo = param.ProductImageInfo,
                BaseUrl = param.BaseUrl,
                CustomerId = param.CustomerId,
                ScopeId = param.ScopeId}).ConfigureAwaitWithCulture(false);


            foreach (var template in vm.RecurringOrderTemplateViewModelList)
            {
                await MapRecurringOrderLineitemFrequencyName(template, param.CultureInfo).ConfigureAwaitWithCulture(false);
            }

            return vm;
        }

        public async Task<List<RecurringOrderTemplateViewModel>> CreateTemplateGroupedShippingAddress(CreateTemplateGroupedShippingAddressParam param)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.ListOfRecurringOrderLineItems == null) { throw new ArgumentNullException(nameof(param.ListOfRecurringOrderLineItems)); }
            if (param.CultureInfo == null) { throw new ArgumentNullException(nameof(param.CultureInfo)); }
            if (param.ProductImageInfo == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo)); }
            if (param.ProductImageInfo.ImageUrls == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo.ImageUrls)); }
            if (param.ScopeId == null) { throw new ArgumentNullException(nameof(param.ScopeId)); }

            var groups = param.ListOfRecurringOrderLineItems.RecurringOrderLineItems.GroupBy(grp => grp.ShippingAddressId);

            var imgDictionary = LineItemHelper.BuildImageDictionaryFor(param.ProductImageInfo.ImageUrls);

            var itemList = new List<RecurringOrderTemplateViewModel>();

            foreach (var group in groups)
            {
                var templateViewModel = new RecurringOrderTemplateViewModel();

                templateViewModel.ShippingAddress = await MapShippingAddress(group.Key, param.CultureInfo).ConfigureAwaitWithCulture(false);

                var tasks = group.Select(g => MapToTemplateLineItemViewModel(new MapToTemplateLineItemViewModelParam
                {
                    RecurringOrderlineItem =  g,
                    CultureInfo = param.CultureInfo,
                    ImageDictionnary =  imgDictionary,
                    BaseUrl = param.BaseUrl
                }));
                var templateLineItems = await Task.WhenAll(tasks).ConfigureAwaitWithCulture(false);

                //Filter null to not have an error when rendering the page
                templateViewModel.RecurringOrderTemplateLineItemViewModels.AddRange(templateLineItems.Where(t => t != null).ToList());
                
                itemList.Add(templateViewModel);
            }

            return itemList;
        }

        private async Task<Customer> GetCustomer(Guid customerId, string scopeId)
        {
            var getCustomerRequest = new GetCustomerRequest
            {
                CustomerId = customerId,
                ScopeId = scopeId
            };

            var customer = await OvertureClient.SendAsync(getCustomerRequest).ConfigureAwaitWithCulture(false);

            return customer;
        }



        public async Task<RecurringOrderTemplateLineItemViewModel> MapToTemplateLineItemViewModel(MapToTemplateLineItemViewModelParam param)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.RecurringOrderlineItem == null) { throw new ArgumentNullException(nameof(param.RecurringOrderlineItem)); }
            if (param.CultureInfo == null) { throw new ArgumentNullException(nameof(param.CultureInfo)); }
            if (param.BaseUrl == null) { throw new ArgumentNullException(nameof(param.BaseUrl)); }
            
            var recrurringLineItem = param.RecurringOrderlineItem;

            var vm = ViewModelMapper.MapTo<RecurringOrderTemplateLineItemViewModel>(recrurringLineItem, param.CultureInfo);

            if (vm.IsValid == null)
            {
                vm.IsValid = true;
            }

            ProductMainImage mainImage;
            if (param.ImageDictionnary.TryGetValue(Tuple.Create(recrurringLineItem.ProductId, recrurringLineItem.VariantId), out mainImage))
            {
                vm.ImageUrl = mainImage.ImageUrl;
                vm.FallbackImageUrl = mainImage.FallbackImageUrl;
            }

            var getProductRequest = new Overture.ServiceModel.Requests.Products.GetProductRequest
            {
                ProductId = recrurringLineItem.ProductId,
                ScopeId = recrurringLineItem.ScopeId,
                CultureName = param.CultureInfo.Name,
                IncludePriceLists = true,
                IncludeRelationships = false,
                IncludeVariants = true
            };
            var getProductResponse = OvertureClient.Send(getProductRequest);

            if (getProductResponse == null ||
                (getProductResponse != null && recrurringLineItem.VariantId != string.Empty
                && recrurringLineItem.VariantId != null && 
                getProductResponse.Variants.SingleOrDefault(v => v.Id == recrurringLineItem.VariantId) == null))
            {
                var deleteRecurringLineItem = new DeleteRecurringOrderLineItemsRequest
                {
                    CustomerId = recrurringLineItem.CustomerId,
                    RecurringOrderLineItemIds = new List<Guid> {
                        recrurringLineItem.RecurringOrderLineItemId,
                    },
                    ScopeId = recrurringLineItem.ScopeId
                };
                OvertureClient.Send(deleteRecurringLineItem);

                return await Task.FromResult<RecurringOrderTemplateLineItemViewModel>(null).ConfigureAwaitWithCulture(false);
            }

            var variant = getProductResponse.Variants.SingleOrDefault(v => v.Id == recrurringLineItem.VariantId);

            vm.FormattedNextOccurence = vm.NextOccurence == DateTime.MinValue
                    ? string.Empty
                    : string.Format(param.CultureInfo, "{0:D}", vm.NextOccurence);

            vm.Id = recrurringLineItem.RecurringOrderLineItemId;
            vm.ProductSummary = new CartProductSummaryViewModel();
            vm.ProductSummary.DisplayName = Composer.Utils.ProductHelper.GetProductOrVariantDisplayName(getProductResponse, variant, param.CultureInfo);
            
            var productsPricesVm = await ProductPriceViewService.CalculatePricesAsync(new GetProductsPriceParam()
            {
                CultureInfo = param.CultureInfo,
                Scope = recrurringLineItem.ScopeId,
                ProductIds = new List<string>() { recrurringLineItem.ProductId }
            }).ConfigureAwaitWithCulture(false);

            vm.DefaultListPrice = GetProductOrVariantListPrice(getProductResponse, variant, param.CultureInfo);
            vm.ListPrice = vm.DefaultListPrice;
            var productPriceVm = productsPricesVm.ProductPrices.SingleOrDefault(p => p.ProductId == recrurringLineItem.ProductId);
            if (productPriceVm != null)
            {
                var variantPriceVm = productPriceVm.VariantPrices.SingleOrDefault(v => v.VariantId == recrurringLineItem.VariantId);
                if (variantPriceVm != null)
                {
                    vm.ListPrice = variantPriceVm.ListPrice;
                    vm.IsOnSale = string.CompareOrdinal(variantPriceVm.ListPrice, vm.ListPrice) != 0;
                }
            }

            decimal price;
            var conv = decimal.TryParse(vm.ListPrice, NumberStyles.Currency, param.CultureInfo.NumberFormat, out price);
            if (conv)
            {
                vm.TotalWithoutDiscount = LocalizationProvider.FormatPrice((decimal)vm.Quantity * price, param.CultureInfo);

                vm.Total = LocalizationProvider.FormatPrice((decimal)vm.Quantity * price, param.CultureInfo);
            }

            vm.ProductSummary.Brand = getProductResponse.Brand;
            //TODO
            var list = await Helper.ProductHelper.GetKeyVariantAttributes(getProductResponse, variant, param.CultureInfo, OvertureClient).ConfigureAwaitWithCulture(false);
            if (list != null && list.Count > 0)
            {
                vm.KeyVariantAttributesList = list.ToList();
            }

            vm.ShippingMethodName = recrurringLineItem.FulfillmentMethodName;

            vm.ProductUrl = ProductUrlProvider.GetProductUrl(new GetProductUrlParam
            {
                CultureInfo = param.CultureInfo,
                VariantId = recrurringLineItem.VariantId,
                ProductId = recrurringLineItem.ProductId,
                ProductName = vm.ProductSummary.DisplayName
            });

            return vm;
        }

        private string GetProductOrVariantListPrice(Orckestra.Overture.ServiceModel.Products.Product product, Variant variant, CultureInfo culture)
        {
            if (variant != null)
            {
                return LocalizationProvider.FormatPrice(variant.ListPrice.Value, culture);
            }
            else
            {
                return LocalizationProvider.FormatPrice(product.ListPrice.Value, culture);
            }
        }

        private async Task<AddressViewModel> MapShippingAddress(Guid shippingAddressId, CultureInfo culture)
        {
            var address = await AddressRepository.GetAddressByIdAsync(shippingAddressId).ConfigureAwaitWithCulture(false);

            return CartViewModelFactory.GetAddressViewModel(address, culture);
        }     

        public async virtual Task MapRecurringOrderLineitemFrequencyName(RecurringOrderTemplateViewModel template, CultureInfo culture)
        {
            if (template.RecurringOrderTemplateLineItemViewModels == null) { return; }

            var uniqueProgramNames = template.RecurringOrderTemplateLineItemViewModels
                                            .Select(x => x.RecurringOrderProgramName)
                                            .Where(l => !string.IsNullOrWhiteSpace(l))
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .ToList();

            if (uniqueProgramNames.Count > 0)
            {
                var tasks = uniqueProgramNames.Select(programName => RecurringOrdersRepository.GetRecurringOrderProgram(ComposerContext.Scope, programName));
                var programs = await Task.WhenAll(tasks).ConfigureAwait(false);

                foreach (var lineitem in template.RecurringOrderTemplateLineItemViewModels)
                {
                    if (RecurringOrderTemplateHelper.IsRecurringOrderLineItemValid(lineitem))
                    {
                        var program = programs.FirstOrDefault(p => string.Equals(p.RecurringOrderProgramName, lineitem.RecurringOrderProgramName, StringComparison.OrdinalIgnoreCase));

                        if (program != null)
                        {
                            var frequency = program.Frequencies.FirstOrDefault(f => string.Equals(f.RecurringOrderFrequencyName, lineitem.RecurringOrderFrequencyName, StringComparison.OrdinalIgnoreCase));

                            if (frequency != null)
                            {
                                var localization = frequency.Localizations.FirstOrDefault(l => string.Equals(l.CultureIso, culture.Name, StringComparison.OrdinalIgnoreCase));

                                if (localization != null)
                                    lineitem.RecurringOrderFrequencyDisplayName = localization.DisplayName;
                                else
                                    lineitem.RecurringOrderFrequencyDisplayName = frequency.RecurringOrderFrequencyName;
                            }
                        }
                    }
                }
            }
        }


        public virtual RecurringOrderShippingMethodViewModel GetShippingMethodViewModel(FulfillmentMethodInfo fulfillmentMethodInfo, CultureInfo cultureInfo)
        {
            if (fulfillmentMethodInfo == null)
            {
                return null;
            }

            var shippingMethodViewModel = ViewModelMapper.MapTo<RecurringOrderShippingMethodViewModel>(fulfillmentMethodInfo, cultureInfo);

            return shippingMethodViewModel;
        }
    }
}