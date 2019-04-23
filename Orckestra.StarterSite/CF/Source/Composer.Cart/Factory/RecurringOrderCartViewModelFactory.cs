﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orckestra.Composer.Cart.Helper;
using Orckestra.Composer.Cart.Parameters;
using Orckestra.Composer.Cart.ViewModels;
using Orckestra.Composer.Parameters;
using Orckestra.Composer.Providers;
using Orckestra.Composer.Services;
using Orckestra.Composer.ViewModels;
using Orckestra.Overture.ServiceModel.Orders;
using Orckestra.Overture.ServiceModel.RecurringOrders;

namespace Orckestra.Composer.Cart.Factory
{
    public class RecurringOrderCartViewModelFactory : IRecurringOrderCartViewModelFactory
    {
        protected ICartViewModelFactory CartViewModelFactory { get; private set; }
        protected IViewModelMapper ViewModelMapper { get; private set; }
        protected IComposerContext ComposerContext { get; private set; }
        protected IRecurringCartUrlProvider RecurringCartUrlProvider { get; private set; }

        public RecurringOrderCartViewModelFactory(
            ICartViewModelFactory cartViewModelFactory,
            IViewModelMapper viewModelMapper,
            IComposerContext composerContext,
            IRecurringCartUrlProvider recurringCartUrlProvider)
        {
            if (cartViewModelFactory == null) { throw new ArgumentNullException(nameof(cartViewModelFactory)); }
            if (viewModelMapper == null) { throw new ArgumentNullException(nameof(viewModelMapper)); }
            if (composerContext == null) { throw new ArgumentNullException(nameof(composerContext)); }
            if (recurringCartUrlProvider == null) { throw new ArgumentNullException(nameof(recurringCartUrlProvider)); }

            CartViewModelFactory = cartViewModelFactory;
            ViewModelMapper = viewModelMapper;
            ComposerContext = composerContext;
            RecurringCartUrlProvider = recurringCartUrlProvider;
        }

        public IRecurringOrderCartViewModel CreateRecurringOrderCartViewModel(CreateRecurringOrderCartViewModelParam param)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.CultureInfo == null) { throw new ArgumentNullException(nameof(param.CultureInfo)); }
            if (param.ProductImageInfo == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo)); }
            if (param.ProductImageInfo.ImageUrls == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo.ImageUrls)); }
            if (string.IsNullOrWhiteSpace(param.BaseUrl)) { throw new ArgumentException(nameof(param.BaseUrl)); }

            var vm = CartViewModelFactory.CreateCartViewModel(new CreateCartViewModelParam
            {
                Cart = param.Cart,
                CultureInfo = param.CultureInfo,
                ProductImageInfo = param.ProductImageInfo,
                BaseUrl = param.BaseUrl,
                PaymentMethodDisplayNames = param.PaymentMethodDisplayNames,
                IncludeInvalidCouponsMessages = param.IncludeInvalidCouponsMessages
            });

            var roCartVm = vm.AsExtensionModel<IRecurringOrderCartViewModel>();

            FillNextOcurrence(roCartVm, param.Cart, param.CultureInfo);
            MapRecurringOrderLineitemFrequencyName(vm, param.CultureInfo, param.RecurringOrderPrograms);

            return roCartVm;
        }

        private void FillNextOcurrence(IRecurringOrderCartViewModel vm, Overture.ServiceModel.Orders.Cart cart, CultureInfo cultureInfo)
        {
            vm.NextOccurence = GetNextOccurenceDate(cart.Shipments.First());
            vm.FormatedNextOccurence = GetFormattedNextOccurenceDate(vm.NextOccurence, cultureInfo);
        }

        private void FillNextOcurrence(LightRecurringOrderCartViewModel vm, Overture.ServiceModel.Orders.Cart cart, CultureInfo cultureInfo)
        {
            vm.NextOccurence = GetNextOccurenceDate(cart.Shipments.First());
            vm.FormatedNextOccurence = GetFormattedNextOccurenceDate(vm.NextOccurence, cultureInfo);
        }

        private static DateTime GetNextOccurenceDate(Shipment shipment)
        {
            return shipment.FulfillmentScheduledTimeBeginDate ?? DateTime.MinValue;
        }

        private static string GetFormattedNextOccurenceDate(DateTime date, CultureInfo culture)
        {
            return date == DateTime.MinValue
                    ? string.Empty
                    : string.Format(culture, "{0:D}", date);
        }

        private void MapRecurringOrderLineitemFrequencyName(CartViewModel recurringOrderCartViewModel, CultureInfo culture, List<RecurringOrderProgram> recurringOrderPrograms)
        {
            if (recurringOrderCartViewModel.LineItemDetailViewModels == null) { return; }

            foreach (var lineitem in recurringOrderCartViewModel.LineItemDetailViewModels)
            {
                if (RecurringOrderCartHelper.IsRecurringOrderLineItemValid(lineitem))
                {
                    var program = recurringOrderPrograms.FirstOrDefault(p => string.Equals(p.RecurringOrderProgramName, lineitem.RecurringOrderProgramName, StringComparison.OrdinalIgnoreCase));

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

        public LightRecurringOrderCartViewModel CreateLightRecurringOrderCartViewModel(CreateLightRecurringOrderCartViewModelParam param)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            if (param.CultureInfo == null) { throw new ArgumentNullException(nameof(param.CultureInfo)); }
            if (param.ProductImageInfo == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo)); }
            if (param.ProductImageInfo.ImageUrls == null) { throw new ArgumentNullException(nameof(param.ProductImageInfo.ImageUrls)); }
            if (string.IsNullOrWhiteSpace(param.BaseUrl)) { throw new ArgumentException(nameof(param.BaseUrl)); }
            
            var vm = ViewModelMapper.MapTo<LightRecurringOrderCartViewModel>(param.Cart, param.CultureInfo);

            //TODO 
            //vm.LineItemDetailViewModels = LineItemViewModelFactory.CreateLightViewModel(new CreateLightListOfLineItemDetailViewModelParam
            //{
            //    Cart = param.Cart,
            //    LineItems = param.Cart.GetLineItems(),
            //    CultureInfo = param.CultureInfo,
            //    ImageInfo = param.ProductImageInfo,
            //    BaseUrl = param.BaseUrl
            //}).ToList();
            
            FillNextOcurrence(vm, param.Cart, param.CultureInfo);
            vm.CartDetailUrl = GetRecurringCartDetailUrl(param.CultureInfo, param.Cart.Name);

            // Reverse the items order in the Cart so the last added item will be the first in the list
            if (vm.LineItemDetailViewModels != null)
            {
                vm.LineItemDetailViewModels.Reverse();
            }

            vm.IsAuthenticated = ComposerContext.IsAuthenticated;

            return vm;
            
        }

        private string GetRecurringCartDetailUrl(CultureInfo cultureInfo, string cartName)
        {
            string recurringCartsPageUrl = RecurringCartUrlProvider.GetRecurringCartsUrl(new GetRecurringCartsUrlParam
            {
                CultureInfo = cultureInfo
            });

            return RecurringCartUrlProvider.GetRecurringCartDetailsUrl(new GetRecurringCartDetailsUrlParam
            {
                CultureInfo = cultureInfo,
                ReturnUrl = recurringCartsPageUrl,
                RecurringCartName = cartName
            });
        }
    }
}
