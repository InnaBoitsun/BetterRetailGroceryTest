///<reference path='../../../../Composer.UI/Source/Typings/tsd.d.ts' />


module Orckestra.Composer {
    export interface IAddRecurringOrderCartLineItemParam {
        cartName: string;
        productId: string;
        productDisplayName: string;
        variantId: string;
        sku: string;
        quantity: number;
        recurringOrderFrequencyName: string;
        recurringOrderProgramName: string;
    }

    export interface IRecurringOrderCartUpdateShippingMethodParam {
        shippingProviderId: string;
        shippingMethodName: string;
        cartName: string;
    }

    export interface IRecurringOrderGetCartShippingMethods {
        cartName: string;
    }

    export interface IRecurringOrderLineItemDeleteParam {
        lineItemId: string;
        cartName: string;
    }

    export interface IRecurringOrderLineItems {
        RecurringOrderTemplateLineItemId: string;
    }

    export interface IRecurringOrderLineItemsDeleteParam {
        lineItemsIds: string[];
        cartName: string;
    }

    export interface IRecurringOrderLineItemsUpdateDateParam {
        CartName: string;
        LineItems: IRecurringOrderLineItems[];
        NextRunDate: Date;
    }

    export interface IRecurringOrderProgramsByNamesParam {
        recurringOrderProgramNames: string[];
    }

    export interface IRecurringOrderTemplateLineItemDeleteParam {
        lineItemId: string;
    }

    export interface IRecurringOrderTemplateLineItemUpdateParam {
        paymentMethodId: string;
        shippingAddressId: string;
        billingAddressId: string;
        lineItemId: string;
        nextOccurence: Date;
        frequencyName: string;
        shippingProviderId: string;
        shippingMethodName: string;
    }

    export interface IRecurringOrderTemplateLineItemsDeleteParam {
        lineItemsIds: string[];
    }

    export interface IRecurringOrderTemplateUpdateLineItemQuantityParam {
        lineItemId: string;
        quantity: number;
    }

    export interface IRecurringOrderUpdateLineItemQuantityParam {
        lineItemId: string;
        quantity: number;
        cartName: string;
    }

    export interface IRecurringOrderUpdateTemplateAddressParam {
        shippingAddressId: string;
        billingAddressId: string;
        cartName: string;
        useSameForShippingAndBilling: boolean;
    }

    export interface IRecurringOrderUpdateTemplatePaymentMethodParam {
        paymentMethodId: string;
        cartName: string;
        providerName: string;
    }
}
