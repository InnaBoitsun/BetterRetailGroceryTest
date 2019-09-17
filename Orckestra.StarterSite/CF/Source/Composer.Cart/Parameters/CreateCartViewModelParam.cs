﻿using Orckestra.Composer.Providers.Dam;
using System.Collections.Generic;
using System.Globalization;

namespace Orckestra.Composer.Cart.Parameters
{
    public class CreateCartViewModelParam
    {
        /// <summary>
        /// Cart to map.
        /// </summary>
        public Overture.ServiceModel.Orders.Cart Cart { get; set; }

        /// <summary>
        /// Culture Info for the ViewModel.
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// Product Image information
        /// </summary>
        public ProductImageInfo ProductImageInfo { get; set; }

        /// <summary>
        /// Determines if the invalid coupon messages are included in the VM or not.
        /// </summary>
        public bool IncludeInvalidCouponsMessages { get; set; }

        /// <summary>
        /// The Request Base Url
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The Payment Methods display name.
        /// </summary>
        public Dictionary<string, string> PaymentMethodDisplayNames { get; set; }
    }
}
