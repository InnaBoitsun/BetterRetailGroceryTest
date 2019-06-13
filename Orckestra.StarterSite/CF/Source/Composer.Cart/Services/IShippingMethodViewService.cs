﻿using System.Threading.Tasks;
using Orckestra.Composer.Cart.Parameters;
using Orckestra.Composer.Cart.ViewModels;

namespace Orckestra.Composer.Cart.Services
{
    /// <summary>
    /// Service for dealing with Shipping Methods.
    /// </summary>
    public interface IShippingMethodViewService
    {
        /// <summary>
        /// Get the Shipping methods available for a shipment.
        /// </summary>
        /// <param name="param"></param>
        /// <returns>The ShippingMethodsViewModel</returns>
        Task<ShippingMethodsViewModel> GetShippingMethodsAsync(GetShippingMethodsParam param);

        /// <summary>
        /// Set the cheapest shipping method in the cart.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<CartViewModel> SetCheapestShippingMethodAsync(SetCheapestShippingMethodParam param);

        /// <summary>
        /// Estimates the shipping method. Does not save the cart.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<ShippingMethodViewModel> EstimateShippingAsync(EstimateShippingParam param);
    }
}